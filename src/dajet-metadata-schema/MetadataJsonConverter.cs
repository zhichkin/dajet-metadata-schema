using DaJet.Json;
using DaJet.Metadata.Model;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DaJet.Metadata.Schema
{
    public sealed class MetadataJsonConverter : JsonConverter<ApplicationObject>
    {
        private readonly InfoBase _infoBase;
        private readonly IReferenceResolver _resolver;
        private readonly ISerializationBinder _binder;

        #region "String resources"

        private const string CONST_jcfg = "jcfg";
        private const string CONST_TYPE = "#type";
        private const string CONST_VALUE = "#value";

        private const string KEYWORD_id = "$id";
        private const string KEYWORD_ref = "$ref";
        private const string KEYWORD_schema = "$schema";
        private const string KEYWORD_type = "type";
        private const string KEYWORD_oneOf = "oneOf";
        private const string KEYWORD_null = "null";
        private const string KEYWORD_uuid = "uuid";
        private const string KEYWORD_date = "date";
        private const string KEYWORD_time = "time";
        private const string KEYWORD_date_time = "date-time";
        private const string KEYWORD_base64 = "base64";
        private const string KEYWORD_const = "const";
        private const string KEYWORD_format = "format";
        private const string KEYWORD_string = "string";
        private const string KEYWORD_number = "number";
        private const string KEYWORD_object = "object";
        private const string KEYWORD_boolean = "boolean";
        private const string KEYWORD_minLength = "minLength";
        private const string KEYWORD_maxLength = "maxLength";
        private const string KEYWORD_enum = "enum";
        private const string KEYWORD_array = "array";
        private const string KEYWORD_items = "items";
        private const string KEYWORD_properties = "properties";
        private const string KEYWORD_required = "required";
        private const string KEYWORD_additionalProperties = "additionalProperties";
        private const string KEYWORD_pattern = "pattern";
        private const string KEYWORD_definitions = "definitions";

        #endregion

        #region "Dictionaries"

        private Dictionary<Type, string> BaseTypeNames = new Dictionary<Type, string>()
        {
            { typeof(Catalog), "CatalogObject" },
            { typeof(Document), "DocumentObject" },
            { typeof(Enumeration), "Enumeration" },
            { typeof(InformationRegister), "InformationRegisterRecordSet" },
            { typeof(AccumulationRegister), "AccumulationRegisterRecordSet" }
        };
        private Dictionary<Type, string> ReferenceTypeNames = new Dictionary<Type, string>()
        {
            { typeof(Enumeration), "EnumRef" },
            { typeof(Catalog), "CatalogRef" },
            { typeof(Document), "DocumentRef" }
        };
        private Dictionary<string, string> SystemPropertyNames = new Dictionary<string, string>()
        {
            { "ВерсияДанных", null },
            { "Предопределённый", null },
            { "КлючСтроки", null },
            { "НомерСтроки", null },
            { "ЭтоГруппа", "IsFolder" },
            { "Ссылка", "Ref" },
            { "ПометкаУдаления", "DeletionMark" },
            { "Владелец", "Owner" },
            { "Родитель", "Parent" },
            { "Код", "Code" },
            { "Наименование", "Description" },
            { "ПериодНомера", null },
            { "Дата", "Date" },
            { "Номер", "Number" },
            { "Проведён", "Posted" }
        };

        #endregion

        public MetadataJsonConverter(InfoBase infoBase, ISerializationBinder binder, IReferenceResolver resolver) : base()
        {
            _binder = binder;
            _resolver = resolver;
            _infoBase = infoBase;
        }
        public override bool CanConvert(Type typeToConvert)
        {
            if (typeToConvert == null) return false;

            return typeToConvert == typeof(Catalog)
                || typeToConvert == typeof(Document)
                || typeToConvert == typeof(Enumeration)
                || typeToConvert == typeof(InformationRegister)
                || typeToConvert == typeof(AccumulationRegister);

            //return typeToConvert.IsAssignableFrom(typeof(ApplicationObject));
        }


        private string GetObjectTypeName(ApplicationObject value)
        {
            if (!BaseTypeNames.TryGetValue(value.GetType(), out string baseName))
            {
                throw new ArgumentOutOfRangeException($"Unknown type: {value.GetType()}.");
            }
            return string.Format("{0}.{1}", baseName, value.Name);
        }
        private string GetReferenceTypeName(ApplicationObject value)
        {
            if (ReferenceTypeNames.TryGetValue(value.GetType(), out string typeName))
            {
                return string.Format("{0}.{1}", typeName, value.Name);
            }
            return string.Format("{0}.{1}", "Unknown", value.Name);
        }


        public override ApplicationObject Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }



        public override void Write(Utf8JsonWriter writer, ApplicationObject value, JsonSerializerOptions options)
        {
            if (value is Enumeration enumeration)
            {
                WriteEnumeration(writer, enumeration, options);
            }
            else
            {
                WriteReferenceType(writer, value, options);
            }
        }
        private void WriteEnumeration(Utf8JsonWriter writer, Enumeration enumeration, JsonSerializerOptions options)
        {
            writer.WriteStartObject(); // start schema
            //writer.WriteString(KEYWORD_schema, SCHEMA_VERSION);
            writer.WriteString(KEYWORD_id, string.Format("{0}:{1}", CONST_jcfg, GetObjectTypeName(enumeration)));
            writer.WritePropertyName(KEYWORD_enum);
            writer.WriteStartArray();
            foreach (EnumValue value in enumeration.Values)
            {
                writer.WriteStringValue(value.Name);
            }
            writer.WriteEndArray();
            writer.WriteEndObject(); // end schema
        }
        private void WriteReferenceType(Utf8JsonWriter writer, ApplicationObject value, JsonSerializerOptions options)
        {
            writer.WriteStartObject(); // start schema
            //writer.WriteString(KEYWORD_schema, SCHEMA_VERSION);
            writer.WriteString(KEYWORD_id, string.Format("{0}:{1}", CONST_jcfg, GetObjectTypeName(value)));
            writer.WriteString(KEYWORD_type, KEYWORD_object);
            writer.WriteBoolean(KEYWORD_additionalProperties, false);

            writer.WritePropertyName(KEYWORD_required);
            writer.WriteStartArray();
            writer.WriteStringValue(CONST_TYPE);
            writer.WriteStringValue(CONST_VALUE);
            writer.WriteEndArray();

            writer.WritePropertyName(KEYWORD_properties);

            writer.WriteStartObject(); // start properties

            writer.WritePropertyName(CONST_TYPE);
            writer.WriteStartObject(); // start #type
            writer.WriteString(KEYWORD_const, string.Format("{0}:{1}", CONST_jcfg, GetObjectTypeName(value)));
            writer.WriteEndObject(); // end #type

            writer.WritePropertyName(CONST_VALUE);
            writer.WriteStartObject(); // start #value
            writer.WriteString(KEYWORD_type, KEYWORD_object);
            writer.WriteBoolean(KEYWORD_additionalProperties, false);
            writer.WritePropertyName(KEYWORD_properties);

            WriteProperties(writer, value);

            writer.WriteEndObject(); // end #value
            writer.WriteEndObject(); // end properties
            writer.WriteEndObject(); // end schema
        }

        private void WriteProperties(Utf8JsonWriter writer, ApplicationObject value)
        {
            writer.WriteStartObject();

            foreach (MetadataProperty property in value.Properties)
            {
                if (property.Name == "Ссылка" && value.GetType() == typeof(TablePart))
                {
                    continue;
                }
                if (IgnoreProperty(property.Name))
                {
                    continue;
                }
                WriteProperty(writer, property);
            }

            foreach (TablePart tablePart in value.TableParts)
            {
                WriteTablePart(writer, tablePart);
            }

            writer.WriteEndObject();
        }
        private bool IgnoreProperty(string name)
        {
            if (!SystemPropertyNames.TryGetValue(name, out string systemName))
            {
                return false;
            }
            return (systemName == null);
        }
        private void WriteProperty(Utf8JsonWriter writer, MetadataProperty property)
        {
            if (SystemPropertyNames.TryGetValue(property.Name, out string systemName))
            {
                writer.WritePropertyName(systemName);
            }
            else
            {
                writer.WritePropertyName(property.Name);
            }

            if (property.PropertyType.IsMultipleType)
            {
                WriteMultipleTypeProperty(writer, property);
            }
            else
            {
                WriteSingleTypeProperty(writer, property);
            }
        }
        private void WriteSingleTypeProperty(Utf8JsonWriter writer, MetadataProperty property)
        {
           if (property.PropertyType.IsUuid)
            {
                writer.WriteStartObject();
                writer.WriteString(KEYWORD_type, KEYWORD_string);
                writer.WriteString(KEYWORD_format, KEYWORD_uuid);
                writer.WriteEndObject();
            }
            else if (property.PropertyType.IsValueStorage)
            {
                writer.WriteStartObject();
                writer.WriteString(KEYWORD_type, KEYWORD_string);
                writer.WriteString(KEYWORD_format, KEYWORD_base64);
                writer.WriteEndObject();
            }
            else if (property.PropertyType.CanBeString)
            {
                writer.WriteStartObject();
                writer.WriteString(KEYWORD_type, KEYWORD_string);
                if (property.PropertyType.StringLength > 0)
                {
                    writer.WriteNumber(KEYWORD_maxLength, property.PropertyType.StringLength);
                }
                writer.WriteEndObject();
            }
            else if (property.PropertyType.CanBeNumeric)
            {
                writer.WriteStartObject();
                writer.WriteString(KEYWORD_type, KEYWORD_number);
                writer.WriteEndObject();
            }
            else if (property.PropertyType.CanBeBoolean)
            {
                writer.WriteStartObject();
                writer.WriteString(KEYWORD_type, KEYWORD_boolean);
                writer.WriteEndObject();
            }
            else if (property.PropertyType.CanBeDateTime)
            {
                writer.WriteStartObject();
                writer.WriteString(KEYWORD_type, KEYWORD_string);
                writer.WriteString(KEYWORD_format, KEYWORD_date_time);
                writer.WriteEndObject();
            }
            else if (property.PropertyType.CanBeReference)
            {
                writer.WriteStartObject();
                if (_infoBase.ReferenceTypeUuids.TryGetValue(property.PropertyType.ReferenceTypeUuid, out ApplicationObject appObj))
                {
                    writer.WriteString(KEYWORD_id, string.Format("{0}:{1}", CONST_jcfg, GetReferenceTypeName(appObj)));
                }
                writer.WriteString(KEYWORD_type, KEYWORD_string);
                if (appObj == null || appObj.GetType() != typeof(Enumeration))
                {
                    writer.WriteString(KEYWORD_format, KEYWORD_uuid);
                }
                writer.WriteEndObject();
            }
        }
        private void WriteMultipleTypeProperty(Utf8JsonWriter writer, MetadataProperty property)
        {
            writer.WriteStartObject(); // start oneOf
            writer.WritePropertyName(KEYWORD_oneOf);
            writer.WriteStartArray();
            
            writer.WriteStartObject();
            writer.WriteString(KEYWORD_type, KEYWORD_null);
            writer.WriteEndObject();

            writer.WriteStartObject();
            writer.WriteString(KEYWORD_type, KEYWORD_object);
            writer.WriteBoolean(KEYWORD_additionalProperties, false);
            writer.WritePropertyName(KEYWORD_required);
            writer.WriteStartArray();
            writer.WriteStringValue(CONST_TYPE);
            writer.WriteStringValue(CONST_VALUE);
            writer.WriteEndArray();
            writer.WritePropertyName(KEYWORD_properties);
            writer.WriteStartObject();
            writer.WritePropertyName(CONST_TYPE);
            writer.WriteStartObject();
            writer.WriteString(KEYWORD_type, KEYWORD_string);
            writer.WriteEndObject();
            writer.WritePropertyName(CONST_VALUE);
            writer.WriteStartObject();
            writer.WritePropertyName(KEYWORD_type);
            writer.WriteStartArray();
            writer.WriteStringValue(KEYWORD_string);
            writer.WriteStringValue(KEYWORD_number);
            writer.WriteStringValue(KEYWORD_boolean);
            writer.WriteEndArray();
            writer.WriteEndObject();
            writer.WriteEndObject();
            writer.WriteEndObject();

            writer.WriteEndArray();
            writer.WriteEndObject(); // end oneOf

            // {
            //   "oneOf":
            //   [
            //     { "type": "null" },
            //     {
            //       "type": "object",
            //       "additionalProperties": false,
            //       "required": [ "#type", "#value" ],
            //       "properties":
            //       {
            //         "#type": { "type": "string" },
            //         "#value": { "type": [ "string", "number", "boolean" ] }
            //       }
            //     }
            //   ]
            // }
        }
        private void WriteTablePart(Utf8JsonWriter writer, TablePart tablePart)
        {
            writer.WritePropertyName(tablePart.Name);
            
            writer.WriteStartObject();
            writer.WriteString(KEYWORD_type, KEYWORD_array);
            writer.WritePropertyName(KEYWORD_items);

            writer.WriteStartObject();
            writer.WriteString(KEYWORD_type, KEYWORD_object);

            writer.WriteBoolean(KEYWORD_additionalProperties, false);
            writer.WritePropertyName(KEYWORD_properties);

            WriteProperties(writer, tablePart);

            writer.WriteEndObject();
            writer.WriteEndObject();
        }
    }
}