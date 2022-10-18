using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FracView.World
{
    /// <summary>
    /// Contains evaluation results for a single pixel of data.
    /// </summary>
    public record EvalComplexUnit
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EvalComplexUnit"/> class.
        /// </summary>
        public EvalComplexUnit()
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="EvalComplexUnit"/> class.
        /// </summary>
        /// <param name="index">Pixel grid location.</param>
        public EvalComplexUnit(Point2DInt index)
        {
            Index = index;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EvalComplexUnit"/> class.
        /// </summary>
        /// <param name="index">Pixel grid location.</param>
        /// <param name="ComplexPoint">World location.</param>
        public EvalComplexUnit(Point2DInt index, ComplexPoint worldPos)
        {
            Index = index;
            WorldPos = worldPos;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EvalComplexUnit"/> class.
        /// </summary>
        /// <param name="rowx">Pixel grid x.</param>
        /// <param name="coly">Pixel grid y.</param>
        /// <param name="ComplexPoint">World location.</param>
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

        /// <summary>
        /// If a histogram is computed for the entire scene, this will store the result for this pixel.
        /// </summary>
        public double HistogramValue { get; set; } = 0;
    }
}
