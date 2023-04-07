using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace AnnoMapEditor.DataArchives
{
    public interface IDataArchive
    {
        bool IsValid { get; }

        string DataPath { get; }


        Stream? OpenRead(string filePath);

        IEnumerable<string> Find(string pattern);

        BitmapImage? TryLoadPng(string pngPath);

        ImageSource? TryLoadIcon(string iconPath, Point? desiredSize = null);
    }
}
