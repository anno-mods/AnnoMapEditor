using AnnoMapEditor.MapTemplates.Models;
using System.Windows.Media.Imaging;

namespace AnnoMapEditor.UI.Controls.MapTemplates
{
    public class FixedIslandViewModel : IslandViewModel
    {
        private readonly FixedIslandElement _fixedIsland;

        public override string? Label => _fixedIsland.Label;

        public override int SizeInTiles => _fixedIsland.IslandAsset.SizeInTiles;

        public override BitmapImage? Thumbnail => _fixedIsland.IslandAsset.Thumbnail;

        public override int? ThumbnailRotation => _thumbnailRotation;
        private int? _thumbnailRotation;


        public FixedIslandViewModel(Session session, FixedIslandElement fixedIsland) 
            : base(session, fixedIsland)
        {
            _fixedIsland = fixedIsland;

            UpdateThumbnailRotation();

            fixedIsland.PropertyChanged += FixedIsland_PropertyChanged;
        }

        private void FixedIsland_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(FixedIslandElement.Label))
                OnPropertyChanged(nameof(Label));

            if (e.PropertyName == nameof(FixedIslandElement.Rotation))
                UpdateThumbnailRotation();
        }

        private void UpdateThumbnailRotation()
        {
            _thumbnailRotation = _fixedIsland.Rotation * 90;
            OnPropertyChanged(nameof(ThumbnailRotation));
        }
    }
}
