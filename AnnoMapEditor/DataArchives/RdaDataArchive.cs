using AnnoMapEditor.Utilities;
using AnnoRDA;
using AnnoRDA.Builder;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AnnoMapEditor.DataArchives
{
    public class RdaDataArchive : ObservableBase, IDataArchive
    {
        private static readonly Logger<RdaDataArchive> _logger = new();
        public string Path { get; }

        public bool IsLoaded
        {
            get => _isLoading;
            private set => SetProperty(ref _isLoading, value);
        }
        private bool _isLoading = false;

        public bool IsValid
        {
            get => _isValid;
            private set => SetProperty(ref _isValid, value);
        }
        private bool _isValid = false;

        private FileSystem _fileSystem;

        private Task? _loadingTask;

        public RdaDataArchive(string folderPath)
        {
            Path = folderPath;
            IsValid = false;
        }



        public async Task LoadAsync()
        {
            IsLoaded = false;
            IsValid = false;
            await Task.Run(() => 
            {
                _fileSystem = FileSystemBuilder.Create()
                .FromPath(System.IO.Path.Combine(Path, "maindata"))
                .WithDefaultSorting()
                .AddWhitelisted("*.a7tinfo", "*.png", "*.a7minfo", "*.a7t", "*.a7te", "assets.xml")
                .Build();
            });
            IsLoaded = true;
            IsValid = true;
        }

        public async Task AwaitLoadingAsync()
        {
            if (_loadingTask != null)
                await _loadingTask;
            else
                throw new Exception($"LoadAsync has not been called.");
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
            return _fileSystem.FindFiles(pattern);
        }
    }
}
