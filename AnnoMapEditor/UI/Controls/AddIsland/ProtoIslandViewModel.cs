using AnnoMapEditor.DataArchives.Assets.Models;
using AnnoMapEditor.MapTemplates.Enums;
using AnnoMapEditor.MapTemplates.Models;
using AnnoMapEditor.Utilities;
using System.Windows.Media.Imaging;

namespace AnnoMapEditor.UI.Controls.MapTemplates
{
    public class ProtoIslandViewModel : IslandViewModel
    {
        public override string? Label => _label;
        private string _label;

        public MapElementType MapElementType { get; init; }

        public IslandSize IslandSize { get; init; }

        public override int SizeInTiles => IslandSize.DefaultSizeInTiles;

        public override BitmapImage? Thumbnail => null;

        public override int? ThumbnailRotation => null;


        public ProtoIslandViewModel(Session session, MapElementType elementType, IslandType islandType, IslandSize islandSize, Vector2 position)
            : base(session, new ProtoIslandElement(islandType, islandSize) { Position = position })
        {
            string prefix = elementType == MapElementType.PoolIsland ? "Random" : "Fixed";
            string suffix = islandType == IslandType.ThirdParty || islandType == IslandType.PirateIsland 
                ? islandType.Name : islandSize.Name;
            _label = $"{prefix}\n{suffix}";

            MapElementType = elementType;
            IslandSize = islandSize;
        }


        class ProtoIslandElement : IslandElement
        {
            public IslandSize IslandSize { get; init; }


            public ProtoIslandElement(IslandType islandType, IslandSize islandSize)
                : base(islandType)
            {
                IslandSize = islandSize;
            }
        }
    }
}
