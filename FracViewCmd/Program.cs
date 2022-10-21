using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using SkiaSharp;
using FracView.Algorithms;
using FracView.Gfx;
using FracView;
using CommandLine.Text;
using System.Reflection.Metadata.Ecma335;
using System.Diagnostics;
using System.Reflection;

namespace FracViewCmd
{
    internal class Program
    {
        private const string SaveAsDefaultFilename = "mandelbrot";

        static void Main(string[] args)
        {
            var parser = new CommandLine.Parser(with =>
            {
                with.HelpWriter = null;
            });

            var parserResult = parser.ParseArguments<Options>(args);

            parserResult
                .WithParsed(options => CheckRun(parserResult, options))
                .WithNotParsed(errs => DisplayHelp(parserResult, errs));
        }

        /// <summary>
        /// Verifies options are sane, then calls actual run method.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="result"></param>
        /// <param name="opts"></param>
        private static void CheckRun<T>(ParserResult<T> result, object opts)
        {
            var options = opts as Options;
            if (object.ReferenceEquals(null, options))
            {
                // this shouldn't happen ...
                DisplayHelp(result, new List<Error>());
                Environment.Exit(1);
            }

            // Find available implementations of EscapeAlgorithm
            var availableAlgorithmTypes = Assembly
                .GetAssembly(typeof(FracView.Algorithms.EscapeAlgorithm))!
                .GetTypes()
                .Where(x =>
                    !x.IsAbstract
                    && typeof(FracView.Algorithms.EscapeAlgorithm).IsAssignableFrom(x))
                .ToList();

            var cmdSettings = new CmdRunSettings();

            /* begin validation */

            if (options.StepWidth < 0)
            {
                ConsoleColor.ConsoleWriteLineRed($"{nameof(options.StepWidth)} value must be positive integer: {options.StepWidth}");
                DisplayHelp(result, new List<Error>());
                Environment.Exit(1);
            }

            if (options.StepHeight < 0)
            {
                ConsoleColor.ConsoleWriteLineRed($"{nameof(options.StepHeight)} value must be positive integer: {options.StepHeight}");
                DisplayHelp(result, new List<Error>());
                Environment.Exit(1);
            }

            if (options.MaxIterations < 0)
            {
                ConsoleColor.ConsoleWriteLineRed($"{nameof(options.MaxIterations)} value must be positive integer: {options.MaxIterations}");
                DisplayHelp(result, new List<Error>());
                Environment.Exit(1);
            }

            /* validate filename and extension */

            var outputFormat = options.OutputFormat;
            if (!outputFormat.StartsWith('.'))
            {
                outputFormat = "." + outputFormat;
            }

            try
            {
                cmdSettings.OutputFormat = FracView.Converters.SkiaConverters.ExtensionToFormat(outputFormat);
            }
            catch (NotSupportedException)
            {
                ConsoleColor.ConsoleWriteLineRed($"Unsupported format: {options.OutputFormat}");
                DisplayHelp(result, new List<Error>());
                Environment.Exit(1);
            }

            var expectedExtension = FracView.Converters.SkiaConverters.FormatToExtension(cmdSettings.OutputFormat);

            if (string.IsNullOrEmpty(options.UserOutputFilename))
            {
                options.OutputFilename = SaveAsDefaultFilename
                    + cmdSettings.RunTime.ToString("yyyyMMdd-HHmmss")
                    + expectedExtension;
            }
            else
            {
                options.OutputFilename = options.UserOutputFilename;
            }

            var currentExtension = System.IO.Path.GetExtension(options.OutputFilename);
            if (!string.IsNullOrEmpty(currentExtension) && string.Compare(expectedExtension, expectedExtension, true) != 0)
            {
                int place = options.OutputFilename.LastIndexOf(currentExtension);
                if (place > -1)
                {
                    options.OutputFilename = options.OutputFilename.Remove(place, currentExtension.Length).Insert(place, expectedExtension);
                }
            }

            /* check that filename is sane. */
            File.Create(options.OutputFilename).Dispose();
            if (File.Exists(options.OutputFilename))
            {
                File.Delete(options.OutputFilename);
            }

            /* validate algorithm name */
            var fullname = options.AlgorithmClassName;
            if (!fullname.StartsWith("FracView.Algorithms."))
            {
                fullname = "FracView.Algorithms." + fullname;
            }

            var algorithmType = availableAlgorithmTypes.FirstOrDefault(x => x.FullName == fullname);
            if (object.ReferenceEquals(null, algorithmType))
            {
                ConsoleColor.ConsoleWriteLineRed($"Could not resolve algorithm to class available at runtime: {options.AlgorithmClassName}");
                DisplayHelp(result, new List<Error>());
                Environment.Exit(1);
            }

            options.AlgorithmType = algorithmType;

            DoTheRun(options, cmdSettings);
        }

        /// <summary>
        /// Top level help information.
        /// </summary>
        /// <typeparam name="T">Parser type.</typeparam>
        /// <param name="result">Parser result.</param>
        /// <param name="errs">Parser errors.</param>
        private static void DisplayHelp<T>(ParserResult<T> result, IEnumerable<Error> errs)
        {
            if (errs.IsVersion())
            {
                Console.Write(HeadingInfo.Default);
                return;
            }

            var optionType = typeof(Options);
            var optionProperties = optionType.GetProperties().Where(prop => prop.IsDefined(typeof(CommandLine.OptionAttribute), false));
            var zzz = optionProperties.Select(x => x.GetCustomAttributes(typeof(CommandLine.OptionAttribute), false)).SelectMany(x => x);
            var attributes = zzz.Cast<CommandLine.OptionAttribute>().ToList();

            var errorLines = new List<string>();
            var missingRequiredCount = 0;

            bool anyRequiredError = false;
            bool anyUnknownError = false;

            // Print an error message for each option not supplied that is required.
            if (result is NotParsed<T>)
            {
                var np = result as NotParsed<T>;
                if (!object.ReferenceEquals(null, np) && np.Errors.Any())
                {
                    var missingRequired = np.Errors.Where(x => x is MissingRequiredOptionError).Cast<MissingRequiredOptionError>();

                    foreach (var missing in missingRequired)
                    {
                        var matching = attributes.Where(x => 
                            !string.IsNullOrEmpty(missing.NameInfo.LongName)
                            && x.LongName == missing.NameInfo.LongName).FirstOrDefault();

                        if (!object.ReferenceEquals(null, matching))
                        {
                            // If this has a default value, ignore the error that the value is missing.
                            if (!object.ReferenceEquals(null, matching.Default))
                            {
                                continue;
                            }
                        }

                        missingRequiredCount++;
                        errorLines.Add($"Error: missing required option: {missing.NameInfo.LongName}");
                        anyRequiredError = true;
                    }
                }
            }

            // Print an error message for each option that is not recognized.
            if (!object.ReferenceEquals(null, errs))
            {
                if (anyRequiredError)
                {
                    errorLines.Add(String.Empty);
                }

                var unknownOptionErrors = errs.Where(x => x is UnknownOptionError).Cast<UnknownOptionError>();
                foreach (var uoe in unknownOptionErrors)
                {
                    errorLines.Add($"Error: unknown option: {uoe.Token}");
                    anyUnknownError = true;
                }
            }

            if (anyRequiredError || anyUnknownError)
            {
                errorLines.Add(String.Empty);
            }

            foreach (var error in errorLines)
            {
                ConsoleColor.ConsoleWriteLineRed(error);
            }

            /* Done with error printing. Now print standard text. */

            var helpText = new HelpText(HeadingInfo.Default, CopyrightInfo.Default);
            helpText.AddDashesToOption = true;
            helpText.MaximumDisplayWidth = 100;
            helpText.AdditionalNewLineAfterOption = false;
            helpText.AddOptions(result);

            var texty = helpText.ToString();

            Console.WriteLine(texty);

            // If no options are specified (all required options that do not have a default are missing),
            // then print an example.
            var requiredOptionCount = attributes.Count(x => x.Required == true && object.ReferenceEquals(null, x.Default));
            if (missingRequiredCount == requiredOptionCount)
            {
                Console.WriteLine(HelpText.RenderUsageText<T>(result));
            }
        }

        /// <summary>
        /// Print callback method.
        /// </summary>
        /// <param name="progress"></param>
        private static void PrintProgress(ProgressReport progress)
        {
            var elapsedMinutes = (int)(progress.ElapsedSeconds / 60);
            var elapsedSecondsD = progress.ElapsedSeconds;
            var sb = new StringBuilder();

            if (elapsedMinutes > 0)
            {
                sb.Append($"{elapsedMinutes} min ");
                elapsedSecondsD -= elapsedMinutes * 60;
            }

            sb.Append($"{elapsedSecondsD:N2} sec");

            double donePercent = 100.0 * (double)progress.CurrentStep / (double)progress.TotalSteps;
            Console.WriteLine($"work: {progress.CurrentWorkName}");
            Console.WriteLine($"elapsed: {sb.ToString()}");
            Console.WriteLine($"pixels: {progress.CurrentStep} / {progress.TotalSteps} = {donePercent:N2}%");
            Console.WriteLine();
        }

        /// <summary>
        /// Main run method. This assumes all options are valid.
        /// </summary>
        /// <param name="runSettings"></param>
        /// <param name="cmdSettings"></param>
        private static void DoTheRun(Options runSettings, CmdRunSettings cmdSettings)
        {
            int progressInterval = runSettings.Quiet ? 0 : runSettings.ProgressReportIntervalSeconds;
            Action<ProgressReport>? progressCallback = runSettings.Quiet ? null : PrintProgress;

            var runStopWatch = System.Diagnostics.Stopwatch.StartNew();
            if (!runSettings.Quiet)
            {
                ConsoleLog($"starting");
            }

            EscapeAlgorithm? algorithm = null;

            try
            {
                algorithm = (EscapeAlgorithm)Activator.CreateInstance(runSettings.AlgorithmType, new object[] { progressInterval, progressCallback });
            }
            catch (Exception ex)
            {
                ConsoleColor.ConsoleWriteLineRed($"Error instantiating algorithm: {ex.Message}");
                Environment.Exit(1);
            }

            if (object.ReferenceEquals(null, algorithm))
            {
                // This shouldn't happen unless the cast fails for some reason.
                ConsoleColor.ConsoleWriteLineRed($"Error instantiating algorithm. Constructor succeeded, but instance is null.");
                Environment.Exit(1);
            }

            algorithm.ProgressCallbackIntervalSec = progressInterval;
            algorithm.Origin = (runSettings.OriginX, runSettings.OriginY);
            algorithm.FractalWidth = runSettings.FractalWidth;
            algorithm.FractalHeight = runSettings.FractalHeight;
            algorithm.StepWidth = runSettings.StepWidth;
            algorithm.StepHeight = runSettings.StepHeight;
            algorithm.MaxIterations = runSettings.MaxIterations;
            algorithm.UseHistogram = runSettings.UseHistogram;

            var scene = new Scene();

            scene.AddDefaultSceneKeyframes();

            var cancellationToken = new CancellationTokenSource();

            algorithm.EvaluatePoints(cancellationToken.Token);

            var bmp = scene.ProcessPointsToPixels(algorithm, cancellationToken.Token, progressInterval, progressCallback);

            using (MemoryStream memStream = new MemoryStream())
            using (SKManagedWStream wstream = new SKManagedWStream(memStream))
            {
                bmp.Encode(wstream, cmdSettings.OutputFormat, 100);
                byte[] data = memStream.ToArray();
                System.IO.File.WriteAllBytes(runSettings.OutputFilename, data);
            }

            if (!runSettings.Quiet)
            {
                ConsoleLog($"saved image to {runSettings.OutputFilename}");
            }

            if (runSettings.WriteMetaData)
            {
                var baseFilename = System.IO.Path.GetFileNameWithoutExtension(runSettings.OutputFilename);
                var metadataFilename = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(runSettings.OutputFilename)!, baseFilename + ".txt");
                var sb = new StringBuilder();
                sb.AppendLine($"runtime: {cmdSettings.RunTime.ToLongDateString()} {cmdSettings.RunTime.ToLongTimeString()}");
                sb.AppendLine($"runtime.iso: {cmdSettings.RunTime.ToString("yyyy-MM-ddTHH:mm:sszzz", System.Globalization.CultureInfo.InvariantCulture)}");
                sb.AppendLine($"Algorithm: {runSettings.AlgorithmType.FullName}");
                sb.AppendLine($"Origin.X: {runSettings.OriginX}");
                sb.AppendLine($"Origin.Y: {runSettings.OriginY}");
                sb.AppendLine($"FractalWidth: {runSettings.FractalWidth}");
                sb.AppendLine($"FractalHeight: {runSettings.FractalHeight}");
                sb.AppendLine($"StepWidth: {runSettings.StepWidth}");
                sb.AppendLine($"StepHeight: {runSettings.StepHeight}");
                sb.AppendLine($"MaxIterations: {runSettings.MaxIterations}");
                sb.AppendLine($"UseHistogram: {runSettings.UseHistogram}");

                System.IO.File.WriteAllText(metadataFilename, sb.ToString());

                if (!runSettings.Quiet)
                {
                    ConsoleLog($"saved metadata to {metadataFilename}");
                }
            }

            runStopWatch.Stop();
            if (!runSettings.Quiet)
            {
                var elapsedMinutes = (int)(runStopWatch.Elapsed.TotalSeconds / 60);
                var elapsedSecondsD = runStopWatch.Elapsed.TotalSeconds;
                var sb = new StringBuilder();

                if (elapsedMinutes > 0)
                {
                    sb.Append($"{elapsedMinutes} min ");
                    elapsedSecondsD -= elapsedMinutes * 60;
                }

                sb.Append($"{elapsedSecondsD:N2} sec");

                ConsoleLog($"done. Total runtime: {sb.ToString()}");
            }
        }

        private static void ConsoleLog(string msg)
        {
            if (string.IsNullOrEmpty(msg))
            {
                Console.WriteLine(string.Empty);
            }
            else
            {
                Console.WriteLine($"{DateTime.Now.ToString("yyyy-MM-ddTHH:mm:sszzz")}: {msg}");
            }
        }
    }
}
