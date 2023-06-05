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
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace AnnoMapEditor.UI.Controls
{
    public partial class MapView : UserControl
    {
        public static readonly double MAP_ROTATION_ANGLE = -135;


        private MapTemplate? _mapTemplate { get; set; }
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
            //Unlink event handlers from old map template object
            if(mapTemplate != this._mapTemplate)
            {
                if (_mapTemplate is not null)
                    UnlinkMapTemplateEventHandlers(_mapTemplate);

                _mapTemplate = mapTemplate;

                mapTemplateCanvas.Children.Clear();
                if (mapTemplate is null)
                {
                    mapRect = null;
                    playableRect = null;
                    AddIslands = null;
                    return;
                }

                if(mapTemplate is not null)
                {
                    LinkMapTemplateEventHandlers(mapTemplate);

                    mapRect = new Rectangle
                    {
                        Fill = new SolidColorBrush(Color.FromArgb(255, 3, 19, 28)),
                        Width = mapTemplate.Size.X,
                        Height = mapTemplate.Size.Y
                    };
                    mapTemplateCanvas.Children.Add(mapRect);

                    playableRect = new PlayableAreaControl(mapTemplate);
                    mapTemplateCanvas.Children.Add(playableRect);

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

                        control.DataContext = viewModel;
                        mapTemplateCanvas.Children.Add(control);
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
            if (_mapTemplate is null || AddIslands is null) return;

            AddIslandViewModel viewModel = new(mapElementType, type, size);
            AddIslandButton button = new()
            {
                DataContext = viewModel
            };

            viewModel.IslandAdded += IslandAdded;

            AddIslands.Add(button);
            mapTemplateCanvas.Children.Add(button);

            MoveAddIsland(button);
        }

        private void MoveAddIsland(AddIslandButton addIsland)
        {
            if (_mapTemplate is null || AddIslands is null) return;

            if (AddIslands.Contains(addIsland) && addIsland.DataContext is AddIslandViewModel island)
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

        private void IslandAdded(object? sender, IslandAddedEventArgs e)
        {
            ProtoIslandViewModel protoViewModel = new(_mapTemplate, e.MapElementType, e.IslandType, e.IslandSize, e.Position);
            mapTemplateCanvas.Children.Add(new IslandControl()
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
                        SelectIslandViewModel selectIslandViewModel = new(_mapTemplate.Region, protoViewModel.Island.IslandType, protoViewModel.IslandSize);
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
                        _mapTemplate!.Elements.Add(islandElement);
                    }
                    else
                        throw new NotImplementedException();
                }

                // remove the proto island
                foreach (var child in mapTemplateCanvas.Children)
                {
                    if (child is IslandControl islandControl && islandControl.DataContext == protoViewModel)
                    {
                        protoViewModel.DragEnded -= ProtoIsland_DragEnded;
                        mapTemplateCanvas.Children.Remove(islandControl);
                        break;
                    }
                }
            }
        }

        public void EnableDisableIslands(bool enable)
        {
            foreach (object item in mapTemplateCanvas.Children)
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
            _mapTemplate.Elements.Add(fixedIslandElement);
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
            mapTemplate.MapSizeConfigChanged += HandleMapTemplateResized;
            mapTemplate.MapSizeConfigCommitted += HandleMapTemplateSizeCommitted;
            mapTemplate.Elements.CollectionChanged += MapTemplate_ElementsChanged;
        }
        private void UnlinkMapTemplateEventHandlers(MapTemplate mapTemplate)
        {
            mapTemplate.MapSizeConfigCommitted -= HandleMapTemplateSizeCommitted;
            mapTemplate.MapSizeConfigChanged -= HandleMapTemplateResized;
            mapTemplate.Elements.CollectionChanged -= MapTemplate_ElementsChanged;
        }

        private void MapTemplate_ElementsChanged(object? sender, NotifyCollectionChangedEventArgs e)
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
//                    viewModel.IsSelected = true;
//                    viewModel.BeginDrag(Vector2.Zero);
                }
            }
        }

        private void HandleMapTemplateResized(object? sender, MapTemplate.MapTemplateResizeEventArgs args)
        {
            if(sender is MapTemplate mapTemplate)
            {
                if (oldSize is null)
                    oldSize = new Vector2(args.OldMapSize);

                if(mapRect is not null)
                {
                    mapRect.Width = mapTemplate.Size.X;
                    mapRect.Height = mapTemplate.Size.Y;

                    Canvas.SetBottom(mapRect, -mapTemplate.Size.Y);
                }

                bool sizeIncrease = mapTemplate.Size.X > oldSize.X;

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

        private void HandleMapTemplateSizeCommitted(object? sender, EventArgs _)
        {
            oldSize = null;
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
