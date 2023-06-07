using AnnoMapEditor.DataArchives.Assets.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AnnoMapEditor.UI.Controls.Fertilities
{
    /// <summary>
    /// Sorts the island's fertilities the same way as on the minimap in game.
    /// Feature request courtesy of Taubenangriff.
    /// </summary>
    public class FertilityComparer : IComparer<FertilityAsset>
    {
        public static readonly FertilityComparer Instance = new();


        private static readonly Dictionary<long, int> _orderLookup;
        static FertilityComparer()
        {
            int index = 0;
            _orderLookup = MinimapSceneAsset.Instance.FertilityOrderGuids
                .ToDictionary(f => f, f => index++);
        }


        public int Compare(FertilityAsset? x, FertilityAsset? y)
        {
            if (x == null || y == null)
                throw new ArgumentNullException();

            if (!_orderLookup.TryGetValue(x.GUID, out int xIndex))
                return 1;

            if (!_orderLookup.TryGetValue(y.GUID, out int yIndex))
                return -1;

            return xIndex.CompareTo(yIndex);
        }
    }
}
