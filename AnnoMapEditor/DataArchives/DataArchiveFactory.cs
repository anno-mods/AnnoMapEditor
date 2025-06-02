using AnnoMapEditor.Utilities;
using AnnoRDA;
using AnnoRDA.Builder;
using System;
using System.Threading.Tasks;

namespace AnnoMapEditor.DataArchives
{
    public class DataArchiveFactory : IDataArchiveFactory
    {
        private static readonly Logger<DataArchiveFactory> _logger = new();

        private static readonly string[] EXTENSION_WHITELIST = new[] { "*.a7tinfo", "*.png", "*.a7minfo", "*.a7t", "*.a7te", "assets.xml", "*.a7m", "*.dds" };


        public async Task<IDataArchive> CreateDataArchiveAsync(string dataPath)
        {
            return await Task.Run(() => CreateDataArchive(dataPath));
         }

        public IDataArchive CreateDataArchive(string dataPath)
        {
            // RdaDataArchive
            try
            {
                _logger.LogInformation($"Trying to load '{dataPath}' as a RdaDataArchive.");
                return CreateRdaDataArchive(dataPath);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex.Message);
            }

            // FolderDataArchive
            try
            {
                _logger.LogInformation($"Trying to load '{dataPath}' as a FolderDataArchive.");
                return CreateFolderDataArchive(dataPath);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex.Message);
            }

            throw new Exception($"Could not find valid Anno 1800 data at '{dataPath}'.");
        }

        public IDataArchive CreateRdaDataArchive(string dataPath)
        {
            _logger.LogInformation($"Discovering RDA archives at '{dataPath}'.");

            FileSystemBuilder builder = FileSystemBuilder.Create()
                .FromPath(dataPath)
                .OnlyArchivesMatchingWildcard("data*.rda")
                .WithDefaultSorting()
                .ConfigureLoadZeroByteFiles(false)
                .AddWhitelisted(EXTENSION_WHITELIST);

            FileSystem fileSystem;
            try
            {
                fileSystem = builder.Build();
            }
            catch (Exception e)
            {
                throw new Exception($"Could not build RdaFileSystem at '{dataPath}'.", e);
            }

            var loadedCount = builder.ArchiveFileNames.Count;
            if (loadedCount == 0)
                throw new Exception($"Could not find any .rda files at '{dataPath}'.");

            _logger.LogInformation($"Loaded {loadedCount} RDAs.");
            return new RdaDataArchive(fileSystem);
        }

        public IDataArchive CreateFolderDataArchive(string dataPath)
        {
            return new FolderDataArchive(dataPath);
        }
    }
}
