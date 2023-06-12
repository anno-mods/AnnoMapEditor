using AnnoMapEditor.Utilities;
using AnnoRDA;
using AnnoRDA.Builder;
using FileDBReader.src.XmlRepresentation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace AnnoMapEditor.DataArchives
{
    public class RdaDataArchive : DataArchive, IDataArchive
    {
        private static readonly Logger<RdaDataArchive> _logger = new();

        public override string DataPath { get; }

        public bool IsLoaded
        {
            get => _isLoading;
            private set => SetProperty(ref _isLoading, value);
        }
        private bool _isLoading = false;

        public override bool IsValid
        {
            get => _isValid;
            protected set => SetProperty(ref _isValid, value);
        }
        private bool _isValid = false;

        private FileSystem _fileSystem;


        public RdaDataArchive(string folderPath)
        {
            DataPath = folderPath;
            IsValid = false;
        }



        public async Task LoadAsync()
        {
            IsLoaded = false;
            IsValid = false;
            _logger.LogInformation($"Discovering RDA archives at '{DataPath}'.");
            var builder = FileSystemBuilder.Create()
                .FromPath(DataPath)
                .OnlyArchivesMatchingWildcard("data*.rda")
                .WithDefaultSorting()
                .ConfigureLoadZeroByteFiles(false)
                .AddWhitelisted("*.a7tinfo", "*.png", "*.a7minfo", "*.a7t", "*.a7te", "assets.xml", "*.a7m", "*.dds");

            await Task.Run(() => 
            {
                try
                {
                    _fileSystem = builder.Build();
                }
                catch (Exception e){
                    _logger.LogError($"error loading RDAs from {DataPath}", e);
                    IsValid = false;
                    return;
                }
            });

            var loadedCount = builder.ArchiveFileNames.Count();
            if (loadedCount == 0)
            {
                _logger.LogWarning($"No .rda files found at {DataPath}");
                MessageBox.Show($"Something went wrong opening the RDA files.\n\nDo you have another Editor or the RDAExplorer open by any chance?\n\nLog file: {Log.LogFilePath}", App.TitleShort, MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
            _logger.LogInformation($"Loaded {loadedCount} RDAs.");
            IsLoaded = true;
            IsValid = true;
        }

        public override Stream? OpenRead(string filePath)
        {
            if (!IsValid)
            {
                _logger.LogWarning($"archive not ready: {filePath}");
                return null;
            }
                
            filePath = filePath.Replace("\\", "/");
            try
            {
                return _fileSystem.OpenRead(filePath);
            }
            catch (FileNotFoundException e)
            {
                _logger.LogWarning($"not found in archive: {filePath}");
            }
            catch (Exception e)
            {
                _logger.LogError($"error reading archive: {filePath}", e);
            }
            return null;
        }

        public override IEnumerable<string> Find(string pattern)
        {
            return _fileSystem.FindFiles(pattern);
        }
    }
}
