using System.Windows;

namespace AnnoMapEditor.Utilities
{

    public class Line
    {
        private readonly double _a;

        private readonly double _b;

        private readonly double _c;


        public Line(double a, double b, Point center)
        {
            _a = a;
            _b = b;
            _c = a * center.X + b * center.Y;
        }


        public static Point GetIntersection(Line p, Line q)
        {
            double determinant = p._a * q._b - q._a * p._b;
            double x = (q._b * p._c - p._b * q._c) / determinant;
            double y = (p._a * q._c - q._a * p._c) / determinant;

            return new(x, y);
        }
    }
}
