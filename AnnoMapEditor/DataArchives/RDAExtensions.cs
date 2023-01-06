using RDAExplorer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AnnoMapEditor.DataArchives
{
    public static class RDAExtensions
    {
        public static RDAFile? GetFileByPath(this RDAReader that, string path)
        {
            Queue<string>? parts = new(Path.GetDirectoryName(path)?.Split('\\') ?? Array.Empty<string>());
            if (!parts.Any())
                return null;

            RDAFolder? currentFolder = that.rdaFolder;

            while (parts.Count > 0 && currentFolder is not null)
            {
                var part = parts.Dequeue();
                currentFolder = currentFolder.Folders.FirstOrDefault(x => x.Name == part);
            }

            if (currentFolder is null)
                return null;

            var fileName = Path.GetFileName(path);
            RDAFile? file = currentFolder.Files.FirstOrDefault(x => Path.GetFileName(x.FileName) == fileName);
            return file;
        }
    }
}
