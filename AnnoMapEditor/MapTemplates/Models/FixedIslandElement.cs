using Anno.FileDBModels.Anno1800.MapTemplate;
using AnnoMapEditor.DataArchives.Assets.Models;
using AnnoMapEditor.DataArchives.Assets.Repositories;
using AnnoMapEditor.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
            set => SetProperty(ref _islandAsset, value);
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


        public FixedIslandElement(IslandAsset islandAsset, IslandType islandType)
            : base(islandType)
        {
            _islandAsset = islandAsset;

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

        public FixedIslandElement(Element sourceElement)
            : base(sourceElement)
        {
            string islandFilePath = sourceElement.MapFilePath
                ?? throw new ArgumentException($"Missing property '{nameof(Element.MapFilePath)}'.");
            IslandRepository islandRepository = Settings.Instance.IslandRepository
                ?? throw new Exception($"No {nameof(IslandRepository)} could be found.");
            if (!islandRepository.IsLoaded)
                throw new Exception($"The {nameof(IslandRepository)} has not been loaded.");

            if (!islandRepository.TryGetByFilePath(islandFilePath, out var islandAsset))
                throw new NullReferenceException($"Unknown island '{islandFilePath}'.");

            _islandAsset       = islandAsset;
            _randomizeRotation = sourceElement.Rotation90 == null;
            _rotation          = sourceElement.Rotation90;
            _sourceElement     = sourceElement;

            AssetRepository assetRepository = Settings.Instance.AssetRepository
                ?? throw new Exception($"The {nameof(AssetRepository)} has not been loaded.");

            // fertilities
            _randomizeFertilities = sourceElement.RandomizeFertilities != false;
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
            _randomizeSlots = sourceElement.MineSlotMapping == null || sourceElement.MineSlotMapping.Count == 0;
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
