using AnnoMapEditor.Utilities;

namespace AnnoMapEditor.UI.Controls.Dragging
{
    public delegate void DraggingEventHandler(object? sender, DraggingEventArgs e);


    public class DraggingEventArgs
    {
        public Vector2 Delta { get; init; }


        public DraggingEventArgs(Vector2 delta)
        {
            Delta = delta;
        }
    }
}