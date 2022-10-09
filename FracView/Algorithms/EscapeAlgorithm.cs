using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FracView.Gfx;
using FracView.World;

namespace FracView.Algorithms
{
    public abstract class EscapeAlgorithm : IEscapeAlgorithm
    {
        private bool _isInit = false;
        private bool _pointsEvaluated = false;
        private object _lockObject = new object();
        private List<EvalComplexUnit>? _consideredPoints = null;

        public (decimal, decimal) Origin { get; set; }

        public decimal FractalWidth { get; set; }
        public decimal FractalHeight { get; set; }

        /// <summary>
        /// Number of steps to divide world range into, used as number of pixels.
        /// </summary>
        public int StepWidth { get; set; }

        /// <summary>
        /// Number of steps to divide world range into, used as number of pixels.
        /// </summary>
        public int StepHeight { get; set; }

        protected int TotalSteps => StepWidth * StepHeight;

        protected int[]? NumIterationsPerPixel = null;

        public int MaxIterations { get; set; }

        public decimal? IterationBreak { get; set; } = null;

        public bool UseHistogram { get; set; }

        public List<EvalComplexUnit> ConsideredPoints
        {
            get
            {
                if (!_pointsEvaluated)
                {
                    throw new InvalidOperationException($"Call {nameof(EvaluatePoints)} first");
                }

                return _consideredPoints;

            }

            private set
            {
                _consideredPoints = value;
            }
        }

        public int ProgressCallbackIntervalSec { get; set; }
        public Action<ProgressReport>? ProgressCallback { get; set; }

        public EscapeAlgorithm(int progressCallbackIntervalSec = 0, Action<ProgressReport>? progressCallback = null)
        {
            ProgressCallbackIntervalSec = progressCallbackIntervalSec;
            ProgressCallback = progressCallback;
        }
       
        public abstract bool IsStable(EvalComplexUnit eu);

        public bool EvaluatePoints(CancellationToken token)
        {
            Init();

            bool interrupted = false;
            var reportTimer = Stopwatch.StartNew();
            var totalTimer = Stopwatch.StartNew();
            int stepCount = 0;

            _pointsEvaluated = false;

            ProgressCallback(new ProgressReport(
                0,
                0,
                TotalSteps,
                $"{nameof(EscapeAlgorithm)}.{nameof(EvaluatePoints)}"
                ));

            Parallel.ForEach(_consideredPoints, (evalPoint, loopState) => {

                if (token.IsCancellationRequested)
                {
                    interrupted = true;
                    loopState.Break();
                }

                lock(_lockObject)
                {
                    if (ProgressCallback != null && ProgressCallbackIntervalSec > 0 && reportTimer.Elapsed.TotalSeconds > ProgressCallbackIntervalSec)
                    {
                        ProgressCallback(new ProgressReport(
                            totalTimer.Elapsed.TotalSeconds,
                            stepCount,
                            TotalSteps,
                            $"{nameof(EscapeAlgorithm)}.{nameof(EvaluatePoints)}"
                            ));

                        reportTimer.Restart();
                    }
                }

                evalPoint.IsStable = IsStable(evalPoint);

                lock (_lockObject)
                {
                    stepCount++;
                }
            });

            _pointsEvaluated = true;

            if (UseHistogram)
            {
                ComputeHistogram(token);
            }

            return !interrupted;
        }

        private void Init()
        {
            if (_isInit)
            {
                return;
            }

            if (FractalWidth <= 0)
            {
                throw new ArgumentException($"{nameof(FractalWidth)} must be positive");
            }

            if (FractalHeight <= 0)
            {
                throw new ArgumentException($"{nameof(FractalHeight)} must be positive");
            }

            if (StepWidth <= 0)
            {
                throw new ArgumentException($"{nameof(StepWidth)} must be positive");
            }

            if (StepHeight <= 0)
            {
                throw new ArgumentException($"{nameof(StepHeight)} must be positive");
            }

            if (MaxIterations <= 0)
            {
                throw new ArgumentException($"{nameof(MaxIterations)} must be positive");
            }

            var reportTimer = Stopwatch.StartNew();
            var totalTimer = Stopwatch.StartNew();

            try
            {
                _consideredPoints = new List<EvalComplexUnit>(TotalSteps);

                decimal startX = Origin.Item1 - (FractalWidth / 2);
                decimal stepX = FractalWidth / StepWidth;
                decimal x;

                decimal startY = Origin.Item2 - (FractalHeight / 2);
                decimal stepY = FractalHeight / StepHeight;
                decimal y;

                y = startY;
                x = startX;
                int currentStepCount = 0;

                for (int j = 0; j < StepHeight; j++)
                {
                    x = startX;

                    for (int i = 0; i < StepWidth; i++)
                    {
                        var eu = new EvalComplexUnit((i, j), (x, y));

                        _consideredPoints.Add(eu);

                        lock (_lockObject)
                        {
                            if (ProgressCallback != null && ProgressCallbackIntervalSec > 0 && reportTimer.Elapsed.TotalSeconds > ProgressCallbackIntervalSec)
                            {
                                ProgressCallback(new ProgressReport(
                                    totalTimer.Elapsed.TotalSeconds,
                                    currentStepCount,
                                    TotalSteps,
                                    $"{nameof(EscapeAlgorithm)}.{nameof(Init)}"
                                    ));

                                reportTimer.Restart();
                            }
                        }

                        x += stepX;
                        currentStepCount++;
                    }

                    y += stepY;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unable to allocate evaluation grid for processing.");
                Console.WriteLine(ex.Message);

                throw;
            }

            _isInit = true;
        }


        private void ComputeHistogram(CancellationToken token)
        {
            if (!_isInit)
            {
                throw new InvalidOperationException($"Call {nameof(Init)} first");
            }

            if (MaxIterations < 1)
            {
                throw new InvalidOperationException($"{nameof(MaxIterations)} not set");
            }

            try
            {
                NumIterationsPerPixel = Enumerable.Repeat(0, MaxIterations + 1).ToArray();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unable to allocate evaluation numIterationsPerPixel container.");
                Console.WriteLine(ex.Message);

                throw;
            }

            if (object.ReferenceEquals(null, NumIterationsPerPixel))
            {
                throw new NullReferenceException($"{nameof(NumIterationsPerPixel)} is null, call {nameof(Init)} first");
            }

            if (MaxIterations > NumIterationsPerPixel.Length)
            {
                throw new InvalidOperationException($"There are more iterations to evaluate than space in {nameof(NumIterationsPerPixel)}.");
            }

            int iterationCount = 0;
            long iterationsTotal = 0;

            var reportTimer = Stopwatch.StartNew();
            var totalTimer = Stopwatch.StartNew();

            ProgressCallback(new ProgressReport(
                0,
                0,
                _consideredPoints.Count,
                $"{nameof(EscapeAlgorithm)}.{nameof(ComputeHistogram)}, loop=1)"
                ));

            _consideredPoints.ForEach(x =>
            {
                NumIterationsPerPixel[x.IterationCount]++;

                // not parallel, no need to lock.
                if (ProgressCallback != null && ProgressCallbackIntervalSec > 0 && reportTimer.Elapsed.TotalSeconds > ProgressCallbackIntervalSec)
                {
                    ProgressCallback(new ProgressReport(
                        totalTimer.Elapsed.TotalSeconds,
                        iterationCount,
                        _consideredPoints.Count,
                        $"{nameof(EscapeAlgorithm)}.{nameof(ComputeHistogram)}, loop=1)"
                        ));

                    reportTimer.Restart();
                }

                iterationCount++;
            });

            for (int i = 0; i < MaxIterations; i++)
            {
                iterationsTotal += NumIterationsPerPixel[i];
            }

            iterationCount = 0;
            reportTimer.Restart();

            Parallel.ForEach(_consideredPoints, x =>
            {
                for (int i = 0; i < x.IterationCount; i++)
                {
                    x.HistogramValue += (double)NumIterationsPerPixel[i] / (double)iterationsTotal;
                }

                lock (_lockObject)
                {
                    if (ProgressCallback != null && ProgressCallbackIntervalSec > 0 && reportTimer.Elapsed.TotalSeconds > ProgressCallbackIntervalSec)
                    {
                        ProgressCallback(new ProgressReport(
                            totalTimer.Elapsed.TotalSeconds,
                            iterationCount,
                            _consideredPoints.Count,
                            $"{nameof(EscapeAlgorithm)}.{nameof(ComputeHistogram)}, loop=3)"
                            ));

                        reportTimer.Restart();
                    }

                    iterationCount++;
                }
            });
        }
    }
}
