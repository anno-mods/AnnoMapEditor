using AnnoMapEditor.Utilities;

namespace AnnoMapEditor.MapTemplates.Models
{
    public abstract class MapElement : ObservableBase
    {
        public Vector2 Position
        {
            get => _position;
            set => SetProperty(ref _position, value);
        }
        private Vector2 _position = Vector2.Zero;


        public MapElement(Vector2 position)
        {
            _position = position;
        }
    }
}
