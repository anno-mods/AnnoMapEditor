using AnnoMapEditor.Utilities;
using Microsoft.Extensions.FileSystemGlobbing;
using RDAExplorer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using AnnoMapEditor.Utilities;
using AnnoRDA;
using AnnoRDA.Builder;
using Microsoft.Extensions.FileSystemGlobbing;
using RDAExplorer;

namespace AnnoMapEditor.DataArchives
{
    public class RdaDataArchive : IDataArchive
    {
        public string Path { get; }

        public bool IsValid { get; } = true;

        private FileSystem fileSystem;

        public RdaDataArchive(string folderPath)
        {
            Path = folderPath;
        }
        

        public async Task LoadAsync()
        {
            await Task.Run(() => 
            {
                fileSystem = FileSystemBuilder.Create()
                .FromPath(System.IO.Path.Combine(Path, "maindata"))
                .WithDefaultSorting()
                .AddWhitelisted("*.a7tinfo", "*.png", "*.a7minfo", "*.a7t", "*.a7te", "assets.xml")
                .Build();
            });
        }

        public Stream? OpenRead(string filePath)
        {
            filePath = filePath.Replace("\\", "/");
            return fileSystem.OpenRead(filePath);
        }

        public IEnumerable<string> Find(string pattern)
        {
            return fileSystem.Root.MatchFiles(pattern).Select(x => x.Name);
        }
    }
}
