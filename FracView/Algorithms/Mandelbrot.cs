using FracView.World;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FracView.Algorithms
{
    public class Mandelbrot : EscapeAlgorithm
    {
        public override string AlgorithmName => "Mandelbrot";

        public override string AlgorithmDescription => "Mandelbrot";

        public Mandelbrot()
        {
            IterationBreak = 12;
        }

        public override bool IsStable(EvalComplexUnit eu)
        {
            ComplexPoint pa = eu.WorldPos;
            ComplexPoint pb;

            for (eu.IterationCount = 1; eu.IterationCount < MaxIterations; eu.IterationCount++)
            {
                pb = pa * pa;
                pa = pb + eu.WorldPos;

                var break_value = pa.Abs();
                if (break_value > IterationBreak.Value || break_value <= 0.0)
                {
                    eu.LastPos = pa;

                    return false;
                }
            }

            eu.LastPos = pa;

            return true;
        }
    }
}
