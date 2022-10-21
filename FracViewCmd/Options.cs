using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine.Text;
using CommandLine;

namespace FracViewCmd
{

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public class Options
    {
        [Option('x', "originx", Required = true, HelpText = "World origin x location.")]
        public decimal OriginX { get; set; }

        [Option('y', "originy", Required = true, HelpText = "World origin y location.")]
        public decimal OriginY { get; set; }

        [Option("fractalwidth", Required = true, HelpText = "Fractal width.")]
        public decimal FractalWidth { get; set; }

        [Option("fractalheight", Required = true, HelpText = "Fractal height.")]
        public decimal FractalHeight { get; set; }

        [Option("stepwidth", Required = true, HelpText = "Set output pixel resolution width.")]
        public int StepWidth { get; set; }

        [Option("stepheight", Required = true, HelpText = "Set output pixel resolution height.")]
        public int StepHeight { get; set; }

        [Option('i', "maxiterations", Required = true, HelpText = "Max number of iterations.")]
        public int MaxIterations { get; set; }

        [Option('h', "usehistogram", Default = true, HelpText = "Whether or not to calculate histogram data.")]
        public bool UseHistogram { get; set; }

        [Option('f', "format", Default = "png", HelpText = "Output file format. Supported formats: bmp,gif,jpg,png.")]
        public string OutputFormat { get; set; }

        [Option("report", Default = 5, HelpText = "Progress report interval in seconds.")]
        public int ProgressReportIntervalSeconds { get; set; }

        [Option('q', "quiet", Default = false, HelpText = "Disable output progress.")]
        public bool Quiet { get; set; }

        // Suggested output filename.
        [Option('o', "output", HelpText = "Output filename. Defaults to run timestamp. Provided extension will be ignored; resolved based on format.")]
        public string UserOutputFilename { get; set; }

        [Option('m', "meta", Default = false, HelpText = "Write metadata file in addition to image.")]
        public bool WriteMetaData { get; set; }

        [Option('a', "algorithm", Default = "FracView.Algorithms.MandelbrotDouble", HelpText = "Name of class to instantiate. FracView.Algorithms namespace is assumed.")]
        public string AlgorithmClassName { get; set; }

        // Actual output filename used by the application.
        public string OutputFilename { get; set; }

        public Type AlgorithmType { get; set; }

        [Usage(ApplicationAlias = "FracViewCmd")]
        public static IEnumerable<Example> Examples
        {
            get
            {
                return new List<Example>()
                {
                    new Example("Example", new Options
                    {
                        OriginX = 0.29999999799999m,
                        OriginY = 0.4491000000000016m,
                        FractalWidth = 0.250m,
                        FractalHeight = 0.250m,
                        StepWidth = 512,
                        StepHeight = 512,
                        MaxIterations = 1000,
                        UseHistogram = true
                    })
                };
            }
        }
    }

#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
}
