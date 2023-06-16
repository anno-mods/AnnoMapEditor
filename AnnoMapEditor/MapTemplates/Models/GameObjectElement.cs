using Anno.FileDBModels.Anno1800.Gamedata.Models.Shared.AreaManagerData.GameObjectStructure;
using AnnoMapEditor.Utilities;
using System;

namespace AnnoMapEditor.MapTemplates.Models
{
    public class GameObjectElement : MapElement
    {
        public GameObject GameObject { get; init; }

        public long? Id => GameObject.ID;

        public float? Direction
        {
            get => _direction;
            set => SetProperty(ref _direction, value);
        }
        private float? _direction;


        public GameObjectElement(GameObject gameObject, Vector2 position)
            : base(position)
        {
            GameObject = gameObject;
        }


        // ---- Serialization ----

        public static GameObjectElement FromGameObject(GameObject gameObject)
        {
            if (gameObject.Position == null)
                throw new ArgumentException("GameObject did not contain a position.");

            Vector2 position = new((int)gameObject.Position[2], (int)gameObject.Position[0]);
            return new(gameObject, position)
            {
                Direction = gameObject.Direction
            };
        }
    }
}
