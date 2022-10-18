using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FracView.Algorithms;
using Microsoft.VisualBasic.FileIO;
using SkiaSharp;
using static System.Formats.Asn1.AsnWriter;

namespace FracView.Gfx
{
    /// <summary>
    /// Container for rendering final result of computed area.
    /// </summary>
    public class Scene : IScene
    {
        private object _lockObject = new object();

        /// <summary>
        /// Gets or sets the color used for bounded/unset pixels.
        /// </summary>
        public SKColor StableColor { get; set; } = ColorRef.Black;

        /// <summary>
        /// Gets or sets the color ramp used to map values to colors.
        /// </summary>
        public ColorRamp ColorRamp { get; set; } = new ColorRamp();

        /// <summary>
        /// Initializes a new instance of the <see cref="Scene"/> class.
        /// </summary>
        public Scene()
        {
        }

        /// <summary>
        /// Allocates memory to be used by graphic result container.
        /// </summary>
        /// <param name="algorithm">Algorithm; only uses pixel dimensions.</param>
        /// <returns>Allocated image container.</returns>
        private SKBitmap AllocateBitmap(IEscapeAlgorithm algorithm)
        {
            SKBitmap bmp;

            try
            {
                bmp = new SKBitmap(algorithm.StepWidth, algorithm.StepHeight, SKColorType.Rgba8888, SKAlphaType.Opaque);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unable to allocate bitmap");
                Console.WriteLine(ex.Message);

                throw;
            }

            return bmp;
        }

        /// <summary>
        /// Takes the computed results from <see cref="IEscapeAlgorithm.ConsideredPoints"/> and maps world points to pixels with color values.
        /// If the histogram data needs to be computed or recomputed, that will happen automatically here.
        /// </summary>
        /// <param name="algorithm">Algorithm with associated points.</param>
        /// <param name="token">Cancellation token.</param>
        /// <param name="progressCallbackIntervalSec">Interval in seconds that progress should be reported.</param>
        /// <param name="progressCallback">Reporting callback method.</param>
        /// <returns>Image with pixels set.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown if algorithm has not computed points yet, or colorramp has not been set in this instance.
        /// </exception>
        public SKBitmap ProcessPointsToPixels(IEscapeAlgorithm algorithm, CancellationToken token, int progressCallbackIntervalSec = 0, Action<ProgressReport>? progressCallback = null)
        {
            if (!ColorRamp.Keyframes.Any())
            {
                throw new InvalidOperationException($"{nameof(ColorRamp)} needs at least one keyframe defined");
            }

            var bmp = AllocateBitmap(algorithm);

            var points = algorithm.ConsideredPoints;
            var reportTimer = Stopwatch.StartNew();
            var totalTimer = Stopwatch.StartNew();
            int iterationCount = 0;

            if (!points.Any())
            {
                throw new InvalidOperationException($"algorithm has no points to render");
            }

            if (algorithm.UseHistogram)
            {
                iterationCount = 0;
                reportTimer.Restart();

                if (!algorithm.HistogramIsEvaluated)
                {
                    algorithm.ComputeHistogram(token);

                    if (token.IsCancellationRequested)
                    {
                        return null;
                    }
                }

                Parallel.ForEach(points, (x, loopState) =>
                {
                    if (token.IsCancellationRequested)
                    {
                        loopState.Break();
                    }

                    SKColor pixelColor = ColorRef.White;

                    if (x.IsStable == true)
                    {
                        pixelColor = StableColor;
                    }
                    else
                    {
                        pixelColor = ResolveColorByPercent(x.HistogramValue);
                    }

                    lock (_lockObject)
                    {
                        // see comments in other section about speed.
                        unsafe
                        {
                            IntPtr pixelsAddr = bmp.GetPixels();
                            byte* ptr = (byte*)pixelsAddr.ToPointer();
                            ptr += (algorithm.StepWidth * (algorithm.StepHeight - 1 - x.Index.Y) + (x.Index.X)) * 4;
                            *ptr++ = pixelColor.Red;   // red
                            *ptr++ = pixelColor.Green; // green
                            *ptr++ = pixelColor.Blue;  // blue
                            *ptr++ = 0xFF;             // alpha
                        }

                        if (progressCallback != null && progressCallbackIntervalSec > 0 && reportTimer.Elapsed.TotalSeconds > progressCallbackIntervalSec)
                        {
                            progressCallback(new ProgressReport(
                                totalTimer.Elapsed.TotalSeconds,
                                iterationCount,
                                points.Count,
                                $"{nameof(Scene)}.{nameof(ProcessPointsToPixels)} ({nameof(IEscapeAlgorithm.UseHistogram)}==true) writing image"
                                ));

                            reportTimer.Restart();
                        }

                        iterationCount++;
                    }
                });
            }
            else
            {
                iterationCount = 0;

                //var writeTime = Stopwatch.StartNew();

                Parallel.ForEach(points, (x, loopState) =>
                {
                    if (token.IsCancellationRequested)
                    {
                        loopState.Break();
                    }

                    SKColor pixelColor = ColorRef.White;

                    if (x.IsStable == true)
                    {
                        pixelColor = StableColor;
                    }
                    else
                    {
                        pixelColor = ResolveColorByIterations(x.IterationCount, algorithm.MaxIterations);
                    }

                    lock (_lockObject)
                    {
                        /** writing pixels for (1024 x 1024) image.
                         * 
                         * slow: ~ 8.5 sec
                         * _bmp.SetPixel(x.Index.X, algorithm.StepHeight - 1 - x.Index.Y, pixelColor);
                         * 
                         * fast (below) ~ 0.5 sec
                         * 
                         * https://learn.microsoft.com/en-us/xamarin/xamarin-forms/user-interface/graphics/skiasharp/bitmaps/pixel-bits
                         */

                        // fast:
                        unsafe
                        {
                            IntPtr pixelsAddr = bmp.GetPixels();
                            byte* ptr = (byte*)pixelsAddr.ToPointer();
                            // ((vertical offset times width) + horizontal offset) * size of pixel
                            ptr += (algorithm.StepWidth * (algorithm.StepHeight - 1 - x.Index.Y) + (x.Index.X)) * 4;
                            *ptr++ = pixelColor.Red;   // red
                            *ptr++ = pixelColor.Green; // green
                            *ptr++ = pixelColor.Blue;  // blue
                            *ptr++ = 0xFF;             // alpha
                        }

                        if (progressCallback != null && progressCallbackIntervalSec > 0 && reportTimer.Elapsed.TotalSeconds > progressCallbackIntervalSec)
                        {
                            progressCallback(new ProgressReport(
                                totalTimer.Elapsed.TotalSeconds,
                                iterationCount,
                                points.Count,
                                $"{nameof(Scene)}.{nameof(ProcessPointsToPixels)} ({nameof(IEscapeAlgorithm.UseHistogram)}==false) writing image"
                                ));

                            reportTimer.Restart();
                        }

                        iterationCount++;
                    }
                });

                //Console.WriteLine($"write image: {writeTime.Elapsed.TotalSeconds:N2} sec");
            }

            if (token.IsCancellationRequested)
            {
                return null;
            }

            return bmp;
        }

        /// <summary>
        /// Converts histogram percent value to color value.
        /// </summary>
        /// <param name="percent">Value between zero and one.</param>
        /// <returns>Color according to colorramp.</returns>
        public SKColor ResolveColorByPercent(double percent)
        {
            if (percent < 0)
            {
                percent = 0;
            }
            else if (percent > 1)
            {
                percent = 1;
            }

            return ColorRamp.InterpolateFromKeyframes(percent);
        }

        /// <summary>
        /// Converts iteration percent to color value.
        /// </summary>
        /// <param name="iterationCount">Iteration count of point.</param>
        /// <param name="maxIterations">Max iterations defined by algorithm.</param>
        /// <returns>Color according to colorramp.</returns>
        public SKColor ResolveColorByIterations(int iterationCount, int maxIterations)
        {
            double percent;

            percent = (double)iterationCount / (double)maxIterations;

            if (percent < 0)
            {
                percent = 0;
            }
            else if (percent > 1)
            {
                percent = 1;
            }

            return ColorRamp.InterpolateFromKeyframes(percent);
        }

        /// <summary>
        /// Adds the default keyframes to <see cref="ColorRamp.Keyframes"/>.
        /// </summary>
        public void AddDefaultSceneKeyframes()
        {
            ColorRamp.Keyframes.Add(new Keyframe<SKColor, double>()
            {
                IntervalStart = 0,
                IntervalEnd = 0.88,
                ValueStart = new SKColor(0, 0, 0), // black
                ValueEnd = new SKColor(20, 250, 250), // cyan
            });
            ColorRamp.Keyframes.Add(new Keyframe<SKColor, double>()
            {
                IntervalStart = 0.88,
                IntervalEnd = 0.97,
                ValueStart = new SKColor(20, 250, 250), // cyan
                ValueEnd = new SKColor(255, 255, 40), // yellow
            });
            ColorRamp.Keyframes.Add(new Keyframe<SKColor, double>()
            {
                IntervalStart = 0.97,
                IntervalEnd = 0.99,
                ValueStart = new SKColor(255, 255, 40), // yellow
                ValueEnd = new SKColor(250, 128, 0), // orange
            });
            ColorRamp.Keyframes.Add(new Keyframe<SKColor, double>()
            {
                IntervalStart = 0.99,
                IntervalEnd = 1,
                ValueStart = new SKColor(250, 128, 0), // orange
                ValueEnd = new SKColor(120, 60, 0), // orange
            });
        }
    }
}
