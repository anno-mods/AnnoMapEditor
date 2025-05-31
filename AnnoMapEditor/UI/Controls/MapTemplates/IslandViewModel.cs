using AnnoMapEditor.MapTemplates.Models;
using AnnoMapEditor.Utilities;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace AnnoMapEditor.UI.Controls.MapTemplates
{
    public abstract class IslandViewModel : MapElementViewModel
    {
        static readonly Dictionary<string, SolidColorBrush> BorderBrushes = new()
        {
            ["Normal"] = new(Color.FromArgb(255, 8, 172, 137)),
            ["Starter"] = new(Color.FromArgb(255, 130, 172, 8)),
            ["ThirdParty"] = new(Color.FromArgb(255, 189, 73, 228)),
            ["Decoration"] = new(Color.FromArgb(255, 151, 162, 125)),
            ["PirateIsland"] = new(Color.FromArgb(255, 186, 0, 36)),
            ["Cliff"] = new(Color.FromArgb(255, 103, 105, 114)),
            ["Selected"] = new(Color.FromArgb(255, 255, 255, 255))
        };
        static readonly Dictionary<string, SolidColorBrush> BackgroundBrushes = new()
        {
            ["Normal"] = new(Color.FromArgb(32, 8, 172, 137)),
            ["Starter"] = new(Color.FromArgb(32, 130, 172, 8)),
            ["ThirdParty"] = new(Color.FromArgb(32, 189, 73, 228)),
            ["Decoration"] = new(Color.FromArgb(32, 151, 162, 125)),
            ["PirateIsland"] = new(Color.FromArgb(32, 186, 0, 36)),
            ["Cliff"] = new(Color.FromArgb(32, 103, 105, 114)),
            ["Selected"] = new(Color.FromArgb(32, 255, 255, 255))
        };


        protected readonly MapTemplate _mapTemplate;

        public IslandElement Island { get; init; }

        public SolidColorBrush BackgroundBrush
        {
            get => _backgroundBrush;
            set => SetProperty(ref _backgroundBrush, value);
        }
        private SolidColorBrush _backgroundBrush = BackgroundBrushes["Normal"];

        public SolidColorBrush BorderBrush
        {
            get => _borderBrush;
            set => SetProperty(ref _borderBrush, value);
        }
        private SolidColorBrush _borderBrush = BorderBrushes["Normal"];

        public bool IsOutOfBounds
        {
            get => _isOutOfBounds;
            set => SetProperty(ref _isOutOfBounds, value);
        }
        private bool _isOutOfBounds;

        public abstract string? Label { get; }

        public bool ShowLabel => _showLabel;
        private bool _showLabel;

        public virtual BitmapImage? Thumbnail { get; }

        public virtual int ThumbnailRotation { get; }

        public virtual bool RandomizeRotation => true;

        public virtual ObservableCollection<SlotAssignment>? SlotAssignments { get; protected set; }


        public IslandViewModel(MapTemplate mapTemplate, IslandElement island)
            : base(island)
        {
            _mapTemplate = mapTemplate;
            Island = island;

            UpdateBackground();
            UpdateLabelVisibility();

            PropertyChanged += This_PropertyChanged;
            Island.PropertyChanged += Island_PropertyChanged;
            _mapTemplate.PropertyChanged += MapTemplate_ConfigChanged;
        }


        private void This_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IsSelected))
            {
                UpdateBackground();
                UpdateLabelVisibility();
            }

            if (e.PropertyName == nameof(Label))
                UpdateLabelVisibility();
        }

        private void Island_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IslandElement.IslandType))
                UpdateBackground();

            else if (e.PropertyName == nameof(FixedIslandElement.RandomizeRotation))
                UpdateRotation();

            else if (e.PropertyName == nameof(MapElement.Position))
                BoundsCheck();

            else if (e.PropertyName == nameof(IslandElement.Label))
                UpdateLabelVisibility();
        }

        private void MapTemplate_ConfigChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(MapTemplate.ShowLabels))
                UpdateLabelVisibility();
        }

        private void UpdateBackground()
        {
            if (IsSelected)
            {
                BorderBrush = BorderBrushes["Selected"];
                BackgroundBrush = BackgroundBrushes["Selected"];
            }
            else
            {
                BorderBrush = BorderBrushes[Island.IslandType.Name];
                BackgroundBrush = BackgroundBrushes[Island.IslandType.Name];
            }
        }

        private void UpdateRotation()
        {
            OnPropertyChanged(nameof(RandomizeRotation));
        }

        public override void Move(Point delta)
        {
            Vector2 vectorDelta = new(delta);
            Vector2 newPosition = Element.Position + vectorDelta;

            Rect2 mapArea = new(_mapTemplate.Size - Island.SizeInTiles + Vector2.Tile);

            // provide resistance against moving islands of the map
            if (!IsOutOfBounds && !newPosition.Within(mapArea) && vectorDelta.Length < 250)
                Element.Position = newPosition.Clamp(mapArea);

            else
                Element.Position = newPosition;
        }

        public void BoundsCheck()
        {
            var mapArea = new Rect2(_mapTemplate.Size - Island.SizeInTiles + Vector2.Tile);
            Vector2 position = Element.Position;
            IsOutOfBounds = !position.Within(mapArea);
        }

        private void UpdateLabelVisibility()
        {
            _showLabel = _mapTemplate.ShowLabels && (Label != null);
            // if (IsSelected && Island is FixedIslandElement)
            //     _showLabel = false;
            OnPropertyChanged(nameof(ShowLabel));
        }
    }
}
