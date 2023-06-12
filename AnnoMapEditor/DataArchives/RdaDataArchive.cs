using AnnoMapEditor.Utilities;
using AnnoRDA;
using System;
using System.Collections.Generic;
using System.IO;

namespace AnnoMapEditor.DataArchives
{
    public class RdaDataArchive : DataArchive
    {
        private static readonly Logger<RdaDataArchive> _logger = new();


        private readonly FileSystem _fileSystem;


        public RdaDataArchive(FileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }


        public override Stream? OpenRead(string filePath)
        {
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
