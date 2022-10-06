using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FracView.World;

namespace FracView.Algorithms
{
    public abstract class EscapeAlgorithm
    {
        private bool _isInit = false;
        private bool _pointsEvaluated = false;
        private object _lockObject = new object();
        private List<EvalComplexUnit>? _consideredPoints = null;

        public abstract string AlgorithmName { get; }
        public abstract string AlgorithmDescription { get; }

        public ComplexPoint Origin { get; set; } = ComplexPoint.Zero;

        public double FractalWidth { get; set; }
        public double FractalHeight { get; set; }

        public int StepWidth { get; set; }
        public int StepHeight { get; set; }

        protected int TotalSteps => StepWidth * StepHeight;

        public int MaxIterations { get; set; }

        public double? IterationBreak { get; set; } = null;

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

        public EscapeAlgorithm()
        {
        }

        public void Init()
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

            try
            {
                _consideredPoints = new List<EvalComplexUnit>(TotalSteps);

                double startX = Origin.Real - (FractalWidth / 2);
                double stepX = FractalWidth / (double)StepWidth;
                double x;

                double startY = Origin.Imag - (FractalHeight / 2);
                double stepY = FractalHeight / (double)StepHeight;
                double y;

                y = startY;
                x = startX;

                for (int j = 0; j < StepHeight; j++)
                {
                    x = startX;

                    for (int i = 0; i < StepWidth; i++)
                    {
                        var eu = new EvalComplexUnit((i, j), (x, y));

                        _consideredPoints.Add(eu);

                        x += stepX;
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

        public abstract bool IsStable(EvalComplexUnit eu);

        public bool EvaluatePoints(CancellationToken token, int elapsedSecondsProgress = 0, Action<ProgressReport>? progressCallback = null)
        {
            if (!_isInit)
            {
                throw new InvalidOperationException($"Call {nameof(Init)} first");
            }

            bool interrupted = false;
            var startTime = DateTime.Now;
            var lastProgressOutput = DateTime.Now.AddYears(1);
            int stepCount = 0;

            _pointsEvaluated = false;

            Parallel.ForEach(_consideredPoints, (evalPoint, loopState) => {

                if (token.IsCancellationRequested)
                {
                    interrupted = true;
                    loopState.Break();
                }

                if (elapsedSecondsProgress > 0 && progressCallback != null)
                {
                    lock (_lockObject)
                    {
                        if (Math.Abs((DateTime.Now - lastProgressOutput).TotalSeconds) > (double)elapsedSecondsProgress)
                        {
                            lastProgressOutput = DateTime.Now;
                            progressCallback(new ProgressReport(
                                (DateTime.Now - startTime).TotalSeconds,
                                stepCount,
                                TotalSteps,
                                evalPoint.WorldPos
                                ));
                        }
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
