using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace AnnoMapEditor.DataArchives
{
    public class InvalidDataArchive : IDataArchive
    {
        public bool IsValid { get; } = false;
        public Stream? OpenRead(string filePath) => null;
        public string Path { get; }


        public InvalidDataArchive(string path)
        {
            Path = path;
        }


        public Task LoadAsync()
        {
            return Task.Run(() => { });
        }

        public IEnumerable<string> Find(string pattern)
        {
            return Array.Empty<string>();
        }
    }
}
