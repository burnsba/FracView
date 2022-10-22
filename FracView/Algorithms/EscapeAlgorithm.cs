using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FracView.Dto;
using FracView.Gfx;
using FracView.World;

namespace FracView.Algorithms
{
    public abstract class EscapeAlgorithm : IEscapeAlgorithm
    {
        private readonly object _lockObject = new();
        private decimal _iterationBreak = 0;
        private decimal _iterationBreakSquare = 0;
        private bool _isInit = false;
        private bool _pointsEvaluated = false;
        private bool _histogramIsEvaluated = false;
        private List<EvalComplexUnit>? _consideredPoints = null;

        /// <inheritdoc />
        public decimal FractalWidth { get; set; }

        /// <inheritdoc />
        public decimal FractalHeight { get; set; }

        /// <inheritdoc />
        public int StepWidth { get; set; }

        /// <inheritdoc />
        public int StepHeight { get; set; }

        /// <inheritdoc />
        public int MaxIterations { get; set; }

        /// <inheritdoc />
        public bool UseHistogram { get; set; }

        /// <inheritdoc />
        public bool HistogramIsEvaluated => _histogramIsEvaluated;

        /// <inheritdoc />
        public int ProgressCallbackIntervalSec { get; set; }

        /// <inheritdoc />
        public Action<ProgressReport>? ProgressCallback { get; set; }

        /// <summary>
        /// Gets or sets world origin point.
        /// </summary>
        public (decimal, decimal) Origin { get; set; }

        /// <summary>
        /// Gets or sets the points computed by the algorithm.
        /// <see cref="EvaluatePoints"/> must have been called at least once.
        /// </summary>
        public List<EvalComplexUnit> ConsideredPoints
        {
            get
            {
                if (!_pointsEvaluated)
                {
                    throw new InvalidOperationException($"Call {nameof(EvaluatePoints)} first");
                }

                return _consideredPoints!;

            }

            private set
            {
                _consideredPoints = value;
            }
        }

        /// <summary>
        /// Gets or sets the value used by the escape algorithm to determine
        /// that a point has escaped.
        /// </summary>
        public decimal IterationBreak
        {
            get => _iterationBreak;
            set
            {
                _iterationBreak = value;
                _iterationBreakSquare = value * value;
            }
        }

        /// <summary>
        /// Gets the square of the <see cref="IterationBreak"/> value.
        /// </summary>
        public decimal IterationBreakSquare => _iterationBreakSquare;

        /// <summary>
        /// Gets the total number of steps as <see cref="StepWidth"/> times <see cref="StepHeight"/>.
        /// </summary>
        protected int TotalSteps => StepWidth * StepHeight;

        /// <summary>
        /// Container for computing histogram data.
        /// </summary>
        protected int[]? NumIterationsPerPixel = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="EscapeAlgorithm"/> class.
        /// </summary>
        /// <param name="progressCallbackIntervalSec">Interval in seconds that progress should be reported.</param>
        /// <param name="progressCallback">Reporting callback method.</param>
        /// <returns>Image with pixels set.</returns>
        public EscapeAlgorithm(int progressCallbackIntervalSec = 0, Action<ProgressReport>? progressCallback = null)
        {
            ProgressCallbackIntervalSec = progressCallbackIntervalSec;
            ProgressCallback = progressCallback;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EscapeAlgorithm"/> class.
        /// </summary>
        /// <param name="settings">Settings used by algorithm.</param>
        /// <param name="progressCallbackIntervalSec">Interval in seconds that progress should be reported.</param>
        /// <param name="progressCallback">Reporting callback method.</param>
        /// <returns>Image with pixels set.</returns>
        public EscapeAlgorithm(RunSettings settings, int progressCallbackIntervalSec = 0, Action<ProgressReport>? progressCallback = null)
            : this(progressCallbackIntervalSec, progressCallback)
        {
            Origin = (settings.OriginX, settings.OriginY);
            FractalWidth = settings.FractalWidth;
            FractalHeight = settings.FractalHeight;
            StepWidth = settings.StepWidth;
            StepHeight = settings.StepHeight;
            MaxIterations = settings.MaxIterations;
            UseHistogram = settings.UseHistogram;
        }
       
        /// <summary>
        /// Method to determine whether a point is stable or not.
        /// </summary>
        /// <param name="eu">Point to consider.</param>
        /// <returns>True if point remains bounded for evaluation, false otherwise.</returns>
        public abstract bool IsStable(EvalComplexUnit eu);

        /// <summary>
        /// Evaluates all points defined within the world range.
        /// </summary>
        /// <param name="token">Cancellation token.</param>
        /// <returns>True if computation finished successfully, false otherwise.</returns>
        public bool EvaluatePoints(CancellationToken token)
        {
            Init(token);

            if (token.IsCancellationRequested)
            {
                return false;
            }

            bool interrupted = false;
            var reportTimer = Stopwatch.StartNew();
            var totalTimer = Stopwatch.StartNew();
            int stepCount = 0;

            _pointsEvaluated = false;

            if (!object.ReferenceEquals(null, ProgressCallback))
            {
                ProgressCallback(new ProgressReport(
                    0,
                    0,
                    TotalSteps,
                    $"{nameof(EscapeAlgorithm)}.{nameof(EvaluatePoints)}"
                    ));
            }

            Parallel.ForEach(_consideredPoints!, (evalPoint, loopState) => {

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

            if (token.IsCancellationRequested)
            {
                return false;
            }

            _pointsEvaluated = true;

            if (UseHistogram)
            {
                ComputeHistogram(token);
            }

            return !interrupted;
        }

        /// <summary>
        /// Computes the histogram for all evaluated points.
        /// </summary>
        /// <param name="token">Cancellation token.</param>
        /// <exception cref="InvalidOperationException">If init has not been called yet, if maxiterations is not set, or there isn't space in NumIterationsPerPixel.</exception>
        /// <exception cref="NullReferenceException">NumIterationsPerPixel is null.</exception>
        public void ComputeHistogram(CancellationToken token)
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

            if (token.IsCancellationRequested)
            {
                return;
            }

            int iterationCount = 0;
            long iterationsTotal = 0;

            var reportTimer = Stopwatch.StartNew();
            var totalTimer = Stopwatch.StartNew();

            _histogramIsEvaluated = false;

            if (!object.ReferenceEquals(null, ProgressCallback))
            {
                ProgressCallback(new ProgressReport(
                    0,
                    0,
                    /* set not null from _isInit */
                    _consideredPoints!.Count,
                    $"{nameof(EscapeAlgorithm)}.{nameof(ComputeHistogram)}, (loop=1)"
                    ));
            }

            /* set not null from _isInit */
            _consideredPoints!.ForEach(x =>
            {
                if (token.IsCancellationRequested)
                {
                    return;
                }

                NumIterationsPerPixel[x.IterationCount]++;

                // not parallel, no need to lock.
                if (ProgressCallback != null && ProgressCallbackIntervalSec > 0 && reportTimer.Elapsed.TotalSeconds > ProgressCallbackIntervalSec)
                {
                    ProgressCallback(new ProgressReport(
                        totalTimer.Elapsed.TotalSeconds,
                        iterationCount,
                        _consideredPoints.Count,
                        $"{nameof(EscapeAlgorithm)}.{nameof(ComputeHistogram)}, (loop=1)"
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

            Parallel.ForEach(_consideredPoints, (x, loopState) =>
            {
                if (token.IsCancellationRequested)
                {
                    loopState.Break();
                }

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
                            $"{nameof(EscapeAlgorithm)}.{nameof(ComputeHistogram)}, (loop=3)"
                            ));

                        reportTimer.Restart();
                    }

                    iterationCount++;
                }
            });

            if (token.IsCancellationRequested)
            {
                return;
            }

            // Else, wasn't cancelled, so points have been evaluated.
            _histogramIsEvaluated = true;
        }

        /// <summary>
        /// Allocates memory for <see cref="_consideredPoints"/>.
        /// Initializes each point-pixel pair that will be evaluated and adds to <see cref="_consideredPoints"/>.
        /// This method can only be called once.
        /// </summary>
        /// <param name="token">Cancellation token.</param>
        /// <exception cref="ArgumentException">If initial run settings are invalid.</exception>
        private void Init(CancellationToken token)
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
                        if (token.IsCancellationRequested)
                        {
                            _consideredPoints = new List<EvalComplexUnit>(TotalSteps);
                            _isInit = false;
                            return;
                        }

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
    }
}
