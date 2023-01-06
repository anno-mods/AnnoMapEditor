using AnnoRDA;
using AnnoRDA.Builder;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AnnoMapEditor.DataArchives
{
    public class RdaDataArchive : IDataArchive
    {
        public string Path { get; }
        public bool IsValid { get; private set; }

        private FileSystem _fileSystem;

        public RdaDataArchive(string folderPath)
        {
            Path = folderPath;
            IsValid = false;
        }
        

        public async Task LoadAsync()
        {
            await Task.Run(() => 
            {
                _fileSystem = FileSystemBuilder.Create()
                .FromPath(System.IO.Path.Combine(Path, "maindata"))
                .WithDefaultSorting()
                .AddWhitelisted("*.a7tinfo", "*.png", "*.a7minfo", "*.a7t", "*.a7te", "assets.xml")
                .Build();
            });
            IsValid = true;
        }

        public Stream? OpenRead(string filePath)
        {
            if (!IsValid)
                throw new InvalidOperationException("The data archive must be loaded before opening files!");
            filePath = filePath.Replace("\\", "/");
            return _fileSystem.OpenRead(filePath);
        }

        public IEnumerable<string> Find(string pattern)
        {
            return _fileSystem.Root.MatchFiles(pattern).Select(x => x.Name);
        }
    }
}
