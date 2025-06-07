using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using AnnoMapEditor.MapTemplates.Enums;
using AnnoMapEditor.MapTemplates.Models;
using AnnoMapEditor.UI.Controls.Fertilities;
using AnnoMapEditor.UI.Controls.Slots;
using AnnoMapEditor.Utilities;
using AnnoMapEditor.Utilities.UndoRedo;

namespace AnnoMapEditor.UI.Controls.IslandProperties
{
    /**
     * Unified ViewModel for fixed and random islands. Every property change is done through this ViewModel to ensure
     * it is put onto the Undo/Redo Stack. The main purpose is to make sure that the UI does not directly change any
     * island or map properties. Only Undo and Redo actions should directly change properties.
     */
    public class UnifiedIslandPropertiesViewModel : ObservableBase
    {
        
        /** Valid island sizes depending on the island type */
        private static readonly Dictionary<IslandType, List<IslandSize>> AllowedSizesPerType = new()
        {
            [IslandType.Normal]       = new() { IslandSize.Large, IslandSize.Medium, IslandSize.Small },
            [IslandType.Starter]      = new() { IslandSize.Large, IslandSize.Medium },
            [IslandType.ThirdParty]   = new() { IslandSize.Small },
            [IslandType.PirateIsland] = new() { IslandSize.Small }
        };

        /** Allowed island types depending on the MapElementType */
        private static readonly Dictionary<MapElementType, List<IslandType>> AllowedTypesPerElementType = new()
        {
            [MapElementType.FixedIsland] = new() { IslandType.Normal, IslandType.Starter},
            [MapElementType.PoolIsland]  = new() { IslandType.Normal, IslandType.Starter, IslandType.ThirdParty, IslandType.PirateIsland },
        };

        public MapTemplate? MapTemplate
        {
            get => _mapTemplate;
            private set =>  SetProperty(ref _mapTemplate, value);
        }
        private MapTemplate? _mapTemplate;
        
        public IslandElement? SelectedIsland { get; private set; }
        public string IslandCategoryLabel => GetIslandCategoryLabel();
        public ObservableCollection<IslandType> IslandTypeItems { get; } = new();
        public bool IslandTypeItemsVisible => IslandTypeItems.Count > 1;
        public ObservableCollection<IslandSize> IslandSizeItems { get; } = new();
        public bool IslandSizeItemsVisible => IslandSizeItems.Count > 1;
        public bool IslandRotationPropertiesVisible => (SelectedIsland is FixedIslandElement && 
                                                        (SelectedIsland as FixedIslandElement)?.IslandAsset.IslandSize.FirstOrDefault() != IslandSize.Continental);
        public bool SlotsAndFertilitiesVisible => (SelectedIsland is FixedIslandElement &&
                                                   (SelectedIsland.IslandType == IslandType.Normal ||
                                                    SelectedIsland.IslandType == IslandType.Starter));
        public FertilitiesViewModel? FertilitiesViewModel { get; private set; }
        public SlotsViewModel? SlotsViewModel { get; private set; }
        
        /**
         * Set the Island to be used by the Island Properties.
         */
        public void SetIsland(IslandElement? island, MapTemplate? mapTemplate)
        {
            SelectedIsland = island;
            MapTemplate = mapTemplate;
            
            UpdatePropertyChanges();
            MapTemplateChanged();
            
            OnPropertyChanged(nameof(SlotsAndFertilitiesVisible));
            OnPropertyChanged(nameof(IslandCategoryLabel));
            OnPropertyChanged(nameof(IslandRotationPropertiesVisible));

            if (MapTemplate != null)
                MapTemplate.PropertyChanged += (sender, args) => { MapTemplateChanged(); };
        }

        /**
         * Change Island Type. Checks for allowed size and type combinations. Puts the change on the
         * Undo/Redo Stack.
         */
        public void ChangeIslandType(IslandType newIslandType)
        {
            if (SelectedIsland == null) return;
            
            var oldIslandSize = (SelectedIsland as RandomIslandElement)?.IslandSize ?? null;
            var oldIslandType = SelectedIsland.IslandType;
            
            SelectedIsland.IslandType = newIslandType;

            if (SelectedIsland is RandomIslandElement randomIsland)
            {
                if (randomIsland.IslandType == IslandType.PirateIsland || randomIsland.IslandType == IslandType.ThirdParty)
                    randomIsland.IslandSize = IslandSize.Small;
                if (randomIsland.IslandType == IslandType.Starter && (randomIsland.IslandSize != IslandSize.Large && randomIsland.IslandSize != IslandSize.Medium)) 
                    randomIsland.IslandSize = IslandSize.Medium;
            }
            
            UpdatePropertyChanges();
            
            var newIslandSize = (SelectedIsland as RandomIslandElement)?.IslandSize ?? null;
            
            UndoRedoStack.Instance.Do(new IslandPropertiesStackEntry(
                element:  SelectedIsland,
                oldIslandType: oldIslandType,
                newIslandType: SelectedIsland.IslandType,
                oldIslandSize: oldIslandSize,
                newIslandSize: newIslandSize
            ));
        }

        /**
         * Change Island Size. Checks for allowed size and type combinations. Puts the change on the
         * Undo/Redo Stack.
         */
        public void ChangeIslandSize(IslandSize newIslandSize)
        {
            if (SelectedIsland == null) return;
            
            var oldIslandSize = (SelectedIsland as RandomIslandElement)?.IslandSize ?? null;
            var oldIslandType = SelectedIsland.IslandType;
            
            if (SelectedIsland is RandomIslandElement randomIsland)
                randomIsland.IslandSize = newIslandSize;
            
            UpdatePropertyChanges();
            
            var realNewIslandSize = (SelectedIsland as RandomIslandElement)?.IslandSize ?? null;
            
            UndoRedoStack.Instance.Do(new IslandPropertiesStackEntry(
                element:  SelectedIsland,
                oldIslandType: oldIslandType,
                newIslandType: SelectedIsland.IslandType,
                oldIslandSize: oldIslandSize,
                newIslandSize: realNewIslandSize
            ));
        }

        /**
         * Enable or disable random island rotation and put it on the Undo/Redo Stack.
         */
        public void SetRandomIslandRotation(bool randomRotation)
        {
            if (SelectedIsland is not FixedIslandElement fixedIsland) return;
            
            fixedIsland.RandomizeRotation = randomRotation;

            if (!randomRotation && fixedIsland.Rotation == null)
                fixedIsland.Rotation = 0;
            
            UndoRedoStack.Instance.Do(new IslandPropertiesStackEntry(
                element: fixedIsland,
                randomizeRotation: randomRotation
            ));
        }
        
        /**
         * Rotate the Island and put it on the Undo/Redo Stack.
         */
        public void RotateIsland(bool clockwise)
        {
            if (SelectedIsland is not FixedIslandElement fixedIsland) return;
            
            byte islandRotation = fixedIsland.Rotation ?? 0;
            if (clockwise)
                fixedIsland.Rotation = (byte)((islandRotation - 1) % 4);
            else 
                fixedIsland.Rotation = (byte)((islandRotation + 1) % 4);
            
            UndoRedoStack.Instance.Do(new MapElementTransformStackEntry(
                fixedIsland, 
                fixedIsland.Position, fixedIsland.Position,
                islandRotation, fixedIsland.Rotation
            ));
        }

        /**
         * Notify property changes and update lists of viable sizes and types.
         */
        private void UpdatePropertyChanges()
        {
            IslandTypeItems.Clear();
            IslandSizeItems.Clear();
            if (SelectedIsland is FixedIslandElement fixedIsland)
            {
                if (fixedIsland.IslandType == IslandType.Normal || fixedIsland.IslandType == IslandType.Starter)
                    IslandTypeItems.AddRange(AllowedTypesPerElementType[MapElementType.FixedIsland]);
            }
            else if (SelectedIsland is RandomIslandElement randomIsland)
            {
                IslandTypeItems.AddRange(AllowedTypesPerElementType[MapElementType.PoolIsland]);
                IslandSizeItems.AddRange(AllowedSizesPerType[randomIsland.IslandType]);
            }
            OnPropertyChanged(nameof(IslandTypeItemsVisible));
            OnPropertyChanged(nameof(IslandSizeItemsVisible));
            OnPropertyChanged(nameof(SelectedIsland));
        }

        /**
         * When the map template changes, update te relevant components to reflect the overall map properties within
         * the island properties.
         */
        private void MapTemplateChanged()
        {
            OnPropertyChanged(nameof(MapTemplate));
            if (SelectedIsland is FixedIslandElement fixedIsland && MapTemplate != null)
            {
                FertilitiesViewModel = new(fixedIsland, MapTemplate.Session.Region);
                SlotsViewModel = new(fixedIsland, MapTemplate.Session.Region);
                OnPropertyChanged(nameof(FertilitiesViewModel));
                OnPropertyChanged(nameof(SlotsViewModel));
                System.Diagnostics.Debug.WriteLine("MapTemplate config changed");
            }
        }
        
        /**
         * Returns the label text for the properties section depending on the selected element type.
         */
        private string GetIslandCategoryLabel()
        {
            return (SelectedIsland) switch
            {
                null => "No island selected",
                FixedIslandElement => "Fixed Island Properties",
                RandomIslandElement => "Random Island Properties",
                _ => "Unknown Map Element"
            };
        }
    }
}