using AnnoMapEditor.DataArchives.Assets.Models;
using AnnoMapEditor.MapTemplates.Models;
using AnnoMapEditor.Utilities;
using System;
using System.Collections.ObjectModel;
using System.Windows.Data;
using System.Windows.Media;

namespace AnnoMapEditor.UI.Overlays.SelectSlots
{
    public class SlotAssignmentViewModel : ObservableBase
    {
        public static readonly SlotAsset EmptySlot = new()
        {
            DisplayName = "Empty"
        };


        public SlotAssignment SlotAssignment { get; init; }

        public SlotAsset AssignedSlot
        {
            get => SlotAssignment.AssignedSlot ?? EmptySlot;
            set
            {
                SlotAssignment.AssignedSlot = value != EmptySlot ? value : null;
                OnPropertyChanged();

                UpdateBrushes();
            }
        }

        public ObservableCollection<SlotAsset> SlotItems { get; init; }

        public bool HasFocus
        {
            get => _hasFocus;
            private set => SetProperty(ref _hasFocus, value);
        }
        private bool _hasFocus;

        public int PathThickness
        {
            get => _pathThickness;
            private set => SetProperty(ref _pathThickness, value);
        }
        private int _pathThickness = 1;

        public Brush PinBrush
        {
            get => _pinBrush;
            private set => SetProperty(ref _pinBrush, value);
        }
        private Brush _pinBrush;

        public Brush BackgroundBrush
        {
            get => _backgroundBrush;
            private set => SetProperty(ref _backgroundBrush, value);
        }
        private Brush _backgroundBrush;

        public RegionAsset SelectedRegion
        {
            get => _selectedRegion;
            set
            {
                SetProperty(ref _selectedRegion, value);
                UpdateFilter();
            }
        }
        private RegionAsset _selectedRegion;


        public SlotAssignmentViewModel(SlotAssignment slotAssignment, RegionAsset selectedRegion)
        {
            SlotAssignment = slotAssignment;
            _selectedRegion = selectedRegion;

            // copy the list of allowed replacements and add the EmptySlot to the end
            SlotItems = new(slotAssignment.Slot.SlotAsset!.ReplacementSlotAssets);
            SlotItems.Add(EmptySlot);

            CollectionView fertilitiesView = (CollectionView)CollectionViewSource.GetDefaultView(SlotItems);
            fertilitiesView.Filter = SlotAssetFilter;

            UpdateBrushes();
        }


        private bool SlotAssetFilter(object item)
        {
            if (item is not SlotAsset slotAsset)
                throw new ArgumentException();

            // Warning: Hardcoding
            // SouthAmerica uses both random mine slots 614 and 1000029. However, none of the
            // replacements for 1000029 contain the New World as an AssociatedRegion. Despite of
            // this, it is possible to have both 1010502 Iron Deposit and 1010507 Gold Deposit in
            // the New World.
            if (_selectedRegion == RegionAsset.SouthAmerica
                && SlotAssignment.Slot.ObjectGuid == SlotAsset.RANDOM_MINE_OLD_WORLD_GUID
                && (slotAsset.GUID == 1010501 || slotAsset.GUID == 1010507))
                return true;

            if (slotAsset == EmptySlot || slotAsset == AssignedSlot)
                return true;

            if (slotAsset.AssociatedRegions.Contains(_selectedRegion))
                return true;

            return false;
        }

        private void UpdateFilter()
        {
            CollectionViewSource.GetDefaultView(SlotItems).Refresh();
        }

        public void OnGotFocus()
        {
            HasFocus = true;
            PathThickness = 3;
        }

        public void OnLostFocus()
        {
            HasFocus = false;
            PathThickness = 1;
        }

        private void UpdateBrushes()
        {
            if (AssignedSlot == EmptySlot)
            {
                PinBrush = Brushes.LightGray;
                BackgroundBrush = Brushes.Transparent;
            }
            else
            {
                long randomSlotGuid = SlotAssignment.Slot.ObjectGuid;

                if (randomSlotGuid == SlotAsset.RandomMineOldWorld.GUID
                    || randomSlotGuid == SlotAsset.RandomMineNewWorld.GUID
                    || randomSlotGuid == SlotAsset.RandomMineArctic.GUID)
                    PinBrush = Brushes.Gray;
                else if (randomSlotGuid == SlotAsset.RandomClay.GUID)
                    PinBrush = Brushes.SandyBrown;
                else if (randomSlotGuid == SlotAsset.RandomOil.GUID)
                    PinBrush = Brushes.DarkSlateGray;
                else
                    PinBrush = Brushes.Red;

                BackgroundBrush = PinBrush;
            }
        }
    }
}
