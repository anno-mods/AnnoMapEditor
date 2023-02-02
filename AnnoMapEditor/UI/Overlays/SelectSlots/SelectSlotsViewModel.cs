using AnnoMapEditor.MapTemplates.Enums;
using AnnoMapEditor.MapTemplates.Models;
using AnnoMapEditor.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Data;

namespace AnnoMapEditor.UI.Overlays.SelectSlots
{
    public class SelectSlotsViewModel : ObservableBase, IOverlayViewModel
    {
        public Region Region { get; init; }

        public FixedIslandElement FixedIsland { get; init; }

        public bool ShowMines
        {
            get => _showMines;
            set
            {
                SetProperty(ref _showMines, value);
                UpdateFilter();
            }
        }
        private bool _showMines = true;

        public bool ShowClay
        {
            get => _showClay;
            set
            {
                SetProperty(ref _showClay, value);
                UpdateFilter();
            }
        }
        private bool _showClay = true;

        public bool ShowOil
        {
            get => _showOil;
            set
            {
                SetProperty(ref _showOil, value);
                UpdateFilter();
            }
        }
        private bool _showOil = true;


        public SelectSlotsViewModel(Region region, FixedIslandElement fixedIsland)
        {
            Region = region;
            FixedIsland = fixedIsland;

            CollectionView slotsLeftView = (CollectionView)CollectionViewSource.GetDefaultView(FixedIsland.SlotAssignments.Values);
            slotsLeftView.Filter = SlotFilter;
        }


        private bool SlotFilter(object item)
        {
            const int RANDOM_MINE_GROUP_ID = 1000029;
            const int RANDOM_CLAY_GROUP_ID = 100417;
            const int RANDOM_OIL_GROUP_ID  = 100849;

            if (item is not SlotAssignment slotAssignment)
                throw new ArgumentException();
            long slotGroupId = slotAssignment.Slot.GroupId;

            if (!ShowMines && slotGroupId == RANDOM_MINE_GROUP_ID)
                return false;

            if (!ShowClay && slotGroupId == RANDOM_CLAY_GROUP_ID)
                return false;

            if (!ShowOil && slotGroupId == RANDOM_OIL_GROUP_ID)
                return false;

            return true;
        }

        private void UpdateFilter()
        {
            CollectionViewSource.GetDefaultView(FixedIsland.SlotAssignments.Values).Refresh();
        }
    }
}
