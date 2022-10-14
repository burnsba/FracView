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
    public class ColorWindowViewModel : WindowViewModelBase
    {
        private IScene _scene;

        private System.Windows.Media.Color _stableColor;

        public event EventHandler<SceneEventArgs>? SceneChanged;

        public bool AnyChanges { get; set; } = false;

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

        public ObservableCollection<ColorKeyframeViewModel> SceneKeyframes { get; set; }

        /// <summary>
        /// Gets or sets ok button command.
        /// </summary>
        public ICommand ApplyCloseCommand { get; set; }

        public ICommand ApplyCommand { get; set; }

        public ICommand CancelCommand { get; set; }

        public ICommand InsertBeforeCommand { get; set; }
        public ICommand InsertAfterCommand { get; set; }

        public ICommand RemoveKeyframeCommand { get; set; }
        public ICommand RemoveAllCommand { get; set; }
        public ICommand ResetKeyframesCommand { get; set; }

        public ColorKeyframeViewModel? SelectedKeyframe { get; set; }

        public ColorWindowViewModel(IScene scene)
        {
            _scene = scene;

            if (!object.ReferenceEquals(null, scene) && scene.ColorRamp.Keyframes.Any())
            {
                SceneKeyframes = new ObservableCollection<ColorKeyframeViewModel>(
                    scene.ColorRamp.Keyframes.Select((x, i) => new ColorKeyframeViewModel(x) { Index = i })
                );

                foreach (var kf in SceneKeyframes)
                {
                    kf.PostChanged += SetAnyChanges;
                }
            }
            else
            {
                SceneKeyframes = new ObservableCollection<ColorKeyframeViewModel>();
            }

            if (!object.ReferenceEquals(null, _scene))
            {
                SceneStableColor = System.Windows.Media.Color.FromRgb(_scene.StableColor.Red, _scene.StableColor.Green, _scene.StableColor.Blue);
            }

            CancelCommand = new RelayCommand<ICloseable>(w => { AnyChanges = false; CloseWindow(w); });
            ApplyCloseCommand = new RelayCommand<ICloseable>(w => { AnyChanges = true; OnSceneChanged(); CloseWindow(w); });

            ApplyCommand = new CommandHandler(OnApplyChanges);

            InsertBeforeCommand = new CommandHandler(InsertBeforeCommandHandler);
            InsertAfterCommand = new CommandHandler(InsertAfterCommandHandler);
            RemoveKeyframeCommand = new RelayCommand<ColorKeyframeViewModel>(RemoveKeyframeCommandHandler);

            RemoveAllCommand = new CommandHandler(RemoveAllCommandHandler);
            ResetKeyframesCommand = new CommandHandler(ResetKeyframesCommandHandler);

            // If any property setters above triggered updating the AnyChanges flag, make sure it's cleared.
            AnyChanges = false;
        }

        public void OnSceneChanged()
        {
            if (AnyChanges && !object.ReferenceEquals(null, _scene))
            {
                _scene.StableColor = new SKColor(SceneStableColor.R, SceneStableColor.G, SceneStableColor.B);
                _scene.ColorRamp.Keyframes = SceneKeyframes.Select(x => x.ToKeyframe()).ToList();

                SceneChanged?.Invoke(this, new SceneEventArgs(_scene));
            }
        }

        private void OnApplyChanges()
        {
            OnSceneChanged();

            AnyChanges = false;
        }

        private void SetAnyChanges(object? sender, ColorKeyframeChangeEventArgs e)
        {
            AnyChanges = true;

            // If the interval start/end value changed, apply this to other keyframes as a new max/min value (if needed).
            // This assumes the value is valid.
            if (!object.ReferenceEquals(null, e))
            {
                if (e.PropetyType == typeof(double))
                {
                    double oldValue = (double)e.OldValue;
                    double newValue = (double)e.NewValue;

                    int startIndex = -1;
                    if (!object.ReferenceEquals(null, sender) && sender.GetType() == typeof(ColorKeyframeViewModel))
                    {
                        var ckvm = (ColorKeyframeViewModel)sender;
                        startIndex = ckvm.Index;
                    }

                    // If the start value got smaller, push this down to earlier keyframes as a 
                    // new max value.
                    if (e.PropertyName == ColorKeyframeViewModel.RefIntervalStartProperty
                        && newValue < oldValue)
                    {
                        if (startIndex > -1)
                        {
                            // Start changing values on the keyframe before the current one.
                            startIndex--;
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
                    // Else if the end value got bigger, push this forward to later keyframes
                    // as a new min value.
                    else if (e.PropertyName == ColorKeyframeViewModel.RefIntervalEndProperty
                        && newValue > oldValue)
                    {
                        if (startIndex > -1)
                        {
                            // Start changing values on the keyframe after the current one.
                            startIndex++;
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
            kf.PostChanged += SetAnyChanges;

            for (int i = selectedIndex; i < SceneKeyframes.Count; i++)
            {
                SceneKeyframes[i].Index++;
            }

            SceneKeyframes.Insert(selectedIndex, kf);
        }

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
            kf.PostChanged += SetAnyChanges;

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
        }

        private void RemoveAllCommandHandler()
        {
            while (SceneKeyframes.Count > 0)
            {
                var kf = SceneKeyframes.First();
                kf.PostChanged -= SetAnyChanges;
                SceneKeyframes.Remove(kf);
            }
        }

        private void ResetKeyframesCommandHandler()
        {
            RemoveAllCommandHandler();

            var scene = new Scene();
            scene.AddDefaultSceneKeyframes();

            var keyframes = scene.ColorRamp.Keyframes.Select((x, i) => new ColorKeyframeViewModel(x) { Index = i }).ToList();

            foreach (var kf in keyframes)
            {
                kf.PostChanged += SetAnyChanges;
                SceneKeyframes.Add(kf);
            }
        }

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
            kf.PostChanged -= SetAnyChanges;
        }
    }
}
