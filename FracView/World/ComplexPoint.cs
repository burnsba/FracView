using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FracView.World
{
    public record ComplexPoint
    {
        public ComplexPoint(double real, double imag)
        {
            Real = real;
            Imag = imag;
        }

        public double Real { get; }
        public double Imag { get; }

        public double Abs()
        {
            return Math.Sqrt((Real * Real) + (Imag * Imag));
        }

        public static ComplexPoint Zero => new ComplexPoint(0, 0);
        public static ComplexPoint One => new ComplexPoint(1, 0);
        public static ComplexPoint I => new ComplexPoint(0, 1);

        public static implicit operator ComplexPoint(ValueTuple<double, double> d)
        {
            return new ComplexPoint(d.Item1, d.Item2);
        }

        public static ComplexPoint operator+(ComplexPoint p1, ComplexPoint p2)
        {
            return new ComplexPoint(p1.Real + p2.Real, p1.Imag + p2.Imag);
        }

        public static ComplexPoint operator-(ComplexPoint p1, ComplexPoint p2)
        {
            return new ComplexPoint(p1.Real - p2.Real, p1.Imag - p2.Imag);
        }

        public static ComplexPoint operator*(ComplexPoint p1, ComplexPoint p2)
        {
            return new ComplexPoint((p1.Real * p2.Real) - (p1.Imag * p2.Imag), (p1.Real * p2.Imag) + (p1.Imag * p2.Real));
        }
    }
}
