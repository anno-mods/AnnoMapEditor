using AnnoMapEditor.Utilities;
using System.Windows;

namespace AnnoMapEditor.UI.Controls.Dragging
{
    public abstract class DraggingViewModel : ObservableBase
    {
        public event DraggingEventHandler? Dragging;

        public event DragEndedEventHandler? DragEnded;


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


        public void BeginDrag(Point mouseOffset)
        {
            MouseOffset = mouseOffset;
            IsDragging = true;
        }

        public void EndDrag()
        {
            MouseOffset = null;
            IsDragging = false;
            DragEnded?.Invoke(this, new());
        }


        public void ContinueDrag(Point mouseOffset)
        {
            Point delta = new(mouseOffset.X - _mouseOffset!.Value.X, mouseOffset.Y - _mouseOffset!.Value.Y);
            Move(delta);
        }

        public virtual void Move(Point delta)
        {
            Dragging?.Invoke(this, new(delta));
        }
    }
}
