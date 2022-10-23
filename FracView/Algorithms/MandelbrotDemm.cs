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
    /// Fast implementation of Mandelbrot algorithm, using <see cref="double"/>.
    /// </summary>
    public class MandelbrotDemm : EscapeAlgorithm
    {
        private double _iterationBreakDouble;

        public MandelbrotDemm(int progressCallbackIntervalSec = 0, Action<ProgressReport>? progressCallback = null)
            : base(progressCallbackIntervalSec, progressCallback)
        {
            IterationBreak = 12;
            _iterationBreakDouble = (double)(IterationBreak * IterationBreak);
        }

        public MandelbrotDemm(RunSettings settings, int progressCallbackIntervalSec = 0, Action<ProgressReport>? progressCallback = null)
            : base(settings, progressCallbackIntervalSec, progressCallback)
        {
            IterationBreak = 12;
            _iterationBreakDouble = (double)(IterationBreak * IterationBreak);
        }

        public override bool IsStable(EvalComplexUnit eu)
        {
            double eu_real = (double)eu.WorldPos.Real;
            double eu_imag = (double)eu.WorldPos.Imag;
            double pa_x = eu_real;
            double pa_y = eu_imag;

            double dz_x = 1;
            double dz_y = 0;

            for (eu.IterationCount = 1; eu.IterationCount < MaxIterations; eu.IterationCount++)
            {
                // Distance estimator.
                //
                // really not sure if this is correct or not.
                //
                // https://en.wikibooks.org/wiki/Fractals/Iterations_in_the_complex_plane/demm

                double temp_x = 2 * (pa_x * dz_x - pa_y * dz_y) + 1;
                double temp_y = 2 * (pa_x * dz_y + pa_y * dz_x);

                dz_x = temp_x;
                dz_y = temp_y;

                double xsquare = pa_x * pa_x;
                double ysquare = pa_y * pa_y;
                double xy = pa_x * pa_y;

                pa_x = xsquare - ysquare + eu_real;
                pa_y = xy + xy + eu_imag;

                xsquare += ysquare;

                if (xsquare >= _iterationBreakDouble || xsquare == 0)
                {
                    eu.LastPos = (0, 0);

                    double distance;
                    double absdZ2 = dz_x * dz_x + dz_y * dz_y;
                    distance = Math.Sqrt(xsquare / absdZ2) * Math.Log(xsquare);
                    distance = Math.Pow(4.0 * distance, 0.25);
                    distance = Math.Abs(distance);

                    if (distance < 0)
                    {
                        distance = 0;
                    }
                    else if (distance > 1)
                    {
                        distance = 1;
                    }

                    distance = 1 - distance;

                    eu.IterationCount = (int)(distance * (double)MaxIterations);

                    if (eu.IterationCount < 1)
                    {
                        eu.IterationCount = 1;
                    }

                    return false;
                }
            }

            if (eu.IterationCount < 1)
            {
                eu.IterationCount = 1;
            }

            eu.LastPos = (0, 0);

            return true;
        }
    }
}
