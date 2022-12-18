using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnnoMapEditor.MapTemplates.Islands
{
    public class IslandMap
    {
        public string FilePath { get; }

        public int SizeInTiles { get; }


        public IslandMap(string filePath, int sizeInTiles)
        {
            FilePath = filePath;
            SizeInTiles = sizeInTiles;
        }
    }
}
