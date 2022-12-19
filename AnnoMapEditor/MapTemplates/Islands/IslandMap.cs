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

        public readonly BitmapImage? MapImage;


        public IslandMap(string filePath, int? sizeInSiles)
        {
            FilePath = filePath;
            SizeInTiles = sizeInSiles;
            MapImage = LoadMapImage(filePath);
        }


        public static IEnumerable<IslandMap> FromFilePathTemplate(string filePathTemplate, int count)
            => FromFilePathTemplate(filePathTemplate, Enumerable.Range(0, count).ToArray());

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


        private static BitmapImage? LoadMapImage(string mapFilePath)
        {
            // determine the mapimage's path
            string mapImagePath = Path.Combine(
               Path.GetDirectoryName(mapFilePath)!,
               "_gamedata",
               Path.GetFileNameWithoutExtension(mapFilePath),
               "mapimage.png"
            );

            try {
                using Stream? stream = Settings.Instance.DataArchive.OpenRead(mapImagePath);
               
                if (stream == null)
                    return null;

                BitmapImage png = new();
                png.BeginInit();
                png.StreamSource = stream;
                png.CacheOption = BitmapCacheOption.OnLoad;
                png.EndInit();
                png.Freeze();
                return png;
                    
            }
            catch (Exception ex)
            {
                throw new Exception($"Could not load map image for '{mapFilePath}'.", ex);
            }
        }
    }
}
