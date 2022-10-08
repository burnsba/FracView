using FracView.World;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FracView.Algorithms
{
    public class OldMandelbrot
    {
        public OldMandelbrot()
        {
            //IterationBreak = 12;
        }

        public bool IsStable()
        {
            // convert to native types for performance. Avoids runtime overhead of creating so many records.
            /*
            double pa_x = eu.WorldPos.Real;
            double pa_y = eu.WorldPos.Imag;
            double pb_x = 0;
            double pb_y = 0;
            double break_value;

            for (eu.IterationCount = 1; eu.IterationCount < MaxIterations; eu.IterationCount++)
            {
                pb_x = pa_x * pa_x - pa_y * pa_y;
                pb_y = pa_x * pa_y + pa_y * pa_x;

                pa_x = pb_x + eu.WorldPos.Real;
                pa_y = pb_y + eu.WorldPos.Imag;

                break_value = Math.Sqrt((pa_x * pa_x) + (pa_y * pa_y));

                if (break_value > IterationBreak.Value || break_value <= 0.0)
                {
                    eu.LastPos = (pa_x, pa_y);

                    return false;
                }
            }

            eu.LastPos = (pa_x, pa_y);
            */

            return true;
        }
    }
}
