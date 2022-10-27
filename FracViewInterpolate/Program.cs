using System.Text;

namespace FracViewInterpolate
{
    /**
     * Helper application to generate command line script.
     * This will logarithmically interpolate between the starting condition and end condition.
     */
    internal class Program
    {
        static void Main(string[] args)
        {
            string exec = ".\\FracViewCmd.exe";
            string json = "session_more_blue.json";

            /* run settings */

            string algorithm = "FracView.Algorithms.MandelbrotCos";

            //double start_origin_x = -0.5;
            //double start_origin_y = 0;

            double end_origin_x = -2.1011605333502466882633114270;
            double end_origin_y = -0.3788836511657677714962222817;

            double start_width = 8;
            double start_height = 4.5;

            double end_width = 0.0000005109339873288371142448;
            double end_height = end_width * ((double)1080 / (double)1920);

            int maxIterations = 15000;
            int stepwidth = 1920 * 2;
            int stepheight = 1080 * 2;

            // ------------------------------------------------

            int fps = 24;
            int durationSec = 180;

            int steps = durationSec * fps;

            string outDirName = BuildOutputDirName(algorithm);

            //double origin_range_x = end_origin_x - start_origin_x;
            //double origin_range_y = end_origin_y - start_origin_y;

            double ln_start_width = Math.Log(start_width);
            double ln_start_height = Math.Log(start_height);
            double ln_end_width = Math.Log(end_width);
            double ln_end_height = Math.Log(end_height);

            // yes, this is backwards.
            double ln_range_width = ln_start_width - ln_end_width;
            double ln_range_height = ln_start_height - ln_end_height;

            double steps_log = Math.Log(steps);

            var runoutput = outDirName + ".ps1";

            using (StreamWriter sw = File.CreateText(runoutput))
            {
                var command = new StringBuilder();

                command.Append($"{exec} ");
                command.Append($"--json {json} ");
                command.Append($"--meta ");
                command.Append($"--dir {outDirName} ");
                command.Append($"--output 000000 ");
                command.Append($"--usehistogram ");
                command.Append($"--algorithm {algorithm} ");
                command.Append($"--maxiterations {maxIterations} ");
                command.Append($"--stepheight {stepheight} ");
                command.Append($"--stepwidth {stepwidth} ");
                command.Append($"--fractalwidth {start_width} ");
                command.Append($"--fractalheight {start_height} ");
                command.Append($"--originx {end_origin_x} ");
                command.Append($"--originy {end_origin_y} ");

                Console.WriteLine(command.ToString());
                Console.WriteLine();

                sw.WriteLine(command.ToString());
                sw.WriteLine();

                for (int i = 1; i <= steps; i++)
                {
                    command = new StringBuilder();

                    double percent = (double)i / (double)steps;
                    //double log_scale = Math.Log(i) / steps_log;

                    double percentTimesLogRangeWidth = percent * ln_range_width;
                    double percentTimesLogRangeHeight = percent * ln_range_height;

                    //double step_origin_x = start_origin_x + origin_range_x * log_scale;
                    //double step_origin_y = start_origin_y + origin_range_y * log_scale;
                    double step_width = Math.Exp(ln_start_width - percentTimesLogRangeWidth);
                    double step_height = Math.Exp(ln_start_height - percentTimesLogRangeHeight);

                    command.Append($"{exec} ");
                    command.Append($"--json {json} ");
                    command.Append($"--meta ");
                    command.Append($"--dir {outDirName} ");
                    command.Append($"--output {i:D6} ");
                    command.Append($"--usehistogram ");
                    command.Append($"--algorithm {algorithm} ");
                    command.Append($"--maxiterations {maxIterations} ");
                    command.Append($"--stepheight {stepheight} ");
                    command.Append($"--stepwidth {stepwidth} ");
                    command.Append($"--fractalwidth {step_width} ");
                    command.Append($"--fractalheight {step_height} ");
                    command.Append($"--originx {end_origin_x} ");
                    command.Append($"--originy {end_origin_y} ");

                    Console.WriteLine(command.ToString());
                    Console.WriteLine();

                    sw.WriteLine(command.ToString());
                    sw.WriteLine();
                }
            }
        }

        private static string BuildOutputDirName(string algorithm)
        {
            var prefix = algorithm;
            if (algorithm.StartsWith("FracView.Algorithms."))
            {
                prefix = algorithm.Substring("FracView.Algorithms.".Length);
            }

            return prefix + "-" + DateTime.Now.ToString("yyyyMMdd-HHmmss");
        }
    }
}