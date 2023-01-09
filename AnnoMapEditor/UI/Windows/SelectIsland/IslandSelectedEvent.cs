using AnnoMapEditor.DataArchives.Assets.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnnoMapEditor.UI.Windows.SelectIsland
{
    public delegate void IslandSelectedEventHandler(object? sender, IslandSelectedEventArgs e);


    public class IslandSelectedEventArgs
    {
        public IslandAsset IslandAsset { get; init; }


        public IslandSelectedEventArgs(IslandAsset islandAsset)
        {
            IslandAsset = islandAsset;
        }
    }
}
