//using System;
//using System.Diagnostics;
//using System.Drawing;
//using System.Drawing.Imaging;

//namespace FracViewCmd
//{
//    internal class OldProgram
//    {
//        const int MaxIterations = 5000;

//        private static ColorRamp _colorRamp = new ColorRamp();

//        static void OldMain(string[] args)
//        {
//            _colorRamp.Keyframes.Add(new Keyframe<Color, double>()
//            {
//                IntervalStart = 0,
//                IntervalEnd = 0.88,
//                ValueStart = Color.FromArgb(20, 20, 240), // blue
//                ValueEnd = Color.FromArgb(20, 250, 250), // cyan
//            });
//            _colorRamp.Keyframes.Add(new Keyframe<Color, double>()
//            {
//                IntervalStart = 0.88,
//                IntervalEnd = 0.93,
//                ValueStart = Color.FromArgb(20, 250, 250), // cyan
//                ValueEnd = Color.FromArgb(255, 255, 40), // yellow
//            });
//            _colorRamp.Keyframes.Add(new Keyframe<Color, double>()
//            {
//                IntervalStart = 0.93,
//                IntervalEnd = 0.97,
//                ValueStart = Color.FromArgb(255, 255, 40), // yellow
//                ValueEnd = Color.FromArgb(250, 128, 0), // orange
//            });
//            _colorRamp.Keyframes.Add(new Keyframe<Color, double>()
//            {
//                IntervalStart = 0.97,
//                IntervalEnd = 1,
//                ValueStart = Color.FromArgb(250, 128, 0), // orange
//                ValueEnd = Color.FromArgb(120, 60, 0), // orange
//            });
            

//            //int i, j;
//            double originX = 0.29999999799999;
//            double originY = 0.4491;

//            /*
//            double fractalHeight = 0.000000000125;
//            double fractalWidth = 0.000000000250;
//            */

//            double fractalHeight = 0.000000000125;
//            double fractalWidth = 0.000000000250;

//            //int renderWidth = 16384;
//            //int renderHeight = 8192;

//            int renderWidth = 16384;
//            int renderHeight = 8192;

//            // 2048x2048  ~ 10 sec

//            var lockObj = new object();

//            Process proc = Process.GetCurrentProcess();
//            long memoryUsage = proc.PrivateMemorySize64;

//            DateTime startTime = DateTime.Now;
//            double runtimeElapsed;
//            DateTime lastConsoleOutput = DateTime.MinValue;
//            int outputInterval = 10;

//            int pixelCount = 0;
//            int totalPixels = renderWidth * renderHeight;

//            Bitmap? bmp = null;
//            List<EvalUnit> consideredPoints = null;
//            int[] numIterationsPerPixel = null;

//            Console.WriteLine("Allocating memory.");

//            try
//            {
//                bmp = new Bitmap(renderWidth, renderHeight, PixelFormat.Format16bppArgb1555);
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine("Unable to allocate bitmap");
//                Console.WriteLine(ex.Message);

//                Environment.Exit(1);
//            }

//            try
//            {
//                consideredPoints = new List<EvalUnit>(totalPixels);

//                double startX = originX - (fractalWidth / 2);
//                double stepX = fractalWidth / (double)renderWidth;
//                double x;

//                double startY = originY - (fractalHeight / 2);
//                double stepY = fractalHeight / (double)renderHeight;
//                double y;

//                y = startY;
//                x = startX;

//                for (int j = 0; j < renderHeight; j++)
//                {
//                    x = startX;

//                    for (int i = 0; i < renderWidth; i++)
//                    {
//                        var eu = new EvalUnit()
//                        {
//                            RowX = i,
//                            ColY = j,
//                            WorldX = x,
//                            WorldY = y,
//                        };

//                        consideredPoints.Add(eu);

//                        x += stepX;
//                    }

//                    y += stepY;
//                }
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine("Unable to allocate evaluation grid for processing.");
//                Console.WriteLine(ex.Message);

//                Environment.Exit(1);
//            }

//            try
//            {
//                numIterationsPerPixel = Enumerable.Repeat(0, MaxIterations + 1).ToArray();
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine("Unable to allocate evaluation numIterationsPerPixel container.");
//                Console.WriteLine(ex.Message);

//                Environment.Exit(1);
//            }

//            Console.WriteLine("Evaluating points.");

//            //for (i = 0; i < renderWidth; i++)
//            //{
//            //Parallel.For(0, totalPixels, pindex => {
//            Parallel.ForEach(consideredPoints, evalPoint => { 

//                int iterationCount;
//                double last_x;
//                double last_y;

//                lock (lockObj)
//                {
//                    if (Math.Abs((DateTime.Now - lastConsoleOutput).TotalSeconds) > (double)outputInterval)
//                    {
//                        runtimeElapsed = (DateTime.Now - startTime).TotalSeconds;
//                        lastConsoleOutput = DateTime.Now;
//                        double donePercent = 100.0 * (double)pixelCount / (double)totalPixels;
//                        Console.WriteLine($"elapsed: {runtimeElapsed:N2} sec");
//                        Console.WriteLine($"pixels: {pixelCount} / {totalPixels} = {donePercent:N2}%");
//                        Console.WriteLine($"point: [{evalPoint.WorldX}, {evalPoint.WorldY}]");
//                        Console.WriteLine();
//                    }
//                }

//                evalPoint.IsStable = IsStable(evalPoint.WorldX, evalPoint.WorldY, MaxIterations, 12.0, out iterationCount, out last_x, out last_y);
//                evalPoint.IterationCount = iterationCount;
//                evalPoint.LastReal = last_x;
//                evalPoint.LastImag = last_y;

//                //Color pixelColor = Color.White;

//                //if (evalPoint.IsStable == true)
//                //{
//                //    pixelColor = Color.Black;
//                //}
//                //else
//                //{
//                //    pixelColor = ResolveColor(iterationCount);
//                //}

//                lock (lockObj)
//                {
//                    // image rows increase downwards, opposite of graph
//                    //bmp.SetPixel(evalPoint.RowX, renderHeight - 1 - evalPoint.ColY, pixelColor);

//                    if (proc.PrivateMemorySize64 > memoryUsage)
//                    {
//                        memoryUsage = proc.PrivateMemorySize64;
//                    }

//                    pixelCount++;
//                }
//            });
//            //}

//            var processPointsElapsed = (DateTime.Now - startTime).TotalSeconds;

//            Console.WriteLine($"Finished processing points, time: {processPointsElapsed:N2} sec");
//            Console.WriteLine("Creating histogram");

//            int minIterationCount = int.MaxValue;
//            int maxIterationCount = 1;
//            long iterationsTotal = 0;

//            consideredPoints.ForEach(x =>
//            {
//                if (x.IterationCount > 0 && x.IterationCount < minIterationCount)
//                {
//                    minIterationCount = x.IterationCount;
//                }

//                if (x.IterationCount > 1 && x.IterationCount > maxIterationCount)
//                {
//                    maxIterationCount = x.IterationCount;
//                }

//                numIterationsPerPixel[x.IterationCount]++;
//            });

//            for (int i=0; i<MaxIterations; i++)
//            {
//                iterationsTotal += numIterationsPerPixel[i];
//            }

//            consideredPoints.ForEach(x =>
//            {
//                for (int i = 0; i < x.IterationCount; i++)
//                {
//                    x.HistogramValue += (double)numIterationsPerPixel[i] / (double)iterationsTotal;
//                }
//            });

//            Console.WriteLine("Writing image ");

//            consideredPoints.ForEach(x =>
//            {
//                Color pixelColor = Color.White;

//                if (x.IsStable == true)
//                {
//                    pixelColor = Color.Black;
//                }
//                else
//                {
//                    pixelColor = ResolveColorHue(x.HistogramValue);
//                }

//                bmp.SetPixel(x.RowX, renderHeight - 1 - x.ColY, pixelColor);
//            });

//            bmp.Save("output.png");
//            bmp.Dispose();
//            bmp = null;

//            DateTime endTime = DateTime.Now;

//            Console.WriteLine("Done.");
//            Console.WriteLine($"Max memory usage: {memoryUsage}");
//            runtimeElapsed = (endTime - startTime).TotalSeconds;
//            Console.WriteLine($"total runtime: {runtimeElapsed:N2} sec");
//        }

//        static Color GrayscaleColor(int iterationCount)
//        {
//            double percent = (double)iterationCount / (double)MaxIterations;
//            int c;

//            if (percent < 0)
//            {
//                percent = 0;
//            }
//            else if (percent > 1.0)
//            {
//                percent = 1.0;
//            }

//            c = (int)(255 * percent);
//            return Color.FromArgb(c, c, c);
//        }

//        static Color ResolveColorHue(double hv)
//        {
//            return _colorRamp.InterpolateFromKeyframes(hv);
//            /*
//            int c;
//            double percent;

//            if (hv > 0.90)
//            {
//                return Color.White;
//            }
//            else if (hv > 0.80)
//            {
//                percent = (double)(hv - 0.80) / 0.20;
//                c = 127 + (int)(percent * 128);
//                return Color.FromArgb(240, c, 20);
//            }
//            else
//            {
//                percent = (double)(hv) / 0.80;
//                c = 40 + (int)(percent * 200);
//                return Color.FromArgb(0, c, 240);
//            }
//            */
//            /*
//            if (percent < 0)
//            {
//                percent = 0;
//            }
//            else if (percent > 1.0)
//            {
//                percent = 1.0;
//            }

//            if (percent < 0.95)
//            {
//                return ColorFromHSV(240, 1.0 - (percent / 0.95), 0.9);
//            }
//            else
//            {
//                return ColorFromHSV(60, (percent - 0.95) / 0.05, 0.9);
//            }
//            */
//            /*
//            int c;

//            if (percent < 0)
//            {
//                percent = 0;
//            }
//            else if (percent > 1.0)
//            {
//                percent = 1.0;
//            }

//            if (percent < 0.25)
//            {
//                c = (int)(255 * percent * (1 / 0.25));
//                if (c > 255)
//                {
//                    c = 255;
//                }
//                return Color.FromArgb(20, 20, c);
//            }
//            else if (percent < 0.5)
//            {
//                c = (int)(255 * (percent - 0.25) * (1 / 0.5));
//                if (c > 255)
//                {
//                    c = 255;
//                }
//                return Color.FromArgb(20, c, 240);
//            }
//            else if (percent < 0.75)
//            {
//                c = (int)(255 * (percent - 0.5) * (1 / 0.75));
//                if (c > 255)
//                {
//                    c = 255;
//                }

//                int g = 255 - c;
//                if (g < 0)
//                {
//                    g = 0;
//                }

//                int b = 240 - c;
//                if (b < 0)
//                {
//                    b = 0;
//                }

//                return Color.FromArgb(c, 240, b);
//            }
//            else
//            {
//                c = 20 + (int)(255 * (percent - 0.75));

//                if (c > 255)
//                {
//                    c = 255;
//                }

//                return Color.FromArgb(240, 150, c);
//            }
//            */
//        }

//        static Color ResolveColor(int iterationCount)
//        {
//            double percent;

//            //if (MaxIterations < 5000)
//            //{
//                percent = (double)iterationCount / (double)MaxIterations;
//            //}
//            //else
//            //{
//            //    percent = (double)iterationCount / (double)5000;
//            //}

//            int c;

//            if (percent < 0)
//            {
//                percent = 0;
//            }
//            else if (percent > 1.0)
//            {
//                percent = 1.0;
//            }

//            if (percent < 0.05)
//            {
//                c = (int)(255 * percent * 20);
//                if (c > 255)
//                {
//                    c = 255;
//                }
//                return Color.FromArgb(20, 20, c);
//            }
//            else if (percent < 0.1)
//            {
//                c = (int)(255 * (percent - 0.05) * 33.3);
//                if (c > 255)
//                {
//                    c = 255;
//                }
//                return Color.FromArgb(20, c, 240);
//            }
//            else if (percent < 0.20)
//            {
//                c = (int)(255 * (percent - 0.1) * 10);
//                if (c > 255)
//                {
//                    c = 255;
//                }

//                int g = 255 - c;
//                if (g < 0)
//                {
//                    g = 0;
//                }

//                int b = 240 - c;
//                if (b < 0)
//                {
//                    b = 0;
//                }

//                return Color.FromArgb(c, 240, b);
//            }
//            else
//            {
//                c = 20 + (int)(255 * (percent - 0.2));

//                if (c > 255)
//                {
//                    c = 255;
//                }

//                return Color.FromArgb(240, 150, c);
//            }
//        }

//        static bool IsStable(double p_real, double p_imag, int maxIterations, double iterationBreak, out int iterationCount, out double last_x, out double last_y)
//        {
//            double pa_x = p_real;
//            double pa_y = p_imag;
//            double pb_x = 0;
//            double pb_y = 0;
//            double break_value;

//            for (iterationCount = 1; iterationCount < maxIterations; iterationCount++)
//            {
//                pb_x = pa_x * pa_x - pa_y * pa_y;
//                pb_y = pa_x * pa_y + pa_y * pa_x;

//                pa_x = pb_x + p_real;
//                pa_y = pb_y + p_imag;

//                break_value = Math.Sqrt((pa_x * pa_x) + (pa_y * pa_y));

//                if (break_value > iterationBreak || break_value <= 0.0)
//                {
//                    last_x = pa_x;
//                    last_y = pa_y;
                    
//                    return false;
//                }
//            }

//            last_x = pa_x;
//            last_y = pa_y;

//            return true;
//        }

//        /// <summary>
//        /// Single grid square to evaluate point at.
//        /// </summary>
//        public class EvalUnit
//        {
//            /// <summary>
//            /// Creation row number (~ pixel offset)
//            /// </summary>
//            public int RowX { get; set; }

//            /// <summary>
//            /// Creation column number (~ pixel offset)
//            /// </summary>
//            public int ColY { get; set;  }

//            /// <summary>
//            /// Evaluation point x position.
//            /// </summary>
//            public double WorldX { get; set; }

//            /// <summary>
//            /// Evaluation point y position.
//            /// </summary>
//            public double WorldY { get; set; }

//            /// <summary>
//            /// Whether or not the point escaped within the max number of iterations.
//            /// </summary>
//            public bool? IsStable { get; set; } = null;

//            /// <summary>
//            /// Iteration count the point escaped at.
//            /// </summary>
//            public int IterationCount { get; set; }

//            /// <summary>
//            /// Last real value the point evaluated to before escape.
//            /// </summary>
//            public double LastReal { get; set; }

//            /// <summary>
//            /// Last imaginary value the point evaluated to before escape.
//            /// </summary>
//            public double LastImag { get; set; }

//            public double HistogramValue { get; set; } = 0.0;
//        }

//        // https://stackoverflow.com/a/1626175/1462295
//        public static void ColorToHSV(Color color, out double hue, out double saturation, out double value)
//        {
//            int max = Math.Max(color.R, Math.Max(color.G, color.B));
//            int min = Math.Min(color.R, Math.Min(color.G, color.B));

//            hue = color.GetHue();
//            saturation = (max == 0) ? 0 : 1d - (1d * min / max);
//            value = max / 255d;
//        }

//        // https://stackoverflow.com/a/1626175/1462295
//        public static Color ColorFromHSV(double hue, double saturation, double value)
//        {
//            int hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
//            double f = hue / 60 - Math.Floor(hue / 60);

//            value = value * 255;
//            int v = Convert.ToInt32(value);
//            int p = Convert.ToInt32(value * (1 - saturation));
//            int q = Convert.ToInt32(value * (1 - f * saturation));
//            int t = Convert.ToInt32(value * (1 - (1 - f) * saturation));

//            if (hi == 0)
//                return Color.FromArgb(255, v, t, p);
//            else if (hi == 1)
//                return Color.FromArgb(255, q, v, p);
//            else if (hi == 2)
//                return Color.FromArgb(255, p, v, t);
//            else if (hi == 3)
//                return Color.FromArgb(255, p, q, v);
//            else if (hi == 4)
//                return Color.FromArgb(255, t, p, v);
//            else
//                return Color.FromArgb(255, v, p, q);
//        }

//        public class Keyframe<TKey, TInterval>
//        {
//            public TInterval IntervalStart { get; set; }
//            public TInterval IntervalEnd { get; set; }
//            public TKey ValueStart { get; set; }
//            public TKey ValueEnd { get; set; }
//        }

//        public class ColorRamp
//        {
//            public List<Keyframe<Color, double>> Keyframes { get; set; } = new List<Keyframe<Color, double>>();

//            public Color InterpolateFromKeyframes(double value)
//            {
//                if (value < 0)
//                {
//                    value = 0;
//                }
//                else if (value > 1)
//                {
//                    value = 1;
//                }

//                var kf = Keyframes.FirstOrDefault(x => value >= x.IntervalStart && value <= x.IntervalEnd);
//                if (object.ReferenceEquals(null, kf))
//                {
//                    throw new InvalidOperationException("keyframe not found for value");
//                }

//                int rval = (int)((double)(kf.ValueEnd.R - kf.ValueStart.R) * value) + kf.ValueStart.R;
//                if (rval < 0)
//                {
//                    rval = 0;
//                }
//                else if (rval > 255)
//                {
//                    rval = 255;
//                }

//                int gval = (int)((double)(kf.ValueEnd.G - kf.ValueStart.G) * value) + kf.ValueStart.G;
//                if (gval < 0)
//                {
//                    gval = 0;
//                }
//                else if (gval > 255)
//                {
//                    gval = 255;
//                }

//                int bval = (int)((double)(kf.ValueEnd.B - kf.ValueStart.B) * value) + kf.ValueStart.B;
//                if (bval < 0)
//                {
//                    bval = 0;
//                }
//                else if (bval > 255)
//                {
//                    bval = 255;
//                }

//                return Color.FromArgb(rval, gval, bval);
//            }
//        }
//    }
//}