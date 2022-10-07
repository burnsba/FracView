using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SkiaSharp;
using FracView.Algorithms;
using FracView.Gfx;
using MultiPrecision;

namespace FracViewCmd
{
    internal class Program
    {
        private static int _outputIntervalSec = 4;

        static void Main(string[] args)
        {
            var algorithm = new Mandelbrot()
            {
                Origin = (0.29999999799999, 0.4491),
                FractalWidth = 0.0250,
                FractalHeight = 0.0250,
                StepWidth = 1024,
                StepHeight = 1024,
                MaxIterations = 1200,
            };

            var scene = new Scene(algorithm.MaxIterations)
            {
                RenderWidth = algorithm.StepWidth,
                RenderHeight = algorithm.StepHeight,
            };

            scene.ColorRamp.Keyframes.Add(new Keyframe<SKColor, double>()
            {
                IntervalStart = 0,
                IntervalEnd = 0.88,
                ValueStart = new SKColor(20, 20, 240), // blue
                ValueEnd = new SKColor(20, 250, 250), // cyan
            });
            scene.ColorRamp.Keyframes.Add(new Keyframe<SKColor, double>()
            {
                IntervalStart = 0.88,
                IntervalEnd = 0.93,
                ValueStart = new SKColor(20, 250, 250), // cyan
                ValueEnd = new SKColor(255, 255, 40), // yellow
            });
            scene.ColorRamp.Keyframes.Add(new Keyframe<SKColor, double>()
            {
                IntervalStart = 0.93,
                IntervalEnd = 0.97,
                ValueStart = new SKColor(255, 255, 40), // yellow
                ValueEnd = new SKColor(250, 128, 0), // orange
            });
            scene.ColorRamp.Keyframes.Add(new Keyframe<SKColor, double>()
            {
                IntervalStart = 0.97,
                IntervalEnd = 1,
                ValueStart = new SKColor(250, 128, 0), // orange
                ValueEnd = new SKColor(120, 60, 0), // orange
            });

            algorithm.Init(_outputIntervalSec, PrintProgress);
            scene.Init();

            var cancellationToken = new CancellationTokenSource();

            algorithm.EvaluatePoints(cancellationToken.Token, _outputIntervalSec, PrintProgress);

            scene.ProcessPointsToPixels(algorithm, _outputIntervalSec, PrintProgress);

            var bmp = scene.GetImage();
            //bmp.Save("output2.png");
            using (MemoryStream memStream = new MemoryStream())
            using (SKManagedWStream wstream = new SKManagedWStream(memStream))
            {
                bmp.Encode(wstream, SKEncodedImageFormat.Png, 100);
                byte[] data = memStream.ToArray();
                System.IO.File.WriteAllBytes("output2.png", data);
            }
        }

        static void PrintProgress(ProgressReport progress)
        {
            double donePercent = 100.0 * (double)progress.CurrentStep / (double)progress.TotalSteps;
            Console.WriteLine($"work: {progress.CurrentWorkName}");
            Console.WriteLine($"elapsed: {progress.ElapsedSeconds:N2} sec");
            Console.WriteLine($"pixels: {progress.CurrentStep} / {progress.TotalSteps} = {donePercent:N2}%");
            Console.WriteLine($"point: [{progress.CurrentPoint.Real}, {progress.CurrentPoint.Imag}]");
            Console.WriteLine();
        }

        static void PrintProgress<T>(ProgressReportMp<T> progress) where T : struct, MultiPrecision.IConstant
        {
            double donePercent = 100.0 * (double)progress.CurrentStep / (double)progress.TotalSteps;
            Console.WriteLine($"work: {progress.CurrentWorkName}");
            Console.WriteLine($"elapsed: {progress.ElapsedSeconds:N2} sec");
            Console.WriteLine($"pixels: {progress.CurrentStep} / {progress.TotalSteps} = {donePercent:N2}%");
            Console.WriteLine($"point: [{progress.CurrentPoint.Real}, {progress.CurrentPoint.Imag}]");
            Console.WriteLine();
        }
    }
}
