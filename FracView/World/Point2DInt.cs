using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FracView.World
{
    public record Point2DInt
    {
        public Point2DInt(int x, int y)
        {
            X = x;
            Y = y;
        }

        public int X { get; }
        public int Y { get; }

        public static Point2DInt Zero => new Point2DInt(0, 0);

        public static implicit operator Point2DInt(ValueTuple<int, int> d)
        {
            return new Point2DInt(d.Item1, d.Item2);
        }

        public static Point2DInt operator +(Point2DInt p1, Point2DInt p2)
        {
            return new Point2DInt(p1.X + p2.X, p1.Y + p2.Y);
        }

        public static Point2DInt operator -(Point2DInt p1, Point2DInt p2)
        {
            return new Point2DInt(p1.X - p2.X, p1.Y - p2.Y);
        }

        public static Point2DInt operator *(Point2DInt p1, int i)
        {
            return new Point2DInt(p1.X * i, p1.Y * i);
        }

        public static Point2DInt operator *(int i, Point2DInt p1)
        {
            return new Point2DInt(p1.X * i, p1.Y * i);
        }
    }
}
