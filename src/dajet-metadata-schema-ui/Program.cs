using DaJet.Metadata.Model;
using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Text;

namespace DaJet.Metadata.Schema.UI
{
    public static class Program
    {
        private const string SERVER_IS_NOT_DEFINED_ERROR = "Server address is not defined.";
        private const string DATABASE_IS_NOT_DEFINED_ERROR = "Database name is not defined.";
        public static int Main(string[] args)
        {
#if DEBUG
            args = new string[] { "--ms", "zhichkin", "--d", "dajet-metadata", "--out-file", "C:\\temp\\json_schema"};
#endif
            RootCommand command = new RootCommand()
            {
                new Option<string>("--ms", "Microsoft SQL Server address or name"),
                new Option<string>("--pg", "PostgresSQL server address or name"),
                new Option<string>("--d", "Database name"),
                new Option<string>("--u", "User name (Windows authentication is used if not defined)"),
                new Option<string>("--p", "User password if SQL Server authentication is used"),
                new Option<string>("--m", "Publication name to generate JSON Schema"),
                new Option<FileInfo>("--out-file", "Catalog path to save generated JSON Schema")
            };
            command.Description = "DaJet (JSON Schema generation utility)";
            command.Handler = CommandHandler.Create<string, string, string, string, string, string, FileInfo>(ExecuteCommand);
            return command.Invoke(args);
        }
        private static void ShowErrorMessage(string errorText)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(errorText);
            Console.ForegroundColor = ConsoleColor.White;
        }
        private static void ExecuteCommand(string ms, string pg, string d, string u, string p, string m, FileInfo outFile)
        {
            if (string.IsNullOrWhiteSpace(ms) && string.IsNullOrWhiteSpace(pg))
            {
                ShowErrorMessage(SERVER_IS_NOT_DEFINED_ERROR); return;
            }
            if (string.IsNullOrWhiteSpace(d))
            {
                ShowErrorMessage(DATABASE_IS_NOT_DEFINED_ERROR); return;
            }

            IMetadataService metadataService = new MetadataService();

            if (!string.IsNullOrWhiteSpace(ms))
            {
                metadataService
                    .UseDatabaseProvider(DatabaseProvider.SQLServer)
                    .ConfigureConnectionString(ms, d, u, p);
            }
            else if (!string.IsNullOrWhiteSpace(pg))
            {
                metadataService
                    .UseDatabaseProvider(DatabaseProvider.PostgreSQL)
                    .ConfigureConnectionString(pg, d, u, p);
            }

            if (outFile != null && !string.IsNullOrWhiteSpace(m))
            {
                SaveSchemaToFile(outFile.FullName, metadataService);
            }
        }
        private static void SaveSchemaToFile(string catalogPath, IMetadataService metadataService)
        {
            InfoBase infoBase = metadataService.OpenInfoBase();

            MetadataJsonSerializer serializer = new MetadataJsonSerializer(infoBase);

            string infoBaseCatalog = Path.Combine(catalogPath, infoBase.ConfigInfo.Name);
            if (!Directory.Exists(infoBaseCatalog))
            {
                Directory.CreateDirectory(infoBaseCatalog);
            }

            string rootCatalog = Path.Combine(infoBaseCatalog, infoBase.ConfigInfo.ConfigVersion.Replace('.', '_'));
            if (!Directory.Exists(rootCatalog))
            {
                Directory.CreateDirectory(rootCatalog);
            }

            GenerateSchemaCatalog(rootCatalog, infoBase, serializer);
            GenerateSchemaDocument(rootCatalog, infoBase, serializer);
            GenerateSchemaEnumeration(rootCatalog, infoBase, serializer);
        }
        private static void GenerateSchemaEnumeration(string rootCatalog, InfoBase infoBase, MetadataJsonSerializer serializer)
        {
            string catalogPath = Path.Combine(rootCatalog, "Перечисления");
            if (!Directory.Exists(catalogPath))
            {
                Directory.CreateDirectory(catalogPath);
            }

            foreach (ApplicationObject appObj in infoBase.Enumerations.Values)
            {
                string schema = serializer.ToJson(appObj);

                string fileName = Path.Combine(catalogPath, appObj.Name + ".json");

                using (StreamWriter writer = new StreamWriter(fileName, false, Encoding.UTF8))
                {
                    writer.Write(schema);
                }
            }
        }
        private static void GenerateSchemaCatalog(string rootCatalog, InfoBase infoBase, MetadataJsonSerializer serializer)
        {
            string catalogPath = Path.Combine(rootCatalog, "Справочники");
            if (!Directory.Exists(catalogPath))
            {
                Directory.CreateDirectory(catalogPath);
            }

            foreach (ApplicationObject appObj in infoBase.Catalogs.Values)
            {
                string schema = serializer.ToJson(appObj);

                string fileName = Path.Combine(catalogPath, appObj.Name + ".json");

                using (StreamWriter writer = new StreamWriter(fileName, false, Encoding.UTF8))
                {
                    writer.Write(schema);
                }
            }
        }
        private static void GenerateSchemaDocument(string rootCatalog, InfoBase infoBase, MetadataJsonSerializer serializer)
        {
            string catalogPath = Path.Combine(rootCatalog, "Документы");
            if (!Directory.Exists(catalogPath))
            {
                Directory.CreateDirectory(catalogPath);
            }

            foreach (ApplicationObject appObj in infoBase.Documents.Values)
            {
                string schema = serializer.ToJson(appObj);

                string fileName = Path.Combine(catalogPath, appObj.Name + ".json");

                using (StreamWriter writer = new StreamWriter(fileName, false, Encoding.UTF8))
                {
                    writer.Write(schema);
                }
            }
        }
    }
}