using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FracView.Algorithms;
using SkiaSharp;
using static System.Formats.Asn1.AsnWriter;

namespace FracView.Gfx
{
    public class Scene
    {
        private object _lockObject = new object();

        public ColorRamp ColorRamp { get; set; } = new ColorRamp();

        public Scene()
        {
        }

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

        public void ProcessPointsToPixels(IEscapeAlgorithm algorithm, string saveToFilename, int progressCallbackIntervalSec = 0, Action<ProgressReport>? progressCallback = null)
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

            using (MemoryStream memStream = new MemoryStream())
            using (SKManagedWStream wstream = new SKManagedWStream(memStream))
            {
                bmp.Encode(wstream, SKEncodedImageFormat.Png, 100);
                byte[] data = memStream.ToArray();
                System.IO.File.WriteAllBytes(saveToFilename, data);
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
