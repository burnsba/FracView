using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FracView.Algorithms;
using SkiaSharp;

namespace FracView.Gfx
{
    public class Scene : IDisposable
    {
        private bool _isInit = false;
        private object _lockObject = new object();

        private SKBitmap? _bmp = null;

        public int RenderWidth { get; set; }
        public int RenderHeight { get; set; }

        public bool UseHistogram => HistogramMaxIterations > 0;

        public ColorRamp ColorRamp { get; set; } = new ColorRamp();

        protected int[]? NumIterationsPerPixel = null;

        public int HistogramMaxIterations { get; set; }

        public Scene(int histogramMaxIterations)
        {
            HistogramMaxIterations = histogramMaxIterations;

            if (HistogramMaxIterations < 1)
            {
                HistogramMaxIterations = 0;
            }
        }

        public void Init()
        {
            if (_isInit)
            {
                return;
            }

            try
            {
                _bmp = new SKBitmap(RenderWidth, RenderHeight);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unable to allocate bitmap");
                Console.WriteLine(ex.Message);

                throw;
            }

            if (UseHistogram)
            {
                try
                {
                    NumIterationsPerPixel = Enumerable.Repeat(0, HistogramMaxIterations + 1).ToArray();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Unable to allocate evaluation numIterationsPerPixel container.");
                    Console.WriteLine(ex.Message);

                    throw;
                }
            }

            _isInit = true;
        }

        public void ProcessPointsToPixels(EscapeAlgorithm algorithm, int progressCallbackIntervalSec = 0, Action<ProgressReport>? progressCallback = null)
        {
            if (!_isInit)
            {
                throw new InvalidOperationException($"Call {nameof(Init)} first");
            }

            if (!ColorRamp.Keyframes.Any())
            {
                throw new InvalidOperationException($"{nameof(ColorRamp)} needs at least one keyframe defined");
            }

            var points = algorithm.ConsideredPoints;
            var reportTimer = Stopwatch.StartNew();
            var totalTimer = Stopwatch.StartNew();
            int iterationCount = 0;

            if (UseHistogram)
            {
                int minIterationCount = int.MaxValue;
                int maxIterationCount = 1;
                long iterationsTotal = 0;

                if (object.ReferenceEquals(null, NumIterationsPerPixel))
                {
                    throw new NullReferenceException($"{nameof(NumIterationsPerPixel)} is null, call {nameof(Init)} first");
                }

                if (HistogramMaxIterations > NumIterationsPerPixel.Length)
                {
                    throw new InvalidOperationException($"There are more iterations to evaluate than space in {nameof(NumIterationsPerPixel)}.");
                }

                iterationCount = 0;
                reportTimer.Restart();

                points.ForEach(x =>
                {
                    if (x.IterationCount > 0 && x.IterationCount < minIterationCount)
                    {
                        minIterationCount = x.IterationCount;
                    }

                    if (x.IterationCount > 1 && x.IterationCount > maxIterationCount)
                    {
                        maxIterationCount = x.IterationCount;
                    }

                    NumIterationsPerPixel[x.IterationCount]++;

                    // not parallel, no need to lock.
                    if (progressCallback != null && progressCallbackIntervalSec > 0 && reportTimer.Elapsed.TotalSeconds > progressCallbackIntervalSec)
                    {
                        progressCallback(new ProgressReport(
                            totalTimer.Elapsed.TotalSeconds,
                            iterationCount,
                            points.Count,
                            x.WorldPos,
                            $"{nameof(Scene)}.{nameof(ProcessPointsToPixels)} ({nameof(UseHistogram)}==true, loop=1)"
                            ));

                        reportTimer.Restart();
                    }

                    iterationCount++;
                });

                for (int i = 0; i < HistogramMaxIterations; i++)
                {
                    iterationsTotal += NumIterationsPerPixel[i];
                }

                iterationCount = 0;
                reportTimer.Restart();

                Parallel.ForEach(points, x =>
                {
                    for (int i = 0; i < x.IterationCount; i++)
                    {
                        x.HistogramValue += (double)NumIterationsPerPixel[i] / (double)iterationsTotal;

                    }

                    lock (_lockObject)
                    {
                        if (progressCallback != null && progressCallbackIntervalSec > 0 && reportTimer.Elapsed.TotalSeconds > progressCallbackIntervalSec)
                        {
                            progressCallback(new ProgressReport(
                                totalTimer.Elapsed.TotalSeconds,
                                iterationCount,
                                points.Count,
                                x.WorldPos,
                                $"{nameof(Scene)}.{nameof(ProcessPointsToPixels)} ({nameof(UseHistogram)}==true, loop=3)"
                                ));

                            reportTimer.Restart();
                        }

                        iterationCount++;
                    }
                });

                iterationCount = 0;
                reportTimer.Restart();

                Parallel.ForEach(points, x =>
                {
                    SKColor pixelColor = ColorRef.White;

                    if (x.IsStable == true)
                    {
                        pixelColor = ColorRef.Black;
                    }
                    else
                    {
                        pixelColor = ResolveColorByPercent(x.HistogramValue);
                    }

                    lock (_lockObject)
                    {
                        _bmp.SetPixel(x.Index.X, algorithm.StepHeight - 1 - x.Index.Y, pixelColor);

                        if (progressCallback != null && progressCallbackIntervalSec > 0 && reportTimer.Elapsed.TotalSeconds > progressCallbackIntervalSec)
                        {
                            progressCallback(new ProgressReport(
                                totalTimer.Elapsed.TotalSeconds,
                                iterationCount,
                                points.Count,
                                x.WorldPos,
                                $"{nameof(Scene)}.{nameof(ProcessPointsToPixels)} ({nameof(UseHistogram)}==true) writing image"
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

                Parallel.ForEach(points, x =>
                {
                    SKColor pixelColor = ColorRef.White;

                    if (x.IsStable == true)
                    {
                        pixelColor = ColorRef.Black;
                    }
                    else
                    {
                        pixelColor = ResolveColorByIterations(x.IterationCount, algorithm.MaxIterations);
                    }

                    lock (_lockObject)
                    {
                        _bmp.SetPixel(x.Index.X, algorithm.StepHeight - 1 - x.Index.Y, pixelColor);

                        if (progressCallback != null && progressCallbackIntervalSec > 0 && reportTimer.Elapsed.TotalSeconds > progressCallbackIntervalSec)
                        {
                            progressCallback(new ProgressReport(
                                totalTimer.Elapsed.TotalSeconds,
                                iterationCount,
                                points.Count,
                                x.WorldPos,
                                $"{nameof(Scene)}.{nameof(ProcessPointsToPixels)} ({nameof(UseHistogram)}==false) writing image"
                                ));

                            reportTimer.Restart();
                        }

                        iterationCount++;
                    }
                });
            }
        }

        public SKBitmap GetImage()
        {
            if (object.ReferenceEquals(null, _bmp))
            {
                throw new InvalidOperationException($"Call {nameof(ProcessPointsToPixels)} first");
            }

            return _bmp;
        }

        public void Dispose()
        {
            if (!object.ReferenceEquals(null, _bmp))
            {
                _bmp.Dispose();
                _bmp = null;
            }
        }

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
    }
}
