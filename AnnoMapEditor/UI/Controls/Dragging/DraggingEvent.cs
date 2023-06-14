using System.Windows;

namespace AnnoMapEditor.UI.Controls.Dragging
{
    public delegate void DraggingEventHandler(object? sender, DraggingEventArgs e);


    public class DraggingEventArgs
    {
        public Point Delta { get; init; }


        public DraggingEventArgs(Point delta)
        {
            Delta = delta;
        }
    }
}
