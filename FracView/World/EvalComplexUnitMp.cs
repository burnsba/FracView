using MultiPrecision;
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
    public record EvalComplexUnitMp<T> where T : struct, MultiPrecision.IConstant
    {
        public EvalComplexUnitMp()
        { }

        public EvalComplexUnitMp(Point2DInt index)
        {
            Index = index;
        }

        public EvalComplexUnitMp(Point2DInt index, ComplexPointMp<T> worldPos)
        {
            Index = index;
            WorldPos = worldPos;
        }

        public EvalComplexUnitMp(int rowx, int coly, ComplexPointMp<T> worldPos)
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
        public ComplexPointMp<T> WorldPos { get; set; } = ComplexPointMp<T>.Zero;

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
        public ComplexPointMp<T> LastPos { get; set; } = ComplexPointMp<T>.Zero;

        public MultiPrecision<T> HistogramValue { get; set; } = MultiPrecision<T>.Zero;
    }
}
