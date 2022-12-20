using AnnoMapEditor.MapTemplates.Serializing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace AnnoMapEditor.MapTemplates.Islands
{
    public class IslandMap
    {
        public readonly string FilePath;

        public readonly int? SizeInTiles;

        private BitmapImage? _mapImage;
        public BitmapImage? MapImage
        {
            get => _mapImage ?? TryLoadMapImage();
        }


        public IslandMap(string filePath, int? sizeInSiles)
        {
            FilePath = filePath;
            SizeInTiles = sizeInSiles;
            TryLoadMapImage();
        }


        public static IEnumerable<IslandMap> FromFilePathTemplate(string filePathTemplate, int count)
            => FromFilePathTemplate(filePathTemplate, Enumerable.Range(1, count).ToArray());

        public static IEnumerable<IslandMap> FromFilePathTemplate(string filePathTemplate, int start, int count)
            => FromFilePathTemplate(filePathTemplate, Enumerable.Range(start, count).ToArray());

        public static IEnumerable<IslandMap> FromFilePathTemplate(string filePathTemplate, IEnumerable<int> indices)
        {
            return indices.Select(index =>
            {
                string mapFilePath = string.Format(filePathTemplate, string.Format("{0:00}", index));
                int? sizeInTiles = Task.Run(() => IslandReader.ReadTileInSizeFromFileAsync(mapFilePath)).Result;
                return new IslandMap(mapFilePath, sizeInTiles);
            });
        }


        private BitmapImage? TryLoadMapImage()
        {
            // determine the mapimage's path
            string mapImagePath = Path.Combine(
               Path.GetDirectoryName(FilePath)!,
               "_gamedata",
               Path.GetFileNameWithoutExtension(FilePath),
               "mapimage.png"
            );

            try {
                using Stream? stream = Settings.Instance.DataArchive.OpenRead(mapImagePath);

                if (stream != null)
                {
                    BitmapImage png = new();
                    png.BeginInit();
                    png.StreamSource = stream;
                    png.CacheOption = BitmapCacheOption.OnLoad;
                    png.EndInit();
                    png.Freeze();

                    return _mapImage = png;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {
                throw new Exception($"Could not load map image for '{FilePath}'.", ex);
            }
        }
    }
}
