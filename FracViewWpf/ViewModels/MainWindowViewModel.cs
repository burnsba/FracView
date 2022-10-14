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
using FracViewWpf.Windows;
using Microsoft.Extensions.DependencyInjection;
using SkiaSharp;
using static System.Formats.Asn1.AsnWriter;

namespace FracViewWpf.ViewModels
{
    public class MainWindowViewModel : WindowViewModelBase
    {
        private CancellationTokenSource _cancellationToken = new CancellationTokenSource();
        private ComputeState _computeState = ComputeState.NotRunning;
        private bool _hasRunData = false;

        private int _outputIntervalSec = 1;

        private Stopwatch _algorithmTimer;

        private IEscapeAlgorithm _algorithm;
        private IScene _scene;

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

        public string StatusBarCurrentWork { get; private set; }
        public string StatusBarStepText { get; private set; }
        public double StatusBarProgressValue { get; private set; }
        public string StatusBarElapsedText { get; private set; }

        public ICommand ComputeCommand { get; set; }

        public ICommand ShowColorsWindowCommand { get; set; }
        public ICommand RecolorCommand { get; set; }

        public ImageSource ImageSource => _imageSource;

        public Func<double>? GetParentDisplayGridImageWidth { get; set; } = null;
        public Func<double>? GetParentDisplayGridImageHeight { get; set; } = null;

        public MainWindowViewModel(IScene scene)
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
            ShowColorsWindowCommand = new CommandHandler(ShowColorsWindowCommandHandler);
            RecolorCommand = new CommandHandler(RecolorCommandHandler);

            ComputeCommandText = GetComputeCommandText();

            _scene = scene;

            if (!_scene.ColorRamp.Keyframes.Any())
            {
                _scene.AddDefaultSceneKeyframes();
            }
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

            if (ImageWidth <= 0)
            {
                ImageWidth = 0;
            }

            if (ImageHeight <= 0)
            {
                ImageHeight = 0;
            }

            OnPropertyChanged(nameof(ImageHeight));
            OnPropertyChanged(nameof(ImageWidth));
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

                UiStatusCancelRun();
            }
            else
            {
                _computeState = ComputeState.Running;
                _cancellationToken.TryReset();

                DoTheAlgorithm();

                ComputeCommandText = GetComputeCommandText();
                OnPropertyChanged(nameof(ComputeCommandText));

                UiStatusStartRun();
            }
        }

        private void ShowColorsWindowCommandHandler()
        {
            var svm = ActivatorUtilities.CreateInstance<ColorWindowViewModel>(Workspace.Instance.ServiceProvider, this._scene);

            Workspace.Instance.RecreateSingletonWindow<ColorWindow>(svm);
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
            _hasRunData = false;

            Task.Factory.StartNew(() =>
                {
                    _algorithm.EvaluatePoints(_cancellationToken.Token);
                    _hasRunData = true;
                })
                .ContinueWith(err1 => Workspace.Instance.ShowTaskException(err1, "Error evaluating points"), TaskContinuationOptions.OnlyOnFaulted)
                .ContinueWith(t1 =>
                {
                    RenderImageSource();
                })
                .ContinueWith(err2 => Workspace.Instance.ShowTaskException(err2, "Error rendering image"), TaskContinuationOptions.OnlyOnFaulted)
                .ContinueWith(t2 =>
                {
                    _computeState = ComputeState.NotRunning;
                    _algorithmTimer.Stop();
                    UiStatusFinishRunSuccess();

                    ComputeCommandText = GetComputeCommandText();
                    OnPropertyChanged(nameof(ComputeCommandText));
                })
                .ContinueWith(err3 => Workspace.Instance.ShowTaskException(err3, "Error finalizing image render"), TaskContinuationOptions.OnlyOnFaulted);
        }

        private void UiStatusCancelRun()
        {
            UiUpdateProgress(new ProgressReport(_algorithmTimer.Elapsed.TotalSeconds, 0, 0, "Cancelled."));
        }

        private void UiStatusStartRun()
        {
            UiUpdateProgress(new ProgressReport(_algorithmTimer.Elapsed.TotalSeconds, 0, 0, "Started."));
        }

        private void UiStatusFinishRunSuccess()
        {
            UiUpdateProgress(new ProgressReport(_algorithmTimer.Elapsed.TotalSeconds, 0, 0, "Done."));
        }

        private void UiUpdateProgress(ProgressReport progress)
        {
            var sb = new StringBuilder();
            double donePercent = 0;
            
            if (progress.TotalSteps > 0)
            {
                donePercent = 100.0 * (double)progress.CurrentStep / (double)progress.TotalSteps;
                StatusBarStepText = $"{progress.CurrentStep} / {progress.TotalSteps}";
            }
            else
            {
                StatusBarStepText = String.Empty;
            }

            var elapsedMinutes = (int)(progress.ElapsedSeconds / 60);
            var elapsedSecondsD = progress.ElapsedSeconds;

            if (elapsedMinutes > 0)
            {
                sb.Append($"{elapsedMinutes} min ");
                elapsedSecondsD -= elapsedMinutes * 60;
            }

            sb.Append($"{elapsedSecondsD:N2} sec");

            StatusBarCurrentWork = progress.CurrentWorkName ?? String.Empty;
            
            StatusBarProgressValue = donePercent;
            StatusBarElapsedText = sb.ToString();

            OnPropertyChanged(nameof(StatusBarCurrentWork));
            OnPropertyChanged(nameof(StatusBarStepText));
            OnPropertyChanged(nameof(StatusBarProgressValue));
            OnPropertyChanged(nameof(StatusBarElapsedText));
        }

        private void RenderImageSource()
        {
            var bmp = _scene.ProcessPointsToPixels(_algorithm, _outputIntervalSec, UiUpdateProgress);
            // create an image WRAPPER
            SKImage image = SKImage.FromPixels(bmp.PeekPixels());
            // encode the image (defaults to PNG)
            SKData encoded = image.Encode();
            // get a stream over the encoded data
            Stream stream = encoded.AsStream();

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
        }

        private void RecolorCommandHandler()
        {
            if (_computeState == ComputeState.NotRunning && _hasRunData)
            {
                _algorithmTimer = Stopwatch.StartNew();

                Task.Factory.StartNew(() =>
                {
                    RenderImageSource();
                })
                .ContinueWith(err1 => Workspace.Instance.ShowTaskException(err1, "Error rendering image"), TaskContinuationOptions.OnlyOnFaulted)
                .ContinueWith(t2 =>
                {
                    _computeState = ComputeState.NotRunning;
                    _algorithmTimer.Stop();
                    UiStatusFinishRunSuccess();

                    ComputeCommandText = GetComputeCommandText();
                    OnPropertyChanged(nameof(ComputeCommandText));
                })
                .ContinueWith(err2 => Workspace.Instance.ShowTaskException(err2, "Error finalizing image render"), TaskContinuationOptions.OnlyOnFaulted);
            }
        }

        private enum ComputeState
        {
            NotRunning,
            Running,
            Cancelling,
        }
    }
}
