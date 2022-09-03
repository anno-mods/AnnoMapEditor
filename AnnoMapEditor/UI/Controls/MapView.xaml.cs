using AnnoMapEditor.MapTemplates;
using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace AnnoMapEditor.UI.Controls
{
    public static class CanvasExtensions
    {
        public static void SetPosition(this UIElement that, Vector2 position)
        {
            Canvas.SetLeft(that, position.X);
            Canvas.SetTop(that, position.Y);
        }
    }

    public partial class MapView : UserControl
    {
        Session? session;

        public static readonly DependencyProperty SelectedIslandProperty =
             DependencyProperty.Register("SelectedIsland",
                propertyType: typeof(Island),
                ownerType: typeof(MapView),
                typeMetadata: new FrameworkPropertyMetadata(defaultValue: null));

        public Island? SelectedIsland
        {
            get { return (Island?)GetValue(SelectedIslandProperty); }
            set
            {
                if ((Island?)GetValue(SelectedIslandProperty) != value)
                {
                    SetValue(SelectedIslandProperty, value);
                    SelectedIslandChanged?.Invoke(this, new() { Island = value });
                }
            }
        }

        public class SelectedIslandChangedEventArgs
        {
            public Island? Island { get; set; }
        }
        public delegate void SelectionChangedEventHandler(object sender, SelectedIslandChangedEventArgs e);
        public event SelectionChangedEventHandler? SelectedIslandChanged;

        public MapView()
        {
            InitializeComponent();

            SizeChanged += MapView_SizeChanged;
            DataContextChanged += MapView_DataContextChanged;

            Settings.Instance.PropertyChanged += Settings_PropertyChanged;
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            SelectedIsland = null;
            base.OnMouseDown(e);
        }

        private void MapView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            UpdateIslands(DataContext as Session);
        }

        private async void Settings_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (DataContext is not Session session)
                return;

            await session.UpdateExternalDataAsync();
            UpdateIslands(session);
        }

        private void MapView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateSize();
        }

        private void UpdateIslands(Session? session)
        {
            this.session = session;
            sessionCanvas.Children.Clear();
            if (session is null)
                return;

            sessionCanvas.Children.Add(new Rectangle
            {
                Fill = new SolidColorBrush(Color.FromArgb(255, 3, 19, 28)),
                Width = session.Size.X,
                Height = session.Size.Y
            });
            var playableArea = new Rectangle
            {
                Fill = new SolidColorBrush(Color.FromArgb(255, 24, 43, 63)),
                Width = session.PlayableArea.Width,
                Height = session.PlayableArea.Height
            };
            Canvas.SetLeft(playableArea, session.PlayableArea.X);
            Canvas.SetTop(playableArea, session.Size.Y - session.PlayableArea.Height - session.PlayableArea.Y - 1);
            sessionCanvas.Children.Add(playableArea);

            double requiredScaleX = sessionCanvas.ActualWidth / session.Size.X;
            double requiredScaleY = sessionCanvas.ActualHeight / session.Size.Y;
            float scale = (float)Math.Min(requiredScaleX, requiredScaleY);
            sessionCanvas.RenderTransform = new ScaleTransform(scale, scale);
            //sessionCanvas.Scale = new Vector3(scale, scale, 1);

            // add session islands
            var islands = session.Islands.Where(x => !x.Hide);
            foreach (var island in islands)
            {
                var obj = new MapObject(session, this)
                {
                    DataContext = island
                };
                sessionCanvas.Children.Add(obj);
            }

            // create add islands
            CreateAddIsland(IslandSize.Small, IslandType.PirateIsland);
            CreateAddIsland(IslandSize.Small, IslandType.ThirdParty);
            CreateAddIsland(IslandSize.Small, IslandType.Normal);
            CreateAddIsland(IslandSize.Medium, IslandType.Normal);
            CreateAddIsland(IslandSize.Large, IslandType.Normal);

            UpdateSize();
        }

        private void CreateAddIsland(IslandSize size, IslandType type)
        {
            if (session is null) return;

            int islandLength = IslandSize.Small.InTiles * 2 + 10 +
                    IslandSize.Medium.InTiles + 25 +
                    IslandSize.Large.InTiles + 25;
            int offset = Math.Max(250, (session.Size.Y - islandLength) / 2);

            Action<int, int> add = (x, y) => _ = sessionCanvas.Children.Add(new MapObject(session, this)
            {
                DataContext = Island.Create(size, type, session.Size + new Vector2(x, y))
            });

            // pirate & 3rd party
            if (size == IslandSize.Small && type == IslandType.PirateIsland)
                add(20, -offset - IslandSize.Small.InTiles);
            else if (size == IslandSize.Small && type == IslandType.ThirdParty)
                add(40 + IslandSize.Small.InTiles, -offset - 10 - IslandSize.Small.InTiles * 2);
            // player islands
            else if (size == IslandSize.Small && type == IslandType.Normal)
                add(20, -offset - 10 - IslandSize.Small.InTiles * 2);
            else if (size == IslandSize.Large && type == IslandType.Normal)
                add(20, -offset - 35 - IslandSize.Small.InTiles * 2 - IslandSize.Large.InTiles);
            else if (size == IslandSize.Medium && type == IslandType.Normal)
                add(20, -offset - 60 - IslandSize.Small.InTiles * 2 - IslandSize.Medium.InTiles - IslandSize.Large.InTiles);
        }

        void UpdateSize()
        {
            if (session is null)
                return;

            double size = Math.Min(ActualWidth, ActualHeight);
            size = Math.Sqrt((size * size) / 2);

            double requiredScaleX = size / session.Size.X;
            double requiredScaleY = size / session.Size.Y;
            float scale = (float)Math.Min(requiredScaleX, requiredScaleY);

            sessionCanvas.RenderTransform = new ScaleTransform(scale, scale);
            rotationCanvas.Width = scale * session.Size.X;
            rotationCanvas.Height = scale * session.Size.Y;
        }

        public void ReleaseMapObject(MapObject mapObject)
        {
            if (session is null || mapObject.DataContext is not Island island)
                return;

            if (mapObject.IsMarkedForDeletion)
            {
                sessionCanvas.Children.Remove(mapObject);
                session.RemoveIsland(island);
                SelectedIsland = null;
            }
        }

        public void MoveMapObject(MapObject mapObject, Vector2 position)
        {
            if (session is null) return;

            _MoveMapObject(mapObject, position);
        }

        private void _MoveMapObject(MapObject mapObject, Vector2 position)
        {
            if (session is null || mapObject.DataContext is not Island island) return;

            var mapArea = new Rect2(session.Size - island.SizeInTiles + Vector2.Tile);

            // provide some resitence when moving out
            var ensured = island.IsNew ? position : position.Clamp(mapArea);
            if ((ensured - position).Length > 50)
            {
                ensured = position;
            }

            if (island.IsNew && position.Within(mapArea))
            {
                // convert add island to real island when entering session area
                session.AddIsland(island);
                CreateAddIsland(island.Size, island.Type);
            }

            island.Position = ensured.FlipYItem(session.Size.Y, island.SizeInTiles);
            mapObject.SetPosition(ensured);
            mapObject.IsMarkedForDeletion = !island.IsNew && !ensured.Within(mapArea);
        }
    }
}
