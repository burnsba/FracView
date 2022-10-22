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
    /// Modification of Mandelbrot algorithm in computation of Z^2, to use cos.
    /// </summary>
    public class MandelbrotCos : EscapeAlgorithm
    {
        private double _iterationBreakSquareLog;

        public MandelbrotCos(int progressCallbackIntervalSec = 0, Action<ProgressReport>? progressCallback = null)
            : base(progressCallbackIntervalSec, progressCallback)
        {
            IterationBreak = 12;
            _iterationBreakSquareLog = (double)(IterationBreak * IterationBreak);
        }

        public MandelbrotCos(RunSettings settings, int progressCallbackIntervalSec = 0, Action<ProgressReport>? progressCallback = null)
            : base(settings, progressCallbackIntervalSec, progressCallback)
        {
            IterationBreak = 12;
            _iterationBreakSquareLog = (double)(IterationBreak * IterationBreak);
        }

        public override bool IsStable(EvalComplexUnit eu)
        {
            double eu_real = (double)eu.WorldPos.Real;
            double eu_imag = (double)eu.WorldPos.Imag;
            double pa_x = eu_real;
            double pa_y = eu_imag;

            _iterationBreakSquareLog = 300;


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

                double xsquare = 0;
                double ysquare = 0;
                double xy;

                xsquare = pa_x * pa_x;
                ysquare = pa_y * pa_y;

                xy = Math.Cos(pa_x * pa_y);

                pa_x = xsquare - ysquare + eu_real;

                pa_y = xy + xy + eu_imag;

                xsquare += ysquare;

                if ((Math.Abs(xsquare) >= _iterationBreakSquareLog) || xsquare == 0)
                {
                    eu.LastPos = (0,0);

                    return false;
                }
            }

            eu.LastPos = (0, 0);

            return true;
        }
    }
}
