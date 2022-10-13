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

        public int Index { get; set; }

        public string IntervalStartText
        {
            get => _intervalStart.ToString();

            set
            {
                double d;
                if (double.TryParse(value, out d))
                {
                    if (d >= 0 && d <= 1)
                    {
                        _intervalStart = d;
                        OnPropertyChanged(nameof(IntervalStartText));
                        OnIntervalStartChanged();
                        OnPostChanged();

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
                double d;
                if (double.TryParse(value, out d))
                {
                    if (d >= 0 && d <= 1)
                    {
                        _intervalEnd = d;
                        OnPropertyChanged(nameof(IntervalEndText));
                        OnIntervalEndChanged();
                        OnPostChanged();

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
                _valueStart = value;
                OnPropertyChanged(nameof(ValueStart));
                OnPostChanged();
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
                _valueEnd = value;
                OnPropertyChanged(nameof(ValueEnd));
                OnPostChanged();
            }
        }

        public event EventHandler<EventArgs>? IntervalStartChanged;
        public event EventHandler<EventArgs>? IntervalEndChanged;
        public event EventHandler<EventArgs>? PostChanged;

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
        protected void OnPostChanged()
        {
            PostChanged?.Invoke(this, new EventArgs());
        }

        public override string ToString()
        {
            var start = Converters.ColorConverters.WindowsMedia.ToHexSeven(ValueStart);
            var end = Converters.ColorConverters.WindowsMedia.ToHexSeven(ValueEnd);
            return $"{_intervalStart}->{_intervalEnd}; {start}->{end}";
        }
    }
}
