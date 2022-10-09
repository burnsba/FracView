using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using FracView.Algorithms;
using FracView.Gfx;
using FracViewWpf.Mvvm;
using SkiaSharp;
using static System.Formats.Asn1.AsnWriter;

namespace FracViewWpf.ViewModels
{
    public class MainWindowViewModel : WindowViewModelBase
    {
        private CancellationTokenSource _cancellationToken = new CancellationTokenSource();
        private ComputeState _computeState = ComputeState.NotRunning;

        private int _outputIntervalSec = 4;

        private Stopwatch _algorithmTimer;

        private IEscapeAlgorithm _algorithm;
        private Scene _scene;

        private ImageSource _imageSource;

        private string _textOriginX;
        private string _textOriginY;
        private string _textFractalWidth;
        private string _textFractalHeight;
        private string _textStepWidth;
        private string _textStepHeight;
        private string _textMaxIterations;

        public string TextOriginX
        {
            get => _textOriginX;

            set
            {
                _textOriginX = value;

                if (decimal.TryParse(_textOriginX, out decimal d))
                {
                    OriginX = d;
                    OriginXIsValid = true;

                    OnPropertyChanged(nameof(OriginX));
                }
                else
                {
                    OriginXIsValid = false;
                }

                OnPropertyChanged(nameof(OriginXIsValid));
            }
        }

        public string TextOriginY
        {
            get => _textOriginY;

            set
            {
                _textOriginY = value;

                if (decimal.TryParse(_textOriginY, out decimal d))
                {
                    OriginY = d;
                    OriginYIsValid = true;
                    
                    OnPropertyChanged(nameof(OriginY));
                }
                else
                {
                    OriginYIsValid = false;
                }

                OnPropertyChanged(nameof(OriginYIsValid));
            }
        }

        public string TextFractalWidth
        {
            get => _textFractalWidth;

            set
            {
                _textFractalWidth = value;

                if (decimal.TryParse(_textFractalWidth, out decimal d))
                {
                    FractalWidth = d;
                    FractalWidthIsValid = true;
                    
                    OnPropertyChanged(nameof(FractalWidth));
                }
                else
                {
                    FractalWidthIsValid = false;
                }

                OnPropertyChanged(nameof(FractalWidthIsValid));
            }
        }

        public string TextFractalHeight
        {
            get => _textFractalHeight;

            set
            {
                _textFractalHeight = value;

                if (decimal.TryParse(_textFractalHeight, out decimal d))
                {
                    FractalHeight = d;
                    FractalHeightIsValid = true;

                    OnPropertyChanged(nameof(FractalHeight));
                }
                else
                {
                    FractalHeightIsValid = false;
                }

                OnPropertyChanged(nameof(FractalHeightIsValid));
            }
        }

        public string TextStepWidth
        {
            get => _textStepWidth;

            set
            {
                _textStepWidth = value;

                if (int.TryParse(_textStepWidth, out int i))
                {
                    StepWidth = i;
                    StepWidthIsValid = true;

                    RecomputeImageScreenDimensions();

                    OnPropertyChanged(nameof(StepWidth));
                }
                else
                {
                    StepWidthIsValid = false;
                }

                OnPropertyChanged(nameof(StepWidthIsValid));
            }
        }

        public string TextStepHeight
        {
            get => _textStepHeight;

            set
            {
                _textStepHeight = value;

                if (int.TryParse(_textStepHeight, out int i))
                {
                    StepHeight = i;
                    StepHeightIsValid = true;

                    RecomputeImageScreenDimensions();

                    OnPropertyChanged(nameof(StepHeight));
                }
                else
                {
                    StepHeightIsValid = false;
                }

                OnPropertyChanged(nameof(StepHeightIsValid));
            }
        }

        public string TextMaxIterations
        {
            get => _textMaxIterations;

            set
            {
                _textMaxIterations = value;

                if (int.TryParse(_textMaxIterations, out int i))
                {
                    MaxIterations = i;
                    MaxIterationsIsValid = true;

                    OnPropertyChanged(nameof(MaxIterations));
                }
                else
                {
                    MaxIterationsIsValid = false;
                }

                OnPropertyChanged(nameof(MaxIterationsIsValid));
            }
        }

        public decimal OriginX { get; private set; }
        public decimal OriginY { get; private set; }
        public decimal FractalWidth { get; private set; }
        public decimal FractalHeight { get; private set; }
        public int StepWidth { get; private set; }
        public int StepHeight { get; private set; }
        public int MaxIterations { get; private set; }
        public bool UseHistogram { get; set; }

        public bool OriginXIsValid { get; private set; }
        public bool OriginYIsValid { get; private set; }
        public bool FractalWidthIsValid { get; private set; }
        public bool FractalHeightIsValid { get; private set; }
        public bool StepWidthIsValid { get; private set; }
        public bool StepHeightIsValid { get; private set; }
        public bool MaxIterationsIsValid { get; private set; }

        public int ImageWidth { get; private set; }
        public int ImageHeight { get; private set; }

        public string ComputeCommandText { get; private set; }

        public string StatusBarText { get; private set; }

        public ICommand ComputeCommand { get; set; }

        public ImageSource ImageSource => _imageSource;

        public Func<double>? GetParentDisplayGridImageWidth { get; set; } = null;
        public Func<double>? GetParentDisplayGridImageHeight { get; set; } = null;

        public MainWindowViewModel()
        {
            OriginX = decimal.Parse("0.29999999799999");
            _textOriginX = OriginX.ToString();

            OriginY = decimal.Parse("0.4491000000000016");
            _textOriginY = OriginY.ToString();

            FractalWidth = decimal.Parse("0.00250");
            _textFractalWidth = FractalWidth.ToString();

            FractalHeight = decimal.Parse("0.00250");
            _textFractalHeight = FractalHeight.ToString();

            StepWidth = 512;
            ImageWidth = StepWidth;
            _textStepWidth = StepWidth.ToString();

            StepHeight = 512;
            ImageHeight = StepHeight;
            _textStepHeight = StepHeight.ToString();

            MaxIterations = 1000;
            _textMaxIterations = MaxIterations.ToString();

            UseHistogram = true;

            ComputeCommand = new CommandHandler(ComputeCommandHandler);

            ComputeCommandText = GetComputeCommandText();
        }

        private void ComputeCommandHandler()
        {
            if (_computeState == ComputeState.Running)
            {
                _computeState = ComputeState.NotRunning;
                _cancellationToken.Cancel();

                if (!object.ReferenceEquals(null, _algorithmTimer))
                {
                    _algorithmTimer.Stop();
                }

                ComputeCommandText = GetComputeCommandText();
                OnPropertyChanged(nameof(ComputeCommandText));

                StatusBarText = "Cancelled";
                OnPropertyChanged(nameof(StatusBarText));
            }
            else
            {
                _computeState = ComputeState.Running;
                _cancellationToken.TryReset();

                DoTheAlgorithm();

                ComputeCommandText = GetComputeCommandText();
                OnPropertyChanged(nameof(ComputeCommandText));

                StatusBarText = "Started";
                OnPropertyChanged(nameof(StatusBarText));
            }
        }

        private string GetComputeCommandText()
        {
            return _computeState switch
            {
                ComputeState.Running => "Cancel",
                _ => "Go",
            };
        }

        private void DoTheAlgorithm()
        {
            _algorithm = new Mandelbrot(
                _outputIntervalSec,
                UiUpdateProgress)
            {
                Origin = (OriginX, OriginY),
                FractalWidth = FractalWidth,
                FractalHeight = FractalHeight,
                StepWidth = StepWidth,
                StepHeight = StepHeight,
                MaxIterations = MaxIterations,
                UseHistogram = UseHistogram,
            };

            _algorithmTimer = Stopwatch.StartNew();

            Task.Factory.StartNew(() =>
                {
                    _algorithm.EvaluatePoints(_cancellationToken.Token);
                })
                .ContinueWith(t1 =>
                {
                    _computeState = ComputeState.NotRunning;
                    _algorithmTimer.Stop();
                    UiStatusFinishRunSuccess();
                })
                .ContinueWith(t2 =>
                {
                    RenderImageSource();

                    ComputeCommandText = GetComputeCommandText();
                    OnPropertyChanged(nameof(ComputeCommandText));
                });
        }

        private void UiStatusFinishRunSuccess()
        {
            var sb = new StringBuilder();

            sb.Append("Done.");

            var elapsedMinutes = (int)_algorithmTimer.Elapsed.TotalMinutes;
            var elapsedSecondsD = _algorithmTimer.Elapsed.TotalSeconds;

            if (elapsedMinutes > 0)
            {
                sb.Append($" {elapsedMinutes} min");

                elapsedSecondsD -= elapsedMinutes * 60;
            }

            sb.Append($" {elapsedSecondsD:N2} sec");

            StatusBarText = sb.ToString();
            OnPropertyChanged(nameof(StatusBarText));
        }

        private void UiUpdateProgress(ProgressReport progress)
        {
            var sb = new StringBuilder();
            double donePercent = 100.0 * (double)progress.CurrentStep / (double)progress.TotalSteps;
            sb.Append($"work: {progress.CurrentWorkName}; ");
            sb.Append($"elapsed: {progress.ElapsedSeconds:N2} sec; ");
            sb.Append($"pixels: {progress.CurrentStep} / {progress.TotalSteps} = {donePercent:N2}%");

            StatusBarText = sb.ToString();
            OnPropertyChanged(nameof(StatusBarText));
        }

        private void SetSceneKeyframes(Scene scene)
        {
            scene.ColorRamp.Keyframes.Add(new Keyframe<SKColor, double>()
            {
                IntervalStart = 0,
                IntervalEnd = 0.88,
                ValueStart = new SKColor(20, 20, 240), // blue
                ValueEnd = new SKColor(20, 250, 250), // cyan
            });
            scene.ColorRamp.Keyframes.Add(new Keyframe<SKColor, double>()
            {
                IntervalStart = 0.88,
                IntervalEnd = 0.93,
                ValueStart = new SKColor(20, 250, 250), // cyan
                ValueEnd = new SKColor(255, 255, 40), // yellow
            });
            scene.ColorRamp.Keyframes.Add(new Keyframe<SKColor, double>()
            {
                IntervalStart = 0.93,
                IntervalEnd = 0.97,
                ValueStart = new SKColor(255, 255, 40), // yellow
                ValueEnd = new SKColor(250, 128, 0), // orange
            });
            scene.ColorRamp.Keyframes.Add(new Keyframe<SKColor, double>()
            {
                IntervalStart = 0.97,
                IntervalEnd = 1,
                ValueStart = new SKColor(250, 128, 0), // orange
                ValueEnd = new SKColor(120, 60, 0), // orange
            });
        }

        private void RenderImageSource()
        {
            if (object.ReferenceEquals(null, _scene))
            {
                _scene = new Scene();
                SetSceneKeyframes(_scene);
            }

            var bmp = _scene.ProcessPointsToPixels(_algorithm, _outputIntervalSec, UiUpdateProgress);
            // create an image WRAPPER
            SKImage image = SKImage.FromPixels(bmp.PeekPixels());
            // encode the image (defaults to PNG)
            SKData encoded = image.Encode();
            // get a stream over the encoded data
            Stream stream = encoded.AsStream();

            //using (MemoryStream stream = new MemoryStream())
            //{
                //bitmap.Save(stream, ImageFormat.Bmp);

                stream.Position = 0;
                BitmapImage result = new BitmapImage();
                result.BeginInit();
                // According to MSDN, "The default OnDemand cache option retains access to the stream until the image is needed."
                // Force the bitmap to load right now so we can dispose the stream.
                result.CacheOption = BitmapCacheOption.OnLoad;
                result.StreamSource = stream;
                result.EndInit();
                result.Freeze();

                _imageSource = result;
            OnPropertyChanged(nameof(ImageSource));
            //}
        }

        public void RecomputeImageScreenDimensions()
        {
            if (GetParentDisplayGridImageWidth == null || GetParentDisplayGridImageHeight == null)
            {
                return;
            }

            var displayGridImageHeight = GetParentDisplayGridImageHeight();
            var displayGridImageWidth = GetParentDisplayGridImageWidth();

            if (StepHeight == 0 || displayGridImageHeight == 0)
            {
                return;
            }

            var worldPixelRatio = (double)StepWidth / (double)StepHeight;
            var uiPixelRatio = (double)displayGridImageWidth / (double)displayGridImageHeight;

            if (uiPixelRatio >= 1)
            {
                // ui display width is larger, height needs to shrink to accomodate
                ImageHeight = (int)(displayGridImageHeight) - 1;
                ImageWidth = (int)((double)ImageHeight * worldPixelRatio) - 1;
            }
            else
            {
                // ui display height is larger, width needs to shrink to accomodate
                ImageWidth = (int)(displayGridImageWidth) - 1;
                ImageHeight = (int)((double)ImageWidth * worldPixelRatio) - 1;
            }

            OnPropertyChanged(nameof(ImageHeight));
            OnPropertyChanged(nameof(ImageWidth));
        }

        private enum ComputeState
        {
            NotRunning,
            Running,
            Cancelling,
        }
    }
}
