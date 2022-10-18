using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FracView.Algorithms
{
    /// <summary>
    /// Container to desribe parameters used to render image.
    /// </summary>
    public record RunSettings
    {
        /// <summary>
        /// Gets or sets world origin x (real).
        /// </summary>
        public decimal OriginX { get; set; }

        /// <summary>
        /// Gets or sets world origin y (imaginary).
        /// </summary>
        public decimal OriginY { get; set; }

        /// <summary>
        /// Gets or sets world width/range.
        /// </summary>
        public decimal FractalWidth { get; set; }

        /// <summary>
        /// Gets or sets world height/range.
        /// </summary>
        public decimal FractalHeight { get; set; }

        /// <summary>
        /// Gets or sets image pixel count width.
        /// </summary>
        public int StepWidth { get; set; }

        /// <summary>
        /// Gets or sets image pixel count height.
        /// </summary>
        public int StepHeight { get; set; }

        /// <summary>
        /// Gets or sets max number of iterations used by the algorithm.
        /// </summary>
        public int MaxIterations { get; set; }

        /// <summary>
        /// Gets or sets whether or not histogram was computed.
        /// </summary>
        public bool UseHistogram { get; set; }
    }
}
