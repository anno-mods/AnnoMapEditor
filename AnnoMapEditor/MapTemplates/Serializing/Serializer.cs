using FileDBReader;
using FileDBReader.src.XmlRepresentation;
using FileDBSerializing;
using FileDBSerializing.ObjectSerializer;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml;

namespace AnnoMapEditor.MapTemplates.Serializing
{
    internal class Serializer
    {
        public static async Task<T?> ReadAsync<T>(Stream? stream) where T : class, new()
        {
            if (stream is null)
                return null;

            return await Task.Run(() =>
            {
                try
                {
                    stream.Seek(0, SeekOrigin.Begin);
                    var version = VersionDetector.GetCompressionVersion(stream);
                    stream.Seek(0, SeekOrigin.Begin);
                    return FileDBConvert.DeserializeObject<T>(stream,
                        new FileDBSerializerOptions() { Version = version });
                }
                catch
                {
                    return null;
                }
            });
        }

        public static async Task<T?> ReadFromXmlAsync<T>(Stream? stream) where T : class, new()
        {
            if (stream is null)
                return null;

            return await Task.Run(() =>
            {
                try
                { 
                    // load xml
                    XmlDocument xmlDocument = new();
                    xmlDocument.Load(stream);

                    // convert to bytes
                    XmlDocument? interpreterDocument = GetEmbeddedXmlDocument("AnnoMapEditor.Mods.Serialization.a7tinfo.xml");
                    if (interpreterDocument is null)
                        return null;
                    XmlDocument xmlWithBytes = new XmlExporter(xmlDocument, new(interpreterDocument)).Run();

                    // convert to FileDB
                    XmlFileDbConverter converter = new(FileDBDocumentVersion.Version1);
                    IFileDBDocument doc = converter.ToFileDb(xmlWithBytes);

                    // construct deserialize into objects
                    FileDBDocumentDeserializer<T> deserializer = new(new FileDBSerializerOptions() { IgnoreMissingProperties = true });
                    return deserializer.GetObjectStructureFromFileDBDocument(doc);
                }
                catch
                {
                    return null;
                }
            });
        }

        public static async Task WriteAsync(object data, Stream? stream)
        {
            if (stream is null)
                return;

            await Task.Run(() =>
            {
                try
                {
                    FileDBConvert.SerializeObject(data, new() { Version = FileDBDocumentVersion.Version1 }, stream);
                }
                catch
                {
                }
            });
        }

        public static async Task WriteToXmlAsync(object data, Stream? stream)
        {
            if (stream is null)
                return;

            await Task.Run(() =>
            {
                try
                {
                    FileDBDocumentSerializer serializer = new(new FileDBSerializerOptions());
                    IFileDBDocument doc = serializer.WriteObjectStructureToFileDBDocument(data);

                    // convert to xml with bytes
                    FileDbXmlConverter converter = new();
                    XmlDocument xmlWithBytes = converter.ToXml(doc);

                    // interpret bytes
                    XmlDocument? interpreterDocument = GetEmbeddedXmlDocument("AnnoMapEditor.Mods.Serialization.a7tinfo.xml");
                    if (interpreterDocument is null)
                        return;
                    XmlDocument xmlDocument = new XmlInterpreter(xmlWithBytes, new(interpreterDocument)).Run();

                    xmlDocument.Save(stream);
                }
                catch
                {
                }
            });
        }

        private static XmlDocument? GetEmbeddedXmlDocument(string resourceName)
        {
            Assembly me = Assembly.GetExecutingAssembly();
            using (var resource = me.GetManifestResourceStream(resourceName))
            {
                if (resource is not null)
                {
                    XmlDocument doc = new();
                    doc.Load(resource);
                    return doc;
                }
            }
            return null;
        }
    }
}
