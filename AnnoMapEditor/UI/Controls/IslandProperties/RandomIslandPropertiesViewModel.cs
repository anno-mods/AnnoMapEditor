using AnnoMapEditor.MapTemplates.Enums;
using AnnoMapEditor.MapTemplates.Models;
using AnnoMapEditor.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace AnnoMapEditor.UI.Controls.IslandProperties
{
    public class RandomIslandPropertiesViewModel : ObservableBase
    {
        private static readonly Dictionary<IslandType, List<IslandSize>> _allowedSizesPerType = new()
        {
            [IslandType.Normal]       = new() { IslandSize.Large, IslandSize.Medium, IslandSize.Small },
            [IslandType.Starter]      = new() { IslandSize.Large, IslandSize.Medium },
            [IslandType.ThirdParty]   = new() { IslandSize.Small },
            [IslandType.PirateIsland] = new() { IslandSize.Small }
        };

        
        public RandomIslandElement RandomIsland { get; init; }
        
        public ObservableCollection<IslandType> IslandTypeItems { get; } = new();

        public ObservableCollection<IslandSize> IslandSizeItems { get; } = new();


        public RandomIslandPropertiesViewModel(RandomIslandElement randomIsland)
        {
            RandomIsland = randomIsland;

            IslandTypeItems.AddRange(_allowedSizesPerType.Keys);
            IslandSizeItems.AddRange(_allowedSizesPerType[randomIsland.IslandType]);

            randomIsland.PropertyChanged += RandomIsland_PropertyChanged;
        }

        
        private void RandomIsland_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            // only allow valid type/size combinations
            if (e.PropertyName == nameof(RandomIsland.IslandType))
            {
                // add the new list
                IEnumerable<IslandSize> allowedSizes = _allowedSizesPerType[RandomIsland.IslandType];
                foreach (IslandSize allowedSize in allowedSizes)
                    if (!IslandSizeItems.Contains(allowedSize))
                        IslandSizeItems.Add(allowedSize);

                if (!allowedSizes.Contains(RandomIsland.IslandSize))
                    RandomIsland.IslandSize = allowedSizes.First();

                // remove obsolete items
                for (int i = 0; i < IslandSizeItems.Count; ++i)
                    if (!allowedSizes.Contains(IslandSizeItems[i]))
                    {
                        IslandSizeItems.RemoveAt(i);
                        --i;
                    }
            }
        }
    }
}
