using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FracView.Dto;
using FracView.World;

namespace FracView.Algorithms
{
    /// <summary>
    /// Precision implementation of Mandelbrot algorithm, using <see cref="decimal"/>.
    /// </summary>
    public class Mandelbrot : EscapeAlgorithm
    {
        public Mandelbrot(int progressCallbackIntervalSec = 0, Action<ProgressReport>? progressCallback = null)
            : base(progressCallbackIntervalSec, progressCallback)
        {
            IterationBreak = 12;
        }

        public Mandelbrot(RunSettings settings, int progressCallbackIntervalSec = 0, Action<ProgressReport>? progressCallback = null)
            : base(settings, progressCallbackIntervalSec, progressCallback)
        {
            IterationBreak = 12;
        }

        public override bool IsStable(EvalComplexUnit eu)
        {
            decimal pa_x = eu.WorldPos.Real;
            decimal pa_y = eu.WorldPos.Imag;

            for (eu.IterationCount = 1; eu.IterationCount < MaxIterations; eu.IterationCount++)
            {
                //convert to native types for performance. Avoids runtime overhead of creating so many records.

                /*
                
                // naive implementation.

                decimal pb_x = pa_x * pa_x - pa_y * pa_y;
                decimal pb_y = pa_x * pa_y + pa_y * pa_x;

                pa_x = pb_x + eu.WorldPos.Real;
                pa_y = pb_y + eu.WorldPos.Imag;

                decimal break_value = (pa_x * pa_x) + (pa_y * pa_y);

                if (break_value >= IterationBreakSquare || break_value <= 0)
                {
                    eu.LastPos = (pa_x, pa_y);

                    return false;
                }*/

                // optimized to reduce multiplications.

                decimal xsquare = pa_x * pa_x;
                decimal ysquare = pa_y * pa_y;
                decimal xy = pa_x * pa_y;

                pa_x = xsquare - ysquare + eu.WorldPos.Real;
                pa_y = xy + xy + eu.WorldPos.Imag;

                xsquare += ysquare;

                if (xsquare >= IterationBreakSquare || xsquare == 0)
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
