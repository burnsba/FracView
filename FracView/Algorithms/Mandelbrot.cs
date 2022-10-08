using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FracView.World;

namespace FracView.Algorithms
{
    public class Mandelbrot : EscapeAlgorithm
    {
        public Mandelbrot(int progressCallbackIntervalSec = 0, Action<ProgressReport>? progressCallback = null)
            : base(progressCallbackIntervalSec, progressCallback)
        {
            IterationBreak = 12;
        }

        public override bool IsStable(EvalComplexUnit eu)
        {
            decimal pa_x = eu.WorldPos.Real;
            decimal pa_y = eu.WorldPos.Imag;
            decimal pb_x = 0;
            decimal pb_y = 0;
            decimal break_value;

            decimal iterationBreakSquare = 0;

            if (IterationBreak != null)
            {
                iterationBreakSquare = IterationBreak.Value * IterationBreak.Value;
            }

            for (eu.IterationCount = 1; eu.IterationCount < MaxIterations; eu.IterationCount++)
            {
                //convert to native types for performance.Avoids runtime overhead of creating so many records.

                pb_x = pa_x * pa_x - pa_y * pa_y;
                pb_y = pa_x * pa_y + pa_y * pa_x;

                pa_x = pb_x + eu.WorldPos.Real;
                pa_y = pb_y + eu.WorldPos.Imag;

                break_value = (pa_x * pa_x) + (pa_y * pa_y);

                if ((IterationBreak != null && (break_value >= iterationBreakSquare)) || break_value <= 0)
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
