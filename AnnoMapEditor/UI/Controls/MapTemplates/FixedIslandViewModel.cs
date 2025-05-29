using AnnoMapEditor.MapTemplates.Enums;
using AnnoMapEditor.MapTemplates.Models;
using AnnoMapEditor.Utilities;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Imaging;

namespace AnnoMapEditor.UI.Controls.MapTemplates
{
    public class FixedIslandViewModel : IslandViewModel
    {
        private readonly FixedIslandElement _fixedIsland;

        public override string? Label => _fixedIsland?.Label;

        public override BitmapImage? Thumbnail => _fixedIsland.IslandAsset.Thumbnail;

        public override int ThumbnailRotation => _thumbnailRotation ?? 90;
        private int? _thumbnailRotation;

        public override bool RandomizeRotation => _fixedIsland.RandomizeRotation;

        private readonly bool _isContinentalIsland;


        public FixedIslandViewModel(MapTemplate mapTemplate, FixedIslandElement fixedIsland)
            : base(mapTemplate, fixedIsland)
        {
            _fixedIsland = fixedIsland;
            _isContinentalIsland = _fixedIsland.IslandAsset.IslandSize.FirstOrDefault() == IslandSize.Continental;

            if (_fixedIsland.Label != null) OnPropertyChanged(nameof(Label));

            SnapContinentalIsland();
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

            if (e.PropertyName == nameof(FixedIslandElement.Position))
                SnapContinentalIsland();
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

        // Rotates and snaps continental islands to the corners of the map.
        // If a continental island is placed elswhere, it would result in graphical glitches.
        private void SnapContinentalIsland()
        {
            if (!IsOutOfBounds && _isContinentalIsland)
            {
                Vector2 islandCenter = _fixedIsland.Position;
                Vector2 mapCenterOffset = new((_mapTemplate.Size.X - Island.SizeInTiles) / 2, (_mapTemplate.Size.Y - Island.SizeInTiles) / 2);
                string islandFileName = System.IO.Path.GetFileNameWithoutExtension(_fixedIsland.IslandAsset.FilePath);
                int rotationOffset = ContinentalRotationAtTop[islandFileName];

                if (islandCenter.X <= mapCenterOffset.X)
                {
                    // bottom
                    if (islandCenter.Y <= mapCenterOffset.Y)
                    {
                        _fixedIsland.Rotation = (byte)((2 + rotationOffset) % 4);
                        _fixedIsland.Position = new(0, 0);
                    }

                    // right
                    else
                    {
                        _fixedIsland.Rotation = (byte)((3 + rotationOffset) % 4);
                        _fixedIsland.Position = new(0, _mapTemplate.Size.Y - Island.SizeInTiles);
                    }
                }
                else
                {
                    // left
                    if (islandCenter.Y <= mapCenterOffset.Y)
                    {
                        _fixedIsland.Rotation = (byte)((1 + rotationOffset) % 4);
                        _fixedIsland.Position = new(_mapTemplate.Size.X - Island.SizeInTiles, 0);
                    }

                    // top
                    else
                    {
                        _fixedIsland.Rotation = (byte)((0 + rotationOffset) % 4);
                        _fixedIsland.Position = new(_mapTemplate.Size.X - Island.SizeInTiles, _mapTemplate.Size.Y - Island.SizeInTiles);
                    }
                }
            }
        }

        private static readonly Dictionary<string, int> ContinentalRotationAtTop = new Dictionary<string, int>()
        {
            { "colony01_c_01", 0 },
            { "moderate_c_01", 1},
            { "colony03_a03_01", 2}
        };
    }
}
