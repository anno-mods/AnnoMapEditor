using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace AnnoMapEditor.Mods.Serialization
{
    public class A7teExporter
    {
        public A7teExporter(int mapSize)
        {
            MapSize = mapSize;
        }

        private int MapSize { get; }

        public void ExportA7te(string a7tePath)
        {
            AnnoEditorLevel a7te = new(MapSize);

            XmlSerializer a7teSerializer = new(typeof(AnnoEditorLevel));
            XmlWriterSettings xmlSettings = new() { Indent = true, IndentChars = "  ", OmitXmlDeclaration = true, Async = true };
            XmlSerializerNamespaces noNamespaces = new(new XmlQualifiedName[] { XmlQualifiedName.Empty });

            using StreamWriter streamWriter = new(a7tePath, false, Encoding.UTF8);
            using XmlWriter xmlWriter = XmlWriter.Create(streamWriter, xmlSettings);

            a7teSerializer.Serialize(xmlWriter, a7te, noNamespaces);
        }
    }
}
