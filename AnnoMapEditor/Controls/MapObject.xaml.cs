using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Input;
using System.Windows.Shapes;
using System.Windows.Media.Imaging;
using AnnoMapEditor.MapTemplates;

namespace AnnoMapEditor.Controls
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
            ["Cliff"] = new(Color.FromArgb(255, 103, 105, 114))
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
        static readonly SolidColorBrush Yellow = new(Color.FromArgb(255, 234, 224, 83));
        readonly Session session;

        public Point MouseOffset;

        public MapObject(Session session)
        {
            InitializeComponent();

            this.session = session;
            DataContextChanged += MapObject_DataContextChanged;
            MouseMove += OnMouseMove;
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                MouseOffset = Mouse.GetPosition(this);
                DragDrop.DoDragDrop(this, this, DragDropEffects.Move);                
            }
        }

        private void MapObject_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (DataContext is not Island island)
                return;

            if (island.ElementType == 2)
            {
                var circle = new Ellipse()
                {
                    Width = 20,
                    Height = 20,
                    Fill = Yellow,
                };

                Canvas.SetLeft(circle, -10);
                Canvas.SetTop(circle, -10);
                canvas.Children.Add(circle);

                Width = 1;
                Height = 1;
                Canvas.SetLeft(this, island.Position.X);
                Canvas.SetTop(this, session.Size.Y - island.Position.Y);
                Panel.SetZIndex(this, 100);
                return;
            }
            else
            {

                Width = island.SizeInTiles;
                Height = island.SizeInTiles;
                Canvas.SetLeft(this, island.Position.X);
                Canvas.SetTop(this, session.Size.Y - island.Position.Y - island.SizeInTiles);
                Panel.SetZIndex(this, ZIndex[island.Type]);

                Image? image = null;
                if (island.ImageFile != null)
                {
                    image = new();
                    BitmapImage? png = null;
                    //await System.Threading.Tasks.Task.Run(() =>
                    //{
                        png = new();
                        try
                        {
                            using Stream? stream = Utils.Settings.Instance.DataArchive?.OpenRead(island.ImageFile);
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
                    //});

                    if (png is not null)
                    {
                        image.Width = island.SizeInTiles;
                        image.Height = island.SizeInTiles;
                        image.RenderTransform = new RotateTransform(island.Rotation * -90);
                        image.RenderTransformOrigin = new Point(0.5, 0.5);
                        image.Source = png;
                        canvas.Children.Add(image);
                    }
                }


                Rectangle rect = new();
                if (image is not null)
                {
                    rect.Stroke = MapObjectColors[island.Type.ToString()];
                    rect.StrokeThickness = 5;
                }
                else
                {
                    rect.Fill = MapObjectColors[island.Type.ToString()];
                }
                rect.Width = island.SizeInTiles;
                rect.Height = island.SizeInTiles;
                canvas.Children.Add(rect);

                var circle = new Ellipse()
                {
                    Width = 10,
                    Height = 10,
                    Fill = White,
                };

                Canvas.SetLeft(circle, 0);
                Canvas.SetTop(circle, island.SizeInTiles - 10);
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
                        title.Text = "Starter\n" + title.Text;
                }
                else
                    title.Text = "";

                titleBackground.Visibility = title.Text == "" ? Visibility.Collapsed : Visibility.Visible;
            }
        }
    }
}
