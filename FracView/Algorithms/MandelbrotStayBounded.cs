using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FracView.World;

namespace FracView.Algorithms
{
    public class MandelbrotStayBounded : EscapeAlgorithm
    {
        private double _iterationBreakSquareBounded;

        public MandelbrotStayBounded(int progressCallbackIntervalSec = 0, Action<ProgressReport>? progressCallback = null)
            : base(progressCallbackIntervalSec, progressCallback)
        {
            IterationBreak = 12;
            _iterationBreakSquareBounded = (double)(IterationBreak * IterationBreak);
        }

        public MandelbrotStayBounded(RunSettings settings, int progressCallbackIntervalSec = 0, Action<ProgressReport>? progressCallback = null)
            : base(settings, progressCallbackIntervalSec, progressCallback)
        {
            IterationBreak = 12;
            _iterationBreakSquareBounded = (double)(IterationBreak * IterationBreak);
        }

        public override bool IsStable(EvalComplexUnit eu)
        {
            double eu_real = (double)eu.WorldPos.Real;
            double eu_imag = (double)eu.WorldPos.Imag;
            double pa_x = eu_real;
            double pa_y = eu_imag;

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

                double xsquare = pa_x * pa_x;
                double ysquare = pa_y * pa_y;
                double xy = pa_x * pa_y;

                pa_x = xsquare - ysquare + eu_real;
                pa_y = xy + xy + eu_imag;

                xsquare += ysquare;

                if (pa_x > 21)
                {
                    pa_x -= 21;
                }
                else if (pa_x > 13)
                {
                    pa_x -= 13;
                }
                else if (pa_x > 8)
                {
                    pa_x -= 8;
                }
                else if (pa_x > 5)
                {
                    pa_x -= 5;
                }
                else if (pa_x > 3)
                {
                    pa_x -= 3;
                }

                if (pa_y > 21)
                {
                    pa_y -= 21;
                }
                else if (pa_y > 13)
                {
                    pa_y -= 13;
                }
                else if (pa_y > 8)
                {
                    pa_y -= 8;
                }
                else if (pa_y > 5)
                {
                    pa_y -= 5;
                }
                else if (pa_y > 3)
                {
                    pa_y -= 3;
                }

                if ((Math.Abs(xsquare) >= _iterationBreakSquareBounded) || xsquare == 0)
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
