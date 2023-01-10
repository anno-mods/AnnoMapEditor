using Anno.FileDBModels.Anno1800.MapTemplate;
using AnnoMapEditor.DataArchives.Assets.Models;
using System;
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

        public byte? Rotation
        {
            get => _rotation;
            set => SetProperty(ref _rotation, value != null ? (byte)(value % 4) : null);
        }
        private byte? _rotation;


        public FixedIslandElement(IslandAsset islandAsset, IslandType islandType)
            : base(islandType)
        {
            _islandAsset = islandAsset;
        }


        // ---- Serialization ----

        public FixedIslandElement(Element sourceElement)
            : base(sourceElement)
        {
            throw new NotImplementedException();

//            _islandAsset   = sourceElement.MapFilePath
//                ?? throw new ArgumentException($"Missing property '{nameof(Element.MapFilePath)}'.");
//            _rotation      = sourceElement.Rotation90;
//            _sourceElement = sourceElement;
        }

        protected override void ToTemplate(Element resultElement)
        {
            base.ToTemplate(resultElement);

            resultElement.MapFilePath = _islandAsset.FilePath;
            resultElement.Rotation90  = Rotation;

            // MineSlotMapping must always be set, but might be empty.
            resultElement.MineSlotMapping = new();

            if (_sourceElement != null)
            {
                // fixed fertilities
                if (_sourceElement.FertilityGuids != null)
                {
                    resultElement.FertilityGuids = new int[_sourceElement.FertilityGuids.Length];
                    Array.Copy(_sourceElement.FertilityGuids, resultElement.FertilityGuids, _sourceElement.FertilityGuids.Length);
                }

                // randomized fertilities
                resultElement.RandomizeFertilities = _sourceElement.RandomizeFertilities;

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
