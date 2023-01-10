using AnnoMapEditor.Utilities;
using System.Windows.Media.Imaging;

namespace AnnoMapEditor.DataArchives.Assets.Models
{
    public class FixedIslandAsset : ObservableBase
    {
        public string FilePath { get; init; }

        public int SizeInTiles { get; init; }

        public BitmapImage Thumbnail { get; init; }
    }
}
