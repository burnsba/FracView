using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FracView.World;

namespace FracView.Algorithms
{
    /// <summary>
    /// Interface to define algorithm.
    /// </summary>
    public interface IAlgorithm
    {
        /// <summary>
        /// Gets the points computed by this algorithm.
        /// </summary>
        List<EvalComplexUnit> ConsideredPoints { get; }

        /// <summary>
        /// Gets image pixel count width.
        /// Number of steps to divide world range into.
        /// </summary>
        int StepWidth { get; }

        /// <summary>
        /// Gets image pixel count height.
        /// Number of steps to divide world range into.
        /// </summary>
        int StepHeight { get; }

        /// <summary>
        /// Gets or sets world width/range.
        /// </summary>
        decimal FractalWidth { get; set; }

        /// <summary>
        /// Gets or sets world height/range.
        /// </summary>
        decimal FractalHeight { get; set; }

        /// <summary>
        /// Getgs or sets the interval in seconds that progress should be reported.
        /// </summary>
        int ProgressCallbackIntervalSec { get; set; }

        /// <summary>
        /// Gets or sets the reporting callback method.
        /// </summary>
        Action<ProgressReport>? ProgressCallback { get; set; }

        /// <summary>
        /// Gets or sets whether or not histogram should be computed.
        /// </summary>
        bool UseHistogram { get; set; }

        /// <summary>
        /// Gets or sets whether indicating whether or not histogram data has been computed.
        /// </summary>
        bool HistogramIsEvaluated { get; }

        /// <summary>
        /// Evaluates all points described in the range.
        /// </summary>
        /// <param name="token">Cancellation token.</param>
        /// <returns>Whether computation finished successfully.</returns>
        bool EvaluatePoints(CancellationToken token);

        /// <summary>
        /// Computes histogram and updates each <see cref="EvalComplexUnit.HistogramValue"/>.
        /// </summary>
        /// <param name="token">Cancellation token.</param>
        void ComputeHistogram(CancellationToken token);
    }
}
