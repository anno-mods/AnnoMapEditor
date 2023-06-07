using Anno.FileDBModels.Anno1800.MapTemplate;
using AnnoMapEditor.DataArchives.Assets.Models;
using AnnoMapEditor.DataArchives.Assets.Repositories;
using AnnoMapEditor.MapTemplates.Enums;
using AnnoMapEditor.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using IslandType = AnnoMapEditor.MapTemplates.Enums.IslandType;

namespace AnnoMapEditor.MapTemplates.Models
{
    public class FixedIslandElement : IslandElement
    {
        private static readonly Logger<FixedIslandElement> _logger = new();


        // TODO: Deserialize Fertilities and MineSlotMappings instead of copying from the source template.
        // TODO: Remove _sourceTemplate alltogether
        private readonly Element? _sourceElement;

        public IslandAsset IslandAsset 
        { 
            get => _islandAsset;
            [MemberNotNull(nameof(_islandAsset))]
            private set
            {
                SetProperty(ref _islandAsset!, value);
                SizeInTiles = _islandAsset.SizeInTiles;
            }
        }
        private IslandAsset _islandAsset;


        public bool RandomizeRotation
        {
            get => _randomizeRotation;
            set {
                SetProperty(ref _randomizeRotation, value);

                if (!value && Rotation == null)
                    Rotation = 0;
            }
        }
        private bool _randomizeRotation = true;

        public byte? Rotation
        {
            get => _rotation;
            set 
            {
                SetProperty(ref _rotation, value != null ? (byte)(value % 4) : null);
                SetProperty(ref _randomizeRotation, value == null);
            }
        }
        private byte? _rotation;

        public bool RandomizeFertilities
        {
            get => _randomizeFertilities;
            set => SetProperty(ref _randomizeFertilities, value);
        }
        private bool _randomizeFertilities = true;

        public ObservableCollection<FertilityAsset> Fertilities { get; init; } = new();

        public bool RandomizeSlots
        {
            get => _randomizeSlots;
            set => SetProperty(ref _randomizeSlots, value);
        }
        private bool _randomizeSlots = true;

        public Dictionary<long, SlotAssignment> SlotAssignments { get; init; } = new();

        /// <summary>
        /// For tracking if the island needs yet to be loaded.
        /// </summary>
        public bool DelayedLoading
        {
            get => _delayedLoading;
            private set => SetProperty(ref _delayedLoading, value);
        }
        private bool _delayedLoading = false;

        /// <summary>
        /// Used for tracking the original label during delayed loading.
        /// </summary>
        private string? prevLabel;


        public FixedIslandElement(IslandAsset islandAsset, IslandType islandType)
            : base(islandType)
        {
            IslandAsset = islandAsset;

            foreach (Slot slot in islandAsset.Slots.Values)
            {
                SlotAssignments.Add(slot.ObjectId, new()
                {
                    Slot = slot,
                    AssignedSlot = null
                });
            }
        }


        // ---- Serialization ----

        public FixedIslandElement(Element sourceElement) : base(sourceElement)
        {
            _sourceElement = sourceElement;

            string islandFilePath = sourceElement.MapFilePath
                ?? throw new ArgumentException($"Missing property '{nameof(Element.MapFilePath)}'.");

            _randomizeRotation = sourceElement.Rotation90 == null;
            _rotation = sourceElement.Rotation90;

            System.Diagnostics.Debug.WriteLineIf(sourceElement.Rotation90 != null, $"Reading source data {sourceElement.Rotation90} to rotation value of {_rotation} on path {islandFilePath}.");

            _randomizeFertilities = sourceElement.RandomizeFertilities != false;
            _randomizeSlots = sourceElement.MineSlotMapping == null || sourceElement.MineSlotMapping.Count == 0;

            if (Settings.Instance.IsLoading)
            {
                DelayedLoading = true;
                //Show island file if not labelled, while still loading
                prevLabel = Label;
                if (string.IsNullOrEmpty(Label))
                {
                    Label = System.IO.Path.GetFileNameWithoutExtension(islandFilePath);
                }
                SetDummyAsset(sourceElement);
                System.Diagnostics.Debug.WriteLine($"Queueing item {sourceElement!.MapFilePath} for LoadingFinished.");
                Settings.Instance.LoadingFinished += DelayedLoadAssetData;
            }
            else
            {
                LoadIslandDataFromRepository(sourceElement, false);
            }
            
        }

        /// <summary>
        /// Loads the Asset data from the IslandRepository. 
        /// Should only be called when the IslandRepository is actually loaded, errors otherwise.
        /// </summary>
        /// <param name="sourceElement">The element from the template.</param>
        /// <param name="delayed">Whether this is a delayed loading call (which means cleanup needs to be done after loading).</param>
        /// <exception cref="ArgumentException">The sourceElement does not have a MapFilePath. This is forbidden on FixedIslands.</exception>
        /// <exception cref="Exception">The IslandRepository either does not exist or has not finished loading yet.</exception>
        /// <exception cref="NullReferenceException">The given MapFilePath does not match any islands in the IslandRepository.</exception>
        [MemberNotNull(nameof(_islandAsset))]
        private void LoadIslandDataFromRepository(Element sourceElement, bool delayed)
        {
            string islandFilePath = sourceElement.MapFilePath
                ?? throw new ArgumentException($"Missing property '{nameof(Element.MapFilePath)}'.");
            IslandRepository islandRepository = Settings.Instance.IslandRepository
                ?? throw new Exception($"No {nameof(IslandRepository)} could be found.");
            if (!islandRepository.IsLoaded)
                throw new Exception($"The {nameof(IslandRepository)} has not been loaded.");

            if (!islandRepository.TryGetByFilePath(islandFilePath, out var islandAsset))
                throw new NullReferenceException($"Unknown island '{islandFilePath}'.");

            IslandAsset = islandAsset;
            //Rotation is not asset bound, thus loaded in constructor

            AssetRepository assetRepository = Settings.Instance.AssetRepository
                ?? throw new Exception($"The {nameof(AssetRepository)} has not been loaded.");

            // fertilities
            //  _randomizeFertilities is loaded in constructor.
            if (sourceElement.FertilityGuids != null)
            {
                foreach (int guid in sourceElement.FertilityGuids)
                {
                    if (assetRepository.TryGet(guid, out FertilityAsset? fertility) && fertility != null)
                        Fertilities.Add(fertility);
                    else
                        throw new Exception($"Unrecognized {nameof(FertilityAsset)} for GUID {guid}.");
                }
            }

            // fixed slots
            // _randomizeSlots is loaded in constructor.
            if (sourceElement.MineSlotMapping != null)
            {
                foreach ((long objectId, int slotGuid) in sourceElement.MineSlotMapping)
                {
                    // skip unsupported slots
                    if (!islandAsset.Slots.TryGetValue(objectId, out Slot? slot))
                    {
                        _logger.LogWarning($"Unrecognized {nameof(Slot)} id {objectId} on instance of '{islandFilePath}'. The slot will be skipped and the map may be corrupted.");
                        continue;
                    }

                    SlotAsset? slotAsset;

                    // 0 denotes an empty slot
                    if (slotGuid == 0)
                        slotAsset = null;

                    else if (!assetRepository.TryGet(slotGuid, out slotAsset))
                    {
                        _logger.LogWarning($"Unrecognized {nameof(SlotAsset)} GUID {slotGuid} on instance of '{islandFilePath}'. The slot will be skipped and the map may be corrupted.");
                        continue;
                    }

                    SlotAssignments.Add(objectId, new()
                    {
                        Slot = slot,
                        AssignedSlot = slotAsset
                    });
                }
            }

            // add remaining assignable slots
            foreach (Slot slot in islandAsset.Slots.Values)
            {
                if (!SlotAssignments.ContainsKey(slot.ObjectId) && slot.SlotAsset != null)
                {
                    SlotAssignments.Add(slot.ObjectId, new()
                    {
                        Slot = slot,
                        AssignedSlot = null
                    });
                }
            }

            //Deregister when delayed loading
            if (delayed)
            {
                Settings.Instance.LoadingFinished -= DelayedLoadAssetData;

                if(Label != prevLabel)
                {
                    Label = prevLabel;
                }

                DelayedLoading = false;
            }
        }

        /// <summary>
        /// Creates an empty IslandAsset that gets replaced with the correct one as soon as the Repository is loaded.
        /// </summary>
        /// <param name="sourceElement">The element from the template.</param>
        /// <exception cref="ArgumentException">The sourceElement does not have a MapFilePath. This is forbidden on FixedIslands.</exception>
        [MemberNotNull(nameof(_islandAsset))]
        private void SetDummyAsset(Element sourceElement)
        {
            string islandFilePath = sourceElement.MapFilePath
                ?? throw new ArgumentException($"Missing property '{nameof(Element.MapFilePath)}'.");

            IslandSize islandSize = IslandRepository.DetectDefaultIslandSizeFromPath(islandFilePath);
            IslandDifficulty? islandDifficulty = IslandDifficulty.FromElementValue(sourceElement.Difficulty?.id);


            IslandAsset dummyAsset = new IslandAsset()
            {
                FilePath = islandFilePath,
                DisplayName = System.IO.Path.GetFileNameWithoutExtension(islandFilePath),
                Thumbnail = null,
                Region = RegionAsset.DetectFromPath(islandFilePath),
                IslandDifficulty =  new[] { islandDifficulty },
                IslandType = new[] { IslandRepository.DetectIslandTypeFromPath(islandFilePath) },
                IslandSize = new[] { islandSize },
                SizeInTiles = Math.Min(islandSize.DefaultSizeInTiles, 768),
                Slots = new Dictionary<long, Slot>()
            };

            IslandAsset = dummyAsset;
        }

        private void DelayedLoadAssetData(object? sender, EventArgs _)
        {
            LoadIslandDataFromRepository(_sourceElement!, true);
        }

        protected override void ToTemplate(Element resultElement)
        {
            base.ToTemplate(resultElement);

            resultElement.MapFilePath = _islandAsset.FilePath;
            resultElement.Rotation90  = _randomizeRotation ? null : Rotation;

            //
            // Randomization of fertilities is controlled by the flag RandomizeFertilities. Instead
            // of using true/false, true is replaced by null.
            //
            // Randomized fertilities:
            //   RandomizeFertilities = null
            //   FertilityGuids = []
            //
            // Fixed Fertilities:
            //   RandomizedFertilities = false
            //   FertilityGuids = [123456, ...]
            //
            resultElement.RandomizeFertilities = _randomizeFertilities ? null : false;
            if (_randomizeFertilities)
                resultElement.FertilityGuids = Array.Empty<int>();
            else
                resultElement.FertilityGuids = Fertilities.Select(f => (int)f.GUID).ToArray();

            //
            // There exists no additional "RandomizeSlots" flag in the templates. Instead slots
            // will be randomized if MineSlotMapping is an empty list. If it contains any elements,
            // all slots will be fixed.
            //
            // 0 is used to set empty slots.
            //
            // Randomized slots:
            //   MineSlotMapping = []
            // Fixed slots
            //   MineSlotMapping = [(8252351, 1000063), (162612, 0), ...]
            //
            if (_randomizeSlots)
                resultElement.MineSlotMapping = new();
            else
                resultElement.MineSlotMapping = IslandAsset.Slots.Values
                    .Select(s =>
                    {
                        int slotGuid = 0;
                        if (SlotAssignments.TryGetValue(s.ObjectId, out SlotAssignment? assignment))
                            slotGuid = (int)(assignment.AssignedSlot?.GUID ?? 0);

                        return new Tuple<long, int>(s.ObjectId, slotGuid);
                    })
                    .ToList();

            // despite its name, all fixed islands must have a RandomIslandConfig.
            resultElement.RandomIslandConfig = new()
            {
                value = new()
                {
                    Type = new() { id = IslandType.ElementValue },
                    Difficulty = new() { id = IslandDifficulty?.ElementValue }
                }
            };
        }
    }
}
