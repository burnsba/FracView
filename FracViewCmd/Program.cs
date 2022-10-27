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
using FracView.Dto;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Runtime.CompilerServices;

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

            /*
             * json settings can provide values not available to the command line like coloramp.
             * It can also set the "runsettings" options.
             * So it's possible json provides a setting that's required that CommandLineParser won't
             * resolve on the command line. Therefore the options can't be marked required,
             * so required options have to be resolved here.
             */

            var requiredRunSettingsProperties = new HashSet<string>()
            {
                nameof(RunSettings.OriginX),
                nameof(RunSettings.OriginY),
                nameof(RunSettings.FractalWidth),
                nameof(RunSettings.FractalHeight),
                nameof(RunSettings.StepWidth),
                nameof(RunSettings.StepHeight),
                nameof(RunSettings.MaxIterations),
            };

            var setRequiredRunSettingsProperties = new HashSet<string>();

            /* resolve required: step 1, check for json and load if possible */
            if (!string.IsNullOrEmpty(options.JsonSettings))
            {
                if (!File.Exists(options.JsonSettings))
                {
                    ConsoleColor.ConsoleWriteLineRed($"Error loading json settings, file does not exist: {options.JsonSettings}");
                    Environment.Exit(1);
                }

                var fileContent = System.IO.File.ReadAllText(options.JsonSettings);
                SessionSettings? settings = null;

                try
                {
                    settings = JsonConvert.DeserializeObject<SessionSettings>(fileContent);
                }
                catch (Exception ex)
                {
                    ConsoleColor.ConsoleWriteLineRed($"Error reading json settings file: {ex.Message}");
                    Environment.Exit(1);
                }

                if (!object.ReferenceEquals(null, settings))
                {
                    options.SessionSettings = settings;

                    // Allow the RunSettings values to be optional.
                    // Will need a way to resolve if it exists or not, vs being set to default(T).
                    // Parse to generic JObject, and if the property exists mark as requirement being met.
                    var jobj = JObject.Parse(fileContent);
                    var runSettingsJtoken = jobj[nameof(RunSettings)];
                    if (!object.ReferenceEquals(null, runSettingsJtoken))
                    {
                        var runSettingsProperties = typeof(RunSettings).GetProperties();
                        foreach (var property in runSettingsProperties)
                        {
                            if (!object.ReferenceEquals(null, runSettingsJtoken[property.Name]))
                            {
                                if (requiredRunSettingsProperties.Contains(property.Name))
                                {
                                    setRequiredRunSettingsProperties.Add(property.Name);
                                }
                            }
                        }
                    }
                }
            }

            /* resolve required: step 2, if command line options are present that can also be set from json, use those values instead. */

            decimal? maybeDecimal;

            maybeDecimal = AsDecimal(options.UserOriginX) ?? (decimal?)AsDouble(options.UserOriginX);
            if (maybeDecimal.HasValue)
            {
                options.SessionSettings.RunSettings.OriginX = maybeDecimal.Value;
                if (requiredRunSettingsProperties.Contains(nameof(options.SessionSettings.RunSettings.OriginX)))
                {
                    setRequiredRunSettingsProperties.Add(nameof(options.SessionSettings.RunSettings.OriginX));
                }
            }

            maybeDecimal = AsDecimal(options.UserOriginY) ?? (decimal?)AsDouble(options.UserOriginY);
            if (maybeDecimal.HasValue)
            {
                options.SessionSettings.RunSettings.OriginY = maybeDecimal.Value;
                if (requiredRunSettingsProperties.Contains(nameof(options.SessionSettings.RunSettings.OriginY)))
                {
                    setRequiredRunSettingsProperties.Add(nameof(options.SessionSettings.RunSettings.OriginY));
                }
            }

            maybeDecimal = AsDecimal(options.UserFractalWidth) ?? (decimal?)AsDouble(options.UserFractalWidth);
            if (maybeDecimal.HasValue)
            {
                options.SessionSettings.RunSettings.FractalWidth = maybeDecimal.Value;
                if (requiredRunSettingsProperties.Contains(nameof(options.SessionSettings.RunSettings.FractalWidth)))
                {
                    setRequiredRunSettingsProperties.Add(nameof(options.SessionSettings.RunSettings.FractalWidth));
                }
            }

            maybeDecimal = AsDecimal(options.UserFractalHeight) ?? (decimal?)AsDouble(options.UserFractalHeight);
            if (maybeDecimal.HasValue)
            {
                options.SessionSettings.RunSettings.FractalHeight = maybeDecimal.Value;
                if (requiredRunSettingsProperties.Contains(nameof(options.SessionSettings.RunSettings.FractalHeight)))
                {
                    setRequiredRunSettingsProperties.Add(nameof(options.SessionSettings.RunSettings.FractalHeight));
                }
            }

            if (options.UserStepWidth.HasValue)
            {
                options.SessionSettings.RunSettings.StepWidth = options.UserStepWidth.Value;
                if (requiredRunSettingsProperties.Contains(nameof(options.SessionSettings.RunSettings.StepWidth)))
                {
                    setRequiredRunSettingsProperties.Add(nameof(options.SessionSettings.RunSettings.StepWidth));
                }
            }

            if (options.UserStepHeight.HasValue)
            {
                options.SessionSettings.RunSettings.StepHeight = options.UserStepHeight.Value;
                if (requiredRunSettingsProperties.Contains(nameof(options.SessionSettings.RunSettings.StepHeight)))
                {
                    setRequiredRunSettingsProperties.Add(nameof(options.SessionSettings.RunSettings.StepHeight));
                }
            }

            if (options.UserMaxIterations.HasValue)
            {
                options.SessionSettings.RunSettings.MaxIterations = options.UserMaxIterations.Value;
                if (requiredRunSettingsProperties.Contains(nameof(options.SessionSettings.RunSettings.MaxIterations)))
                {
                    setRequiredRunSettingsProperties.Add(nameof(options.SessionSettings.RunSettings.MaxIterations));
                }
            }

            /* resolve required: step 3, identify unset properties and error out if missing. */
            var unsetRequiredProperties = requiredRunSettingsProperties.Except(setRequiredRunSettingsProperties).ToList();
            if (unsetRequiredProperties.Any())
            {
                foreach (var missingRequired in unsetRequiredProperties)
                {
                    ConsoleColor.ConsoleWriteLineRed($"Error: missing required option: {missingRequired}");
                }

                ConsoleColor.ConsoleWriteLineRed(string.Empty);

                DisplayHelp(result, new List<Error>());
                Environment.Exit(1);
            }

            /* set non-required options */
            options.SessionSettings.RunSettings.UseHistogram = options.UseHistogram;

            /* begin validation */

            var runSettings = options.SessionSettings.RunSettings;

            if (runSettings.StepWidth < 0)
            {
                ConsoleColor.ConsoleWriteLineRed($"{nameof(runSettings.StepWidth)} value must be positive integer: {runSettings.StepWidth}");
                DisplayHelp(result, new List<Error>());
                Environment.Exit(1);
            }

            if (runSettings.StepHeight < 0)
            {
                ConsoleColor.ConsoleWriteLineRed($"{nameof(runSettings.StepHeight)} value must be positive integer: {runSettings.StepHeight}");
                DisplayHelp(result, new List<Error>());
                Environment.Exit(1);
            }

            if (runSettings.MaxIterations < 0)
            {
                ConsoleColor.ConsoleWriteLineRed($"{nameof(runSettings.MaxIterations)} value must be positive integer: {runSettings.MaxIterations}");
                DisplayHelp(result, new List<Error>());
                Environment.Exit(1);
            }

            /* validate output directory. */

            options.OutputDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;

            if (!string.IsNullOrEmpty(options.UserOutputDirectory))
            {
                options.OutputDirectory = options.UserOutputDirectory;
            }

            if (!Directory.Exists(options.OutputDirectory))
            {
                try
                {
                    Directory.CreateDirectory(options.OutputDirectory);
                }
                catch (IOException)
                {
                    ConsoleColor.ConsoleWriteLineRed($"Error creating output directory: {options.OutputDirectory}");
                    Environment.Exit(1);
                }
            }

            if (!Directory.Exists(options.OutputDirectory))
            {
                ConsoleColor.ConsoleWriteLineRed($"Could not find/create output directory: {options.OutputDirectory}");
                DisplayHelp(result, new List<Error>());
                Environment.Exit(1);
            }

            /* validate filename and extension */

            var outputFormat = options.UserOutputFormat;
            if (!outputFormat.StartsWith('.'))
            {
                outputFormat = "." + outputFormat;
            }

            try
            {
                options.OutputFormat = FracView.Converters.SkiaConverters.ExtensionToFormat(outputFormat);
            }
            catch (NotSupportedException)
            {
                ConsoleColor.ConsoleWriteLineRed($"Unsupported format: {options.UserOutputFormat}");
                DisplayHelp(result, new List<Error>());
                Environment.Exit(1);
            }

            var expectedExtension = FracView.Converters.SkiaConverters.FormatToExtension(options.OutputFormat);

            options.OutputExtension = expectedExtension;

            if (string.IsNullOrEmpty(options.UserOutputFilename))
            {
                options.OutputBaseFilename = SaveAsDefaultFilename
                    + options.RunTime.ToString("yyyyMMdd-HHmmss");
            }
            else
            {
                options.OutputBaseFilename = Path.GetFileNameWithoutExtension(options.UserOutputFilename);
            }

            /* check that filename is sane. */
            File.Create(options.OutputFilenameWithExtension).Dispose();
            if (File.Exists(options.OutputFilenameWithExtension))
            {
                File.Delete(options.OutputFilenameWithExtension);
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

            DoTheRun(options);
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

            HelpText helpText = new(HeadingInfo.Default, CopyrightInfo.Default)
            {
                AddDashesToOption = true,
                MaximumDisplayWidth = 100,
                AdditionalNewLineAfterOption = false
            };

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
            Console.WriteLine($"elapsed: {sb}");
            Console.WriteLine($"pixels: {progress.CurrentStep} / {progress.TotalSteps} = {donePercent:N2}%");
            Console.WriteLine();
        }

        /// <summary>
        /// Main run method. This assumes all options are valid.
        /// </summary>
        /// <param name="runSettings"></param>
        /// <param name="cmdSettings"></param>
        private static void DoTheRun(Options options)
        {
            var runSettings = options.SessionSettings.RunSettings;

            int progressInterval = options.Quiet ? 0 : options.ProgressReportIntervalSeconds;
            Action<ProgressReport>? progressCallback = options.Quiet ? null : PrintProgress;

            var runStopWatch = System.Diagnostics.Stopwatch.StartNew();
            if (!options.Quiet)
            {
                ConsoleLog($"starting");
            }

            EscapeAlgorithm? algorithm = null;

            try
            {
                algorithm = Activator.CreateInstance(options.AlgorithmType, new object?[] { progressInterval, progressCallback }) as EscapeAlgorithm;
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

            if (options.SessionSettings.ColorRampKeyframes != null && options.SessionSettings.ColorRampKeyframes.Any())
            {
                scene.ColorRamp.Keyframes = options.SessionSettings.GetColorRampKeyframes();
            }
            else
            {
                scene.AddDefaultSceneKeyframes();
            }

            if (!string.IsNullOrEmpty(options.SessionSettings.StableColor))
            {
                scene.StableColor = options.SessionSettings.GetStableColor();
            }

            var cancellationToken = new CancellationTokenSource();

            algorithm.EvaluatePoints(cancellationToken.Token);

            var bmp = scene.ProcessPointsToPixels(algorithm, cancellationToken.Token, progressInterval, progressCallback);

            if (object.ReferenceEquals(null, bmp))
            {
                ConsoleLog($"Algorithm run cancelled (scene result is null).");
                Environment.Exit(0);
            }

            // If the command line program is called from a script, it's possible this could execute fast enough
            // that multiple images are generated within the same second. If that happens, append a counter
            // to the filename, and update the base filename for the metadata file.
            string path = Path.Combine(options.OutputDirectory, options.OutputFilenameWithExtension);
            int extendCount = 1;
            string? updateOutputFilename = null;
            while (File.Exists(path))
            {
                updateOutputFilename = options.OutputBaseFilename + $"-{extendCount:D4}" + options.OutputExtension;
                path = Path.Combine(options.OutputDirectory, updateOutputFilename);
                extendCount++;
            }

            if (!string.IsNullOrEmpty(updateOutputFilename))
            {
                options.OutputBaseFilename = Path.GetFileNameWithoutExtension(updateOutputFilename);
            }

            using (MemoryStream memStream = new())
            using (SKManagedWStream wstream = new(memStream))
            {
                bmp.Encode(wstream, options.OutputFormat, 100);
                byte[] data = memStream.ToArray();
                System.IO.File.WriteAllBytes(path, data);
            }

            if (!options.Quiet)
            {
                ConsoleLog($"saved image to {path}");
            }

            if (options.WriteMetaData)
            {
                var metadataFilename = System.IO.Path.Combine(options.OutputDirectory, options.OutputBaseFilename + ".txt");
                var sb = new StringBuilder();
                sb.AppendLine($"runtime: {options.RunTime.ToLongDateString()} {options.RunTime.ToLongTimeString()}");
                sb.AppendLine($"runtime.iso: {options.RunTime.ToString("yyyy-MM-ddTHH:mm:sszzz", System.Globalization.CultureInfo.InvariantCulture)}");
                sb.AppendLine($"Algorithm: {options.AlgorithmType.FullName}");
                sb.AppendLine($"Origin.X: {runSettings.OriginX}");
                sb.AppendLine($"Origin.Y: {runSettings.OriginY}");
                sb.AppendLine($"FractalWidth: {runSettings.FractalWidth}");
                sb.AppendLine($"FractalHeight: {runSettings.FractalHeight}");
                sb.AppendLine($"StepWidth: {runSettings.StepWidth}");
                sb.AppendLine($"StepHeight: {runSettings.StepHeight}");
                sb.AppendLine($"MaxIterations: {runSettings.MaxIterations}");
                sb.AppendLine($"UseHistogram: {runSettings.UseHistogram}");

                System.IO.File.WriteAllText(metadataFilename, sb.ToString());

                if (!options.Quiet)
                {
                    ConsoleLog($"saved metadata to {metadataFilename}");
                }
            }

            runStopWatch.Stop();
            if (!options.Quiet)
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

                ConsoleLog($"done. Total runtime: {sb}");
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
                Console.WriteLine($"{DateTime.Now:yyyy-MM-ddTHH:mm:sszzz}: {msg}");
            }
        }

        private static Nullable<double> AsDouble(string s)
        {
            double d;
            if (double.TryParse(s, out d))
            {
                return d;
            }

            return (Nullable<double>)null;
        }

        private static Nullable<decimal> AsDecimal(string s)
        {
            decimal d;
            if (decimal.TryParse(s, out d))
            {
                return d;
            }

            return (Nullable<decimal>)null;
        }
    }
}
