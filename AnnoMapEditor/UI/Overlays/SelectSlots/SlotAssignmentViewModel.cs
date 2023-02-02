using AnnoMapEditor.DataArchives.Assets.Models;
using AnnoMapEditor.MapTemplates.Models;
using AnnoMapEditor.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace AnnoMapEditor.UI.Overlays.SelectSlots
{
    public class SlotAssignmentViewModel : ObservableBase
    {
        private static readonly SlotAsset EmptySlot = new()
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
            }
        }

        public IEnumerable<SlotAsset> SlotItems { get; init; }


        public SlotAssignmentViewModel(SlotAssignment slotAssignment)
        {
            SlotAssignment = slotAssignment;

            // copy the list of allowed replacements and add the EmptySlot to the end
            List<SlotAsset> slotItems = new(slotAssignment.Slot.SlotAsset!.ReplacementSlotAssets);
            slotItems.Add(EmptySlot);
            SlotItems = slotItems;
        }
    }
}
