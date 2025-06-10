using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using AnnoMapEditor.DataArchives.Assets.Models;
using AnnoMapEditor.MapTemplates.Enums;
using AnnoMapEditor.MapTemplates.Models;
using AnnoMapEditor.UI.Controls.Slots;
using AnnoMapEditor.UI.Overlays.SelectFertilities;
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

        public UnifiedIslandPropertiesViewModel(IslandElement selectedIsland, MapTemplate mapTemplate)
        {
            // Assign Island and MapTemplate
            SelectedIsland = selectedIsland;
            MapTemplate = mapTemplate;
            
            SelectedIsland.PropertyChanged += SelectedIsland_PropertyChanged;
            MapTemplate.PropertyChanged += SelectedIsland_PropertyChanged;
            if (SelectedIsland is FixedIslandElement fixedIsland)
            {
                fixedIsland.Fertilities.CollectionChanged += Fertilities_CollectionChanged;
                SlotsViewModel = new SlotsViewModel(fixedIsland, MapTemplate.Session.Region);
            }
            
            UpdatePropertyChanges();
            UpdateSelectedFertilities();
        }
        
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
        
        /**
         * Properties used for UI representation:
         */
        public string IslandCategoryLabel => GetIslandCategoryLabel();
        public ObservableCollection<IslandSize> IslandSizeItems { get; } = new();
        public bool IslandSizeItemsVisible => IslandSizeItems.Count > 1;
        public ObservableCollection<IslandType> IslandTypeItems { get; } = new();
        public bool IslandTypeItemsVisible => IslandTypeItems.Count > 1;
        public bool RandomizeRotation
        {
            get => (SelectedIsland as FixedIslandElement)?.RandomizeRotation ?? true;
            set
            {
                SetRandomIslandRotation(value);
                OnPropertyChanged();
            }
        }
        public byte? IslandRotation => (SelectedIsland as FixedIslandElement)?.Rotation;
        public bool IslandRotationPropertiesVisible => (SelectedIsland is FixedIslandElement element && 
                                                        element.IslandAsset.IslandSize.FirstOrDefault() != IslandSize.Continental);
        public bool RandomizeFertilities
        {
            get => (SelectedIsland as FixedIslandElement)?.RandomizeFertilities ?? true;
            set
            {
                if (SelectedIsland is FixedIslandElement fixedIsland)
                {
                    fixedIsland.RandomizeFertilities = value;
                    UndoRedoStack.Instance.Do(new IslandPropertiesStackEntry(fixedIsland, randomizeFertilities: value));
                }
                OnPropertyChanged();
            }
        }
        public ObservableCollection<SelectFertilityItem> FertilityItems { get; private set; } = new();
        public bool SlotsAndFertilitiesVisible => (SelectedIsland is FixedIslandElement &&
                                                   (SelectedIsland.IslandType == IslandType.Normal ||
                                                    SelectedIsland.IslandType == IslandType.Starter));
        public bool AllowedFertilitiesWarning => FertilityItems.Any(f => f is { IsAllowed: false, IsSelected: true })
                                                 && !RandomizeFertilities;
        public SlotsViewModel? SlotsViewModel { get; private set; }

        public IslandSize? IslandSize => (SelectedIsland as RandomIslandElement)?.IslandSize;
        
        
        /**
         * Properties only set inside viewmodel
         */
        private  MapTemplate MapTemplate { get; }
        public IslandElement SelectedIsland { get; }

        /**
         * Change Island Type. Checks for allowed size and type combinations. Puts the change on the
         * Undo/Redo Stack.
         */
        public void ChangeIslandType(IslandType newIslandType)
        {
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
        private void SetRandomIslandRotation(bool randomRotation)
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
            
            var islandRotation = fixedIsland.Rotation ?? 0;
            fixedIsland.Rotation = clockwise ? (byte)((islandRotation - 1) % 4) : (byte)((islandRotation + 1) % 4);
            
            UndoRedoStack.Instance.Do(new MapElementTransformStackEntry(
                fixedIsland, 
                fixedIsland.Position, fixedIsland.Position,
                islandRotation, fixedIsland.Rotation
            ));
        }

        /**
         * Callback for island and map template property changes
         */
        private void SelectedIsland_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            // Clear the items to have a clean sorting order when making changes to the session
            if (e.PropertyName == nameof(MapTemplate.Session))
                FertilityItems.Clear(); 
            UpdatePropertyChanges();
            UpdateSelectedFertilities();
        }
        
        /**
         * Notify property changes and update lists of viable sizes and types.
         */
        private void UpdatePropertyChanges()
        {
            IslandTypeItems.Clear();
            IslandSizeItems.Clear();
            switch (SelectedIsland)
            {
                case FixedIslandElement fixedIsland:
                {
                    if (fixedIsland.IslandType == IslandType.Normal || fixedIsland.IslandType == IslandType.Starter)
                        IslandTypeItems.AddRange(fixedIsland.IslandAsset.IslandType);
                    break;
                }
                case RandomIslandElement randomIsland:
                    IslandTypeItems.AddRange(AllowedTypesPerElementType[MapElementType.PoolIsland]);
                    IslandSizeItems.AddRange(AllowedSizesPerType[randomIsland.IslandType]);
                    break;
            }
            OnPropertyChanged(nameof(SelectedIsland));
            OnPropertyChanged(nameof(IslandTypeItemsVisible));
            OnPropertyChanged(nameof(IslandSizeItemsVisible));
            OnPropertyChanged(nameof(RandomizeFertilities));
            OnPropertyChanged(nameof(RandomizeRotation));
            OnPropertyChanged(nameof(IslandSize));
            OnPropertyChanged(nameof(IslandRotation));
        }

        /**
         * Set a specific fertility to a new state. Passed to fertility selector as callback.
         */
        private void SetFertility(FertilityAsset fertility, bool isSelected)
        {
            if (SelectedIsland is not FixedIslandElement fixedIsland) return;

            if (fixedIsland.Fertilities.Contains(fertility) && !isSelected)
            {
                // removing a fertility:
                fixedIsland.Fertilities.Remove(fertility);
                UndoRedoStack.Instance.Do(new IslandFertilitiesStackEntry(fixedIsland, false, fertility));
            }
            else if (!fixedIsland.Fertilities.Contains(fertility) && isSelected)
            {
                // adding a fertility:
                fixedIsland.Fertilities.Add(fertility);
                UndoRedoStack.Instance.Do(new IslandFertilitiesStackEntry(fixedIsland, true, fertility));
            }
        }

        /**
         * Callback for changed fertilities.
         */
        private void Fertilities_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            UpdateSelectedFertilities();
        }
        
        /**
         * Returns the label text for the properties section depending on the selected element type.
         */
        private string GetIslandCategoryLabel()
        {
            return (SelectedIsland) switch
            {
                FixedIslandElement => "Fixed Island Properties",
                RandomIslandElement => "Random Island Properties",
                _ => "Unknown Map Element"
            };
        }

        /**
         * Update the List of selectable fertilities depending on the map templates region.
         */
        private void UpdateSelectedFertilities()
        {
            if (SelectedIsland is not FixedIslandElement fixedIsland) return;
            var allowedFertilities = MapTemplate.Session.Region.AllowedFertilities
                .Distinct()
                .ToList();
                
            // Update selection and allowance of each selector.
            // also remove fertility selector if it is not allowed or not selected. Keep unallowed if it has been 
            // selected before. It will be marked with a warning.
            foreach (var fertilityItem in FertilityItems.ToList())
            {
                fertilityItem.IsSelected = fixedIsland.Fertilities.Contains(fertilityItem.FertilityAsset);
                fertilityItem.IsAllowed = allowedFertilities.Contains(fertilityItem.FertilityAsset);
                if (fertilityItem is { IsAllowed: false, IsSelected: false })
                    FertilityItems.Remove(fertilityItem);
            }
                
            // If a fertility is not contained in the items list yet, add it. 
            foreach (var fertility in allowedFertilities)
                AddFertilitySelectors(fertility, fixedIsland, allowedFertilities);
                
            foreach (var fertility in fixedIsland.Fertilities)
                AddFertilitySelectors(fertility, fixedIsland, allowedFertilities);
                
            // Update the warning property to show hint
            OnPropertyChanged(nameof(AllowedFertilitiesWarning));
        }

        /**
         * Check if a valid fertility selector is already in the list, if not, add it.
         */
        private void AddFertilitySelectors(FertilityAsset allowedFertility, FixedIslandElement fixedIsland, List<FertilityAsset> allowedFertilities)
        {
            if (FertilityItems.Any(f => f.FertilityAsset == allowedFertility)) return;
            SelectFertilityItem newItem = new(SetFertility)
            {
                FertilityAsset = allowedFertility,
                IsSelected = fixedIsland.Fertilities.Contains(allowedFertility),
                IsAllowed = allowedFertilities.Contains(allowedFertility)
            };
            FertilityItems.Add(newItem);
        }
    }
}