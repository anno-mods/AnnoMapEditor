using AnnoMapEditor.Utilities;

namespace AnnoMapEditor.DataArchives.Assets.Models
{
    public class FixedIslandAsset : ObservableBase
    {
        public string FilePath { get; init; }


        public FixedIslandAsset(string filePath)
        {
            FilePath = filePath;
        }
    }
}
