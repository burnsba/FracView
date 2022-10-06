using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FracView.Algorithms;
using FracView.Gfx;

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
                StepWidth = 4096,
                StepHeight = 4096,
                MaxIterations = 2000,
            };

            var scene = new Scene(algorithm.MaxIterations)
            {
                RenderWidth = algorithm.StepWidth,
                RenderHeight = algorithm.StepHeight,
            };

            scene.ColorRamp.Keyframes.Add(new Keyframe<Color, double>()
            {
                IntervalStart = 0,
                IntervalEnd = 0.88,
                ValueStart = Color.FromArgb(20, 20, 240), // blue
                ValueEnd = Color.FromArgb(20, 250, 250), // cyan
            });
            scene.ColorRamp.Keyframes.Add(new Keyframe<Color, double>()
            {
                IntervalStart = 0.88,
                IntervalEnd = 0.93,
                ValueStart = Color.FromArgb(20, 250, 250), // cyan
                ValueEnd = Color.FromArgb(255, 255, 40), // yellow
            });
            scene.ColorRamp.Keyframes.Add(new Keyframe<Color, double>()
            {
                IntervalStart = 0.93,
                IntervalEnd = 0.97,
                ValueStart = Color.FromArgb(255, 255, 40), // yellow
                ValueEnd = Color.FromArgb(250, 128, 0), // orange
            });
            scene.ColorRamp.Keyframes.Add(new Keyframe<Color, double>()
            {
                IntervalStart = 0.97,
                IntervalEnd = 1,
                ValueStart = Color.FromArgb(250, 128, 0), // orange
                ValueEnd = Color.FromArgb(120, 60, 0), // orange
            });

            algorithm.Init(_outputIntervalSec, PrintProgress);
            scene.Init();

            var cancellationToken = new CancellationTokenSource();

            algorithm.EvaluatePoints(cancellationToken.Token, _outputIntervalSec, PrintProgress);

            scene.ProcessPointsToPixels(algorithm, _outputIntervalSec, PrintProgress);

            var bmp = scene.GetImage();
            bmp.Save("output2.png");
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
    }
}
