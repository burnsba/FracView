using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FracView.World;
using MultiPrecision;

namespace FracView.Algorithms
{
    public class MandelbrotMp<T> : EscapeAlgorithmMp<T> where T : struct, MultiPrecision.IConstant
    {
        public override string AlgorithmName => "Mandelbrot";

        public override string AlgorithmDescription => "Mandelbrot";

        public MandelbrotMp()
        {
            IterationBreak = 12;
        }

        public override bool IsStable(EvalComplexUnitMp<T> eu)
        {
            MultiPrecision<T> pa_x = eu.WorldPos.Real;
            MultiPrecision<T> pa_y = eu.WorldPos.Imag;
            MultiPrecision<T> pb_x = 0;
            MultiPrecision<T> pb_y = 0;
            MultiPrecision<T> break_value;

            for (eu.IterationCount = 1; eu.IterationCount < MaxIterations; eu.IterationCount++)
            {
                pb_x = pa_x * pa_x - pa_y * pa_y;
                pb_y = pa_x * pa_y + pa_y * pa_x;

                pa_x = pb_x + eu.WorldPos.Real;
                pa_y = pb_y + eu.WorldPos.Imag;

                break_value = MultiPrecision<T>.Sqrt((pa_x * pa_x) + (pa_y * pa_y));

                if (break_value > IterationBreak.Value || break_value <= 0.0)
                {
                    eu.LastPos = (pa_x, pa_y);

                    return false;
                }
            }

            eu.LastPos = (pa_x, pa_y);

            return true;
        }
    }
}
