using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace AnnoMapEditor.DataArchives
{
    public interface IDataArchive
    {
        Stream? OpenRead(string filePath);

        IEnumerable<string> Find(string pattern);

        BitmapImage? TryLoadPng(string pngPath);

        ImageSource? TryLoadIcon(string iconPath, Point? desiredSize = null);
    }
}
