using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using FracView.Gfx;
using FracViewWpf.Mvvm;
using SkiaSharp;

namespace FracViewWpf.ViewModels
{
    public class ColorKeyframeViewModel : ViewModelBase
    {
        private System.Windows.Media.Color _valueStart;
        private System.Windows.Media.Color _valueEnd;

        private double _intervalStart;
        private double _intervalEnd;

        private bool _intervalStartIsValid = true;
        private bool _intervalEndIsValid = true;

        public const string RefIntervalStartProperty = "IntervalStart";
        public const string RefIntervalEndProperty = "IntervalEnd";
        public const double MaxValidIntervalValue = 1.0;
        public const double MinValidIntervalValue = 0;

        public int Index { get; set; }

        public string IntervalStartText
        {
            get => _intervalStart.ToString();

            set
            {
                double oldValue = _intervalStart;
                double d;
                if (double.TryParse(value, out d))
                {
                    if (d >= MinValidIntervalValue && d <= MaxValidIntervalValue)
                    {
                        _intervalStart = d;
                        OnPropertyChanged(nameof(IntervalStartText));
                        OnIntervalStartChanged();
                        OnPostChanged(new ColorKeyframeChangeEventArgs()
                        {
                            PropertyName = RefIntervalStartProperty,
                            PropetyType = typeof(double),
                            OldValue = oldValue,
                            NewValue = d,
                        });

                        IntervalStartIsValid = true;
                    }
                    else
                    {
                        IntervalStartIsValid = false;
                    }
                }
                else
                {
                    IntervalStartIsValid = false;
                }
            }
        }

        public bool IntervalStartIsValid
        {
            get => _intervalStartIsValid;

            set
            {
                _intervalStartIsValid = value;
                OnPropertyChanged(nameof(IntervalStartIsValid));
            }
        }

        public string IntervalEndText
        {
            get => _intervalEnd.ToString();

            set
            {
                double oldValue = _intervalEnd;
                double d;
                if (double.TryParse(value, out d))
                {
                    if (d >= MinValidIntervalValue && d <= MaxValidIntervalValue)
                    {
                        _intervalEnd = d;
                        OnPropertyChanged(nameof(IntervalEndText));
                        OnIntervalEndChanged();
                        OnPostChanged(new ColorKeyframeChangeEventArgs()
                        {
                            PropertyName = RefIntervalEndProperty,
                            PropetyType = typeof(double),
                            OldValue = oldValue,
                            NewValue = d,
                        });

                        IntervalEndIsValid = true;
                    }
                    else
                    {
                        IntervalEndIsValid = false;
                    }
                }
                else
                {
                    IntervalEndIsValid = false;
                }
            }
        }

        public bool IntervalEndIsValid
        {
            get => _intervalEndIsValid;

            set
            {
                _intervalEndIsValid = value;
                OnPropertyChanged(nameof(IntervalEndIsValid));
            }
        }

        public System.Windows.Media.Color ValueStart
        {
            get
            {
                return _valueStart;
            }

            set
            {
                Color oldValue = _valueStart;
                _valueStart = value;
                OnPropertyChanged(nameof(ValueStart));
                OnPostChanged(new ColorKeyframeChangeEventArgs()
                {
                    PropertyName = nameof(ValueStart),
                    PropetyType = typeof(Color),
                    OldValue = oldValue,
                    NewValue = value,
                });
            }
        }

        public System.Windows.Media.Color ValueEnd
        {
            get
            {
                return _valueEnd;
            }

            set
            {
                Color oldValue = _valueEnd;
                _valueEnd = value;
                OnPropertyChanged(nameof(ValueEnd));
                OnPostChanged(new ColorKeyframeChangeEventArgs()
                {
                    PropertyName = nameof(ValueEnd),
                    PropetyType = typeof(Color),
                    OldValue = oldValue,
                    NewValue = value,
                });
            }
        }

        public event EventHandler<EventArgs>? IntervalStartChanged;
        public event EventHandler<EventArgs>? IntervalEndChanged;
        public event EventHandler<ColorKeyframeChangeEventArgs>? PostChanged;

        public ColorKeyframeViewModel()
        {
        }

        public ColorKeyframeViewModel(Keyframe<SKColor, double> keyframe)
        {
            _intervalStart = keyframe.IntervalStart;
            _intervalEnd = keyframe.IntervalEnd;

            _valueStart = System.Windows.Media.Color.FromArgb(255, keyframe.ValueStart.Red, keyframe.ValueStart.Green, keyframe.ValueStart.Blue);
            _valueEnd = System.Windows.Media.Color.FromArgb(255, keyframe.ValueEnd.Red, keyframe.ValueEnd.Green, keyframe.ValueEnd.Blue);
        }

        public Keyframe<SKColor, double> ToKeyframe()
        {
            return new Keyframe<SKColor, double>()
            {
                IntervalStart = _intervalStart,
                IntervalEnd = _intervalEnd,
                ValueStart = new SKColor(ValueStart.R, ValueStart.G, ValueStart.B),
                ValueEnd = new SKColor(ValueEnd.R, ValueEnd.G, ValueEnd.B),
            };
        }

        public void QuietClampBelowMax(double d)
        {
            if (_intervalEnd > d && _intervalEndIsValid)
            {
                _intervalEnd = d;
                OnPropertyChanged(nameof(IntervalEndText));
            }

            if (_intervalStart > d && _intervalStartIsValid)
            {
                _intervalStart = d;
                OnPropertyChanged(nameof(IntervalStartText));
            }
        }

        public void QuietClampAboveMin(double d)
        {
            if (_intervalEnd < d && _intervalEndIsValid)
            {
                _intervalEnd = d;
                OnPropertyChanged(nameof(IntervalEndText));
            }

            if (_intervalStart < d && _intervalStartIsValid)
            {
                _intervalStart = d;
                OnPropertyChanged(nameof(IntervalStartText));
            }
        }

        protected void OnIntervalStartChanged()
        {
            IntervalStartChanged?.Invoke(this, new EventArgs());
        }

        protected void OnIntervalEndChanged()
        {
            IntervalEndChanged?.Invoke(this, new EventArgs());
        }

        /// <summary>
        /// Fired when any property changes value. Fires after specific property changed events.
        /// </summary>
        protected void OnPostChanged(ColorKeyframeChangeEventArgs args)
        {
            PostChanged?.Invoke(this, args);
        }

        public override string ToString()
        {
            var start = Converters.ColorConverters.WindowsMedia.ToHexSeven(ValueStart);
            var end = Converters.ColorConverters.WindowsMedia.ToHexSeven(ValueEnd);
            return $"{_intervalStart}->{_intervalEnd}; {start}->{end}";
        }
    }
}
