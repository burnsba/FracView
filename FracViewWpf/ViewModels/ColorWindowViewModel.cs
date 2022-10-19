using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Animation;
using FracView.Gfx;
using FracViewWpf.Mvvm;
using SkiaSharp;

namespace FracViewWpf.ViewModels
{
    /// <summary>
    /// Main class to handle functionality of the color window.
    /// </summary>
    public class ColorWindowViewModel : WindowViewModelBase
    {
        private readonly IScene _scene;

        private System.Windows.Media.Color _stableColor;

        /// <summary>
        /// Initializes a new instance of the <see cref="ColorWindowViewModel"/> class.
        /// </summary>
        /// <param name="scene">Required associated scene.</param>
        /// <exception cref="NullReferenceException">If scene is null.</exception>
        public ColorWindowViewModel(IScene scene)
        {
            if (object.ReferenceEquals(null, scene))
            {
                throw new NullReferenceException(nameof(scene));
            }

            _scene = scene;

            if (scene.ColorRamp.Keyframes.Any())
            {
                SceneKeyframes = new ObservableCollection<ColorKeyframeViewModel>(
                    scene.ColorRamp.Keyframes.Select((x, i) => new ColorKeyframeViewModel(x) { Index = i })
                );

                foreach (var kf in SceneKeyframes)
                {
                    kf.PostChanged += ApplyKeyframeChanges;
                }
            }
            else
            {
                SceneKeyframes = new ObservableCollection<ColorKeyframeViewModel>();
            }

            SceneStableColor = System.Windows.Media.Color.FromRgb(_scene.StableColor.Red, _scene.StableColor.Green, _scene.StableColor.Blue);

            CancelCommand = new RelayCommand<ICloseable>(w => { AnyChanges = false; CloseWindow(w); });
            ApplyCloseCommand = new RelayCommand<ICloseable>(w => { AnyChanges = true; OnSceneChanged(); CloseWindow(w); });

            ApplyCommand = new CommandHandler(OnSceneChanged);

            InsertBeforeCommand = new CommandHandler(InsertBeforeCommandHandler);
            InsertAfterCommand = new CommandHandler(InsertAfterCommandHandler);
            RemoveKeyframeCommand = new RelayCommand<ColorKeyframeViewModel>(RemoveKeyframeCommandHandler);

            RemoveAllCommand = new CommandHandler(RemoveAllCommandHandler);
            ResetKeyframesCommand = new CommandHandler(ResetKeyframesCommandHandler);

            // If any property setters above triggered updating the AnyChanges flag, make sure it's cleared.
            AnyChanges = false;
        }

        /// <summary>
        /// Event to notify that the underlying scene has changed.
        /// Should only be fired on a "save" event. "Cancel" should
        /// leave the scene unchanged.
        /// </summary>
        public event EventHandler<SceneEventArgs>? SceneChanged;

        /// <summary>
        /// Gets or sets a value to indicate whether there are any unsaved
        /// changes to apply on the scene.
        /// </summary>
        public bool AnyChanges { get; set; } = false;

        /// <summary>
        /// Gets or sets the color used for stable points.
        /// </summary>
        public System.Windows.Media.Color SceneStableColor
        {
            get
            {
                return _stableColor;
            }

            set
            {
                _stableColor = value;
                AnyChanges = true;
            }
        }

        /// <summary>
        /// Gets or sets the keyframes used by the colorramp.
        /// </summary>
        public ObservableCollection<ColorKeyframeViewModel> SceneKeyframes { get; set; }

        /// <summary>
        /// Gets or sets ok button / "save and close" command.
        /// </summary>
        public ICommand ApplyCloseCommand { get; set; }

        /// <summary>
        /// Gets or sets the "save" command (save by stay open).
        /// </summary>
        public ICommand ApplyCommand { get; set; }

        /// <summary>
        /// Gets or sets the "cancel and close" command.
        /// Any unsaved changes are discarded, but previously saved changes remain applied.
        /// </summary>
        public ICommand CancelCommand { get; set; }

        /// <summary>
        /// Gets or sets the command used to insert a new keyframe before
        /// the selected keyframe.
        /// </summary>
        public ICommand InsertBeforeCommand { get; set; }

        /// <summary>
        /// Gets or sets the command used to insert a new keyframe after
        /// the selected keyframe.
        /// </summary>
        public ICommand InsertAfterCommand { get; set; }

        /// <summary>
        /// Gets or sets the command used to remove the selected keyframe.
        /// </summary>
        public ICommand RemoveKeyframeCommand { get; set; }

        /// <summary>
        /// Gets or sets the command to remove all keyframes.
        /// </summary>
        public ICommand RemoveAllCommand { get; set; }

        /// <summary>
        /// Gets or sets the command to reset keyframes to default.
        /// This will remove all existing keyframes then add the default keyframes.
        /// </summary>
        public ICommand ResetKeyframesCommand { get; set; }

        /// <summary>
        /// Gets or sets the currently selected keyframe.
        /// </summary>
        public ColorKeyframeViewModel? SelectedKeyframe { get; set; }

        /// <summary>
        /// Command handler to save/apply changes to the underlying scene.
        /// Notifies listeners that the underlying scene has changed.
        /// </summary>
        public void OnSceneChanged()
        {
            FixColorRanges();

            if (AnyChanges && !object.ReferenceEquals(null, _scene))
            {
                _scene.StableColor = new SKColor(SceneStableColor.R, SceneStableColor.G, SceneStableColor.B);
                _scene.ColorRamp.Keyframes = SceneKeyframes.Select(x => x.ToKeyframe()).ToList();

                SceneChanged?.Invoke(this, new SceneEventArgs(_scene));

                AnyChanges = false;
            }
        }

        /// <summary>
        /// Event handler for when a keyframe changes.
        /// If the <see cref="Keyframe{TKey, TInterval}.IntervalStart"/> or <see cref="Keyframe{TKey, TInterval}.IntervalEnd"/>
        /// values change, then these are applied to earlier/later keyframes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ApplyKeyframeChanges(object? sender, ColorKeyframeChangeEventArgs e)
        {
            AnyChanges = true;

            // If the interval start/end value changed, apply this to other keyframes as a new max/min value (if needed).
            // This assumes the value is valid.
            if (!object.ReferenceEquals(null, e))
            {
                if (e.PropetyType == typeof(double))
                {
                    double newValue = (double)e.NewValue;

                    int startIndex = -1;
                    if (!object.ReferenceEquals(null, sender) && sender.GetType() == typeof(ColorKeyframeViewModel))
                    {
                        var ckvm = (ColorKeyframeViewModel)sender;
                        startIndex = ckvm.Index;
                    }

                    // If the start value changed, push to all earlier keyframes.
                    // This wille be marshalled through QuietClampBelowMax, so may not actually
                    // change anything.
                    if (e.PropertyName == /*bogus type parameters*/nameof(Keyframe<int, int>.IntervalStart))
                    {
                        if (startIndex > -1)
                        {
                            // Start changing values on the keyframe before the current one.
                            startIndex--;
                            if (startIndex >= 0)
                            {
                                // update linked value no matter what.
                                SceneKeyframes[startIndex].SetIntervalEndValue(newValue);
                            }

                            for (int i=startIndex; i>= 0; i--)
                            {
                                SceneKeyframes[i].QuietClampBelowMax(newValue);
                            }
                        }
                        else
                        {
                            // Can't determine index, so update everything.
                            foreach (var kf in SceneKeyframes)
                            {
                                kf.QuietClampBelowMax(newValue);
                            }
                        }
                    }
                    // If the end value changed, push to all future keyframes.
                    // This wille be marshalled through QuietClampBelowMax, so may not actually
                    // change anything.
                    else if (e.PropertyName == /*bogus type parameters*/nameof(Keyframe<int, int>.IntervalEnd))
                    {
                        if (startIndex > -1)
                        {
                            // Start changing values on the keyframe after the current one.
                            startIndex++;
                            if (startIndex < SceneKeyframes.Count)
                            {
                                // update linked value no matter what.
                                SceneKeyframes[startIndex].SetIntervalStartValue(newValue);
                            }

                            for (int i = startIndex; i < SceneKeyframes.Count; i++)
                            {
                                SceneKeyframes[i].QuietClampAboveMin(newValue);
                            }
                        }
                        else
                        {
                            // Can't determine index, so update everything.
                            foreach (var kf in SceneKeyframes)
                            {
                                kf.QuietClampAboveMin(newValue);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Instantiates a new keyframe and inserts it before the selected keyframe.
        /// If no keyframe is selected, it is inserted at the start of the list.
        /// </summary>
        private void InsertBeforeCommandHandler()
        {
            var kf = new ColorKeyframeViewModel();

            // default to start of list.
            int selectedIndex = 0;

            if (!object.ReferenceEquals(null, SelectedKeyframe))
            {
                selectedIndex = SelectedKeyframe.Index;
                
                // Set properties to same as selected keyframe valuestart.
                kf.ValueStart = SelectedKeyframe.ValueStart;
                kf.ValueEnd = SelectedKeyframe.ValueStart;
                kf.IntervalStartText = SelectedKeyframe.IntervalStartText;
                kf.IntervalEndText = SelectedKeyframe.IntervalStartText;
            }

            kf.Index = selectedIndex;
            kf.PostChanged += ApplyKeyframeChanges;

            for (int i = selectedIndex; i < SceneKeyframes.Count; i++)
            {
                SceneKeyframes[i].Index++;
            }

            SceneKeyframes.Insert(selectedIndex, kf);
            AnyChanges = true;
        }

        /// <summary>
        /// Instantiates a new keyframe and inserts it after the selected keyframe.
        /// If no keyframe is selected, it is inserted at the end of the list.
        /// </summary>
        private void InsertAfterCommandHandler()
        {
            var kf = new ColorKeyframeViewModel();

            // default to end of list.
            int selectedIndex = SceneKeyframes.Count - 1;

            if (!object.ReferenceEquals(null, SelectedKeyframe))
            {
                selectedIndex = SelectedKeyframe.Index;

                // Set properties to same as selected keyframe valueend.
                kf.ValueStart = SelectedKeyframe.ValueEnd;
                kf.ValueEnd = SelectedKeyframe.ValueEnd;
                kf.IntervalStartText = SelectedKeyframe.IntervalEndText;
                kf.IntervalEndText = SelectedKeyframe.IntervalEndText;
            }

            selectedIndex++;

            kf.Index = selectedIndex;
            kf.PostChanged += ApplyKeyframeChanges;

            for (int i = selectedIndex; i < SceneKeyframes.Count; i++)
            {
                SceneKeyframes[i].Index++;
            }

            if (selectedIndex >= SceneKeyframes.Count)
            {
                SceneKeyframes.Add(kf);
            }
            else
            {
                SceneKeyframes.Insert(selectedIndex, kf);
            }

            AnyChanges = true;
        }

        /// <summary>
        /// Removes all keyframes.
        /// </summary>
        private void RemoveAllCommandHandler()
        {
            while (SceneKeyframes.Count > 0)
            {
                var kf = SceneKeyframes.First();
                kf.PostChanged -= ApplyKeyframeChanges;
                SceneKeyframes.Remove(kf);
                AnyChanges = true;
            }
        }

        /// <summary>
        /// Removes all keyframes then adds the default keyframes.
        /// </summary>
        private void ResetKeyframesCommandHandler()
        {
            RemoveAllCommandHandler();

            var scene = new Scene();
            scene.AddDefaultSceneKeyframes();

            var keyframes = scene.ColorRamp.Keyframes.Select((x, i) => new ColorKeyframeViewModel(x) { Index = i }).ToList();

            foreach (var kf in keyframes)
            {
                kf.PostChanged += ApplyKeyframeChanges;
                SceneKeyframes.Add(kf);
            }

            AnyChanges = true;
        }

        /// <summary>
        /// Removes the given keyframe from the list, based on <see cref="ColorKeyframeViewModel.Index"/>.
        /// </summary>
        /// <param name="kf"></param>
        private void RemoveKeyframeCommandHandler(ColorKeyframeViewModel kf)
        {
            if (object.ReferenceEquals(null, kf))
            {
                return;
            }

            int selectedIndex = kf.Index;

            if (!object.ReferenceEquals(null, SelectedKeyframe))
            {
                if (selectedIndex == SelectedKeyframe.Index)
                {
                    SelectedKeyframe = null;
                }
            }

            for (int i = selectedIndex; i < SceneKeyframes.Count; i++)
            {
                if (SceneKeyframes[i].Index > 0)
                {
                    SceneKeyframes[i].Index--;
                }
            }

            SceneKeyframes.Remove(kf);
            kf.PostChanged -= ApplyKeyframeChanges;
            AnyChanges = true;
        }

        /// <summary>
        /// Helper method to avoid future runtime exceptions.
        /// This ensures at least one keyframe exists.
        /// This ensures interval start and end values cover the entire range from 0-1.
        /// This ensures each end value is the same as the next keyframe start value.
        /// This ensures the start to end values are well ordered.
        /// <see cref="AnyChanges"/> will be set if any changes are made.
        /// </summary>
        private void FixColorRanges()
        {
            if (!SceneKeyframes.Any())
            {
                InsertBeforeCommandHandler();
            }

            int maxIndex;

            if (SceneKeyframes[0].GetIntervalStartValue() > 0)
            {
                SceneKeyframes[0].SetIntervalStartValue(0);
                AnyChanges = true;
            }

            maxIndex = SceneKeyframes.Count - 1;

            if (SceneKeyframes[maxIndex].GetIntervalEndValue() < 1)
            {
                SceneKeyframes[maxIndex].SetIntervalEndValue(1);
                AnyChanges = true;
            }

            double prevUpper = 0;
            for (int i=0; i< SceneKeyframes.Count; i++)
            {
                double lower = SceneKeyframes[i].GetIntervalStartValue();
                double upper = SceneKeyframes[i].GetIntervalEndValue();

                if (lower != prevUpper)
                {
                    SceneKeyframes[i].SetIntervalStartValue(prevUpper);
                    lower = prevUpper;
                    AnyChanges = true;
                }

                if (upper < lower)
                {
                    SceneKeyframes[i].SetIntervalEndValue(lower);
                    AnyChanges = true;
                }

                prevUpper = SceneKeyframes[i].GetIntervalEndValue();
            }
        }
    }
}
