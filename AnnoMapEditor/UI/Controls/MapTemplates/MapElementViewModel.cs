using AnnoMapEditor.MapTemplates.Models;
using AnnoMapEditor.UI.Controls.Dragging;
using AnnoMapEditor.Utilities;
using AnnoMapEditor.Utilities.UndoRedo;
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

        private Vector2? _oldPosition;

        private void Element_TransformationUndo(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IsDragging))
            {
                if (IsDragging) 
                { 
                    _oldPosition = Element.Position;
                }
                else
                {
                    Vector2 currentPosition = Element.Position;
                    if (
                        (currentPosition != null && _oldPosition != null)
                        && (Element is RandomIslandElement || Element is FixedIslandElement || Element is StartingSpotElement)
                        && !((this as IslandViewModel)?.IsOutOfBounds ?? false)
                    ) {
                        UndoRedoStack.Instance.Do(
                            new MapElementTransformStackEntry(
                                Element,
                                _oldPosition,
                                currentPosition
                            )
                        );
                    }
                }
            }
        }
    }
}
