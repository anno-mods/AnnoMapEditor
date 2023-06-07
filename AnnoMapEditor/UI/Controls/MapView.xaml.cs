using AnnoMapEditor.MapTemplates.Enums;
using AnnoMapEditor.MapTemplates.Models;
using AnnoMapEditor.UI.Controls.AddIsland;
using AnnoMapEditor.UI.Controls.Dragging;
using AnnoMapEditor.UI.Controls.MapTemplates;
using AnnoMapEditor.UI.Controls.Selection;
using AnnoMapEditor.UI.Overlays;
using AnnoMapEditor.UI.Overlays.SelectIsland;
using AnnoMapEditor.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
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


        private MapTemplate? _mapTemplate { get; set; }

        private Rectangle? _mapRect { get; set; }

        private PlayableAreaControl? _playableRect { get; set; }

        private IList<AddIslandButton>? _addIslands { get; set; }

        private Vector2? _oldSize { get; set; }

        #region selecting

        private SelectionBoxControl? _selectionBox;

        private MapElementViewModel? _selectedElement;

        private HashSet<MapElementViewModel> _selectedElements = new();

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

        public static readonly DependencyProperty SelectedIslandProperty =
             DependencyProperty.Register("SelectedIsland",
                propertyType: typeof(IslandElement),
                ownerType: typeof(MapView),
                typeMetadata: new FrameworkPropertyMetadata(defaultValue: null));

        #endregion selecting

        #region playable area

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
                    if (_playableRect is not null)
                    {
                        _playableRect.IsEnabled = value;
                    }
                }
            }
        }

        private static void EditPlayableAreaPropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
        {
            // this is the method that is called whenever the dependency property's value has changed
            if (dependencyObject is MapView mvObject && mvObject._playableRect is not null)
            {
                bool val = (bool)args.NewValue;
                mvObject._playableRect.IsEnabled = val;
                if (val)
                    Panel.SetZIndex(mvObject._playableRect, 10);
                else
                    Panel.SetZIndex(mvObject._playableRect, 0);
                mvObject.EnableDisableControls(!val);
            }

        }

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
                    if (_playableRect is not null)
                    {
                        _playableRect.SetShowPlayableAreaMargins(value);
                    }
                }
            }
        }

        private static void ShowPlayableAreaMarginsPropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
        {
            // this is the method that is called whenever the dependency property's value has changed
            if (dependencyObject is MapView mvObject && mvObject._playableRect is not null)
            {
                bool val = (bool)args.NewValue;
                mvObject._playableRect.SetShowPlayableAreaMargins(val);

            }

        }

        #endregion playable area


        public MapView()
        {
            InitializeComponent();

            SizeChanged += MapView_SizeChanged;
            DataContextChanged += MapView_DataContextChanged;

            Settings.Instance.PropertyChanged += Settings_PropertyChanged;
        }


        private void MapView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            UpdateIslands(DataContext as MapTemplate);
        }

        private void Settings_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (DataContext is not MapTemplate mapTemplate)
                return;

            UpdateIslands(mapTemplate);
        }

        private void MapView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateSize();
        }

        private void UpdateIslands(MapTemplate? mapTemplate)
        {
            //Unlink event handlers from old MapTemplate
            if (mapTemplate != _mapTemplate)
            {
                if (_mapTemplate is not null)
                    UnlinkMapTemplateEventHandlers(_mapTemplate);

                _mapTemplate = mapTemplate;

                mapTemplateCanvas.Children.Clear();
                if (mapTemplate is null)
                {
                    _mapRect = null;
                    _playableRect = null;
                    _addIslands = null;
                    return;
                }

                if (mapTemplate is not null)
                {
                    LinkMapTemplateEventHandlers(mapTemplate);

                    _mapRect = new Rectangle
                    {
                        Fill = new SolidColorBrush(Color.FromArgb(255, 3, 19, 28)),
                        Width = mapTemplate.Size.X,
                        Height = mapTemplate.Size.Y
                    };
                    mapTemplateCanvas.Children.Add(_mapRect);

                    _playableRect = new PlayableAreaControl(mapTemplate);
                    mapTemplateCanvas.Children.Add(_playableRect);

                    // add islands
                    foreach (var element in mapTemplate.Elements)
                    {
                        MapElementViewModel viewModel;
                        MapElementControl control;

                        if (element is StartingSpotElement startingSpot)
                        {
                            viewModel = new StartingSpotViewModel(mapTemplate, startingSpot);
                            control = new StartingSpotControl();
                        }
                        else if (element is RandomIslandElement randomIsland)
                        {
                            viewModel = new RandomIslandViewModel(mapTemplate, randomIsland);
                            control = new IslandControl();
                        }
                        else if (element is FixedIslandElement fixedIsland)
                        {
                            viewModel = new FixedIslandViewModel(mapTemplate, fixedIsland);
                            control = new IslandControl();
                        }
                        else
                            throw new NotImplementedException();

                        viewModel.PropertyChanged += MapElementViewModel_PropertyChanged;
                        viewModel.Dragging += MapElementViewModel_Dragging;
                        viewModel.DragEnded += MapElementViewModel_DragEnded;

                        control.DataContext = viewModel;
                        mapTemplateCanvas.Children.Add(control);
                    }

                    _addIslands = new List<AddIslandButton>();

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
            if (_mapTemplate is null || _addIslands is null) return;

            AddIslandViewModel viewModel = new(mapElementType, type, size);
            AddIslandButton button = new()
            {
                DataContext = viewModel
            };

            viewModel.IslandAdded += AddIslandButton_IslandAdded;

            _addIslands.Add(button);
            mapTemplateCanvas.Children.Add(button);

            MoveAddIsland(button);
        }

        private void MoveAddIsland(AddIslandButton addIsland)
        {
            if (_mapTemplate is null || _addIslands is null) return;

            if (_addIslands.Contains(addIsland) && addIsland.DataContext is AddIslandViewModel island)
            {
                IslandSize size = island.IslandSize;
                IslandType type = island.IslandType;

                int islandLength = IslandSize.Small.DefaultSizeInTiles * 2 + 10 +
                        IslandSize.Medium.DefaultSizeInTiles + 25 +
                        IslandSize.Large.DefaultSizeInTiles + 25;
                int offset = Math.Max(250, (_mapTemplate.Size.Y - islandLength) / 2);

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

                addIsland.SetPosition(_mapTemplate.Size + position);
            }
        }

        private void AddIslandButton_IslandAdded(object? sender, IslandAddedEventArgs e)
        {
            DeselectAllMapElements();

            AddIslandButton addIslandButton = _addIslands!.FirstOrDefault(b => b.DataContext == sender)
                ?? throw new ArgumentException();

            Vector2 addIslandPosition = addIslandButton.GetPosition();
            Vector2 protoIslandPosition = new(addIslandPosition.X + (int)e.Delta.X, addIslandPosition.Y + (int)e.Delta.Y);

            AddProtoIsland(protoIslandPosition, e.MapElementType, e.IslandType, e.IslandSize);

        }

        private void AddProtoIsland(Vector2 position, MapElementType mapElementType, IslandType islandType, IslandSize islandSize)
        {
            ProtoIslandViewModel protoViewModel = new(_mapTemplate, mapElementType, islandType, islandSize, position);
            mapTemplateCanvas.Children.Add(new IslandControl()
            {
                DataContext = protoViewModel
            });

            protoViewModel.DragEnded += ProtoIsland_DragEnded;
            protoViewModel.Dragging += MapElementViewModel_Dragging;
            protoViewModel.IsSelected = true;

            _selectedElements.Add(protoViewModel);
            protoViewModel.BeginDrag(new());
        }

        private void ProtoIsland_DragEnded(object? sender, DragEndedEventArgs e)
        {
            if (sender is ProtoIslandViewModel protoIslandViewModel)
            {
                // if the proto island is within bounds add the new island
                if (!protoIslandViewModel.IsOutOfBounds)
                {
                    // if it is a FixedIsland, let the user select the correct island
                    if (protoIslandViewModel.MapElementType == MapElementType.FixedIsland)
                    {
                        SelectIslandViewModel selectIslandViewModel = new(_mapTemplate.Session.Region, protoIslandViewModel.Island.IslandType, protoIslandViewModel.IslandSize);
                        selectIslandViewModel.IslandSelected += (s, e) => SelectIsland_IslandSelected(s, e, protoIslandViewModel.Island.Position);

                        OverlayService.Instance.Show(selectIslandViewModel);
                    }

                    // otherwise create a random island
                    else if (protoIslandViewModel.MapElementType == MapElementType.PoolIsland)
                    {
                        RandomIslandElement islandElement = new(protoIslandViewModel.IslandSize, protoIslandViewModel.Island.IslandType)
                        {
                            Position = protoIslandViewModel.Island.Position
                        };
                        _mapTemplate!.Elements.Add(islandElement);
                    }
                    else
                        throw new NotImplementedException();
                }

                // remove the ProtoIslandViewModel and its control
                IslandControl protoIslandControl = mapTemplateCanvas.Children.OfType<IslandControl>()
                    .FirstOrDefault(c => c.DataContext == protoIslandViewModel)
                    ?? throw new Exception($"Could not find IslandControl for ProtoIslandViewModel.");

                protoIslandViewModel.DragEnded -= ProtoIsland_DragEnded;
                protoIslandViewModel.Dragging -= MapElementViewModel_Dragging;
                mapTemplateCanvas.Children.Remove(protoIslandControl);
                _selectedElements.Remove(protoIslandViewModel);
            }
        }

        #region selecting

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            // TODO:    base.OnMouseLeftButtonDown(e);      Handle clicking on other elements
            if (_selectionBox != null)
                RemoveSelectionBox();

            Point start = e.GetPosition(mapTemplateCanvas);
            SelectionBoxViewModel selectionBoxViewModel = new SelectionBoxViewModel(start);
            _selectionBox = new()
            {
                DataContext = selectionBoxViewModel
            };

            mapTemplateCanvas.Children.Add(_selectionBox);

            selectionBoxViewModel.BeginDrag(start);
            selectionBoxViewModel.DragEnded += SelectionBoxViewModel_DragEnded;
        }

        private void SelectionBoxViewModel_DragEnded(object? sender, DragEndedEventArgs e)
        {
            SelectionBoxViewModel selectionBoxViewModel = sender as SelectionBoxViewModel
                ?? throw new ArgumentException();

            if (!Keyboard.IsKeyDown(Key.LeftShift))
                DeselectAllMapElements();

            foreach (var control in mapTemplateCanvas.Children)
            {
                if (control is StartingSpotControl startingSpotControl)
                {
                    StartingSpotViewModel startingSpotViewModel = startingSpotControl.DataContext as StartingSpotViewModel
                        ?? throw new Exception();

                    if (selectionBoxViewModel.Contains(startingSpotControl.GetPosition()))
                        startingSpotViewModel.IsSelected = true;
                }

                else if (control is IslandControl islandControl)
                {
                    IslandViewModel islandViewModel = islandControl.DataContext as IslandViewModel
                        ?? throw new Exception();
                    int sizeInTiles = islandViewModel.Island.SizeInTiles;

                    Vector2[] islandCorners = new[]
                    {
                        islandControl.GetPosition(),
                        islandControl.GetPosition() + new Vector2(sizeInTiles, 0),
                        islandControl.GetPosition() + new Vector2(0, sizeInTiles),
                        islandControl.GetPosition() + new Vector2(sizeInTiles, sizeInTiles),
                    };

                    if (islandCorners.Any(c => selectionBoxViewModel.Contains(c)))
                        islandViewModel.IsSelected = true;
                }
            }

            RemoveSelectionBox();
        }

        private void RemoveSelectionBox()
        {
            if (_selectionBox != null)
            {
                SelectionBoxViewModel selectionBoxViewModel = _selectionBox.DataContext as SelectionBoxViewModel
                    ?? throw new ArgumentException();
                selectionBoxViewModel.DragEnded -= SelectionBoxViewModel_DragEnded;
                mapTemplateCanvas.Children.Remove(_selectionBox);
                _selectionBox = null;
            }
        }

        protected override void OnMouseRightButtonUp(MouseButtonEventArgs e)
        {
            DeselectAllMapElements();

            e.Handled = true;
            base.OnMouseRightButtonDown(e);
        }

        #endregion selecting

        private void OnMapElementSelected(MapElementViewModel viewModel)
        {
            // if shift is not pressed, deselect all previously selected elements
            if (!Keyboard.IsKeyDown(Key.LeftShift) && _selectionBox == null)
                DeselectAllMapElements();

            _selectedElements.Add(viewModel);

            if (viewModel is IslandViewModel islandElement)
                SelectedIsland = islandElement.Island;
        }

        private void DeselectAllMapElements()
        {
            foreach (var element in _selectedElements)
                DeselectMapElement(element);
        }

        private void DeselectMapElement(MapElementViewModel viewModel)
        {
            _selectedElements.Remove(viewModel);
            viewModel.IsSelected = false;

            if (viewModel is IslandViewModel islandElement && islandElement.Island == SelectedIsland)
                SelectedIsland = null;
        }


        public void EnableDisableControls(bool enable)
        {
            foreach (object item in mapTemplateCanvas.Children)
            {
                if (item is AddIslandButton addIsland)
                    addIsland.IsEnabled = enable;

                else if (item is MapElementControl mapElement)
                    mapElement.IsEnabled = enable;
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
            _mapTemplate.Elements.Add(fixedIslandElement);
        }

        private void MapElementViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            MapElementViewModel mapElementViewModel = sender as MapElementViewModel
                ?? throw new ArgumentException();

            // handle selection of elements
            if (e.PropertyName == nameof(MapElementViewModel.IsSelected))
            {
                if (mapElementViewModel.IsSelected)
                    OnMapElementSelected(mapElementViewModel);

                else
                    DeselectMapElement(mapElementViewModel);
            }

            // handle removal of islands
            else if (e.PropertyName == nameof(IslandViewModel.IsOutOfBounds) || e.PropertyName == nameof(DraggingViewModel.IsDragging))
            {
                if (sender is IslandViewModel viewModel)
                {
                    if (viewModel.IsOutOfBounds && !viewModel.IsDragging && !_mapTemplate.ResizingInProgress)
                    {
                        _mapTemplate.Elements.Remove(viewModel.Element);

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

        private void MapElementViewModel_Dragging(object? sender, DraggingEventArgs e)
        {
            MapElementViewModel senderViewModel = sender as MapElementViewModel
                ?? throw new ArgumentException(nameof(sender));

            if (senderViewModel.IsSelected)
            {
                foreach (MapElementViewModel mapElementViewModel in _selectedElements)
                {
                    mapElementViewModel.Move(e.Delta);
                }
            }
        }

        private void MapElementViewModel_DragEnded(object? sender, DragEndedEventArgs e)
        {
            ClearOOBIslands();
        }

        void UpdateSize()
        {
            if (_mapTemplate is null)
                return;

            double size = Math.Min(ActualWidth, ActualHeight);
            size = Math.Sqrt((size * size) / 2);

            double requiredScaleX = size / _mapTemplate.Size.X;
            double requiredScaleY = size / _mapTemplate.Size.Y;
            float scale = (float)Math.Min(requiredScaleX, requiredScaleY);

            mapTemplateCanvas.RenderTransform = new ScaleTransform(scale, scale);
            rotationCanvas.Width = scale * _mapTemplate.Size.X;
            rotationCanvas.Height = scale * _mapTemplate.Size.Y;
        }

        private void LinkMapTemplateEventHandlers(MapTemplate mapTemplate)
        {
            mapTemplate.MapSizeConfigChanged += MapElement_MapSizeConfigChanged;
            mapTemplate.MapSizeConfigCommitted += MapElement_MapSizeConfigCommitted;
            mapTemplate.Elements.CollectionChanged += MapElement_ElementsChanged;
        }
        private void UnlinkMapTemplateEventHandlers(MapTemplate mapTemplate)
        {
            mapTemplate.MapSizeConfigCommitted -= MapElement_MapSizeConfigCommitted;
            mapTemplate.MapSizeConfigChanged -= MapElement_MapSizeConfigChanged;
            mapTemplate.Elements.CollectionChanged -= MapElement_ElementsChanged;
        }

        private void MapElement_ElementsChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (var oldItem in e.OldItems)
                {
                    if (oldItem is IslandElement island)
                    {
                        // find the correct control
                        foreach (var child in mapTemplateCanvas.Children)
                        {
                            if (child is IslandControl control
                                && control.DataContext is IslandViewModel viewModel
                                && viewModel.Element == island)
                            {
                                viewModel.PropertyChanged -= MapElementViewModel_PropertyChanged;
                                mapTemplateCanvas.Children.Remove(control);

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
                        viewModel = new RandomIslandViewModel(_mapTemplate, randomIsland);
                    else if (newItem is FixedIslandElement fixedIsland)
                        viewModel = new FixedIslandViewModel(_mapTemplate, fixedIsland);
                    else
                        continue;

                    mapTemplateCanvas.Children.Add(new IslandControl()
                    {
                        DataContext = viewModel
                    });

                    viewModel.PropertyChanged += MapElementViewModel_PropertyChanged;
                    viewModel.Dragging += MapElementViewModel_Dragging;
                    viewModel.DragEnded += MapElementViewModel_DragEnded;
                }
            }
        }

        private void MapElement_MapSizeConfigChanged(object? sender, MapTemplate.MapTemplateResizeEventArgs args)
        {
            if (sender is MapTemplate mapTemplate)
            {
                if (_oldSize is null)
                    _oldSize = new Vector2(args.OldMapSize);

                if (_mapRect is not null)
                {
                    _mapRect.Width = mapTemplate.Size.X;
                    _mapRect.Height = mapTemplate.Size.Y;

                    Canvas.SetBottom(_mapRect, -mapTemplate.Size.Y);
                }

                bool sizeIncrease = mapTemplate.Size.X > _oldSize.X;

                if (sizeIncrease)
                {
                    UpdateSize();
                    //Always keep AddIsland controls outside map area
                    RecalculateAddIslandCoordinates();
                }
                else
                {
                    MarkOOBIslands();
                }
            }
        }

        private void MapElement_MapSizeConfigCommitted(object? sender, EventArgs _)
        {
            _oldSize = null;
            RecalculateAddIslandCoordinates();
            UpdateSize();
            KeepStartingSpotsInBounds();
            ClearOOBIslands();
        }

        private void RecalculateAddIslandCoordinates()
        {
            if (_mapTemplate is null) return;

            foreach (object item in mapTemplateCanvas.Children)
            {
                if (item is AddIslandButton addIsland)
                {
                    MoveAddIsland(addIsland);
                }
            }
        }

        private void MarkOOBIslands()
        {
            if (_mapTemplate is null) return;

            foreach (object item in mapTemplateCanvas.Children)
            {
                if (item is IslandControl mapIsland && mapIsland.DataContext is IslandViewModel islandViewModel)
                {
                    islandViewModel.BoundsCheck();
                }
            }
        }

        private void ClearOOBIslands()
        {
            if (_mapTemplate is null) return;

            List<UIElement> childrenCopy = mapTemplateCanvas.Children.Cast<UIElement>().ToList();
            foreach (object item in childrenCopy)
            {
                if (item is IslandControl mapIsland && mapIsland.DataContext is IslandViewModel islandViewModel && islandViewModel.IsOutOfBounds)
                {
                    _mapTemplate.Elements.Remove(islandViewModel.Element);

                    // deselect the island if it was selected
                    if (islandViewModel == _selectedElement)
                    {
                        _selectedElement = null;
                        SelectedIsland = null;
                    }
                }
            }
        }

        private void KeepStartingSpotsInBounds()
        {
            if (_mapTemplate is null) return;

            foreach (object item in mapTemplateCanvas.Children)
            {
                if (item is StartingSpotControl start && start.DataContext is StartingSpotViewModel startViewModel)
                {
                    startViewModel.Element.Position = startViewModel.Element.Position.Clamp(_mapTemplate.PlayableArea);
                }
            }
        }
    }
}
