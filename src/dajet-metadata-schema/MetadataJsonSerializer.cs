using DaJet.Json;
using DaJet.Metadata.Model;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

namespace DaJet.Metadata.Schema
{
    public interface IMetadataJsonSerializer
    {
        ISerializationBinder Binder { get; }
        ApplicationObject FromJson(string json);
        string ToJson(ApplicationObject message);
    }
    public sealed class MetadataJsonSerializer : IMetadataJsonSerializer
    {
        private readonly InfoBase _infoBase;

        private readonly IReferenceResolver _resolver = new JsonReferenceResolver();
        private readonly ISerializationBinder _binder = new JsonSerializationBinder();
        private readonly JsonSerializerOptions _options = new JsonSerializerOptions();
        public MetadataJsonSerializer(InfoBase infoBase)
        {
            _infoBase = infoBase;
            _options.WriteIndented = true;
            _options.Encoder = JavaScriptEncoder.Create(UnicodeRanges.All);
            _options.Converters.Add(new MetadataJsonConverter(_infoBase, _binder, _resolver));
        }
        public ISerializationBinder Binder { get { return _binder; } }
        public ApplicationObject FromJson(string json)
        {
            return JsonSerializer.Deserialize<ApplicationObject>(json, _options);
        }
        public string ToJson(ApplicationObject metadataObject)
        {
            _resolver.Clear();
            return JsonSerializer.Serialize(metadataObject, metadataObject.GetType(), _options);
        }
    }
}