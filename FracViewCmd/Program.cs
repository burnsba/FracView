using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SkiaSharp;
using FracView.Algorithms;
using FracView.Gfx;
using FracView;

namespace FracViewCmd
{
    internal class Program
    {
        private static int _outputIntervalSec = 4;

        static void Main(string[] args)
        {
            var algorithm = new Mandelbrot(
                _outputIntervalSec,
                PrintProgress)
            {
                /*
                Origin = (decimal.Parse("0.29999999799999"), decimal.Parse("0.4491000000000016")),
                FractalWidth = decimal.Parse("0.00000000000000250"),
                FractalHeight = decimal.Parse("0.00000000000000250"),
                */
                Origin = (decimal.Parse("0.29999999799999"), decimal.Parse("0.4491000000000016")),
                FractalWidth = decimal.Parse("0.00000000000000250"),
                FractalHeight = decimal.Parse("0.00000000000000250"),
                StepWidth = 1024,
                StepHeight = 1024,
                MaxIterations = 5000,
                UseHistogram = true,
            };

            var scene = new Scene();

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

            var cancellationToken = new CancellationTokenSource();

            algorithm.EvaluatePoints(cancellationToken.Token);

            scene.ProcessPointsToPixels(algorithm, "output2.png", _outputIntervalSec, PrintProgress);
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
}
