using AnnoMapEditor.DataArchives.Assets.Models;
using AnnoMapEditor.Utilities;

namespace AnnoMapEditor.MapTemplates.Models
{
    public class SlotAssignment : ObservableBase
    {
        public Slot Slot { get; init; }

        public SlotAsset? AssignedSlot
        {
            get => _assignedSlot;
            set => SetProperty(ref _assignedSlot, value);
        }
        private SlotAsset? _assignedSlot;
    }
}
