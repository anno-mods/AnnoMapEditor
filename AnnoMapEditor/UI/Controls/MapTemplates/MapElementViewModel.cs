using AnnoMapEditor.MapTemplates.Models;
using AnnoMapEditor.UI.Controls.Dragging;
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


        public MapElementViewModel(MapElement element)
        {
            Element = element;
        }


        public void Move(Point delta)
        {
            Element.Position = new(Element.Position.X + (int)delta.X, Element.Position.Y + (int)delta.Y);
        }
    }
}
