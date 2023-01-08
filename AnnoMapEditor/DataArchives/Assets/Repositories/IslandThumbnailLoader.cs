using AnnoMapEditor.DataArchives.Assets.Models;
using AnnoMapEditor.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace AnnoMapEditor.DataArchives.Assets.Repositories
{
    public class IslandThumbnailLoader
    {
        public IslandThumbnailLoader()
        {

        }


        public void LoadThumbnail(IslandAsset islandAsset)
        {
            string thumbnailPath = Path.Combine(
                Path.GetDirectoryName(islandAsset.FilePath)!,
                "_gamedata",
                Path.GetFileNameWithoutExtension(islandAsset.FilePath),
                "mapimage.png");
            using Stream? stream = Settings.Instance.DataArchive?.OpenRead(thumbnailPath)!;

            BitmapImage thumbnail = new();
            thumbnail.BeginInit();
            thumbnail.StreamSource = stream;
            thumbnail.CacheOption = BitmapCacheOption.OnLoad;
            thumbnail.EndInit();
            thumbnail.Freeze();

            islandAsset.Thumbnail = thumbnail;
        }
    }
}
