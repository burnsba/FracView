using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using FracView.Algorithms;
using FracView.Dto;
using FracView.Gfx;
using FracViewWpf.Converters;
using FracViewWpf.Dto;
using FracViewWpf.Mvvm;
using FracViewWpf.Windows;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using SkiaSharp;

namespace FracViewWpf.ViewModels
{
    /// <summary>
    /// Primary class of the application.
    /// Handles all "backend" logic routed from main ui window.
    /// </summary>
    public class MainWindowViewModel : WindowViewModelBase
    {
        /// <summary>
        /// Default file to save runtime settings to.
        /// </summary>
        private const string SessionJsonFilename = "session.json";

        /// <summary>
        /// Default filename to save image to.
        /// </summary>
        private const string SaveAsDefaultFilename = "mandelbrot";

        /// <summary>
        /// Default file extension when saving image.
        /// </summary>
        private const string SaveAsDefaultExtension = ".png";

        /// <summary>
        /// Supported file extensions.
        /// </summary>
        private const string SaveAsFilters =
            "PNG format|*.png" +
            "|Jpeg format|*.jpg" +
            "|BMP format|*.bmp" +
            "|Gif format|*.gif";
        
        /// <summary>
        /// Progress report update interval.
        /// </summary>
        private readonly int _outputIntervalSec = 1;
        private readonly IScene _scene;
        private readonly RunSettings _uiRunData = new();
        private readonly RunSettings _targetRunData = new();

        private CancellationTokenSource _cancellationTokenCompute = new();
        private CancellationTokenSource _cancellationTokenColor = new();

        private List<Type> _availableAlgorithmTypes;

        private ComputeState _computeState = ComputeState.NotRunning;
        private bool _hasRunData = false;
        private bool _anyChangeForReset = false;
        private DateTime _runDataTime;

        private Stopwatch? _algorithmTimer;

        private IEscapeAlgorithm? _algorithm;

        private SKBitmap? _skbmp;
        private ImageSource? _imageSource;

        private RunSettings _previousRunData;

        private string _textOriginX;
        private string _textOriginY;
        private string _textFractalWidth;
        private string _textFractalHeight;
        private string _textStepWidth;
        private string _textStepHeight;
        private string _textMaxIterations;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public MainWindowViewModel(IScene scene)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
            if (object.ReferenceEquals(null, scene))
            {
                throw new NullReferenceException(nameof(scene));
            }

            _availableAlgorithmTypes = Assembly
                .GetAssembly(typeof(FracView.Algorithms.EscapeAlgorithm))!
                .GetTypes()
                .Where(x =>
                    !x.IsAbstract
                    && typeof(FracView.Algorithms.EscapeAlgorithm).IsAssignableFrom(x))
                .ToList();

            AvailableAlgorithms = _availableAlgorithmTypes.Select(x => x.FullName).ToList()!;
            SelectedAlgorithmName = _availableAlgorithmTypes.Select(x => x.FullName).First()!;
            SelectedAlgorithmType = _availableAlgorithmTypes.First();

            ResetToDefaultCommandHandler();

            _previousRunData = _uiRunData with { };

            SaveAsCommand = new RelayCommand<string>(b => SaveAsCommandHandler(ToBoolConverter.ToBool(b)), () => HasRunData);

            TargetFromViewCommand = new CommandHandler(TargetFromViewCommandHandler);
            ToggleCrosshairCommand = new CommandHandler(ToggleCrosshairCommandHandler);

            ResetToDefaultCommand = new CommandHandler(ResetToDefaultCommandHandler);
            ResetToPreviousCommand = new CommandHandler(ResetToPreviousCommandHandler, () => HasRunData);

            ComputeCommand = new CommandHandler(ComputeCommandHandler);
            ShowColorsWindowCommand = new CommandHandler(ShowColorsWindowCommandHandler);
            RecolorCommand = new CommandHandler(RecolorCommandHandler, () => HasRunData);
            ResetZoomCommand = new CommandHandler(ResetZoomCommandHandler);

            ComputeCommandText = GetComputeCommandText();

            // set crosshair state to shown, then trigger command to hide, so all relevant state is updated.
            ShowCrosshair = true;
            ToggleCrosshairCommandHandler();

            _scene = scene;

            if (!_scene.ColorRamp.Keyframes.Any())
            {
                _scene.AddDefaultSceneKeyframes();
            }

            HasRunData = false;
            AnyChangeForReset = false;
        }

        /// <summary>
        /// Event after algorithm finishes calculating and rendering points.
        /// Hooked by main window to reset UI scale.
        /// </summary>
        public event EventHandler<EventArgs>? AfterRunCompleted;

        /// <summary>
        /// Event to request user interface to reset zoom settings.
        /// </summary>
        public event EventHandler<EventArgs>? NotifyUiResetZoomRequest;

        /// <summary>
        /// Gets or sets the text field for world origin x.
        /// </summary>
        public string TextOriginX
        {
            get => _textOriginX;

            set
            {
                _textOriginX = value;

                if (MathString.DecimalTryParseMathString(_textOriginX, out decimal d, out bool hasMath))
                {
                    _uiRunData.OriginX = d;
                    OriginXIsValid = true;

                    AnyChangeForReset = true;

                    if (hasMath)
                    {
                        _textOriginX = _uiRunData.OriginX.ToString();
                    }

                    OnPropertyChanged(nameof(TextOriginX));
                }
                else
                {
                    OriginXIsValid = false;
                }

                OnPropertyChanged(nameof(OriginXIsValid));
            }
        }

        /// <summary>
        /// Gets or sets the text field for world origin y.
        /// </summary>
        public string TextOriginY
        {
            get => _textOriginY;

            set
            {
                _textOriginY = value;

                if (MathString.DecimalTryParseMathString(_textOriginY, out decimal d, out bool hasMath))
                {
                    _uiRunData.OriginY = d;
                    OriginYIsValid = true;

                    AnyChangeForReset = true;

                    if (hasMath)
                    {
                        _textOriginY = _uiRunData.OriginY.ToString();
                    }

                    OnPropertyChanged(nameof(TextOriginY));
                }
                else
                {
                    OriginYIsValid = false;
                }

                OnPropertyChanged(nameof(OriginYIsValid));
            }
        }

        /// <summary>
        /// Gets or sets the text field for world width range.
        /// </summary>
        public string TextFractalWidth
        {
            get => _textFractalWidth;

            set
            {
                _textFractalWidth = value;

                if (MathString.DecimalTryParseMathString(_textFractalWidth, out decimal d, out bool hasMath))
                {
                    _uiRunData.FractalWidth = d;
                    FractalWidthIsValid = true;

                    AnyChangeForReset = true;

                    if (hasMath)
                    {
                        _textFractalWidth = _uiRunData.FractalWidth.ToString();
                    }

                    OnPropertyChanged(nameof(TextFractalWidth));
                }
                else
                {
                    FractalWidthIsValid = false;
                }

                OnPropertyChanged(nameof(FractalWidthIsValid));
            }
        }

        /// <summary>
        /// Gets or sets the text field for world height range.
        /// </summary>
        public string TextFractalHeight
        {
            get => _textFractalHeight;

            set
            {
                _textFractalHeight = value;

                if (MathString.DecimalTryParseMathString(_textFractalHeight, out decimal d, out bool hasMath))
                {
                    _uiRunData.FractalHeight = d;
                    FractalHeightIsValid = true;

                    AnyChangeForReset = true;

                    if (hasMath)
                    {
                        _textFractalHeight = _uiRunData.FractalHeight.ToString();
                    }

                    OnPropertyChanged(nameof(TextFractalHeight));
                }
                else
                {
                    FractalHeightIsValid = false;
                }

                OnPropertyChanged(nameof(FractalHeightIsValid));
            }
        }

        /// <summary>
        /// Gets or sets the text field for pixel width.
        /// </summary>
        public string TextStepWidth
        {
            get => _textStepWidth;

            set
            {
                _textStepWidth = value;

                if (MathString.IntTryParseMathString(_textStepWidth, out int i, out bool hasMath))
                {
                    _uiRunData.StepWidth = i;
                    StepWidthIsValid = true;

                    AnyChangeForReset = true;

                    if (hasMath)
                    {
                        _textStepWidth = _uiRunData.StepWidth.ToString();
                    }

                    OnPropertyChanged(nameof(TextStepWidth));
                }
                else
                {
                    StepWidthIsValid = false;
                }

                OnPropertyChanged(nameof(StepWidthIsValid));
            }
        }

        /// <summary>
        /// Gets the backing value for pixel width.
        /// </summary>
        public int StepWidth => _previousRunData.StepWidth;

        /// <summary>
        /// Gets or sets the text field for pixel height.
        /// </summary>
        public string TextStepHeight
        {
            get => _textStepHeight;

            set
            {
                _textStepHeight = value;

                if (MathString.IntTryParseMathString(_textStepHeight, out int i, out bool hasMath))
                {
                    _uiRunData.StepHeight = i;
                    StepHeightIsValid = true;

                    //RecomputeImageScreenDimensions();

                    AnyChangeForReset = true;

                    if (hasMath)
                    {
                        _textStepHeight = _uiRunData.StepHeight.ToString();
                    }

                    OnPropertyChanged(nameof(TextStepHeight));
                }
                else
                {
                    StepHeightIsValid = false;
                }

                OnPropertyChanged(nameof(StepHeightIsValid));
            }
        }

        /// <summary>
        /// Gets the backing value for pixel height.
        /// </summary>
        public int StepHeight => _previousRunData.StepHeight;

        /// <summary>
        /// Gets or sets the text field for max iterations.
        /// </summary>
        public string TextMaxIterations
        {
            get => _textMaxIterations;

            set
            {
                _textMaxIterations = value;

                if (MathString.IntTryParseMathString(_textMaxIterations, out int i, out bool hasMath))
                {
                    _uiRunData.MaxIterations = i;
                    MaxIterationsIsValid = true;

                    AnyChangeForReset = true;

                    if (hasMath)
                    {
                        _textMaxIterations = _uiRunData.MaxIterations.ToString();
                    }

                    OnPropertyChanged(nameof(TextMaxIterations));
                }
                else
                {
                    MaxIterationsIsValid = false;
                }

                OnPropertyChanged(nameof(MaxIterationsIsValid));
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether histogram data should be computed or not.
        /// </summary>
        public bool UseHistogram
        {
            get => _uiRunData.UseHistogram;
            set => _uiRunData.UseHistogram = value;
        }

        /// <summary>
        /// Gets or sets a value indicating whether <see cref="TextOriginX"/> is valid.
        /// </summary>
        public bool OriginXIsValid { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether <see cref="TextOriginY"/> is valid.
        /// </summary>
        public bool OriginYIsValid { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether <see cref="TextFractalWidth"/> is valid.
        /// </summary>
        public bool FractalWidthIsValid { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether <see cref="TextFractalHeight"/> is valid.
        /// </summary>
        public bool FractalHeightIsValid { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether <see cref="TextStepWidth"/> is valid.
        /// </summary>
        public bool StepWidthIsValid { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether <see cref="TextStepHeight"/> is valid.
        /// </summary>
        public bool StepHeightIsValid { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether <see cref="TextMaxIterations"/> is valid.
        /// </summary>
        public bool MaxIterationsIsValid { get; private set; }

        /// <summary>
        /// Gets or sets the text of the button to start/stop computing points.
        /// </summary>
        public string ComputeCommandText { get; private set; }

        /// <summary>
        /// Gets or sets the text of the status bar "current work" section.
        /// </summary>
        public string StatusBarCurrentWork { get; private set; }

        /// <summary>
        /// Gets or sets the text of the status bar "current step count" section.
        /// </summary>
        public string StatusBarStepText { get; private set; }

        /// <summary>
        /// Gets or sets the status bar progress bar percent.
        /// </summary>
        public double StatusBarProgressValue { get; private set; }

        /// <summary>
        /// Gets or sets the status bar "elapsed time" section.
        /// </summary>
        public string StatusBarElapsedText { get; private set; }

        /// <summary>
        /// Gets or sets stats area pixel left/top description.
        /// </summary>
        public string TextPixelLeftTop { get; set; }

        /// <summary>
        /// Gets or sets stats area center pixel description.
        /// </summary>
        public string TextPixelCenter { get; set; }

        /// <summary>
        /// Gets or sets stats area pixel width/height description.
        /// </summary>
        public string TextPixelWidthHeight { get; set; }

        /// <summary>
        /// Gets or sets stats area view area world width/height.
        /// </summary>
        public string TextViewWidthHeight { get; set; }

        /// <summary>
        /// Gets or sets stats area world left/top.
        /// </summary>
        public string TextFractalLeftTop { get; set; }

        /// <summary>
        /// Gets or sets stats area center world description.
        /// </summary>
        public string TextFractalCenter { get; set; }

        /// <summary>
        /// Gets or sets stats area world width/height description.
        /// </summary>
        public string TextFractalWidthHeight { get; set; }

        /// <summary>
        /// Gets or sets stats area mouse pixel position.
        /// </summary>
        public string TextMousePixelXy { get; set; }

        /// <summary>
        /// Gets or sets stats area mouse world position.
        /// </summary>
        public string TextMouseFractalXy { get; set; }

        /// <summary>
        /// Gets or sets current user interface scale.
        /// </summary>
        public double UiScale { get; set; } = 1.0;

        /// <summary>
        /// Gets or sets a value indicating whether the algorithm
        /// has run data associated with it.
        /// </summary>
        public bool HasRunData
        {
            get => _hasRunData;

            set
            {
                _hasRunData = value;
                OnPropertyChanged(nameof(HasRunData));
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether it should be possible
        /// to reset the "standard" settings back to default.
        /// </summary>
        public bool AnyChangeForReset
        {
            get => _anyChangeForReset;

            set
            {
                _anyChangeForReset = value;
                OnPropertyChanged(nameof(AnyChangeForReset));
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the overlay crosshair
        /// should be shown or not.
        /// </summary>
        public bool ShowCrosshair { get; set; }

        /// <summary>
        /// Gets or sets the text of the button to show or hide the crosshair.
        /// </summary>
        public string ShowCrosshairCommandText { get; set; }

        /// <summary>
        /// Gets or sets the UI names of available algorithms.
        /// </summary>
        public List<string> AvailableAlgorithms { get; set; }

        /// <summary>
        /// Gets or sets the UI currently selected algorithm name.
        /// </summary>
        public string? SelectedAlgorithmName
        {
            get => _uiRunData.AlgorithmName;
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    _uiRunData.AlgorithmName = value;
                    SelectedAlgorithmType = _availableAlgorithmTypes.First(x => x.FullName == value);
                }
            }
        }

        /// <summary>
        /// Gets or sets the type of the currently selected algorithm.
        /// </summary>
        public Type SelectedAlgorithmType { get; set; }

        /// <summary>
        /// Gets or sets the command to start/top computing points.
        /// </summary>
        public ICommand ComputeCommand { get; set; }

        /// <summary>
        /// Gets or sets the command to show the colors management windows.
        /// </summary>
        public ICommand ShowColorsWindowCommand { get; set; }

        /// <summary>
        /// Gets or sets the command to apply coloramp to the current run data.
        /// </summary>
        public ICommand RecolorCommand { get; set; }

        /// <summary>
        /// Gets or sets the command to reset settings to default.
        /// </summary>
        public ICommand ResetToDefaultCommand { get; set; }

        /// <summary>
        /// Gets or sets the command to reset settings to the last run data.
        /// </summary>
        public ICommand ResetToPreviousCommand { get; set; }

        /// <summary>
        /// Gets or sets the command to update the run settings based
        /// on the currently viewed area.
        /// </summary>
        public ICommand TargetFromViewCommand { get; set; }

        /// <summary>
        /// Gets or sets the command to save the result as an image.
        /// </summary>
        public ICommand SaveAsCommand { get; set; }

        /// <summary>
        /// Gets or sets the command to show or hide the crosshair.
        /// </summary>
        public ICommand ToggleCrosshairCommand { get; set; }

        /// <summary>
        /// Gets or sets the command to reset the zoom.
        /// </summary>
        public ICommand ResetZoomCommand { get; set; }

        /// <summary>
        /// Gets or sets the image containing the rendered results.
        /// </summary>
        public ImageSource? ImageSource => _imageSource;

        /// <summary>
        /// Updates stats area text properties for view info based on the scrollviewer values and current ui zoom.
        /// </summary>
        /// <param name="scrollInfo">Scroll view values.</param>
        public void UpdateImagePositionStats(ScrollScaleInfo scrollInfo)
        {
            if (scrollInfo.ExtentWidth == 0 || scrollInfo.ExtentHeight == 0)
            {
                return;
            }

            if (_previousRunData.StepWidth == 0 || _previousRunData.StepHeight == 0)
            {
                return;
            }

            double scalePixelLeft = _previousRunData.StepWidth
                * scrollInfo.ContentHorizontalOffset
                / scrollInfo.ExtentWidth;

            double scalePixelTop = _previousRunData.StepHeight
                * scrollInfo.ContentVerticalOffset
                / scrollInfo.ExtentHeight;

            double pixelScaleRatioX = (scrollInfo.DesiredSizeX - System.Windows.SystemParameters.VerticalScrollBarWidth) / scrollInfo.ExtentWidth;
            double pixelScaleRatioY = (scrollInfo.DesiredSizeY - System.Windows.SystemParameters.HorizontalScrollBarHeight) / scrollInfo.ExtentHeight;

            double scalePixelWidth = (double)_previousRunData.StepWidth;
            double scalePixelHeight = (double)_previousRunData.StepHeight;

            double scaleViewWidth = pixelScaleRatioX * scalePixelWidth;
            double scaleViewHeight = pixelScaleRatioY * scalePixelHeight;

            double scalePixelCenterX = scalePixelLeft + (scaleViewWidth / 2);
            double scalePixelCenterY = scalePixelTop + (scaleViewHeight / 2);

            //

            decimal scaleFractalLeft = _previousRunData.OriginX - (_previousRunData.FractalWidth / 2)
                + ((decimal)scalePixelLeft / _previousRunData.StepWidth) * _previousRunData.FractalWidth;

            decimal scaleFractalTop = _previousRunData.OriginY + (_previousRunData.FractalHeight / 2)
                - ((decimal)scalePixelTop / _previousRunData.StepHeight) * _previousRunData.FractalHeight;

            decimal scaleFractalWidth = _previousRunData.FractalWidth / (decimal)UiScale;
            decimal scaleFractalHeight = _previousRunData.FractalHeight / (decimal)UiScale;

            decimal scaleFractalCenterX = scaleFractalLeft + (_previousRunData.FractalWidth * (decimal)pixelScaleRatioX / 2);
            decimal scaleFractalCenterY = scaleFractalTop - (_previousRunData.FractalHeight * (decimal)pixelScaleRatioY / 2);

            _targetRunData.OriginX = scaleFractalCenterX;
            _targetRunData.OriginY = scaleFractalCenterY;
            _targetRunData.FractalWidth = scaleFractalWidth;
            _targetRunData.FractalHeight = scaleFractalHeight;

            TextPixelLeftTop = $"{scalePixelLeft:N2}, {scalePixelTop:N2}";
            OnPropertyChanged(nameof(TextPixelLeftTop));

            TextPixelCenter = $"{scalePixelCenterX:N2}, {scalePixelCenterY:N2}";
            OnPropertyChanged(nameof(TextPixelCenter));

            TextPixelWidthHeight = $"{scalePixelWidth:N2}, {scalePixelHeight:N2}";
            OnPropertyChanged(nameof(TextPixelWidthHeight));

            TextViewWidthHeight = $"{scaleViewWidth:N2}, {scaleViewHeight:N2}";
            OnPropertyChanged(nameof(TextViewWidthHeight));

            TextFractalLeftTop = $"{scaleFractalLeft}, {scaleFractalTop}";
            OnPropertyChanged(nameof(TextFractalLeftTop));

            TextFractalCenter = $"{scaleFractalCenterX}, {scaleFractalCenterY}";
            OnPropertyChanged(nameof(TextFractalCenter));

            TextFractalWidthHeight = $"{scaleFractalWidth}, {scaleFractalHeight}";
            OnPropertyChanged(nameof(TextFractalWidthHeight));
        }

        /// <summary>
        /// Updates stats area text properties for mouse info based on the scrollviewer values and current ui zoom.
        /// </summary>
        /// <param name="scrollInfo">Scroll view values.</param>
        public void UpdateMousePositionStats(Point position)
        {
            decimal fractalMouseX;
            decimal fractalMouseY;

            fractalMouseX = _previousRunData.OriginX
                - (_previousRunData.FractalWidth / 2)
                + (_previousRunData.FractalWidth * (decimal)position.X / (decimal)_previousRunData.StepWidth);

            fractalMouseY = _previousRunData.OriginY
                - (_previousRunData.FractalHeight / 2)
                + (_previousRunData.FractalHeight * (decimal)position.Y / (decimal)_previousRunData.StepHeight);

            TextMousePixelXy = $"{position.X:N2}, {position.Y:N2}";
            OnPropertyChanged(nameof(TextMousePixelXy));

            TextMouseFractalXy = $"{fractalMouseX}, {fractalMouseY}";
            OnPropertyChanged(nameof(TextMouseFractalXy));
        }

        /// <summary>
        /// Attempts to load the default settings json file and set properties.
        /// </summary>
        public void LoadSessionJson()
        {
            if (!System.IO.File.Exists(SessionJsonFilename))
            {
                return;
            }

            var fileContent = System.IO.File.ReadAllText(SessionJsonFilename);
            SessionSettings? settings;

            try
            {
                settings = JsonConvert.DeserializeObject<SessionSettings>(fileContent);
            }
            catch (Exception ex)
            {
                var ewvm = new ErrorWindowViewModel($"Error loading saved session information", ex)
                {
                    ButtonText = "Ok",
                };

                Workspace.Instance.RecreateSingletonWindow<ErrorWindow>(ewvm);

                return;
            }

            if (object.ReferenceEquals(null, settings))
            {
                var ewvm = new ErrorWindowViewModel()
                {
                    HeaderMessage = "Error loading saved session information",
                    ButtonText = "Ok",
                };

                Workspace.Instance.RecreateSingletonWindow<ErrorWindow>(ewvm);

                return;
            }

            _previousRunData = settings.RunSettings with { };
            ResetToPreviousCommandHandler();

            if (!object.ReferenceEquals(null, _scene))
            {
                _scene.StableColor = settings.GetStableColor();
                _scene.ColorRamp.Keyframes = settings.GetColorRampKeyframes();
            }
        }

        /// <summary>
        /// Serializes <see cref="_previousRunData"/> and saves as json text file.
        /// </summary>
        private void SaveSessionJson()
        {
            var settings = new SessionSettings()
            {
                RunSettings = _previousRunData with { },
            };

            if (!object.ReferenceEquals(null, _scene))
            {
                settings.SetStableColor(_scene.StableColor);
                settings.SetColorRampKeyframes(_scene.ColorRamp.Keyframes);
            }

            var json = JsonConvert.SerializeObject(settings, Formatting.Indented);
            System.IO.File.WriteAllText(SessionJsonFilename, json);
        }

        /// <summary>
        /// Command handler for primary "run" button.
        /// Either triggers token cancellation, or starts a new run.
        /// </summary>
        private void ComputeCommandHandler()
        {
            if (_computeState == ComputeState.RunningCompute)
            {
                _cancellationTokenCompute.Cancel();
            }
            else if (_computeState == ComputeState.RunningColor)
            {
                _cancellationTokenColor.Cancel();
            }
            else if(_computeState == ComputeState.NotRunning)
            {
                if (object.ReferenceEquals(null, _algorithmTimer))
                {
                    _algorithmTimer = Stopwatch.StartNew();
                }

                UiStatusStartRun();

                DoTheAlgorithm();
            }
        }

        /// <summary>
        /// Command handler to show the colors management window.
        /// </summary>
        private void ShowColorsWindowCommandHandler()
        {
            var svm = ActivatorUtilities.CreateInstance<ColorWindowViewModel>(Workspace.Instance.ServiceProvider, this._scene);

            svm.SceneChanged += Svm_SceneChanged;

            Workspace.Instance.RecreateSingletonWindow<ColorWindow>(svm);
        }

        /// <summary>
        /// Callback when notified the scene has been changed.
        /// Writes current settings to disk.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Svm_SceneChanged(object? sender, SceneEventArgs e)
        {
            if (object.ReferenceEquals(null, e) || object.ReferenceEquals(null, e.Scene))
            {
                return;
            }

            // the scene in the argument property should be a reference to the same scene
            // in this viewmodel.
            SaveSessionJson();
        }

        /// <summary>
        /// Gets the text to display on the main "run" button.
        /// </summary>
        /// <returns></returns>
        private string GetComputeCommandText()
        {
            return _computeState switch
            {
                ComputeState.RunningCompute => "Cancel",
                ComputeState.RunningColor => "Cancel",
                _ => "Go",
            };
        }

        /// <summary>
        /// Main method to start a new run.
        /// Saves current settings to disk, instantiates a new <see cref="_algorithm"/>,
        /// then starts a new computation task in the background.
        /// </summary>
        private void DoTheAlgorithm()
        {
            _previousRunData = _uiRunData with { };

            // Save current settings, only after setting _previousRunData
            SaveSessionJson();

            _algorithm = Activator.CreateInstance(SelectedAlgorithmType, new object?[] { _previousRunData, _outputIntervalSec, UiUpdateProgress }) as IEscapeAlgorithm;

            Task.Factory.StartNew(() =>
            {
                _cancellationTokenCompute = new CancellationTokenSource();
                _algorithmTimer = Stopwatch.StartNew();
                HasRunData = false;

                _computeState = ComputeState.RunningCompute;
                ComputeCommandText = GetComputeCommandText();
                OnPropertyChanged(nameof(ComputeCommandText));

                _algorithm.EvaluatePoints(_cancellationTokenCompute.Token);
                _runDataTime = DateTime.Now;

                if (_cancellationTokenCompute.IsCancellationRequested)
                {
                    _hasRunData = false;
                    _computeState = ComputeState.NotRunning;
                    _algorithmTimer.Stop();

                    ComputeCommandText = GetComputeCommandText();
                    OnPropertyChanged(nameof(ComputeCommandText));
                    UiStatusCancelRun();

                    return;
                }

                HasRunData = true;

                _computeState = ComputeState.NotRunning;
                _algorithmTimer.Stop();

                RecolorAction();

                if (_cancellationTokenColor.IsCancellationRequested)
                {
                    UiStatusCancelRun();
                }
                else
                {
                    UiStatusFinishRunSuccess();
                }

                ComputeCommandText = GetComputeCommandText();
                OnPropertyChanged(nameof(ComputeCommandText));

                OnAfterRunCompleted();
            })
            .ContinueWith(event_error => {

                _hasRunData = false;
                _computeState = ComputeState.NotRunning;
                _algorithmTimer!.Stop();
                UiStatusCancelRun();
                ComputeCommandText = GetComputeCommandText();
                OnPropertyChanged(nameof(ComputeCommandText));

                Workspace.Instance.ShowTaskException(event_error, "Computation error");
            }, TaskContinuationOptions.OnlyOnFaulted);
        }

        /// <summary>
        /// Updates associated UI text / status when a run is cancelled.
        /// </summary>
        private void UiStatusCancelRun()
        {
            if (object.ReferenceEquals(null, _algorithmTimer))
            {
                return;
            }

            UiUpdateProgress(new ProgressReport(_algorithmTimer.Elapsed.TotalSeconds, 0, 0, "Cancelled."));
        }

        /// <summary>
        /// Updates associated UI text / status when a run is started.
        /// </summary>
        private void UiStatusStartRun()
        {
            if (object.ReferenceEquals(null, _algorithmTimer))
            {
                return;
            }

            UiUpdateProgress(new ProgressReport(_algorithmTimer.Elapsed.TotalSeconds, 0, 0, "Started."));
        }

        /// <summary>
        /// Updates associated UI text / status after a run successfully completes.
        /// </summary>
        private void UiStatusFinishRunSuccess()
        {
            if (object.ReferenceEquals(null, _algorithmTimer))
            {
                return;
            }

            UiUpdateProgress(new ProgressReport(_algorithmTimer.Elapsed.TotalSeconds, 0, 0, "Done."));
        }

        /// <summary>
        /// Callback to update UI status while a run is evaluating.
        /// </summary>
        /// <param name="progress"></param>
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

        /// <summary>
        /// Method to update the main image container after a run completes.
        /// </summary>
        /// <param name="token"></param>
        /// <exception cref="NullReferenceException"></exception>
        private void RenderImageSource(CancellationToken token)
        {
            if (object.ReferenceEquals(null, _algorithm))
            {
                throw new NullReferenceException(nameof(_algorithm));
            }

            var bmp = _scene.ProcessPointsToPixels(_algorithm, token, _outputIntervalSec, UiUpdateProgress);

            if (object.ReferenceEquals(null, bmp) || token.IsCancellationRequested)
            {
                return;
            }

            _skbmp = bmp;

            // create an image WRAPPER
            SKImage image = SKImage.FromPixels(_skbmp.PeekPixels());
            // encode the image (defaults to PNG)
            SKData encoded = image.Encode();
            // get a stream over the encoded data
            Stream stream = encoded.AsStream();

            stream.Position = 0;
            BitmapImage result = new();
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

        /// <summary>
        /// Command handler for "recolor" button.
        /// Either triggers token cancellation, or starts a new recolor action.
        /// </summary>
        private void RecolorCommandHandler()
        {
            if (_computeState == ComputeState.NotRunning && _hasRunData)
            {
                Task.Factory.StartNew(() =>
                {
                    RecolorAction();
                })
                .ContinueWith(err1 =>
                {
                    _computeState = ComputeState.NotRunning;
                    _algorithmTimer!.Stop();
                    UiStatusCancelRun();
                    ComputeCommandText = GetComputeCommandText();
                    OnPropertyChanged(nameof(ComputeCommandText));

                    Workspace.Instance.ShowTaskException(err1, "Error rendering image");
                }, TaskContinuationOptions.OnlyOnFaulted);
            }
            else if (_computeState == ComputeState.RunningColor && _hasRunData)
            {
                _cancellationTokenColor.Cancel();
            }
        }

        /// <summary>
        /// Main method to (re)apply coloramp to run data.
        /// Should only be called from a background task, either <see cref="RecolorCommandHandler"/>
        /// or <see cref="ComputeCommandHandler"/>.
        /// </summary>
        /// <exception cref="NullReferenceException"></exception>
        private void RecolorAction()
        {
            if (object.ReferenceEquals(null, _algorithm))
            {
                throw new NullReferenceException(nameof(_algorithm));
            }

            _cancellationTokenColor = new CancellationTokenSource();
            _algorithmTimer = Stopwatch.StartNew();
            _computeState = ComputeState.RunningColor;
            _algorithm.UseHistogram = _uiRunData.UseHistogram;
            RenderImageSource(_cancellationTokenColor.Token);
            _computeState = ComputeState.NotRunning;
            _algorithmTimer.Stop();
            if (_cancellationTokenColor.IsCancellationRequested)
            {
                UiStatusCancelRun();
            }
            else
            {
                UiStatusFinishRunSuccess();
            }
        }

        /// <summary>
        /// Command handler to reset run settings to the default.
        /// </summary>
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
        /// Command handler to reset run settings to previous run data.
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

        /// <summary>
        /// Command handler to set the run settings according
        /// to the current view area.
        /// </summary>
        private void TargetFromViewCommandHandler()
        {
            TextOriginX = _targetRunData.OriginX.ToString();
            TextOriginY = _targetRunData.OriginY.ToString();
            TextFractalWidth = _targetRunData.FractalWidth.ToString();
            TextFractalHeight = _targetRunData.FractalHeight.ToString();
        }

        /// <summary>
        /// <see cref="AfterRunCompleted"/> handler invocation.
        /// </summary>
        private void OnAfterRunCompleted()
        {
            AfterRunCompleted?.Invoke(this, new EventArgs());
        }

        /// <summary>
        /// Command handler to save image data.
        /// </summary>
        /// <param name="writeMetadataFile">
        /// Whether or not to write an additional text file containing the runtime settings.
        /// This will use the same filename as the image, but with a text extension.
        /// </param>
        private void SaveAsCommandHandler(bool writeMetadataFile = false)
        {
            if (!_hasRunData || object.ReferenceEquals(null, _skbmp))
            {
                return;
            }

            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                FileName = SaveAsDefaultFilename + _runDataTime.ToString("yyyyMMdd-HHmmss"), // Default file name
                DefaultExt = SaveAsDefaultExtension, // Default file extension
                Filter = SaveAsFilters // Filter files by extension
            };

            // Show save file dialog box
            bool? result = dialog.ShowDialog();

            // Process save file dialog box results
            if (result == true && !string.IsNullOrEmpty(dialog.FileName))
            {
                // Save document
                string filename = dialog.FileName;
                var extension = System.IO.Path.GetExtension(filename);
                var format = FracView.Converters.SkiaConverters.ExtensionToFormat(extension);

                using (MemoryStream memStream = new())
                using (SKManagedWStream wstream = new(memStream))
                {
                    _skbmp.Encode(wstream, format, 100);
                    byte[] data = memStream.ToArray();
                    System.IO.File.WriteAllBytes(filename, data);
                }

                if (writeMetadataFile)
                {
                    var baseFilename = System.IO.Path.GetFileNameWithoutExtension(filename);
                    var metadataFilename = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(filename)!, baseFilename + ".txt");
                    var sb = new StringBuilder();
                    sb.AppendLine($"runtime: {_runDataTime.ToLongDateString()} {_runDataTime.ToLongTimeString()}");
                    sb.AppendLine($"runtime.iso: {_runDataTime.ToString("yyyy-MM-ddTHH:mm:sszzz", System.Globalization.CultureInfo.InvariantCulture)}");
                    sb.AppendLine($"Algorithm: {_previousRunData.AlgorithmName}");
                    sb.AppendLine($"Origin.X: {_previousRunData.OriginX}");
                    sb.AppendLine($"Origin.Y: {_previousRunData.OriginY}");
                    sb.AppendLine($"FractalWidth: {_previousRunData.FractalWidth}");
                    sb.AppendLine($"FractalHeight: {_previousRunData.FractalHeight}");
                    sb.AppendLine($"StepWidth: {_previousRunData.StepWidth}");
                    sb.AppendLine($"StepHeight: {_previousRunData.StepHeight}");
                    sb.AppendLine($"MaxIterations: {_previousRunData.MaxIterations}");
                    sb.AppendLine($"UseHistogram: {_previousRunData.UseHistogram}");

                    System.IO.File.WriteAllText(metadataFilename, sb.ToString());
                }
            }
        }

        /// <summary>
        /// Command handler to show/hide crosshair on the UI.
        /// </summary>
        private void ToggleCrosshairCommandHandler()
        {
            if (ShowCrosshair)
            {
                ShowCrosshair = false;
                ShowCrosshairCommandText = "Show Crosshair";
            }
            else
            {
                ShowCrosshair = true;
                ShowCrosshairCommandText = "Hide Crosshair";
            }

            OnPropertyChanged(nameof(ShowCrosshair));
            OnPropertyChanged(nameof(ShowCrosshairCommandText));
        }
        
        /// <summary>
        /// Command handler to reset zoom to default settings.
        /// </summary>
        private void ResetZoomCommandHandler()
        {
            NotifyUiResetZoomRequest?.Invoke(this, new EventArgs());
        }

        private enum ComputeState
        {
            NotRunning,
            RunningCompute,
            RunningColor,
            Cancelling,
        }
    }
}
