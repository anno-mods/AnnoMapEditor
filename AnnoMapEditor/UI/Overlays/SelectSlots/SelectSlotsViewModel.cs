using AnnoMapEditor.DataArchives.Assets.Models;
using AnnoMapEditor.MapTemplates.Enums;
using AnnoMapEditor.MapTemplates.Models;
using AnnoMapEditor.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;

namespace AnnoMapEditor.UI.Overlays.SelectSlots
{
    public delegate void FilterUpdatedEventHandler();


    public class SlotPositionComparer : IComparer<Vector2>
    {
        private static readonly Vector NORMAL = new(1, 0);

        private readonly double _islandHalfSize;

        private readonly Vector _islandCenter;


        public SlotPositionComparer(int islandSize)
        {
            _islandHalfSize = islandSize / 2d;
            _islandCenter = new(_islandHalfSize, _islandHalfSize);
        }


        public int Compare(Vector2? a, Vector2? b)
        {
            if (a == null || b == null)
                throw new ArgumentNullException();

            Vector aToCenter = new(a.X - _islandCenter.X, a.Y - _islandCenter.Y);
            Vector bToCenter = new(b.X - _islandCenter.X, b.Y - _islandCenter.Y);

            double aAngle = Math.Atan2(aToCenter.Y - NORMAL.Y, aToCenter.X - NORMAL.X);
            double bAngle = Math.Atan2(bToCenter.Y - NORMAL.Y, bToCenter.X - NORMAL.X);

            if (aAngle < bAngle)
                return -1;

            else if (aAngle > bAngle)
                return 1;

            else
                return bToCenter.Length.CompareTo(aToCenter.Length);
        }
    }


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

        public ObservableCollection<SlotAssignmentViewModel> SlotAssignmentViewModels { get; init; }
        public IEnumerable<SlotAssignmentViewModel> SlotAssignmentViewModelsLeft { get; init; }
        public IEnumerable<SlotAssignmentViewModel> SlotAssignmentViewModelsRight { get; init; }


        public SelectSlotsViewModel(Region region, FixedIslandElement fixedIsland)
        {
            Region = region;
            FixedIsland = fixedIsland;

            SlotAssignmentViewModels = new(
                fixedIsland.SlotAssignments.Values
                    .Where(s => s.Slot.SlotAsset != null)
                    .Select(s => new SlotAssignmentViewModel(s))
                    .OrderBy(s => s.SlotAssignment.Slot.Position, new SlotPositionComparer(fixedIsland.IslandAsset.SizeInTiles))
                );

            List<SlotAssignmentViewModel> left = new();
            List<SlotAssignmentViewModel> right = new();
            foreach (SlotAssignmentViewModel slot in SlotAssignmentViewModels)
            {
                Vector2 slotPosition = slot.SlotAssignment.Slot.Position;

                if (slotPosition.Y > -slotPosition.X + fixedIsland.IslandAsset.SizeInTiles)
                    left.Add(slot);
                else
                    right.Add(slot);
            }

            SlotAssignmentViewModelsLeft = left;
            SlotAssignmentViewModelsRight = right;

            CollectionView slotsView = (CollectionView)CollectionViewSource.GetDefaultView(SlotAssignmentViewModels);
            CollectionView slotsLeftView = (CollectionView)CollectionViewSource.GetDefaultView(SlotAssignmentViewModelsLeft);
            CollectionView slotsRightView = (CollectionView)CollectionViewSource.GetDefaultView(SlotAssignmentViewModelsRight);
            slotsView.Filter = SlotFilter;
            slotsLeftView.Filter = SlotFilter;
            slotsRightView.Filter = SlotFilter;
        }


        private bool SlotFilter(object item)
        {
            if (item is not SlotAssignmentViewModel slotAssignment)
                throw new ArgumentException();

            long slotGroupId = slotAssignment.SlotAssignment.Slot.ObjectGuid;

            if (!ShowMines && slotGroupId == SlotAsset.RANDOM_MINE_GUID)
                return false;

            if (!ShowClay && slotGroupId == SlotAsset.RANDOM_CLAY_GUID)
                return false;

            if (!ShowOil && slotGroupId == SlotAsset.RANDOM_OIL_GUID)
                return false;

            return true;
        }

        private void UpdateFilter()
        {
            CollectionViewSource.GetDefaultView(SlotAssignmentViewModels).Refresh();
            CollectionViewSource.GetDefaultView(SlotAssignmentViewModelsLeft).Refresh();
            CollectionViewSource.GetDefaultView(SlotAssignmentViewModelsRight).Refresh();
        }

        public void OnClosed()
        {
            OverlayService.Instance.Close(this);
        }
    }
}
