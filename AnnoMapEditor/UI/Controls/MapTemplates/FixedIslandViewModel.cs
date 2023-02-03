using AnnoMapEditor.MapTemplates.Enums;
using AnnoMapEditor.MapTemplates.Models;
using AnnoMapEditor.Utilities;
using System.Linq;
using System.Windows.Media.Imaging;

namespace AnnoMapEditor.UI.Controls.MapTemplates
{
    public class FixedIslandViewModel : IslandViewModel
    {
        private readonly FixedIslandElement _fixedIsland;

        public override string? Label => _fixedIsland.Label;

        public override int SizeInTiles => _fixedIsland.IslandAsset.SizeInTiles;

        public override BitmapImage? Thumbnail => _fixedIsland.IslandAsset.Thumbnail;
        
        public override int ThumbnailRotation => _thumbnailRotation ?? 0;
        private int? _thumbnailRotation;

        public override bool RandomizeRotation => _fixedIsland.RandomizeRotation;

        private readonly bool _isContinentalIsland;


        public FixedIslandViewModel(Session session, FixedIslandElement fixedIsland) 
            : base(session, fixedIsland)
        {
            _fixedIsland = fixedIsland;
            _isContinentalIsland = _fixedIsland.IslandAsset.IslandSize.FirstOrDefault() == IslandSize.Continental;

            UpdateThumbnailRotation();
            SnapContinentalIsland();

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
            // 0 -> 90
            // 1 -> 360
            // 2 -> 270
            // 3 -> 180

            _thumbnailRotation = (_fixedIsland.Rotation - 1) * -90;

            OnPropertyChanged(nameof(ThumbnailRotation));
        }


        public override void OnDragged(Vector2 newPosition)
        {
            base.OnDragged(newPosition);
            SnapContinentalIsland();
        }

        // Rotates and snaps continental islands to the corners of the map.
        // If a continental island is placed elswhere, it would result in graphical glitches.
        private void SnapContinentalIsland()
        {
            if (!IsOutOfBounds && _isContinentalIsland)
            {
                Vector2 islandCenter = _fixedIsland.Position;
                Vector2 mapCenterOffset = new((_session.Size.X - SizeInTiles) / 2, (_session.Size.Y - SizeInTiles) / 2);

                if (islandCenter.X <= mapCenterOffset.X)
                {
                    // bottom
                    if (islandCenter.Y <= mapCenterOffset.Y)
                    {
                        _fixedIsland.Rotation = 3;
                        _fixedIsland.Position = new(0, 0);
                    }

                    // right
                    else
                    {
                        _fixedIsland.Rotation = 0;
                        _fixedIsland.Position = new(0, _session.Size.Y - SizeInTiles);
                    }
                }
                else
                {
                    // left
                    if (islandCenter.Y <= mapCenterOffset.Y)
                    {
                        _fixedIsland.Rotation = 2;
                        _fixedIsland.Position = new(_session.Size.X - SizeInTiles, 0);
                    }

                    // top
                    else
                    {
                        _fixedIsland.Rotation = 1;
                        _fixedIsland.Position = new(_session.Size.X - SizeInTiles, _session.Size.Y - SizeInTiles);
                    }
                }
            }
        }
    }
}
