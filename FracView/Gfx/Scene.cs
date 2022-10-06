using FracView.Algorithms;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FracView.Gfx
{
    public class Scene : IDisposable
    {
        private bool _isInit = false;

        private Bitmap? _bmp = null;

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
                _bmp = new Bitmap(RenderWidth, RenderHeight, PixelFormat.Format16bppArgb1555);
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

        public void ProcessPointsToPixels(EscapeAlgorithm algorithm)
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
                });

                for (int i = 0; i < HistogramMaxIterations; i++)
                {
                    iterationsTotal += NumIterationsPerPixel[i];
                }

                points.ForEach(x =>
                {
                    for (int i = 0; i < x.IterationCount; i++)
                    {
                        x.HistogramValue += (double)NumIterationsPerPixel[i] / (double)iterationsTotal;
                    }
                });

                Console.WriteLine("Writing image ");

                points.ForEach(x =>
                {
                    Color pixelColor = Color.White;

                    if (x.IsStable == true)
                    {
                        pixelColor = Color.Black;
                    }
                    else
                    {
                        pixelColor = ResolveColorByPercent(x.HistogramValue);
                    }

                    _bmp.SetPixel(x.Index.X, algorithm.StepHeight - 1 - x.Index.Y, pixelColor);
                });
            }
            else
            {
                points.ForEach(x =>
                {
                    Color pixelColor = Color.White;

                    if (x.IsStable == true)
                    {
                        pixelColor = Color.Black;
                    }
                    else
                    {
                        pixelColor = ResolveColorByIterations(x.IterationCount, algorithm.MaxIterations);
                    }

                    _bmp.SetPixel(x.Index.X, algorithm.StepHeight - 1 - x.Index.Y, pixelColor);
                });
            }
        }

        public Bitmap GetImage()
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

        public Color ResolveColorByPercent(double percent)
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

        public Color ResolveColorByIterations(int iterationCount, int maxIterations)
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
