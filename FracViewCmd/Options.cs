using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine.Text;
using CommandLine;
using FracView.Algorithms;
using SkiaSharp;
using FracView.Dto;

namespace FracViewCmd
{

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public class Options
    {
        // accept either decimal or double (scientific) format
        [Option('x', "originx", HelpText = "World origin x location.")]
        public string UserOriginX { get; set; }

        // accept either decimal or double (scientific) format
        [Option('y', "originy", HelpText = "World origin y location.")]
        public string UserOriginY { get; set; }

        // accept either decimal or double (scientific) format
        [Option("fractalwidth", HelpText = "Fractal width.")]
        public string UserFractalWidth { get; set; }

        // accept either decimal or double (scientific) format
        [Option("fractalheight", HelpText = "Fractal height.")]
        public string UserFractalHeight { get; set; }

        [Option("stepwidth", HelpText = "Set output pixel resolution width.")]
        public int? UserStepWidth { get; set; }

        [Option("stepheight", HelpText = "Set output pixel resolution height.")]
        public int? UserStepHeight { get; set; }

        [Option('i', "maxiterations", HelpText = "Max number of iterations.")]
        public int? UserMaxIterations { get; set; }

        [Option('h', "usehistogram", Default = true, HelpText = "Whether or not to calculate histogram data.")]
        public bool UseHistogram { get; set; }

        [Option('f', "format", Default = "png", HelpText = "Output file format. Supported formats: bmp,gif,jpg,png.")]
        public string UserOutputFormat { get; set; }

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

        [Option('j', "json", HelpText = "Path to json settings file. Compatible with wpf app saved session. Command line options will override json.")]
        public string JsonSettings { get; set; }

        [Option('d', "dir", HelpText = "Default output directory")]
        public string UserOutputDirectory { get; set; }

        // Actual output filename used by the application, without extension.
        public string OutputBaseFilename { get; set; }

        // Actual output extension used by the application.
        public string OutputExtension { get; set; }

        public string OutputFilenameWithExtension => OutputBaseFilename + OutputExtension;

        // Actual output directory used by the application.
        public string OutputDirectory { get; set; }

        public Type AlgorithmType { get; set; }

        public DateTime RunTime { get; set; } = DateTime.Now;

        public SKEncodedImageFormat OutputFormat { get; set; }

        public SessionSettings SessionSettings { get; set; } = new SessionSettings();

        [Usage(ApplicationAlias = "FracViewCmd")]
        public static IEnumerable<Example> Examples
        {
            get
            {
                return new List<Example>()
                {
                    new Example("Example", new Options
                    {
                        UserOriginX = 0.29999999799999m.ToString(),
                        UserOriginY = 0.4491000000000016m.ToString(),
                        UserFractalWidth = 0.250m.ToString(),
                        UserFractalHeight = 0.250m.ToString(),
                        UserStepWidth = 512,
                        UserStepHeight = 512,
                        UserMaxIterations = 1000,
                        UseHistogram = true
                    })
                };
            }
        }
    }

#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
}
