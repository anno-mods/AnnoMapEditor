using AnnoMapEditor.MapTemplates;
using AnnoMapEditor.MapTemplates.Enums;
using AnnoMapEditor.MapTemplates.Models;
using AnnoMapEditor.UI.Controls.MapTemplates;
using AnnoMapEditor.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace AnnoMapEditor.UI.Controls
{
    public partial class MapView : UserControl
    {
        public static readonly double MAP_ROTATION_ANGLE = -135;


        private Session? session { get; set; }
        private Rectangle? mapRect { get; set; }
        private Rectangle? playableRect { get; set; }
        private IList<MapObject>? AddIslands { get; set; }
        private Vector2? oldSize { get; set; }

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

        private void Settings_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (DataContext is not Session session)
                return;

            session.UpdateExternalData();
            UpdateIslands(session);
        }

        private void MapView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateSize();
        }

        private void UpdateIslands(Session? session)
        {
            //Unlink event handlers from old session object
            if(this.session is not null)
            {
                UnlinkSessionEventHandlers(this.session);
            }

            this.session = session;

            sessionCanvas.Children.Clear();
            if (session is null)
            {
                mapRect = null;
                playableRect = null;
                AddIslands = null;
                return;
            }

            LinkSessionEventHandlers(session);

            mapRect = new Rectangle
            {
                Fill = new SolidColorBrush(Color.FromArgb(255, 3, 19, 28)),
                Width = session.Size.X,
                Height = session.Size.Y
            };
            sessionCanvas.Children.Add(mapRect);

            playableRect = new Rectangle
            {
                Fill = new SolidColorBrush(Color.FromArgb(255, 24, 43, 63)),
                Width = session.PlayableArea.Width,
                Height = session.PlayableArea.Height
            };
            Canvas.SetLeft(playableRect, session.PlayableArea.X);
            Canvas.SetTop(playableRect, session.Size.Y - session.PlayableArea.Height - session.PlayableArea.Y - 1);
            sessionCanvas.Children.Add(playableRect);

            double requiredScaleX = sessionCanvas.ActualWidth / session.Size.X;
            double requiredScaleY = sessionCanvas.ActualHeight / session.Size.Y;
            float scale = (float)Math.Min(requiredScaleX, requiredScaleY);
            sessionCanvas.RenderTransform = new ScaleTransform(scale, scale);
            //sessionCanvas.Scale = new Vector3(scale, scale, 1);

            // add session islands
            foreach (var island in session.Islands)
            {
                var obj = new MapObject(session, this)
                {
                    DataContext = island
                };
                sessionCanvas.Children.Add(obj);
            }

            AddIslands = new List<MapObject>();

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
            if (session is null || AddIslands is null) return;

            MapObject mapObject = new MapObject(session, this)
            {
                DataContext = new Island(Region.Moderate)
                {
                    ElementType = MapElementType.PoolIsland,
                    Position = session.Size + new Vector2(Vector2.Zero),
                    Size = size,
                    Type = type
                }
            };

            AddIslands.Add(mapObject);
            sessionCanvas.Children.Add(mapObject);

            MoveAddIsland(mapObject);
        }

        private void MoveAddIsland(MapObject addIsland)
        {
            if (session is null || AddIslands is null) return;

            if (AddIslands.Contains(addIsland) && addIsland.DataContext is Island island)
            {
                IslandSize size = island.Size;
                IslandType type = island.Type;

                int islandLength = IslandSize.Small.DefaultSizeInTiles * 2 + 10 +
                        IslandSize.Medium.DefaultSizeInTiles + 25 +
                        IslandSize.Large.DefaultSizeInTiles + 25;
                int offset = Math.Max(250, (session.Size.Y - islandLength) / 2);

                Action<int, int> move = (x, y) =>
                {
                    island.Position = session.Size + new Vector2(x, y);
                    var islandCanvasPos = island.Position.FlipYItem(session.Size.Y, island.SizeInTiles);
                    addIsland.SetPosition(islandCanvasPos);
                };

                // pirate & 3rd party
                if (size == IslandSize.Small && type == IslandType.PirateIsland)
                    move(20, -offset - IslandSize.Small.DefaultSizeInTiles);
                else if (size == IslandSize.Small && type == IslandType.ThirdParty)
                    move(40 + IslandSize.Small.DefaultSizeInTiles, -offset - 10 - IslandSize.Small.DefaultSizeInTiles * 2);
                // player islands
                else if (size == IslandSize.Small && type == IslandType.Normal)
                    move(20, -offset - 10 - IslandSize.Small.DefaultSizeInTiles * 2);
                else if (size == IslandSize.Large && type == IslandType.Normal)
                    move(20, -offset - 35 - IslandSize.Small.DefaultSizeInTiles * 2 - IslandSize.Large.DefaultSizeInTiles);
                else if (size == IslandSize.Medium && type == IslandType.Normal)
                    move(20, -offset - 60 - IslandSize.Small.DefaultSizeInTiles * 2 - IslandSize.Medium.DefaultSizeInTiles - IslandSize.Large.DefaultSizeInTiles);
            }
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
                session.Islands.Remove(island);
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
                session.Islands.Add(island);
                AddIslands?.Remove(mapObject);
                CreateAddIsland(island.Size, island.Type);
            }

            island.Position = ensured;
            mapObject.SetPosition(ensured);
            mapObject.IsMarkedForDeletion = !island.IsNew && !ensured.Within(mapArea);
        }

        private void LinkSessionEventHandlers(Session session)
        {
            session.MapSizeConfigChanged += HandleSessionResized;
            session.MapSizeConfigCommitted += HandleSessionSizeCommitted;
        }
        private void UnlinkSessionEventHandlers(Session session)
        {
            session.MapSizeConfigCommitted -= HandleSessionSizeCommitted;
            session.MapSizeConfigChanged -= HandleSessionResized;
        }

        private void HandleSessionResized(object? sender, Session.SessionResizeEventArgs args)
        {
            if(sender is Session session)
            {
                if (oldSize is null)
                    oldSize = new Vector2(args.OldMapSize);

                if(mapRect is not null)
                {
                    mapRect.Width = session.Size.X;
                    mapRect.Height = session.Size.Y;

                    Canvas.SetBottom(mapRect, -session.Size.Y);
                }

                if(playableRect is not null)
                {
                    playableRect.Width = session.PlayableArea.Width;
                    playableRect.Height = session.PlayableArea.Height;

                    //Set Playable area to center of map
                    Canvas.SetLeft(playableRect, session.PlayableArea.X);
                    Canvas.SetTop(playableRect, session.Size.Y - session.PlayableArea.Height - session.PlayableArea.Y - 1);
                }

                bool sizeIncrease = session.Size.X > oldSize.X;

                if (sizeIncrease)
                {
                    UpdateSize();
                }

                MarkMapObjects(sizeIncrease);
            }
        }

        private void MarkMapObjects(bool sizeIncrease)
        {
            if (session is null) return;

            foreach (object item in sessionCanvas.Children)
            {
                if(item is MapObject mapObject && mapObject.DataContext is Island island)
                {
                    if (AddIslands is not null && AddIslands.Contains(mapObject))
                    {
                        //Nested if because AddIslands should not fall through to else if not sizeIncrease
                        if(sizeIncrease)
                        {
                            MoveAddIsland(mapObject);
                        }
                    }
                    else
                    {
                        var islandCanvasPos = island.Position;
                        var mapArea = new Rect2(session.Size - island.SizeInTiles + Vector2.Tile);

                        mapObject.IsMarkedForDeletion = !islandCanvasPos.Within(mapArea);
                    }
                }
            }

            ;

        }

        private void HandleSessionSizeCommitted(object? sender, EventArgs _)
        {
            oldSize = null;
            ClearMarkedMapObjects();
            RecalculateIslandCoordinates();
            UpdateSize();
        }

        private void ClearMarkedMapObjects()
        {
            if (session is null) return;

            List<(MapObject, Island)> toDelete = new List<(MapObject, Island)>();

            foreach (object item in sessionCanvas.Children)
            {
                if (item is MapObject mapObject && mapObject.DataContext is Island island && mapObject.IsMarkedForDeletion)
                {
                    toDelete.Add((mapObject, island));
                }
            }

            foreach((MapObject mapObjectToDelete, Island islandToDelete) in toDelete)
            {
                sessionCanvas.Children.Remove(mapObjectToDelete);
                session.Islands.Remove(islandToDelete);
            }
        }

        private void RecalculateIslandCoordinates()
        {
            if (session is null) return;

            foreach (object item in sessionCanvas.Children)
            {
                if (item is MapObject mapObject && mapObject.DataContext is Island island)
                {
                    if (AddIslands is not null && AddIslands.Contains(mapObject))
                    {
                        MoveAddIsland(mapObject);
                    }
                    else
                    {
                        Vector2 canvasLocation = mapObject.GetPosition();
                        island.Position = canvasLocation;
                    }
                }
            }
        }
    }
}
