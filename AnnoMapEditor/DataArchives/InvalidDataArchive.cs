using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace AnnoMapEditor.DataArchives
{
    public class InvalidDataArchive : IDataArchive
    {
        public bool IsValid { get; } = false;
        public Stream? OpenRead(string filePath) => null;
        public string DataPath { get; }


        public InvalidDataArchive(string path)
        {
            DataPath = path;
        }


        public Task LoadAsync()
        {
            return Task.Run(() => { });
        }

        public IEnumerable<string> Find(string pattern)
        {
            return Array.Empty<string>();
        }

        public BitmapImage? TryLoadPng(string pngPath)
        {
            throw new NotImplementedException();
        }

        public ImageSource? TryLoadIcon(string iconPath, Point? desiredSize = null)
        {
            throw new NotImplementedException();
        }
    }
}
