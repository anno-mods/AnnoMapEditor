using AnnoMapEditor.MapTemplates.Enums;
using AnnoMapEditor.MapTemplates.Models;
using AnnoMapEditor.Utilities;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Media;

namespace AnnoMapEditor.UI.Controls.MapTemplates
{
    public class RandomIslandViewModel : MapElementViewModel
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


        private readonly Session _session;

        private readonly RandomIslandElement _randomIsland;

        public string Label
        {
            get => _label;
            set => SetProperty(ref _label, value);
        }
        private string _label = string.Empty;

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

        public int SizeInTiles => _randomIsland.IslandSize.DefaultSizeInTiles;


        public RandomIslandViewModel(Session session, RandomIslandElement randomIsland)
            : base(randomIsland)
        {
            _session = session;
            _randomIsland = randomIsland;

            UpdateBackground();
            UpdateLabel();

            PropertyChanged += This_PropertyChanged;
            _randomIsland.PropertyChanged += RandomIsland_PropertyChanged;
        }


        private void This_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IsSelected))
                UpdateBackground();
        }

        private void RandomIsland_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IslandElement.IslandType))
                UpdateBackground();

            if (e.PropertyName == nameof(IslandElement.IslandType) || e.PropertyName == nameof(IslandElement.Label))
                UpdateLabel();

            if (e.PropertyName == nameof(RandomIslandElement.IslandSize))
                OnPropertyChanged(nameof(SizeInTiles));
        }


        private void UpdateLabel()
        {
            // use the island's label if it has one
            if (_randomIsland.Label != null)
                Label = _randomIsland.Label;

            else
            {
                string label = $"Random\n{_randomIsland.IslandType.Name}";

                if (_randomIsland.IslandType == IslandType.Starter)
                    label += "\nwith Oil";

                Label = label;
            }
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
                BorderBrush = BorderBrushes[_randomIsland.IslandType.Name];
                BackgroundBrush = BackgroundBrushes[_randomIsland.IslandType.Name];
            }
        }

        public override void OnDragged(Vector2 newPosition)
        {
            // mark the island if it is out of bounds
            var mapArea = new Rect2(_session.Size - _randomIsland.IslandSize.DefaultSizeInTiles + Vector2.Tile);
            IsOutOfBounds = !newPosition.Within(mapArea);

            base.OnDragged(newPosition);
        }
    }
}
