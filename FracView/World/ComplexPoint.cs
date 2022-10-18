using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FracView.World
{
    /// <summary>
    /// Decimale precision value pair.
    /// </summary>
    public class ComplexPoint
    {
        private static ComplexPoint _zero = new ComplexPoint(0, 0);
        private static ComplexPoint _one = new ComplexPoint(1, 0);
        private static ComplexPoint _i = new ComplexPoint(0, 1);

        /// <summary>
        /// Initializes a new instance of the <see cref="ComplexPoint"/> class.
        /// </summary>
        /// <param name="real">Real part.</param>
        /// <param name="imag">Imaginary part.</param>
        public ComplexPoint(decimal real, decimal imag)
        {
            Real = real;
            Imag = imag;
        }

        /// <summary>
        /// Gets the real part.
        /// </summary>
        public decimal Real { get; }

        /// <summary>
        /// Gets the imaginary part.
        /// </summary>
        public decimal Imag { get; }

        /// <summary>
        /// Returns (0,0).
        /// </summary>
        public static ComplexPoint Zero => _zero;

        /// <summary>
        /// Returns (1,0).
        /// </summary>
        public static ComplexPoint One => _one;

        /// <summary>
        /// Returns (0,1).
        /// </summary>
        public static ComplexPoint I => _i;

        public static implicit operator ComplexPoint(ValueTuple<decimal, decimal> d)
        {
            return new ComplexPoint(d.Item1, d.Item2);
        }

        public static ComplexPoint operator +(ComplexPoint p1, ComplexPoint p2)
        {
            return new ComplexPoint(p1.Real + p2.Real, p1.Imag + p2.Imag);
        }

        public static ComplexPoint operator -(ComplexPoint p1, ComplexPoint p2)
        {
            return new ComplexPoint(p1.Real - p2.Real, p1.Imag - p2.Imag);
        }

        public static ComplexPoint operator *(ComplexPoint p1, ComplexPoint p2)
        {
            return new ComplexPoint((p1.Real * p2.Real) - (p1.Imag * p2.Imag), (p1.Real * p2.Imag) + (p1.Imag * p2.Real));
        }
    }
}
