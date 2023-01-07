using AnnoMapEditor.DataArchives.Assets.Models;
using AnnoMapEditor.DataArchives.Assets.Repositories;
using AnnoMapEditor.UI.Controls.MapTemplates;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnnoMapEditor.UI.Windows.SelectIsland
{
    public class SelectIslandViewModel
    {
        public ObservableCollection<IslandAsset> Islands { get; } = new(); 


        public SelectIslandViewModel()
        {
            AssetRepository<RandomIslandAsset> randomIslandRepository = AssetRepository.Get<RandomIslandAsset>();
            randomIslandRepository.CollectionChanged += RandomIslandRepository_CollectionChanged;
            foreach (RandomIslandAsset randomIsland in randomIslandRepository)
                Islands.Add(randomIsland);
        }

        private void RandomIslandRepository_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
                foreach (var item in e.OldItems)
                    if (item is IslandAsset island)
                        App.Current.Dispatcher.Invoke(() =>
                        {
                            Islands.Remove(island);
                        });

            if (e.NewItems != null)
                foreach (var item in e.NewItems)
                    if (item is IslandAsset island)
                        App.Current.Dispatcher.Invoke(() =>
                        {
                            Islands.Add(island);
                        });
        }
    }
}
