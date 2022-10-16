using FracView.Algorithms;
using SkiaSharp;

namespace FracView.Gfx
{
    public interface IScene
    {
        ColorRamp ColorRamp { get; set; }
        SKColor StableColor { get; set; }

        SKBitmap ProcessPointsToPixels(IEscapeAlgorithm algorithm, CancellationToken token, int progressCallbackIntervalSec = 0, Action<ProgressReport>? progressCallback = null);
        SKColor ResolveColorByIterations(int iterationCount, int maxIterations);
        SKColor ResolveColorByPercent(double percent);
        void AddDefaultSceneKeyframes();
    }
}