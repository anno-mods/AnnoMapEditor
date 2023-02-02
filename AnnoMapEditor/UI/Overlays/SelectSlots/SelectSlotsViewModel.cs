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

        public IEnumerable<SlotAssignmentViewModel> SlotAssignmentViewModels { get; init; }


        public SelectSlotsViewModel(Region region, FixedIslandElement fixedIsland)
        {
            Region = region;
            FixedIsland = fixedIsland;

            SlotAssignmentViewModels = fixedIsland.SlotAssignments.Values
                .Where(s => s.Slot.SlotAsset != null)
                .Select(s => new SlotAssignmentViewModel(s))
                .ToList();

            CollectionView slotsLeftView = (CollectionView)CollectionViewSource.GetDefaultView(SlotAssignmentViewModels);
            slotsLeftView.Filter = SlotFilter;
        }


        private bool SlotFilter(object item)
        {
            const int RANDOM_MINE_OBJECT_GUID = 1000029;
            const int RANDOM_CLAY_OBJECT_GUID = 100417;
            const int RANDOM_OIL_OBJECT_GUID  = 100849;

            if (item is not SlotAssignmentViewModel slotAssignmentViewModel)
                throw new ArgumentException();
            long slotGroupId = slotAssignmentViewModel.SlotAssignment.Slot.ObjectGuid;

            if (!ShowMines && slotGroupId == RANDOM_MINE_OBJECT_GUID)
                return false;

            if (!ShowClay && slotGroupId == RANDOM_CLAY_OBJECT_GUID)
                return false;

            if (!ShowOil && slotGroupId == RANDOM_OIL_OBJECT_GUID)
                return false;

            return true;
        }

        private void UpdateFilter()
        {
            CollectionViewSource.GetDefaultView(SlotAssignmentViewModels).Refresh();
        }
    }
}
