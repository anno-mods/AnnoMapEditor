using AnnoMapEditor.DataArchives.Assets.Models;
using AnnoMapEditor.MapTemplates.Models;
using AnnoMapEditor.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Data;

namespace AnnoMapEditor.UI.Overlays.SelectSlots
{
    public delegate void FilterUpdatedEventHandler();

    public class SlotPositionXComparer : IComparer<Vector2>
    {
        /// <summary>
        /// compares the position of slots on the horizontal axis (note: converter is inverted) 
        /// </summary>
        public int Compare(Vector2? a, Vector2? b)
        {
            if (a == null || b == null)
                throw new ArgumentNullException();

            return b.X.CompareTo(a.X);
        }
    }

    public class SlotPositionYComparer : IComparer<Vector2>
    {
        /// <summary>
        /// compares the position of slots on y
        /// </summary>
        public int Compare(Vector2? a, Vector2? b)
        {
            if (a == null || b == null)
                throw new ArgumentNullException();

            return a.Y.CompareTo(b.Y);
        }
    }


    public class SlotPositionVerticalAxisComparer : IComparer<Vector2>
    {
        /// <summary>
        /// compares the position of slots on the vertical view axis 
        /// </summary>
        public int Compare(Vector2? a, Vector2? b)
        {
            if (a == null || b == null)
                throw new ArgumentNullException();

            return LinearCombinationOf(a).CompareTo(LinearCombinationOf(b));
        }

        private float LinearCombinationOf(Vector2 a) => (a.Y - a.X) * 0.5f;
    }

    public class SlotPositionHorizontalAxisComparer : IComparer<Vector2>
    {
        /// <summary>
        /// compares the position of slots on the horizontal view axis 
        /// </summary>
        public int Compare(Vector2? a, Vector2? b)
        {
            if (a == null || b == null)
                throw new ArgumentNullException();

            return LinearCombinationOf(a).CompareTo(LinearCombinationOf(b));
        }

        private float LinearCombinationOf(Vector2 a) => (a.X + a.Y) * 0.5f;
    }

    public class SelectSlotsViewModel : ObservableBase, IOverlayViewModel
    {
        public event EventHandler<FilteredItemsChangedEventArgs<SlotAssignmentViewModel>>? FilterModified;
        public IEnumerable<RegionAsset?> Regions { get; init; } = RegionAsset.SupportedRegions;

        private readonly RegionAsset _initialRegion;

        public RegionAsset SelectedRegion
        {
            get => _selectedRegion;
            set
            {
                _selectedRegion = value;
                UpdateFilter();

                ShowRegionWarning = _selectedRegion != _initialRegion;
            }
        }
        private RegionAsset _selectedRegion;

        public bool ShowRegionWarning
        {
            get => _showRegionWarning;
            set => SetProperty(ref _showRegionWarning, value);
        }
        private bool _showRegionWarning = false;

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

        public ObservableCollection<SlotAssignmentViewModel> SlotAssignmentViewModels { get; init; }
        public IEnumerable<SlotAssignmentViewModel> SlotAssignmentViewModelsLeft { get; private set; }
        public IEnumerable<SlotAssignmentViewModel> SlotAssignmentViewModelsRight { get; private set; }


        public SelectSlotsViewModel(RegionAsset region, FixedIslandElement fixedIsland)
        {
            _selectedRegion = _initialRegion = region;
            FixedIsland = fixedIsland;

            SlotAssignmentViewModels = new(
                fixedIsland.SlotAssignments.Values
                    .Where(s => s.Slot.SlotAsset != null)
                    .Select(s => new SlotAssignmentViewModel(s, region))
                    .OrderByDescending(s => s.SlotAssignment.Slot.Position, new SlotPositionHorizontalAxisComparer())
                );

            ArrangeSlots();

            CollectionView slotsView = (CollectionView)CollectionViewSource.GetDefaultView(SlotAssignmentViewModels);
            CollectionView slotsLeftView = (CollectionView)CollectionViewSource.GetDefaultView(SlotAssignmentViewModelsLeft);
            CollectionView slotsRightView = (CollectionView)CollectionViewSource.GetDefaultView(SlotAssignmentViewModelsRight);
            slotsView.Filter = SlotFilter;
            slotsLeftView.Filter = SlotFilter;
            slotsRightView.Filter = SlotFilter;
        }

        public void ArrangeSlots()
        {
            var (left, right) = SlotAssignmentViewModels.SliceHalf();

            left = left.OrderBy(s => s.SlotAssignment.Slot.Position, new SlotPositionVerticalAxisComparer()).ToList();
            right = right.OrderBy(s => s.SlotAssignment.Slot.Position, new SlotPositionVerticalAxisComparer()).ToList();

            //quarters 
            SlotAssignmentViewModelsLeft = SliceSortRecombine(left,
                new SlotPositionYComparer(),
                new SlotPositionXComparer());
            SlotAssignmentViewModelsRight = SliceSortRecombine(right,
                new SlotPositionXComparer(),
                new SlotPositionYComparer());
        }

        private IEnumerable<SlotAssignmentViewModel> SliceSortRecombine(
            IEnumerable<SlotAssignmentViewModel> slots,
            IComparer<Vector2> upperComparer, 
            IComparer<Vector2> lowerComparer)
        {
            var (upper, lower) = slots.SliceHalf();
            upper = upper.OrderBy(s => s.SlotAssignment.Slot.Position, upperComparer);
            lower = lower.OrderBy(s => s.SlotAssignment.Slot.Position, lowerComparer);
            var result = upper.ToList();
            result.AddRange(lower);
            return result;
        }


        private bool SlotFilter(object item)
        {
            if (item is not SlotAssignmentViewModel slotAssignment)
                throw new ArgumentException();

            long slotGroupId = slotAssignment.SlotAssignment.Slot.ObjectGuid;

            if (!ShowMines && (
                  slotGroupId == SlotAsset.RANDOM_MINE_OLD_WORLD_GUID
               || slotGroupId == SlotAsset.RANDOM_MINE_NEW_WORLD_GUID
               || slotGroupId == SlotAsset.RANDOM_MINE_ARCTIC_GUID))
                return false;

            if (!ShowClay && slotGroupId == SlotAsset.RANDOM_CLAY_GUID)
                return false;

            if (!ShowOil && slotGroupId == SlotAsset.RANDOM_OIL_GUID)
                return false;

            return true;
        }

        private void UpdateFilter()
        {
            HashSet<SlotAssignmentViewModel> before = new(CollectionViewSource.GetDefaultView(SlotAssignmentViewModels).Cast<SlotAssignmentViewModel>());

            CollectionViewSource.GetDefaultView(SlotAssignmentViewModelsLeft).Refresh();
            CollectionViewSource.GetDefaultView(SlotAssignmentViewModelsRight).Refresh();
            CollectionViewSource.GetDefaultView(SlotAssignmentViewModels).Refresh();

            HashSet<SlotAssignmentViewModel> after = new(CollectionViewSource.GetDefaultView(SlotAssignmentViewModels).Cast<SlotAssignmentViewModel>());

            IEnumerable<SlotAssignmentViewModel> removedItems = before.Except(after);
            IEnumerable<SlotAssignmentViewModel> addedItems = after.Except(before);
            IEnumerable<SlotAssignmentViewModel> unchangedItems = before.Intersect(after);

            FilterModified?.Invoke(this, new FilteredItemsChangedEventArgs<SlotAssignmentViewModel>(removedItems, addedItems, unchangedItems));

            foreach (SlotAssignmentViewModel slotAssignment in SlotAssignmentViewModels)
                slotAssignment.SelectedRegion = _selectedRegion;
        }

        public void OnClosed()
        {
            OverlayService.Instance.Close(this);
        }

        public class FilteredItemsChangedEventArgs<T> : EventArgs
        {
            public FilteredItemsChangedEventArgs(IEnumerable<T> removedItems, IEnumerable<T> addedItems, IEnumerable<T> unchangedItems)
            {
                RemovedItems = removedItems;
                AddedItems = addedItems;
                UnchangedItems = unchangedItems;
            }

            public IEnumerable<T> RemovedItems { get; init; }
            public IEnumerable<T> AddedItems { get; init; }
            public IEnumerable<T> UnchangedItems { get; init; }
        }
    }
}
