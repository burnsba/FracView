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
    }
}
