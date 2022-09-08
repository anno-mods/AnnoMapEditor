using System;
using System.IO;
using System.Text;

namespace AnnoMapEditor.MapTemplates.Serializing.A7t.FileCreation
{
    [Obsolete("This class is part of a fully custom a7t XML Exporter. It will be replaced with an Implementation based on FileDBSerializer as soon as possible.")]
    internal abstract class XMLCreatorBase
    {
        public abstract XMLItem MakeXML();

        public void WriteXMLToFile(string filePath)
        {
            XMLItem item = MakeXML();

            using (StreamWriter sw = new StreamWriter(filePath, false, Encoding.UTF8))
            {
                item.WriteToStream(sw);
            }
        }

        public void WriteXMLToStream(Stream target)
        {
            XMLItem item = MakeXML();

            using (StreamWriter sw = new StreamWriter(target, Encoding.UTF8, leaveOpen: true))
            {
                item.WriteToStream(sw);
            }
        }
    }
}
