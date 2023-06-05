using AnnoMapEditor.DataArchives.Assets.Models;
using AnnoMapEditor.MapTemplates.Models;
using AnnoMapEditor.UI.Overlays;
using AnnoMapEditor.UI.Overlays.SelectFertilities;
using AnnoMapEditor.Utilities;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace AnnoMapEditor.UI.Controls.Fertilities
{
    public class FertilitiesViewModel
    {
        public FixedIslandElement FixedIsland { get; init; }

        public RegionAsset Region { get; init; }

        public ObservableCollection<FertilityAsset> Fertilities { get; init; } = new();


        public FertilitiesViewModel(FixedIslandElement fixedIsland, RegionAsset region)
        {
            FixedIsland = fixedIsland;
            Region = region;

            if(fixedIsland.RandomizeFertilities == false)
                SortFertilities();

            fixedIsland.Fertilities.CollectionChanged += SortFertilities;
        }

        private void SortFertilities(object? _ = null, NotifyCollectionChangedEventArgs? __ = null)
        {
            Fertilities.Clear();
            foreach (FertilityAsset fertility in FixedIsland.Fertilities.OrderBy(FertilityComparer.Instance))
                Fertilities.Add(fertility);
        }

        public void OnConfigure()
        {
            SelectFertilitiesViewModel selectViewModel = new(Region, FixedIsland);
            OverlayService.Instance.Show(selectViewModel);
        }
    }
}
