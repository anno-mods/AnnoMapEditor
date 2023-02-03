using AnnoMapEditor.DataArchives.Assets.Models;
using AnnoMapEditor.MapTemplates.Models;
using AnnoMapEditor.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
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

        public IEnumerable<SlotAsset> SlotItems { get; init; }

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


        public SlotAssignmentViewModel(SlotAssignment slotAssignment)
        {
            SlotAssignment = slotAssignment;

            // copy the list of allowed replacements and add the EmptySlot to the end
            List<SlotAsset> slotItems = new(slotAssignment.Slot.SlotAsset!.ReplacementSlotAssets);
            slotItems.Add(EmptySlot);
            SlotItems = slotItems;

            UpdateBrushes();
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
                if (SlotAssignment.Slot.ObjectGuid == SlotAsset.RANDOM_MINE_GUID)
                    PinBrush = Brushes.Gray;
                else if (SlotAssignment.Slot.ObjectGuid == SlotAsset.RANDOM_CLAY_GUID)
                    PinBrush = Brushes.SandyBrown;
                else if (SlotAssignment.Slot.ObjectGuid == SlotAsset.RANDOM_OIL_GUID)
                    PinBrush = Brushes.DarkSlateGray;
                else
                    PinBrush = Brushes.Red;

                BackgroundBrush = PinBrush;
            }
        }
    }
}
