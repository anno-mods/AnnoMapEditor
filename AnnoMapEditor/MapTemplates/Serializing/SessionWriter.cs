using AnnoMapEditor.MapTemplates.Models;
using System.IO;
using System.Threading.Tasks;

namespace AnnoMapEditor.MapTemplates.Serializing
{
    public class SessionWriter
    {
        public async Task ToXmlAsync(Session session, string filePath)
        {
            var export = session.ToTemplate();
            if (export is null)
                return;

            using Stream file = File.OpenWrite(filePath);
            file.SetLength(0); // clear
            await FileDBSerializer.WriteToXmlAsync(export, file);
        }

        public async Task ToA7tinfoAsync(Session session, string filePath)
        {
            var export = session.ToTemplate();
            if (export is null)
                return;

            var parentPath = Path.GetDirectoryName(filePath);
            if (parentPath is not null)
                Directory.CreateDirectory(parentPath);
            using Stream file = File.OpenWrite(filePath);
            file.SetLength(0); // clear
            await FileDBSerializer.WriteAsync(export, file);
        }

    }
}
