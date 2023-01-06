using AnnoMapEditor.MapTemplates;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Input;
using System.Windows.Shapes;
using System.Windows.Media.Imaging;
using AnnoMapEditor.Utilities;
using System.ComponentModel;
using AnnoMapEditor.MapTemplates.Models;
using AnnoMapEditor.MapTemplates.Enums;

namespace AnnoMapEditor.UI.Controls
{
    public partial class MapObject : UserControl
    {
        static readonly Dictionary<string, SolidColorBrush> MapObjectColors = new()
        {
            ["Normal"] = new(Color.FromArgb(255, 8, 172, 137)),
            ["Starter"] = new(Color.FromArgb(255, 130, 172, 8)),
            ["ThirdParty"] = new(Color.FromArgb(255, 189, 73, 228)),
            ["Decoration"] = new(Color.FromArgb(255, 151, 162, 125)),
            ["PirateIsland"] = new(Color.FromArgb(255, 186, 0, 36)),
            ["Cliff"] = new(Color.FromArgb(255, 103, 105, 114)),
            ["Selected"] = new(Color.FromArgb(255, 255, 255, 255))
        };
        static readonly Dictionary<string, SolidColorBrush> MapObjectBackgrounds = new()
        {
            ["Normal"] = new(Color.FromArgb(32, 8, 172, 137)),
            ["Starter"] = new(Color.FromArgb(32, 130, 172, 8)),
            ["ThirdParty"] = new(Color.FromArgb(32, 189, 73, 228)),
            ["Decoration"] = new(Color.FromArgb(32, 151, 162, 125)),
            ["PirateIsland"] = new(Color.FromArgb(32, 186, 0, 36)),
            ["Cliff"] = new(Color.FromArgb(32, 103, 105, 114)),
            ["Selected"] = new(Color.FromArgb(32, 255, 255, 255))
        };
        static readonly Dictionary<IslandType, int> ZIndex = new()
        {
            [IslandType.Normal] = 3,
            [IslandType.Starter] = 2,
            [IslandType.ThirdParty] = 4,
            [IslandType.Decoration] = 1,
            [IslandType.PirateIsland] = 4,
            [IslandType.Cliff] = 0
        };
        static readonly SolidColorBrush White = new(Color.FromArgb(255, 255, 255, 255));

        readonly MapView container;

        public Vector2 MouseOffset;

        public bool IsMarkedForDeletion
        {
            get => crossOut.Visibility == Visibility.Visible;
            set
            {
                if (value != IsMarkedForDeletion)
                    crossOut.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private bool isSelected;

        private Rectangle? borderRectangle;

        private Island? island;

        public MapObject(Session session, MapView container)
        {
            InitializeComponent();

            this.container = container;
            DataContextChanged += MapObject_DataContextChanged;
//            this.container.SelectedIslandChanged += Container_SelectedIslandChanged;
            MouseOffset = Vector2.Zero;

            if (DataContext is Island island)
            {
                this.island = island;
                island.PropertyChanged += Island_PropertyChanged;
            }
        }

        private void Island_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            // island updates may come from background threads, make sure to update in UI
            if (Dispatcher.CheckAccess())
                Update();
            else
                Dispatcher.Invoke(() => Update());
        }

        private void UpdateSelectionBorder()
        {
            if (borderRectangle is not null && island is not null)
                borderRectangle.Stroke = MapObjectColors[isSelected ? "Selected" : island.Type.ToString()];
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            if (DataContext is not Island island)
                return;

//            container.SelectedIsland = island;
            MouseOffset = new(Mouse.GetPosition(this));
            e.Handled = true;
            base.OnMouseLeftButtonDown(e);
            Mouse.Capture(this);
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            if (isSelected)
                container.ReleaseMapObject(this);
            Mouse.Capture(null);
            e.Handled = true;
            base.OnMouseLeftButtonUp(e);
        }

        protected override void OnMouseRightButtonDown(MouseButtonEventArgs e)
        {
            if (isSelected)
                e.Handled = true;
            base.OnMouseRightButtonDown(e);
        }

        private void MapObject_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.island is not null)
                this.island.PropertyChanged -= Island_PropertyChanged;
            if (DataContext is Island island)
            {
                this.island = island;
                island.PropertyChanged += Island_PropertyChanged;
            }
            Update();
        }

        private void Update()
        {
            if (DataContext is not Island island)
                return;

            canvas.Children.Clear();

            Width = island.SizeInTiles;
            Height = island.SizeInTiles;
            this.SetPosition(island.Position);
            Panel.SetZIndex(this, ZIndex[island.Type]);

            Image? image;
            if (island.ImageFile != null)
            {
                image = new();
                BitmapImage? png = new();
                try
                {
                    using Stream? stream = Settings.Instance.DataArchive?.OpenRead(island.ImageFile);
                    if (stream is not null)
                    {
                        png.BeginInit();
                        png.StreamSource = stream;
                        png.CacheOption = BitmapCacheOption.OnLoad;
                        png.EndInit();
                        png.Freeze();
                    }
                }
                catch
                {
                    png = null;
                }

                if (png is not null)
                {
                    image.Width = island.MapSizeInTiles;
                    image.Height = island.MapSizeInTiles;
                    image.RenderTransform = new RotateTransform(island.Rotation * -90);
                    image.RenderTransformOrigin = new Point(0.5, 0.5);
                    image.Source = png;
                    canvas.Children.Add(image);
                    image.SetPosition(new Vector2(0, island.SizeInTiles - island.MapSizeInTiles));
                }
            }


            borderRectangle = new()
            {
                Fill = MapObjectBackgrounds[island.Type.ToString()],
                StrokeThickness = Vector2.Tile.Y,
                Width = island.SizeInTiles,
                Height = island.SizeInTiles
            };
            UpdateSelectionBorder();
            canvas.Children.Add(borderRectangle);

            const int CIRCLE_DIAMETER = 8;
            var circle = new Ellipse()
            {
                Width = CIRCLE_DIAMETER, // technically, should be 8 like the stroke but due to visual illusion 10 is better
                Height = CIRCLE_DIAMETER,
                Fill = White,
            };

            circle.SetPosition(Vector2.Zero);
            canvas.Children.Add(circle);

            if (!string.IsNullOrEmpty(island.Label))
                title.Text = island.Label;
            else if (island.Type == IslandType.PirateIsland)
                title.Text = "Pirate";
            else if (island.Type == IslandType.ThirdParty)
                title.Text = "3rd";
            else if (island.IsPool)
            {
                title.Text = island.Size.ToString();
                if (island.Type == IslandType.Starter)
                    title.Text = title.Text + "\nwith oil";
            }
            else
                title.Text = "";

            titleBackground.Visibility = title.Text == "" ? Visibility.Collapsed : Visibility.Visible;
        }
    }
}
