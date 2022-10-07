using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MultiPrecision;

namespace FracView.World
{
    public class ComplexPointMp<T> where T : struct, MultiPrecision.IConstant
    {
        private static ComplexPointMp<T> _zero = new ComplexPointMp<T>(0, 0);
        private static ComplexPointMp<T> _one = new ComplexPointMp<T>(1, 0);
        private static ComplexPointMp<T> _i = new ComplexPointMp<T>(0, 1);
        
        public ComplexPointMp(MultiPrecision<T> real, MultiPrecision<T> imag)
        {
            Real = real;
            Imag = imag;
        }

        public MultiPrecision<T> Real { get; }
        public MultiPrecision<T> Imag { get; }

        public MultiPrecision<T> Abs()
        {
            MultiPrecision<T>.Mul(Real, Real);
            MultiPrecision<T> s = MultiPrecision<T>.Add((Real * Real), (Imag * Imag));
            return MultiPrecision<T>.Sqrt(s);
        }

        public static ComplexPointMp<T> Zero => _zero;
        public static ComplexPointMp<T> One => _one;
        public static ComplexPointMp<T> I => _i;

        public static implicit operator ComplexPointMp<T>(ValueTuple<MultiPrecision<T>, MultiPrecision<T>> d)
        {
            return new ComplexPointMp<T>(d.Item1, d.Item2);
        }

        public static ComplexPointMp<T> operator +(ComplexPointMp<T> p1, ComplexPointMp<T> p2)
        {
            return new ComplexPointMp<T>(MultiPrecision<T>.Add(p1.Real, p2.Real), MultiPrecision<T>.Add(p1.Imag, p2.Imag));
        }

        public static ComplexPointMp<T> operator -(ComplexPointMp<T> p1, ComplexPointMp<T> p2)
        {
            return new ComplexPointMp<T>(MultiPrecision<T>.Sub(p1.Real, p2.Real), MultiPrecision<T>.Add(p1.Imag, p2.Imag));
        }

        public static ComplexPointMp<T> operator *(ComplexPointMp<T> p1, ComplexPointMp<T> p2)
        {
            return new ComplexPointMp<T>((p1.Real * p2.Real) - (p1.Imag * p2.Imag), (p1.Real * p2.Imag) + (p1.Imag * p2.Real));
        }
    }
}
