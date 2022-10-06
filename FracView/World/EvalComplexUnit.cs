using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FracView.World
{
    /// <summary>
    /// Single grid square to evaluate point at.
    /// </summary>
    public record EvalComplexUnit
    {
        public EvalComplexUnit()
        { }

        public EvalComplexUnit(Point2DInt index)
        {
            Index = index;
        }

        public EvalComplexUnit(Point2DInt index, ComplexPoint worldPos)
        {
            Index = index;
            WorldPos = worldPos;
        }

        public EvalComplexUnit(int rowx, int coly, ComplexPoint worldPos)
        {
            Index = (rowx, coly);
            WorldPos = worldPos;
        }

        /// <summary>
        /// Creation row/column number (~ pixel offset)
        /// </summary>
        public Point2DInt Index { get; set; } = Point2DInt.Zero;

        /// <summary>
        /// Evaluation point position.
        /// </summary>
        public ComplexPoint WorldPos { get; set; } = ComplexPoint.Zero;

        /// <summary>
        /// Whether or not the point escaped within the max number of iterations.
        /// </summary>
        public bool? IsStable { get; set; } = null;

        /// <summary>
        /// Iteration count the point escaped at.
        /// </summary>
        public int IterationCount { get; set; }

        /// <summary>
        /// Last value the point evaluated to before escape
        /// </summary>
        public ComplexPoint LastPos { get; set; } = ComplexPoint.Zero;

        public double HistogramValue { get; set; } = 0.0;
    }
}
