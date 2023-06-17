using AnnoMapEditor.MapTemplates.Enums;
using AnnoMapEditor.MapTemplates.Models;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Imaging;

namespace AnnoMapEditor.UI.Controls.MapTemplates
{
    public class FixedIslandViewModel : IslandViewModel
    {
        private readonly FixedIslandElement _fixedIsland;

        public override string? Label => _fixedIsland.Label;

        public override BitmapImage? Thumbnail => _fixedIsland.IslandAsset.Thumbnail;

        public override int ThumbnailRotation => _thumbnailRotation ?? 90;
        private int? _thumbnailRotation;

        public override bool RandomizeRotation => _fixedIsland.RandomizeRotation;

        public readonly bool IsContinentalIsland;


        public FixedIslandViewModel(MapTemplate mapTemplate, FixedIslandElement fixedIsland)
            : base(mapTemplate, fixedIsland)
        {
            _fixedIsland = fixedIsland;
            IsContinentalIsland = _fixedIsland.IslandAsset.IslandSize.FirstOrDefault() == IslandSize.Continental;

            UpdateThumbnailRotation();

            fixedIsland.PropertyChanged += FixedIsland_PropertyChanged;
        }

        private void FixedIsland_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(FixedIslandElement.Label))
                OnPropertyChanged(nameof(Label));

            if (e.PropertyName == nameof(FixedIslandElement.Rotation))
                UpdateThumbnailRotation();

            if (e.PropertyName == nameof(FixedIslandElement.IslandAsset))
                Redraw();
        }

        private void Redraw()
        {
            OnPropertyChanged(nameof(Label));
            OnPropertyChanged(nameof(Thumbnail));
            OnPropertyChanged(nameof(RandomizeRotation));
            UpdateThumbnailRotation();
        }

        private void UpdateThumbnailRotation()
        {
            // 0 -> 90
            // 1 -> 360
            // 2 -> 270
            // 3 -> 180

            _thumbnailRotation = (_fixedIsland.Rotation - 1) * -90;

            OnPropertyChanged(nameof(ThumbnailRotation));
        }

    }
}
