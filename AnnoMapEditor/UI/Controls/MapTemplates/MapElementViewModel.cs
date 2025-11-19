using Anno.FileDBModels.Anno1800.MapTemplate;
using AnnoMapEditor.MapTemplates.Models;
using AnnoMapEditor.UI.Controls.Dragging;
using AnnoMapEditor.Utilities;
using AnnoMapEditor.Utilities.UndoRedo;
using System.Runtime.CompilerServices;
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
            PropertyChanged += Element_TransformationUndo;
        }


        public virtual void Move(Point delta)
        {
            Element.Position = new(Element.Position.X + (int)delta.X, Element.Position.Y + (int)delta.Y);
        }

        public Vector2? DragStartPosition;

        private void Element_TransformationUndo(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IsDragging))
            {
                if (IsDragging) 
                { 
                    DragStartPosition = Element.Position;
                }
                else
                {
                    Vector2 currentPosition = Element.Position;
                    if (
                        (currentPosition != null && DragStartPosition != null && !Vector2.Equals(currentPosition, DragStartPosition))
                        && (Element is RandomIslandElement || Element is FixedIslandElement || Element is StartingSpotElement)
                        && !((this as IslandViewModel)?.IsOutOfBounds ?? false)
                    ) {
                        UndoRedoStack.Instance.Do(new MapElementTransformStackEntry(Element, DragStartPosition, currentPosition));
                    }
                }
            }
        }
    }
}
