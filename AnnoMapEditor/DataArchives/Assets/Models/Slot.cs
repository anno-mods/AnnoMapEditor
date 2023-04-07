using AnnoMapEditor.Utilities;

namespace AnnoMapEditor.DataArchives.Assets.Models
{
    public class Slot
    {
        public int GroupId { get; init; }

        public long ObjectId { get; init; }

        public int ObjectGuid { get; init; }

        public Vector2 Position { get; init; }
        
        // delayed initialization in IslandRepository.DoLoad()
        public SlotAsset? SlotAsset { get; set; }
    }
}
