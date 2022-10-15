﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using FracView.Algorithms;
using FracView.Gfx;
using FracViewWpf.Dto;
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
        private bool _anyChangeForReset = false;

        private int _outputIntervalSec = 1;

        private Stopwatch _algorithmTimer;

        private IEscapeAlgorithm _algorithm;
        private IScene _scene;

        private ImageSource _imageSource;

        private RunSettings _previousRunData;
        private RunSettings _uiRunData = new RunSettings();

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
                    _uiRunData.OriginX = d;
                    OriginXIsValid = true;

                    AnyChangeForReset = true;

                    OnPropertyChanged(nameof(TextOriginX));
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
                    _uiRunData.OriginY = d;
                    OriginYIsValid = true;

                    AnyChangeForReset = true;

                    OnPropertyChanged(nameof(TextOriginY));
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
                    _uiRunData.FractalWidth = d;
                    FractalWidthIsValid = true;

                    AnyChangeForReset = true;

                    OnPropertyChanged(nameof(TextFractalWidth));
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
                    _uiRunData.FractalHeight = d;
                    FractalHeightIsValid = true;

                    AnyChangeForReset = true;

                    OnPropertyChanged(nameof(TextFractalHeight));
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
                    _uiRunData.StepWidth = i;
                    StepWidthIsValid = true;

                    RecomputeImageScreenDimensions();

                    AnyChangeForReset = true;

                    OnPropertyChanged(nameof(TextStepWidth));
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
                    _uiRunData.StepHeight = i;
                    StepHeightIsValid = true;

                    RecomputeImageScreenDimensions();

                    AnyChangeForReset = true;

                    OnPropertyChanged(nameof(TextStepHeight));
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
                    _uiRunData.MaxIterations = i;
                    MaxIterationsIsValid = true;

                    AnyChangeForReset = true;

                    OnPropertyChanged(nameof(TextMaxIterations));
                }
                else
                {
                    MaxIterationsIsValid = false;
                }

                OnPropertyChanged(nameof(MaxIterationsIsValid));
            }
        }

        public bool UseHistogram
        {
            get => _uiRunData.UseHistogram;
            set => _uiRunData.UseHistogram = value;
        }

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

        public string TextPixelLeftTop { get; set; }
        public string TextPixelCenter { get; set; }
        public string TextPixelWidthHeight { get; set; }

        public string TextFractalLeftTop { get; set; }
        public string TextFractalCenter { get; set; }
        public string TextFractalWidthHeight { get; set; }

        public string TextMousePixelXy { get; set; }
        public string TextMouseFractalXy { get; set; }

        public double UiScale { get; set; } = 1.0;

        public bool HasRunData
        {
            get => _hasRunData;

            set
            {
                _hasRunData = value;
                OnPropertyChanged(nameof(HasRunData));
            }
        }

        public bool AnyChangeForReset
        {
            get => _anyChangeForReset;

            set
            {
                _anyChangeForReset = value;
                OnPropertyChanged(nameof(AnyChangeForReset));
            }
        }

        public ICommand ComputeCommand { get; set; }

        public ICommand ShowColorsWindowCommand { get; set; }
        public ICommand RecolorCommand { get; set; }
        public ICommand ResetToDefaultCommand { get; set; }
        public ICommand ResetToPreviousCommand { get; set; }

        public ImageSource ImageSource => _imageSource;

        public Func<double>? GetParentDisplayGridImageWidth { get; set; } = null;
        public Func<double>? GetParentDisplayGridImageHeight { get; set; } = null;

        public MainWindowViewModel(IScene scene)
        {
            ResetToDefaultCommandHandler();

            ImageWidth = _uiRunData.StepWidth;
            ImageHeight = _uiRunData.StepHeight;

            _previousRunData = _uiRunData with { };

            ResetToDefaultCommand = new CommandHandler(ResetToDefaultCommandHandler);
            ResetToPreviousCommand = new CommandHandler(ResetToPreviousCommandHandler, () => HasRunData);

            ComputeCommand = new CommandHandler(ComputeCommandHandler);
            ShowColorsWindowCommand = new CommandHandler(ShowColorsWindowCommandHandler);
            RecolorCommand = new CommandHandler(RecolorCommandHandler, () => HasRunData);

            ComputeCommandText = GetComputeCommandText();

            _scene = scene;

            if (!_scene.ColorRamp.Keyframes.Any())
            {
                _scene.AddDefaultSceneKeyframes();
            }

            HasRunData = false;
            AnyChangeForReset = false;
        }

        public void RecomputeImageScreenDimensions()
        {
            if (GetParentDisplayGridImageWidth == null || GetParentDisplayGridImageHeight == null)
            {
                return;
            }

            var displayGridImageHeight = GetParentDisplayGridImageHeight();
            var displayGridImageWidth = GetParentDisplayGridImageWidth();

            if (_previousRunData.StepHeight == 0 || displayGridImageHeight == 0)
            {
                return;
            }

            var worldPixelRatio = (double)_previousRunData.StepWidth / (double)_previousRunData.StepHeight;
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

            if (ImageWidth < 0)
            {
                ImageWidth = 0;
            }

            if (ImageHeight < 0)
            {
                ImageHeight = 0;
            }

            OnPropertyChanged(nameof(ImageHeight));
            OnPropertyChanged(nameof(ImageWidth));
        }

        public void UpdateImagePositionStats(ScrollScaleInfo scrollInfo)
        {
            double scalePixelWidth = ImageWidth;
            double scalePixelHeight = ImageHeight;
            double scalePixelTop = 0;
            double scalePixelLeft = 0;
            double scalePixelCenterX = 0;
            double scalePixelCenterY = 0;

            decimal scaleFractalLeft = _previousRunData.OriginX - (_previousRunData.FractalWidth / 2);
            decimal scaleFractalTop = _previousRunData.OriginY - (_previousRunData.FractalHeight / 2);
            decimal scaleFractalWidth = _previousRunData.FractalWidth;
            decimal scaleFractalHeight = _previousRunData.FractalHeight;

            if (UiScale > 1)
            {
                scalePixelWidth /= UiScale;
                scalePixelHeight /= UiScale;

                scaleFractalWidth /= (decimal)UiScale;
                scaleFractalHeight /= (decimal)UiScale;

                scalePixelLeft = scrollInfo.ContentHorizontalOffset / UiScale;
                scalePixelTop = scrollInfo.ContentVerticalOffset / UiScale;
            }

            decimal fractalToPixelConversionX = _previousRunData.FractalWidth / (decimal)scrollInfo.DesiredSizeX;
            decimal fractalToPixelConversionY = _previousRunData.FractalHeight / (decimal)scrollInfo.DesiredSizeY;
            decimal scaleFractalCenterX = 0;
            decimal scaleFractalCenterY = 0;

            if (UiScale > 1)
            {
                scaleFractalLeft += (decimal)scalePixelLeft * fractalToPixelConversionX;
                scaleFractalTop += (decimal)scalePixelTop * fractalToPixelConversionY;
            }

            scalePixelCenterX = scalePixelLeft + (scalePixelWidth / 2);
            scalePixelCenterY = scalePixelTop + (scalePixelHeight / 2);

            scaleFractalCenterX = scaleFractalLeft + (scaleFractalWidth / 2);
            scaleFractalCenterY = scaleFractalTop + (scaleFractalHeight / 2);

            TextPixelLeftTop = $"{scalePixelLeft:N2}, {scalePixelTop:N2}";
            OnPropertyChanged(nameof(TextPixelLeftTop));

            TextPixelCenter = $"{scalePixelCenterX:N2}, {scalePixelCenterY:N2}";
            OnPropertyChanged(nameof(TextPixelCenter));

            TextPixelWidthHeight = $"{scalePixelWidth:N2}, {scalePixelHeight:N2}";
            OnPropertyChanged(nameof(TextPixelWidthHeight));

            TextFractalLeftTop = $"{scaleFractalLeft}, {scaleFractalTop}";
            OnPropertyChanged(nameof(TextFractalLeftTop));

            TextFractalCenter = $"{scaleFractalCenterX}, {scaleFractalCenterY}";
            OnPropertyChanged(nameof(TextFractalCenter));

            TextFractalWidthHeight = $"{scaleFractalWidth}, {scaleFractalHeight}";
            OnPropertyChanged(nameof(TextFractalWidthHeight));
        }

        public void UpdateMousePositionStats(Point position, ScrollScaleInfo scrollInfo)
        {
            double scalePixelTop = 0;
            double scalePixelLeft = 0;
            double mouseX = 0;
            double mouseY = 0;
            double scaleMousePositionX = position.X;
            double scaleMousePositionY = position.Y;
            decimal fractalMouseX = 0;
            decimal fractalMouseY = 0;

            decimal fractalToPixelConversionX = _previousRunData.FractalWidth / (decimal)scrollInfo.DesiredSizeX;
            decimal fractalToPixelConversionY = _previousRunData.FractalHeight / (decimal)scrollInfo.DesiredSizeY;

            decimal scaleFractalLeft = _previousRunData.OriginX - (_previousRunData.FractalWidth / 2);
            decimal scaleFractalTop = _previousRunData.OriginY - (_previousRunData.FractalHeight / 2);

            if (UiScale > 1)
            {
                scalePixelLeft = scrollInfo.ContentHorizontalOffset / UiScale;
                scalePixelTop = scrollInfo.ContentVerticalOffset / UiScale;

                scaleMousePositionX /= UiScale;
                scaleMousePositionY /= UiScale;

                scaleFractalLeft += (decimal)scalePixelLeft * fractalToPixelConversionX;
                scaleFractalTop += (decimal)scalePixelTop * fractalToPixelConversionY;
            }

            mouseX = scalePixelLeft + scaleMousePositionX;
            mouseY = scalePixelTop + scaleMousePositionY;

            fractalMouseX = scaleFractalLeft + ((decimal)scaleMousePositionX * fractalToPixelConversionX);
            fractalMouseY = scaleFractalTop + ((decimal)scaleMousePositionY * fractalToPixelConversionY);

            TextMousePixelXy = $"{mouseX:N2}, {mouseY:N2}";
            OnPropertyChanged(nameof(TextMousePixelXy));

            TextMouseFractalXy = $"{fractalMouseX}, {fractalMouseY}";
            OnPropertyChanged(nameof(TextMouseFractalXy));
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
            _previousRunData = _uiRunData with { };

            _algorithm = new Mandelbrot(
                _previousRunData,
                _outputIntervalSec,
                UiUpdateProgress);

            _algorithmTimer = Stopwatch.StartNew();
            HasRunData = false;

            Task.Factory.StartNew(() =>
                {
                    _algorithm.EvaluatePoints(_cancellationToken.Token);
                    HasRunData = true;
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

        private void ResetToDefaultCommandHandler()
        {
            _uiRunData.OriginX = decimal.Parse("-0.5");
            TextOriginX = _uiRunData.OriginX.ToString();

            _uiRunData.OriginY = decimal.Parse("0");
            TextOriginY = _uiRunData.OriginY.ToString();

            _uiRunData.FractalWidth = decimal.Parse("4");
            TextFractalWidth = _uiRunData.FractalWidth.ToString();

            _uiRunData.FractalHeight = decimal.Parse("4");
            TextFractalHeight = _uiRunData.FractalHeight.ToString();

            _uiRunData.StepWidth = 512;
            TextStepWidth = _uiRunData.StepWidth.ToString();

            _uiRunData.StepHeight = 512;
            TextStepHeight = _uiRunData.StepHeight.ToString();

            _uiRunData.MaxIterations = 100;
            TextMaxIterations = _uiRunData.MaxIterations.ToString();

            UseHistogram = true;

            AnyChangeForReset = false;
        }

        /// <summary>
        /// Reset position information, but leave histogram unchanged.
        /// </summary>
        private void ResetToPreviousCommandHandler()
        {
            _uiRunData.OriginX = _previousRunData.OriginX;
            TextOriginX = _uiRunData.OriginX.ToString();

            _uiRunData.OriginY = _previousRunData.OriginY;
            TextOriginY = _uiRunData.OriginY.ToString();

            _uiRunData.FractalWidth = _previousRunData.FractalWidth;
            TextFractalWidth = _uiRunData.FractalWidth.ToString();

            _uiRunData.FractalHeight = _previousRunData.FractalHeight;
            TextFractalHeight = _uiRunData.FractalHeight.ToString();

            _uiRunData.StepWidth = _previousRunData.StepWidth;
            TextStepWidth = _uiRunData.StepWidth.ToString();

            _uiRunData.StepHeight = _previousRunData.StepHeight;
            TextStepHeight = _uiRunData.StepHeight.ToString();

            _uiRunData.MaxIterations = _previousRunData.MaxIterations;
            TextMaxIterations = _uiRunData.MaxIterations.ToString();

            AnyChangeForReset = false;
        }

        private enum ComputeState
        {
            NotRunning,
            Running,
            Cancelling,
        }
    }
}
