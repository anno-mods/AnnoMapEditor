using AnnoMapEditor.Utilities;
using System.Windows;

namespace AnnoMapEditor.UI.Controls.Dragging
{
    public abstract class DraggingViewModel : ObservableBase
    {
        public event DragEndedEventHandler? DragEnded;

        public event DragStartedEventHandler? DragStarted;

        public event DraggingEventHandler? Dragging;


        public bool IsDragging
        {
            get => _isDragging;
            set => SetProperty(ref _isDragging, value);
        }
        private bool _isDragging;

        public Vector2? MouseOffset
        {
            get => _mouseOffset;
            private set => SetProperty(ref _mouseOffset, value);
        }
        private Vector2? _mouseOffset;


        public void BeginDrag(Vector2 localMousePos)
        {
            MouseOffset = localMousePos;
            IsDragging = true;

            DragStarted?.Invoke(this, new());
        }

        public void EndDrag()
        {
            MouseOffset = null;
            IsDragging = false;
            DragEnded?.Invoke(this, new());
        }

        public void ContinueDrag(Vector2 localMousePos)
        {
            Vector2 delta = new(localMousePos.X - _mouseOffset!.X, localMousePos.Y - _mouseOffset!.Y);
            OnDragged(delta);
        }

        public virtual void OnDragged(Vector2 delta)
        {
            Dragging?.Invoke(this, new(delta));
        }
    }
}
