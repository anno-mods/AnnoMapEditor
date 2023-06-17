using Anno.FileDBModels.Anno1800.Gamedata.Models.Shared.AreaManagerData.GameObjectStructure;
using AnnoMapEditor.DataArchives.Assets.Models;
using AnnoMapEditor.Utilities;
using FileDBSerializing.EncodingAwareStrings;
using System;

namespace AnnoMapEditor.MapTemplates.Models
{
    public class GameObjectElement : MapElement
    {
        public GameObject GameObject { get; init; }

        public StandardAsset? Asset { get; init; }

        public long? Id => GameObject.ID;

        public float? Direction
        {
            get => _direction;
            set => SetProperty(ref _direction, value);
        }
        private float? _direction;

        public string? Label { get; init; }


        public GameObjectElement(GameObject gameObject, Vector2 position, StandardAsset? asset)
            : base(position)
        {
            GameObject = gameObject;
            Asset = asset;
        }


        // ---- Serialization ----

        public static GameObjectElement FromGameObject(GameObject gameObject, StandardAsset? asset, UTF8String? label)
        {
            if (gameObject.Position == null)
                throw new ArgumentException("GameObject did not contain a position.");

            Vector2 position = new((int)gameObject.Position[2], (int)gameObject.Position[0]);
            return new(gameObject, position, asset)
            {
                Direction = gameObject.Direction,
                Label = label?.ToString()
            }; 
        }
    }
}
