using AnnoMapEditor.MapTemplates;
using AnnoMapEditor.MapTemplates.Enums;
using AnnoMapEditor.MapTemplates.Models;
using AnnoMapEditor.UI.Controls.MapTemplates;
using AnnoMapEditor.UI.Windows.SelectIsland;
using AnnoMapEditor.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace AnnoMapEditor.UI.Controls
{
    public partial class MapView : UserControl
    {
        public static readonly double MAP_ROTATION_ANGLE = -135;

        private SelectIslandWindow? _selectIslandWindow;


        private Session? session { get; set; }
        private Rectangle? mapRect { get; set; }
        private Rectangle? playableRect { get; set; }
        private IList<AddIslandButton>? AddIslands { get; set; }
        private Vector2? oldSize { get; set; }


        public static readonly DependencyProperty SelectedIslandProperty =
             DependencyProperty.Register("SelectedIsland",
                propertyType: typeof(RandomIslandElement),
                ownerType: typeof(MapView),
                typeMetadata: new FrameworkPropertyMetadata(defaultValue: null));

        public RandomIslandElement? SelectedIsland
        {
            get { return (RandomIslandElement?)GetValue(SelectedIslandProperty); }
            set
            {
                if ((RandomIslandElement?)GetValue(SelectedIslandProperty) != value)
                {
                    SetValue(SelectedIslandProperty, value);
                }
            }
        }

        private MapElementViewModel? _selectedElement;


        public MapView()
        {
            InitializeComponent();

            SizeChanged += MapView_SizeChanged;
            DataContextChanged += MapView_DataContextChanged;

            Settings.Instance.PropertyChanged += Settings_PropertyChanged;
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
            foreach (var element in session.Elements)
            {
                MapElementViewModel viewModel;
                MapElementControl control;

                if (element is StartingSpotElement startingSpot) {
                    viewModel = new StartingSpotViewModel(session, startingSpot);
                    control = new StartingSpotControl();
                }
                else if (element is RandomIslandElement randomIsland)
                {
                    viewModel = new RandomIslandViewModel(session, randomIsland);
                    control = new IslandControl();
                }
                else if (element is FixedIslandElement fixedIsland)
                {
                    viewModel = new FixedIslandViewModel(session, fixedIsland);
                    control = new IslandControl();
                }
                else
                    throw new NotImplementedException();

                viewModel.PropertyChanged += MapElementViewModel_PropertyChanged;

                control.DataContext = viewModel;
                sessionCanvas.Children.Add(control);
            }

            AddIslands = new List<AddIslandButton>();

            // create add islands
            CreateAddIsland(MapElementType.PoolIsland, IslandSize.Small, IslandType.PirateIsland);
            CreateAddIsland(MapElementType.PoolIsland, IslandSize.Small, IslandType.ThirdParty);
            CreateAddIsland(MapElementType.PoolIsland, IslandSize.Small, IslandType.Normal);
            CreateAddIsland(MapElementType.PoolIsland, IslandSize.Medium, IslandType.Normal);
            CreateAddIsland(MapElementType.PoolIsland, IslandSize.Large, IslandType.Normal);

            CreateAddIsland(MapElementType.FixedIsland, IslandSize.Small, IslandType.PirateIsland);
            CreateAddIsland(MapElementType.FixedIsland, IslandSize.Small, IslandType.ThirdParty);
            CreateAddIsland(MapElementType.FixedIsland, IslandSize.Small, IslandType.Normal);
            CreateAddIsland(MapElementType.FixedIsland, IslandSize.Medium, IslandType.Normal);
            CreateAddIsland(MapElementType.FixedIsland, IslandSize.Large, IslandType.Normal);

            UpdateSize();
        }

        private void CreateAddIsland(MapElementType mapElementType, IslandSize size, IslandType type)
        {
            if (session is null || AddIslands is null) return;

            AddIslandViewModel viewModel = new(mapElementType, type, size);
            AddIslandButton button = new()
            {
                DataContext = viewModel
            };

            viewModel.IslandAdded += IslandAdded;

            AddIslands.Add(button);
            sessionCanvas.Children.Add(button);

            MoveAddIsland(button);
        }

        private void MoveAddIsland(AddIslandButton addIsland)
        {
            if (session is null || AddIslands is null) return;

            if (AddIslands.Contains(addIsland) && addIsland.DataContext is AddIslandViewModel island)
            {
                IslandSize size = island.IslandSize;
                IslandType type = island.IslandType;

                int islandLength = IslandSize.Small.DefaultSizeInTiles * 2 + 10 +
                        IslandSize.Medium.DefaultSizeInTiles + 25 +
                        IslandSize.Large.DefaultSizeInTiles + 25;
                int offset = Math.Max(250, (session.Size.Y - islandLength) / 2);

                Vector2 position;

                // pirate & 3rd party
                if (type == IslandType.PirateIsland)
                    position = new(-offset - IslandSize.Small.DefaultSizeInTiles, 20);
                else if (type == IslandType.ThirdParty)
                    position = new(-offset - 10 - IslandSize.Small.DefaultSizeInTiles * 2, 40 + IslandSize.Small.DefaultSizeInTiles);

                // player islands
                else
                {
                    if (size == IslandSize.Small)
                        position = new(-offset - 10 - IslandSize.Small.DefaultSizeInTiles * 2, 20);
                    else if (size == IslandSize.Large)
                        position = new(-offset - 35 - IslandSize.Small.DefaultSizeInTiles * 2 - IslandSize.Large.DefaultSizeInTiles, 20);
                    else
                        position = new(-offset - 60 - IslandSize.Small.DefaultSizeInTiles * 2 - IslandSize.Medium.DefaultSizeInTiles - IslandSize.Large.DefaultSizeInTiles, 20);
                }

                if (island.MapElementType == MapElementType.PoolIsland)
                    position = new(position.Y, position.X);

                addIsland.SetPosition(session.Size + position);
            }
        }

        private void IslandAdded(object? sender, IslandAddedEventArgs e)
        {
            ProtoIslandViewModel protoViewModel = new(session, e.MapElementType, e.IslandType, e.IslandSize, e.Position);
            sessionCanvas.Children.Add(new IslandControl()
            {
                DataContext = protoViewModel
            });
            protoViewModel.DragEnded += ProtoIsland_DragEnded;
            protoViewModel.IsSelected = true;
            protoViewModel.BeginDrag(Vector2.Zero);
        }

        private void ProtoIsland_DragEnded(object? sender, DragEndedEventArgs e)
        {
            if (sender is ProtoIslandViewModel protoViewModel)
            {
                // if the proto island is within bounds add the new island
                if (!protoViewModel.IsOutOfBounds)
                {
                    // if it is a FixedIsland, let the user select the correct island
                    if (protoViewModel.MapElementType == MapElementType.FixedIsland)
                    {
                        SelectIslandViewModel viewModel = new(protoViewModel.Island.IslandType);
                        viewModel.IslandSelected += (s, e) => SelectIsland_IslandSelected(s, e, protoViewModel.Island.Position);

                        _selectIslandWindow = new()
                        {
                            DataContext = viewModel
                        };
                        _selectIslandWindow.Show();
                    }

                    // otherwise create a random island
                    else if (protoViewModel.MapElementType == MapElementType.PoolIsland)
                    {
                        RandomIslandElement islandElement = new(protoViewModel.IslandSize, protoViewModel.Island.IslandType)
                        {
                            Position = protoViewModel.Island.Position
                        };
                        session!.Elements.Add(islandElement);
                    }
                    else
                        throw new NotImplementedException();
                }

                // remove the proto island
                foreach (var child in sessionCanvas.Children)
                {
                    if (child is IslandControl islandControl && islandControl.DataContext == protoViewModel)
                    {
                        protoViewModel.DragEnded -= ProtoIsland_DragEnded;
                        sessionCanvas.Children.Remove(islandControl);
                        break;
                    }
                }
            }
        }

        private void SelectIsland_IslandSelected(object? sender, IslandSelectedEventArgs e, Vector2 position)
        {
            _selectIslandWindow?.Hide();
            _selectIslandWindow = null;

            // add the new Island
            // TODO: Select the correct IslandType.
            FixedIslandElement fixedIslandElement = new(e.IslandAsset, IslandType.Normal)
            {
                Position = position
            };
            session.Elements.Add(fixedIslandElement);
        }

        private void MapElementViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            // handle selection of elements
            if (e.PropertyName == nameof(MapElementViewModel.IsSelected))
            {
                MapElementViewModel viewModel = sender as MapElementViewModel
                    ?? throw new Exception();

                if (viewModel.IsSelected && viewModel != _selectedElement)
                {
                    // deselect the currently selected
                    if (_selectedElement != null)
                    {
                        _selectedElement.IsSelected = false;
                        SelectedIsland = null;
                    }

                    _selectedElement = viewModel;
                    if (viewModel is RandomIslandViewModel islandViewModel)
                        SelectedIsland = islandViewModel.RandomIsland;
                }
            }

            // handle removal of islands
            else if (e.PropertyName == nameof(IslandViewModel.IsOutOfBounds) || e.PropertyName == nameof(DraggingViewModel.IsDragging))
            {
                if (sender is IslandViewModel viewModel)
                {
                    if (viewModel.IsOutOfBounds && !viewModel.IsDragging)
                        session.Elements.Remove(viewModel.Element);
                }
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
// TODO:                session.Elements.Remove(island);
                SelectedIsland = null;
            }
        }

        private void LinkSessionEventHandlers(Session session)
        {
            session.MapSizeConfigChanged += HandleSessionResized;
            session.MapSizeConfigCommitted += HandleSessionSizeCommitted;
            session.Elements.CollectionChanged += Session_ElementsChanged;
        }
        private void UnlinkSessionEventHandlers(Session session)
        {
            session.MapSizeConfigCommitted -= HandleSessionSizeCommitted;
            session.MapSizeConfigChanged -= HandleSessionResized;
            session.Elements.CollectionChanged -= Session_ElementsChanged;
        }

        private void Session_ElementsChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (var oldItem in e.OldItems)
                {
                    if (oldItem is IslandElement island)
                    {
                        // find the correct control
                        foreach (var child in sessionCanvas.Children)
                        {
                            if (child is IslandControl control 
                                && control.DataContext is IslandViewModel viewModel
                                && viewModel.Element == island)
                            {
                                viewModel.PropertyChanged -= MapElementViewModel_PropertyChanged;
                                sessionCanvas.Children.Remove(control);

                                break;
                            }
                        }
                    }
                }
            }

            if (e.NewItems != null)
            {
                foreach (var newItem in e.NewItems)
                {
                    IslandViewModel viewModel;
                    if (newItem is RandomIslandElement randomIsland)
                        viewModel = new RandomIslandViewModel(session, randomIsland);
                    else if (newItem is FixedIslandElement fixedIsland)
                        viewModel = new FixedIslandViewModel(session, fixedIsland);
                    else
                        continue;

                    sessionCanvas.Children.Add(new IslandControl()
                    {
                        DataContext = viewModel
                    });

                    viewModel.PropertyChanged += MapElementViewModel_PropertyChanged;
//                    viewModel.IsSelected = true;
//                    viewModel.BeginDrag(Vector2.Zero);
                }
            }
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
            }
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
        }

        private void RecalculateIslandCoordinates()
        {
            if (session is null) return;

            foreach (object item in sessionCanvas.Children)
            {
                if (item is AddIslandButton addIsland)
                {
                    MoveAddIsland(addIsland);
                }
                else if (item is MapObject mapObject && mapObject.DataContext is Island island)
                {
                    Vector2 canvasLocation = mapObject.GetPosition();
                    island.Position = canvasLocation;
                }
            }
        }
    }
}
