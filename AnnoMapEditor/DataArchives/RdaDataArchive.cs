using AnnoMapEditor.Utilities;
using Microsoft.Extensions.FileSystemGlobbing;
using RDAExplorer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace AnnoMapEditor.DataArchives
{
    public class RdaDataArchive : ObservableBase, IDataArchive, IDisposable
    {
        private static readonly Logger<RdaDataArchive> _logger = new();


        private Task? _loadingTask;

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


        public string Path { get; }

        private RDAReader[]? _readers;

        private readonly Dictionary<string, RDAFile> _allFiles = new();


        public RdaDataArchive(string folderPath)
        {
            Path = folderPath;

            _loadingTask = LoadAsync();
        }
        

        private Task LoadAsync()
        {
            IsLoaded = false; ;
            return Task.Run(() => 
            {
                Load();
                IsLoaded = true;
            });
        }

        private void Load()
        {
            IsValid = false;
            _logger.LogInformation($"Discovering RDA archives at '{Path}'.");

            // let's skip a few to speed up the loading: 0, 1, 2, 3, 4, 7, 8, 9
            var archives = Directory.
                GetFiles(System.IO.Path.Combine(Path, "maindata"), "*.rda")
                // filter some rda we don't use for sure
                .Where(x => System.IO.Path.GetFileName(x).StartsWith("data") &&
                    !x.EndsWith("data0.rda") && !x.EndsWith("data1.rda") && !x.EndsWith("data2.rda") && !x.EndsWith("data3.rda") &&
                    !x.EndsWith("data4.rda") && !x.EndsWith("data7.rda") && !x.EndsWith("data8.rda") && !x.EndsWith("data9.rda"))
                // load highest numbers last to overwrite lower numbers
                .OrderByDescending(x => int.TryParse(System.IO.Path.GetFileNameWithoutExtension(x)["data".Length..], out int result) ? result : 0);

            _readers = archives.Select(x =>
            {
                try
                {
                    var reader = new RDAReader
                    {
                        FileName = x
                    };
                    reader.ReadRDAFile();
                    foreach (var file in reader.rdaFolder.GetAllFiles())
                        if (file.FileName.EndsWith(".a7tinfo") || file.FileName.EndsWith(".png") || file.FileName.EndsWith(".a7minfo") ||
                            file.FileName.EndsWith(".a7t") || file.FileName.EndsWith(".a7te") || file.FileName.EndsWith("assets.xml") || file.FileName.EndsWith(".a7m"))
                            _allFiles[file.FileName] = file;
                    return reader;
                }
                catch (Exception e)
                {
                    _logger.LogError($"error loading RDAs from {x}", e);
                    IsValid = false;
                    return null;
                }
            }).Where(x => x is not null).Select(x => x!).ToArray();

            IsValid = true;
            _logger.LogInformation($"Loaded {_readers.Length} RDAs.");

            if (_readers.Length == 0)
            {
                _logger.LogWarning($"No .rda files found at {System.IO.Path.Combine(Path, "maindata")}");
                MessageBox.Show($"Something went wrong opening the RDA files.\n\nDo you have another Editor or the RDAExplorer open by any chance?\n\nLog file: {Log.LogFilePath}", App.TitleShort, MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
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
            if (!IsValid || _readers is null)
            {
                _logger.LogWarning($"archive not ready: {filePath}");
                return null;
            }
            Stream? stream = null;

            if (!_allFiles.TryGetValue(filePath.Replace('\\', '/'), out RDAFile? file) || file is null)
            {
                _logger.LogWarning($"not found in archive: {filePath}");
                return null;
            }

            try
            {
                stream = new MemoryStream(file.GetData());
            }
            catch (Exception e)
            {
                _logger.LogError($"error reading archive: {filePath}", e);
            }
            return stream;
        }

        public IEnumerable<string> Find(string pattern)
        {
            if (!IsValid || _readers is null)
                return Array.Empty<string>();

            Matcher matcher = new();
            matcher.AddIncludePatterns(new string[] { pattern });

            PatternMatchingResult result = matcher.Match(_allFiles.Keys);

            return result.Files.Select(x => x.Path);
        }

        public void Dispose()
        {
            if (_readers is not null)
            {
                foreach (var reader in _readers)
                    reader.Dispose();
            }
            GC.SuppressFinalize(this);
        }
    }
}
