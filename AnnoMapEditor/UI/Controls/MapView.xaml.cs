using AnnoMapEditor.MapTemplates.Enums;
using AnnoMapEditor.MapTemplates.Models;
using AnnoMapEditor.UI.Controls.AddIsland;
using AnnoMapEditor.UI.Controls.MapTemplates;
using AnnoMapEditor.UI.Overlays;
using AnnoMapEditor.UI.Overlays.SelectIsland;
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


        private Session? session { get; set; }
        private Rectangle? mapRect { get; set; }
        private PlayableAreaControl? playableRect { get; set; }
        private IList<AddIslandButton>? AddIslands { get; set; }
        private Vector2? oldSize { get; set; }


        public static readonly DependencyProperty SelectedIslandProperty =
             DependencyProperty.Register("SelectedIsland",
                propertyType: typeof(IslandElement),
                ownerType: typeof(MapView),
                typeMetadata: new FrameworkPropertyMetadata(defaultValue: null));

        public IslandElement? SelectedIsland
        {
            get { return (IslandElement?)GetValue(SelectedIslandProperty); }
            set
            {
                if ((IslandElement?)GetValue(SelectedIslandProperty) != value)
                {
                    SetValue(SelectedIslandProperty, value);
                }
            }
        }


        #region EditPlayableArea DependencyProperty
        public static readonly DependencyProperty EditPlayableAreaProperty =
             DependencyProperty.Register("EditPlayableArea",
                propertyType: typeof(bool),
                ownerType: typeof(MapView),
                typeMetadata: new FrameworkPropertyMetadata(defaultValue: false, propertyChangedCallback: EditPlayableAreaPropertyChangedCallback));

        public bool EditPlayableArea
        {
            get { return (bool)GetValue(EditPlayableAreaProperty); }
            set
            {
                if ((bool)GetValue(EditPlayableAreaProperty) != value)
                {
                    SetValue(EditPlayableAreaProperty, value);
                    if(playableRect is not null)
                    {
                        playableRect.IsEnabled = value;
                    }
                }
            }
        }

        private static void EditPlayableAreaPropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
        {
            // this is the method that is called whenever the dependency property's value has changed
            if(dependencyObject is MapView mvObject && mvObject.playableRect is not null)
            {
                bool val = (bool)args.NewValue;
                mvObject.playableRect.IsEnabled = val;
                if (val)
                    Panel.SetZIndex(mvObject.playableRect, 10);
                else
                    Panel.SetZIndex(mvObject.playableRect, 0);
                mvObject.EnableDisableIslands(!val);

            }
            
        }
        #endregion

        #region ShowPlayableAreaMargins DependencyProperty
        public static readonly DependencyProperty ShowPlayableAreaMarginsProperty =
             DependencyProperty.Register("ShowPlayableAreaMargins",
                propertyType: typeof(bool),
                ownerType: typeof(MapView),
                typeMetadata: new FrameworkPropertyMetadata(defaultValue: false, propertyChangedCallback: ShowPlayableAreaMarginsPropertyChangedCallback));

        public bool ShowPlayableAreaMargins
        {
            get { return (bool)GetValue(ShowPlayableAreaMarginsProperty); }
            set
            {
                if ((bool)GetValue(ShowPlayableAreaMarginsProperty) != value)
                {
                    SetValue(ShowPlayableAreaMarginsProperty, value);
                    if (playableRect is not null)
                    {
                        playableRect.SetShowPlayableAreaMargins(value);
                    }
                }
            }
        }

        private static void ShowPlayableAreaMarginsPropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
        {
            // this is the method that is called whenever the dependency property's value has changed
            if (dependencyObject is MapView mvObject && mvObject.playableRect is not null)
            {
                bool val = (bool)args.NewValue;
                mvObject.playableRect.SetShowPlayableAreaMargins(val);

            }

        }
        #endregion

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

            UpdateIslands(session);
        }

        private void MapView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateSize();
        }

        private void UpdateIslands(Session? session)
        {
            //Unlink event handlers from old session object
            if(session != this.session)
            {
                if (this.session is not null)
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

                if(session is not null)
                {
                    LinkSessionEventHandlers(session);

                    mapRect = new Rectangle
                    {
                        Fill = new SolidColorBrush(Color.FromArgb(255, 3, 19, 28)),
                        Width = session.Size.X,
                        Height = session.Size.Y
                    };
                    sessionCanvas.Children.Add(mapRect);

                    playableRect = new PlayableAreaControl(session);
                    sessionCanvas.Children.Add(playableRect);

                    // add session islands
                    foreach (var element in session.Elements)
                    {
                        MapElementViewModel viewModel;
                        MapElementControl control;

                        if (element is StartingSpotElement startingSpot)
                        {
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
                }
                
            }

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
                        SelectIslandViewModel selectIslandViewModel = new(session.Region, protoViewModel.Island.IslandType, protoViewModel.IslandSize);
                        selectIslandViewModel.IslandSelected += (s, e) => SelectIsland_IslandSelected(s, e, protoViewModel.Island.Position);

                        OverlayService.Instance.Show(selectIslandViewModel);
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

        public void EnableDisableIslands(bool enable)
        {
            foreach (object item in sessionCanvas.Children)
            {
                if (item is AddIslandButton addIsland)
                {
                    addIsland.IsEnabled = enable;
                }
                else if (item is MapElementControl mapElement)
                {
                    mapElement.IsEnabled = enable;
                }
            }
        }

        private void SelectIsland_IslandSelected(object? sender, IslandSelectedEventArgs e, Vector2 position)
        {
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
                    if (viewModel is IslandViewModel islandViewModel)
                        SelectedIsland = islandViewModel.Island;
                }
            }

            // handle removal of islands
            else if (e.PropertyName == nameof(IslandViewModel.IsOutOfBounds) || e.PropertyName == nameof(DraggingViewModel.IsDragging))
            {
                if (sender is IslandViewModel viewModel)
                {
                    if (viewModel.IsOutOfBounds && !viewModel.IsDragging)
                    {
                        session.Elements.Remove(viewModel.Element);

                        // deselect the island if it was selected
                        if (viewModel == _selectedElement)
                        {
                            _selectedElement = null;
                            SelectedIsland = null;
                        }
                    }
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
            RecalculateIslandCoordinates();
            UpdateSize();
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
//                else if (item is MapElementControl mapElement && mapElement.DataContext is MapElementViewModel mapElementViewModel)
//                {
//                    Vector2 canvasLocation = mapElement.GetPosition();
//                    mapElementViewModel.Position = canvasLocation;
//                }
            }
        }
    }
}
