using AnnoMapEditor.MapTemplates.Models;
using AnnoMapEditor.UI.Controls.Dragging;
using AnnoMapEditor.Utilities;
using System.Windows;

namespace AnnoMapEditor.UI.Controls.MapTemplates
{
    public abstract class MapElementViewModel : DraggingViewModel
    {
        public MapElement Element { get; init; }

        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }
        private bool _isSelected;

        public bool IsOutOfBounds
        {
            get => _isOutOfBounds;
            set => SetProperty(ref _isOutOfBounds, value);
        }
        private bool _isOutOfBounds;


        public MapElementViewModel(MapElement element)
        {
            Element = element;
        }


        public virtual void Move(Point delta) => Move(new Vector2(delta));

        public virtual void Move(Vector2 delta)
        {
            Element.Position += delta;
        }
    }
}
