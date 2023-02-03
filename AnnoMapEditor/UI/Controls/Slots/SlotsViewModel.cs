using AnnoMapEditor.DataArchives.Assets.Models;
using AnnoMapEditor.MapTemplates.Enums;
using AnnoMapEditor.MapTemplates.Models;
using AnnoMapEditor.UI.Overlays;
using AnnoMapEditor.UI.Overlays.SelectSlots;
using AnnoMapEditor.Utilities;
using System.Collections.ObjectModel;
using System.Linq;

namespace AnnoMapEditor.UI.Controls.Slots
{
    public class SlotsViewModel
    {
        public FixedIslandElement FixedIsland { get; init; }

        public Region Region { get; init; }

        public ObservableCollection<MineSlotCounter> SlotCounters { get; init; } = new();


        public SlotsViewModel(FixedIslandElement fixedIsland, Region region)
        {
            FixedIsland = fixedIsland;
            Region = region;

            RefreshMineSlotCount();
        }

        private void RefreshMineSlotCount()
        {
            SlotCounters.Clear();

            foreach (SlotAssignment mineSlot in FixedIsland.SlotAssignments.Values)
            {
                // do not count empty slots
                if (mineSlot.AssignedSlot == null)
                    continue;

                MineSlotCounter? counter = SlotCounters.FirstOrDefault(c => c.Slot == mineSlot.AssignedSlot);
                if (counter != null)
                    counter.Count++;

                else
                    SlotCounters.Add(new(mineSlot.AssignedSlot)
                    {
                        Count = 1
                    });
            }
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


        public class MineSlotCounter : ObservableBase
        {
            public int Count { get; set; }

            public SlotAsset Slot { get; set; }


            public MineSlotCounter(SlotAsset slot)
            {
                Slot = slot;
            }
        }
    }
}
