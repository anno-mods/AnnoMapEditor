using AnnoMapEditor.UI.Controls.Dragging;
using AnnoMapEditor.Utilities;
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace AnnoMapEditor.UI.Controls.Selection
{
    public class SelectionBoxViewModel : DraggingViewModel
    {
        public SolidColorBrush BackgroundBrush { get; } = new(Color.FromArgb(0x0F, 0xCC, 0xCC, 0xCC));
        public SolidColorBrush BorderBrush { get; } = new(Color.FromArgb(0x00, 0xCC, 0xCC, 0xCC));


        public Point Start { get; init; }

        public Point End
        {
            get => _end;
            private set => SetProperty(ref _end, value);
        }
        private Point _end;

        public Point Size
        {
            get => _size;
            private set => SetProperty(ref _size, value);
        }
        private Point _size;

        public Point StartLocal
        {
            get => _startLocal;
            private set => SetProperty(ref _startLocal, value);
        }
        private Point _startLocal;

        public Point EndLocal
        {
            get => _endLocal;
            private set => SetProperty(ref _endLocal, value);
        }
        private Point _endLocal;

        public PointCollection Boundary
        {
            get => _boundary;
            private set => SetProperty(ref _boundary, value);
        }
        private PointCollection _boundary;


        public SelectionBoxViewModel(Point start)
        {
            Start = start;
            _end = start;
            _size = new(10, 10);

            UpdateOutline();
        }


        public override void ContinueDrag(Point localMousePos)
        {
            // localMousePos is relative to the control's current position
            End = localMousePos;

            UpdateOutline();
        }

        private void UpdateOutline()
        {
            // parallel to the map
            if (Keyboard.IsKeyDown(Key.LeftAlt))
            {
                double x1 = Math.Min(Start.X, End.X);
                double y1 = Math.Min(Start.Y, End.Y);
                double x2 = Math.Max(Start.X, End.X);
                double y2 = Math.Max(Start.Y, End.Y);

                Boundary = new PointCollection(new Point[]
                {
                    new(x1, y1),
                    new(x1, y2),
                    new(x2, y2),
                    new(x2, y1)
                });
            }

            // parallel to the screen
            else
            {
                Line startDecline = new(-1, -1, Start);
                Line startIncline = new(1, -1, Start);
                Line endDecline = new(-1, -1, End);
                Line endIncline = new(1, -1, End);

                Boundary = new PointCollection(new Point[] {
                    Start,
                    Line.GetIntersection(startIncline, endDecline),
                    End,
                    Line.GetIntersection(endIncline, startDecline),
                });
            }
        }



        public bool Contains(Vector2 vector) => Contains(new Point(vector.X, vector.Y));

        public bool Contains(Point point)
        {
            bool result = false;

            int j = _boundary.Count - 1;
            for (int i = 0; i < _boundary.Count; i++)
            {
                Point a = _boundary[i];
                Point b = _boundary[j];

                if (a.Y < point.Y && b.Y >= point.Y || b.Y < point.Y && a.Y >= point.Y)
                    if (a.X + (point.Y - a.Y) / (b.Y - a.Y) * (b.X - a.X) < point.X)
                        result = !result;


                j = i;
            }

            return result;
        }
    }
}