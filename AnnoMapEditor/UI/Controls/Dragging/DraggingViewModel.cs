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

        public Point? MouseOffset
        {
            get => _mouseOffset;
            private set => SetProperty(ref _mouseOffset, value);
        }
        private Point? _mouseOffset;


        public void BeginDrag(Point? localMousePos = null)
        {
            MouseOffset = localMousePos ?? new();
            IsDragging = true;

            DragStarted?.Invoke(this, new());
        }

        public void EndDrag()
        {
            MouseOffset = null;
            IsDragging = false;
            DragEnded?.Invoke(this, new());
        }

        public void ContinueDrag(Point localMousePos)
        {
            Point delta = new(localMousePos.X - _mouseOffset!.Value.X, localMousePos.Y - _mouseOffset!.Value.Y);
            OnDragged(delta);
        }

        public virtual void OnDragged(Point delta)
        {
            Dragging?.Invoke(this, new(delta));
        }
    }
}
