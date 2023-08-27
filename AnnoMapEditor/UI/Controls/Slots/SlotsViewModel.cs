using AnnoMapEditor.DataArchives.Assets.Models;
using AnnoMapEditor.MapTemplates.Models;
using AnnoMapEditor.UI.Overlays;
using AnnoMapEditor.UI.Overlays.SelectSlots;
using AnnoMapEditor.Utilities;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace AnnoMapEditor.UI.Controls.Slots
{
    public class SlotsViewModel
    {
        public FixedIslandElement FixedIsland { get; init; }

        public RegionAsset Region { get; init; }

        public ObservableCollection<SlotCounter> SlotCounters { get; init; } = new();


        public SlotsViewModel(FixedIslandElement fixedIsland, RegionAsset region)
        {
            FixedIsland = fixedIsland;
            Region = region;

            RefreshMineSlotCount();
        }

        private void RefreshMineSlotCount()
        {
            List<SlotCounter> slotCounters = new(SlotCounters);
            foreach (SlotAssignment slot in FixedIsland.SlotAssignments.Values)
            {
                // do not count empty slots
                if (slot.AssignedSlot == null)
                    continue;

                SlotCounter? counter = slotCounters.FirstOrDefault(c => c.Slot == slot.AssignedSlot);
                if (counter != null)
                    counter.Count++;

                else
                    slotCounters.Add(new(slot.AssignedSlot!)
                    {
                        Count = 1
                    });
            }

            SlotCounters.Clear();
            foreach (SlotCounter counter in slotCounters.OrderBy(c => c.Slot, SlotComparer.Instance))
                SlotCounters.Add(counter);
        }

        public void OnConfigure()
        {
            SelectSlotsViewModel selectViewModel = new(Region, FixedIsland);

            // register a one shot event listener
            OverlayClosedEventHandler? closureHandler = null;
            closureHandler = (sender, e) =>
            {
                if (e.OverlayViewModel == selectViewModel)
                {
                    RefreshMineSlotCount();
                    OverlayService.Instance.OverlayClosed -= closureHandler;
                }
            };
            OverlayService.Instance.OverlayClosed += closureHandler;

            OverlayService.Instance.Show(selectViewModel);
        }


        public class SlotCounter : ObservableBase
        {
            public int Count { get; set; }

            public SlotAsset Slot { get; set; }


            public SlotCounter(SlotAsset slot)
            {
                Slot = slot;
            }
        }
    }
}
