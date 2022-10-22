# FracView

This library deals with escape algorithm fractals.

World parameters are given to define the scope of the area to plot. This defines the origin, width and height. Scene parameters are given to define the render resolution, and color ramps for rendering results. There is an option to render colors using a histogram or not.

SkiaSharp is used to convert image data to various formats.

This library currently only implements the Mandelbrot algorithm using native C# types.

There are implementations using decimal (more precise), double (faster), and some modifications to the original Mandelbrot algorithm. No arbitrary precision library was found to be sufficiently fast for rendering results.

## Technical notes

### Code layout

**Algorithms**

Contains generic algorithm base class, interface, and concrete implementations of the Mandelbrot algorithms.

**Converters**

Helper classes to transform objects into other objects.

**Dto**

Simple data transfer objects.

**Gfx**

Classes related to rendering results as images.

**World**

Classes used to track position as used by the fractal, and related support for translating to pixel/image space.

### Implementation

**EscapeAlgorithm**

The `EscapeAlgorithm` contains various properties used to define run parameters. These are used to calculate data. The resulting data is stored in `ConsideredPoints`. The `Init` method is called at least once. This will allocate space for all the points. This will divide the "fractal world" coordinates into a "pixel" grid; this mapping is captured in a `EvalComplexUnit`. After `Init`, `EvaluatePoints` can proceed. This will evaluate each individual point through the concrete implementation of `IsStable`. Since points are independent, parallel tasks are used to evaluate all the points. Each point is updated with the result from `IsStable` as it is processed. After evaluating the points, if `UseHistogram` is set then the histogram data will be computed. This requires several passes over the entire set of computed points; each point will have `HistogramValue` set when this completes.


## Use

Declare an instance of the `Mandelbrot` class and set the runtime settings. Declare a `Scene` and set colorramp values. Call `algorithm.EvaluatePoints`, then `scene.ProcessPointsToPixels`.

    internal class Program
    {
        private static int _outputIntervalSec = 2;

        static void Main(string[] args)
        {
            var algorithm = new Mandelbrot(
                _outputIntervalSec,
                PrintProgress)
            {
                Origin = (decimal.Parse("0.29999999799999"), decimal.Parse("0.4491000000000016")),
                FractalWidth = decimal.Parse("0.250"),
                FractalHeight = decimal.Parse("0.250"),
                StepWidth = 512,
                StepHeight = 512,
                MaxIterations = 1000,
                UseHistogram = true,
            };

            var scene = new Scene();

            scene.AddDefaultSceneKeyframes();

            var cancellationToken = new CancellationTokenSource();

            algorithm.EvaluatePoints(cancellationToken.Token);

            var bmp = scene.ProcessPointsToPixels(algorithm, cancellationToken.Token, _outputIntervalSec, PrintProgress);

            using (MemoryStream memStream = new MemoryStream())
            using (SKManagedWStream wstream = new SKManagedWStream(memStream))
            {
                bmp.Encode(wstream, SKEncodedImageFormat.Png, 100);
                byte[] data = memStream.ToArray();
                System.IO.File.WriteAllBytes($"{DateTime.Now.ToString("yyyyMMdd-HHmmss")}.png", data);
            }
        }

        static void PrintProgress(ProgressReport progress)
        {
            double donePercent = 100.0 * (double)progress.CurrentStep / (double)progress.TotalSteps;
            Console.WriteLine($"work: {progress.CurrentWorkName}");
            Console.WriteLine($"elapsed: {progress.ElapsedSeconds:N2} sec");
            Console.WriteLine($"pixels: {progress.CurrentStep} / {progress.TotalSteps} = {donePercent:N2}%");
            Console.WriteLine();
        }
    }
    
.