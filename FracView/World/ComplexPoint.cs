using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FracView.World
{
    public class ComplexPoint
    {
        private static ComplexPoint _zero = new ComplexPoint(0, 0);
        private static ComplexPoint _one = new ComplexPoint(1, 0);
        private static ComplexPoint _i = new ComplexPoint(0, 1);
        
        public ComplexPoint(decimal real, decimal imag)
        {
            Real = real;
            Imag = imag;
        }

        public decimal Real { get; }
        public decimal Imag { get; }

        public static ComplexPoint Zero => _zero;
        public static ComplexPoint One => _one;
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
