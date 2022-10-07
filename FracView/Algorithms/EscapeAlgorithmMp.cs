using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FracView.World;
using MultiPrecision;

namespace FracView.Algorithms
{
    public abstract class EscapeAlgorithmMp<T> where T : struct, MultiPrecision.IConstant
    {
        private bool _isInit = false;
        private bool _pointsEvaluated = false;
        private object _lockObject = new object();
        private List<EvalComplexUnitMp<T>>? _consideredPoints = null;

        public abstract string AlgorithmName { get; }
        public abstract string AlgorithmDescription { get; }

        public ComplexPointMp<T> Origin { get; set; } = ComplexPointMp<T>.Zero;

        public double FractalWidth { get; set; }
        public double FractalHeight { get; set; }

        public int StepWidth { get; set; }
        public int StepHeight { get; set; }

        protected int TotalSteps => StepWidth * StepHeight;

        public int MaxIterations { get; set; }

        public double? IterationBreak { get; set; } = null;

        public List<EvalComplexUnitMp<T>> ConsideredPoints
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

        public EscapeAlgorithmMp()
        {
        }

        public void Init(int progressCallbackIntervalSec = 0, Action<ProgressReportMp<T>>? progressCallback = null)
        {
            if (_isInit)
            {
                return;
            }

            if (!IterationBreak.HasValue)
            {
                throw new ArgumentException($"{nameof(IterationBreak)} requires value");
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
                _consideredPoints = new List<EvalComplexUnitMp<T>>(TotalSteps);

                MultiPrecision<T> startX = Origin.Real - (FractalWidth / 2);
                MultiPrecision<T> stepX = FractalWidth / (double)StepWidth;
                MultiPrecision<T> x;

                MultiPrecision<T> startY = Origin.Imag - (FractalHeight / 2);
                MultiPrecision<T> stepY = FractalHeight / (double)StepHeight;
                MultiPrecision<T> y;

                y = startY;
                x = startX;
                int currentStepCount = 0;

                for (int j = 0; j < StepHeight; j++)
                {
                    x = startX;

                    for (int i = 0; i < StepWidth; i++)
                    {
                        var eu = new EvalComplexUnitMp<T>((i, j), (x, y));

                        _consideredPoints.Add(eu);

                        lock(_lockObject)
                        {
                            if (progressCallback != null && progressCallbackIntervalSec > 0 && reportTimer.Elapsed.TotalSeconds > progressCallbackIntervalSec)
                            {
                                progressCallback(new ProgressReportMp<T>(
                                    totalTimer.Elapsed.TotalSeconds,
                                    currentStepCount,
                                    TotalSteps,
                                    new(x,y),
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

        public abstract bool IsStable(EvalComplexUnitMp<T> eu);

        public bool EvaluatePoints(CancellationToken token, int progressCallbackIntervalSec = 0, Action<ProgressReportMp<T>>? progressCallback = null)
        {
            if (!_isInit)
            {
                throw new InvalidOperationException($"Call {nameof(Init)} first");
            }

            bool interrupted = false;
            var reportTimer = Stopwatch.StartNew();
            var totalTimer = Stopwatch.StartNew();
            int stepCount = 0;

            _pointsEvaluated = false;

            Parallel.ForEach(_consideredPoints, (evalPoint, loopState) => {

                if (token.IsCancellationRequested)
                {
                    interrupted = true;
                    loopState.Break();
                }

                lock(_lockObject)
                {
                    if (progressCallback != null && progressCallbackIntervalSec > 0 && reportTimer.Elapsed.TotalSeconds > progressCallbackIntervalSec)
                    {
                        progressCallback(new ProgressReportMp<T>(
                            totalTimer.Elapsed.TotalSeconds,
                            stepCount,
                            TotalSteps,
                            evalPoint.WorldPos,
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

            return !interrupted;
        }
    }
}
