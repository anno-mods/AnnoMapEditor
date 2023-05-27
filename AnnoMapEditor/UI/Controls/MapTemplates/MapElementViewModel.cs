using AnnoMapEditor.MapTemplates.Models;
using AnnoMapEditor.UI.Controls.Dragging;
using AnnoMapEditor.Utilities;

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


        public MapElementViewModel(MapElement element)
        {
            Element = element;
        }


        public override void OnDragged(Vector2 newPosition)
        {
            Element.Position = newPosition;
        }
    }
}
