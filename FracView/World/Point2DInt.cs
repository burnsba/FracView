using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FracView.World
{
    /// <summary>
    /// Simple 2d int pair.
    /// </summary>
    public record Point2DInt
    {
        private static Point2DInt _zero = new Point2DInt(0, 0);

        /// <summary>
        /// Initializes a new instance of the <see cref="Point2DInt"/> class.
        /// </summary>
        /// <param name="x">First value.</param>
        /// <param name="y">Second value.</param>
        public Point2DInt(int x, int y)
        {
            X = x;
            Y = y;
        }

        /// <summary>
        /// Gets the first value.
        /// </summary>
        public int X { get; }

        /// <summary>
        /// Gets the second value.
        /// </summary>
        public int Y { get; }

        /// <summary>
        /// Returns (0,0).
        /// </summary>
        public static Point2DInt Zero => _zero;

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
