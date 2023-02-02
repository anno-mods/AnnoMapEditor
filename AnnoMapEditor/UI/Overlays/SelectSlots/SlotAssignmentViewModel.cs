using AnnoMapEditor.MapTemplates.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnnoMapEditor.UI.Overlays.SelectSlots
{
    public class SlotAssignmentViewModel
    {
        public SlotAssignment SlotAssignment { get; init; }


        public SlotAssignmentViewModel(SlotAssignment slotAssignment)
        {
            SlotAssignment = slotAssignment;
        }
    }
}
