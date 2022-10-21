using FracView.Algorithms;
using SkiaSharp;

namespace FracView.Gfx
{
    /// <summary>
    /// Container for rendering final result of computed area.
    /// </summary>
    public interface IScene
    {
        /// <summary>
        /// Gets or sets the color used for bounded/unset pixels.
        /// </summary>
        SKColor StableColor { get; set; }

        /// <summary>
        /// Gets or sets the color ramp used to map values to colors.
        /// </summary>
        ColorRamp ColorRamp { get; set; }

        /// <summary>
        /// Takes the computed results from <see cref="IEscapeAlgorithm.ConsideredPoints"/> and maps world points to pixels with color values.
        /// If the histogram data needs to be computed or recomputed, that will happen automatically here.
        /// </summary>
        /// <param name="algorithm">Algorithm with associated points.</param>
        /// <param name="token">Cancellation token.</param>
        /// <param name="progressCallbackIntervalSec">Interval in seconds that progress should be reported.</param>
        /// <param name="progressCallback">Reporting callback method.</param>
        /// <returns>Image with pixels set. If safely cancelled, returns null.</returns>
        SKBitmap? ProcessPointsToPixels(IEscapeAlgorithm algorithm, CancellationToken token, int progressCallbackIntervalSec = 0, Action<ProgressReport>? progressCallback = null);

        /// <summary>
        /// Converts histogram percent value to color value.
        /// </summary>
        /// <param name="percent">Value between zero and one.</param>
        /// <returns>Color according to colorramp.</returns>
        SKColor ResolveColorByPercent(double percent);

        /// <summary>
        /// Converts iteration percent to color value.
        /// </summary>
        /// <param name="iterationCount">Iteration count of point.</param>
        /// <param name="maxIterations">Max iterations defined by algorithm.</param>
        /// <returns>Color according to colorramp.</returns>
        SKColor ResolveColorByIterations(int iterationCount, int maxIterations);

        /// <summary>
        /// Adds the default keyframes to <see cref="ColorRamp.Keyframes"/>.
        /// </summary>
        void AddDefaultSceneKeyframes();
    }
}