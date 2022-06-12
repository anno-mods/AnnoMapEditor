using System.IO;

namespace AnnoMapEditor.Utils
{
    public class FolderDataArchive : IDataArchive
    {
        public string Path { get; }
        public bool IsValid { get; } = true;

        public FolderDataArchive(string folderPath)
        {
            Path = folderPath;
        }

        public Stream? OpenRead(string filePath)
        {
            if (!IsValid)
                return null;
            Stream? stream = null;

            try
            {
                stream = File.OpenRead(System.IO.Path.Combine(Path, filePath));
            }
            catch { }
            return stream;
        }
    }
}
