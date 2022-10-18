This library deals with fractals.

World parameters are given to define the scope of the area to plot. This defines the origin, width and height. Scene parameters are given to define the render resolution, and color ramps for rendering results. Each computed pixel result is stored in a `EvalComplexUnit`.

SkiaSharp is used to convert image data to various formats.

------

This library currently only supports the Mandelbrot algorithm.

There are implementations using decimal (more precise), double (faster), and some modifications to the original Mandelbrot algorithm. No arbitrary precision library was found to be sufficiently fast for rendering results.

----

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