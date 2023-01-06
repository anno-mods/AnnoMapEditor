using AnnoMapEditor.MapTemplates;
using AnnoMapEditor.MapTemplates.Enums;
using AnnoMapEditor.MapTemplates.Models;
using AnnoMapEditor.Utilities;
using System.Collections.Specialized;
using System.ComponentModel;

namespace AnnoMapEditor.UI.Windows.Main
{
    public class SessionChecker : ObservableBase
    {
        public string Status
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }
        private string _status = "";

        private readonly Session _session;

        public SessionChecker(Session session)
        {
            _session = session;
            _session.Elements.CollectionChanged += Session_OnIslandCollectionChanged;

            foreach (var island in session.Elements)
                island.PropertyChanged += Island_PropertyChanged;

            Check();
        }

        private void Session_OnIslandCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            Check();
        }

        private void Island_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            Check();
        }

        public void Check()
        {
            Status = "";
            CheckPoolCounts(_session);
        }

        private void CheckPoolCounts(Session session)
        {
            int[] pools = new [] { 0, 0, 0 };

            int thirdPartyCount = 0;
            int pirateCount = 0;

            foreach (var island in session.Elements)
            {
// TODO           if (island.Type == IslandType.ThirdParty)
//                    thirdPartyCount++;
//                else if (island.Type == IslandType.PirateIsland)
//                    pirateCount++;
//                if (!island.IsPool || island.IsStarter)
//                    continue;
//
//                pools[island.Size.ElementValue ?? 0]++;
            }

            
            if ((session.Region == Region.Moderate || session.Region == Region.NewWorld) && thirdPartyCount > 0)
            {
                // remove Archi / Nate / Isabel from pool counter
                pools[0]--;
            }
            if (pirateCount > 1)
            {
                // remove all but one pirate island from pool counter
                pools[0] -= pirateCount - 1;
            }

            int maxSmallPoolSize  = Pool.GetPool(session.Region, IslandSize.Small).Size;
            int maxMediumPoolSize = Pool.GetPool(session.Region, IslandSize.Medium).Size;
            int maxLargePoolSize  = Pool.GetPool(session.Region, IslandSize.Large).Size;

            if (pools[0] > maxSmallPoolSize)
            {
                Status = $"⚠ Too many small pool islands.\nOnly the first {maxSmallPoolSize} islands will be loaded.\nThird party and pirate islands\nare considered small pool islands if deactivated.";
            }
            else if (pools[1] > maxMediumPoolSize)
            {
                Status = $"⚠ Too many medium pool islands.\nOnly the first {maxMediumPoolSize} islands will be loaded.";
            }
            else if (pools[2] > maxLargePoolSize)
            {
                Status = $"⚠ Too many large pool islands.\nOnly the first {maxLargePoolSize} islands will be loaded.";
            }
            else if (pools[0] < 0)
            {
                // actually, don't warn
                // Status = "⚠ Archi / Nate need a 3rd party island.";
            }
        }
    }
}
