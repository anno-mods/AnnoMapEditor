using Anno.FileDBModels.Anno1800.MapTemplate;
using AnnoMapEditor.DataArchives.Assets.Models;
using AnnoMapEditor.DataArchives.Assets.Repositories;
using AnnoMapEditor.Utilities;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using IslandType = AnnoMapEditor.MapTemplates.Enums.IslandType;

namespace AnnoMapEditor.MapTemplates.Models
{
    public class FixedIslandElement : IslandElement
    {
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


        public FixedIslandElement(IslandAsset islandAsset, IslandType islandType)
            : base(islandType)
        {
            _islandAsset = islandAsset;
        }


        // ---- Serialization ----

        public FixedIslandElement(Element sourceElement)
            : base(sourceElement)
        {
            string mapFilePath = sourceElement.MapFilePath
                ?? throw new ArgumentException($"Missing property '{nameof(Element.MapFilePath)}'.");
            IslandRepository islandRepository = Settings.Instance.IslandRepository
                ?? throw new Exception($"No {nameof(IslandRepository)} could be found.");
            if (!islandRepository.IsLoaded)
                throw new Exception($"The {nameof(IslandRepository)} has not been loaded.");

            if (!islandRepository.TryGetByFilePath(mapFilePath, out var islandAsset))
                throw new NullReferenceException($"Unknown island '{mapFilePath}'.");

            _sourceElement = sourceElement;
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

            _islandAsset       = islandAsset;
            _randomizeRotation = sourceElement.Rotation90 == null;
            _rotation          = sourceElement.Rotation90;
            _sourceElement     = sourceElement;
        }

        protected override void ToTemplate(Element resultElement)
        {
            base.ToTemplate(resultElement);

            resultElement.MapFilePath = _islandAsset.FilePath;
            resultElement.Rotation90  = _randomizeRotation ? null : Rotation;

            // fertilities
            resultElement.RandomizeFertilities = RandomizeFertilities ? null : false;
            if (RandomizeFertilities)
                resultElement.FertilityGuids = Array.Empty<int>();
            else
                resultElement.FertilityGuids = Fertilities.Select(f => (int)f.GUID).ToArray();

            // MineSlotMapping must always be set, but might be empty.
            resultElement.MineSlotMapping = new();

            if (_sourceElement != null)
            {
                // fixed mineslots
                if (_sourceElement.MineSlotMapping != null)
                {
                    resultElement.MineSlotMapping.AddRange(
                            _sourceElement.MineSlotMapping
                            .Select(x => new Tuple<long, int>(x.Item1, x.Item2))
                        );
                }
            }

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
