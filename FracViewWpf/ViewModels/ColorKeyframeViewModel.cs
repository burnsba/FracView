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
    /// <summary>
    /// ViewModel for coloramp keyframe.
    /// </summary>
    public class ColorKeyframeViewModel : ViewModelBase
    {
        private System.Windows.Media.Color _valueStart;
        private System.Windows.Media.Color _valueEnd;

        private double _intervalStart;
        private double _intervalEnd;

        private bool _intervalStartIsValid = true;
        private bool _intervalEndIsValid = true;

        /// <summary>
        /// Max valid value for keyframe interval.
        /// </summary>
        public const double MaxValidIntervalValue = 1.0;

        /// <summary>
        /// Min valid value for keyframe interval.
        /// </summary>
        public const double MinValidIntervalValue = 0;

        /// <summary>
        /// Gets or sets the index of the keyframe.
        /// Keyframes are contained within a collection for the coloramp.
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// Gets or sets the text associated with the interval start value.
        /// Setting this valud updates the <see cref="IntervalStartIsValid"/> property.
        /// </summary>
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
                        OnPostChanged(new ColorKeyframeChangeEventArgs(/*bogus type parameters*/nameof(Keyframe<int, int>.IntervalStart), typeof(double), oldValue, d));

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

        /// <summary>
        /// Gets or sets value indicating whether the current <see cref="IntervalStartText"/>
        /// contains a valid value. This property should not be set directly.
        /// </summary>
        public bool IntervalStartIsValid
        {
            get => _intervalStartIsValid;

            set
            {
                _intervalStartIsValid = value;
                OnPropertyChanged(nameof(IntervalStartIsValid));
            }
        }

        /// <summary>
        /// Gets or sets the text associated with the interval end value.
        /// Setting this valud updates the <see cref="IntervalEndIsValid"/> property.
        /// </summary>
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
                        OnPostChanged(new ColorKeyframeChangeEventArgs(/*bogus type parameters*/nameof(Keyframe<int, int>.IntervalEnd), typeof(double), oldValue, d));

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

        /// <summary>
        /// Gets or sets value indicating whether the current <see cref="IntervalStartText"/>
        /// contains a valid value. This property should not be set directly.
        /// </summary>
        public bool IntervalEndIsValid
        {
            get => _intervalEndIsValid;

            set
            {
                _intervalEndIsValid = value;
                OnPropertyChanged(nameof(IntervalEndIsValid));
            }
        }

        /// <summary>
        /// Gets or sets starting color value of the keyframe.
        /// </summary>
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
                OnPostChanged(new ColorKeyframeChangeEventArgs(/*bogus type parameters*/nameof(Keyframe<int, int>.ValueStart), typeof(Color), oldValue, value));
            }
        }

        /// <summary>
        /// Gets or sets the ending color value of the keyframe.
        /// </summary>
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
                OnPostChanged(new ColorKeyframeChangeEventArgs(/*bogus type parameters*/nameof(Keyframe<int, int>.ValueEnd), typeof(Color), oldValue, value));
            }
        }

        /// <summary>
        /// Change notification event, fired after one of the following properties has been changed:
        /// <see cref="IntervalStartText"/>,
        /// <see cref="IntervalEndText"/>,
        /// <see cref="ValueStart"/>,
        /// <see cref="ValueEnd"/>.
        /// </summary>
        public event EventHandler<ColorKeyframeChangeEventArgs>? PostChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="ColorKeyframeViewModel"/> class.
        /// </summary>
        public ColorKeyframeViewModel()
        {
            ValueStart = Colors.Black;
            ValueEnd = Colors.Black;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ColorKeyframeViewModel"/> class.
        /// </summary>
        /// <param name="keyframe">Keyframe to initialize values from.</param>
        public ColorKeyframeViewModel(Keyframe<SKColor, double> keyframe)
        {
            _intervalStart = keyframe.IntervalStart;
            _intervalEnd = keyframe.IntervalEnd;

            _valueStart = System.Windows.Media.Color.FromArgb(255, keyframe.ValueStart.Red, keyframe.ValueStart.Green, keyframe.ValueStart.Blue);
            _valueEnd = System.Windows.Media.Color.FromArgb(255, keyframe.ValueEnd.Red, keyframe.ValueEnd.Green, keyframe.ValueEnd.Blue);
        }

        /// <summary>
        /// Converts the viewmodel to a keyframe object.
        /// </summary>
        /// <returns>Keyframe.</returns>
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

        /// <summary>
        /// If the underlying value of <see cref="IntervalStartText"/> is valid, and the underlying value
        /// is greater than <paramref name="d"/>, will update the underlying value and notify property
        /// changed, but without firing the <see cref="PostChanged"/> event.
        /// 
        /// If the underlying value of <see cref="IntervalEndText"/> is valid, and the underlying value
        /// is greater than <paramref name="d"/>, will update the underlying value and notify property
        /// changed, but without firing the <see cref="PostChanged"/> event.
        /// </summary>
        /// <param name="d">Value to compare against.</param>
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

        /// <summary>
        /// If the underlying value of <see cref="IntervalStartText"/> is valid, and the underlying value
        /// is less than <paramref name="d"/>, will update the underlying value and notify property
        /// changed, but without firing the <see cref="PostChanged"/> event.
        /// 
        /// If the underlying value of <see cref="IntervalEndText"/> is valid, and the underlying value
        /// is less than <paramref name="d"/>, will update the underlying value and notify property
        /// changed, but without firing the <see cref="PostChanged"/> event.
        /// </summary>
        /// <param name="d">Value to compare against.</param>
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

        /// <summary>
        /// Gets the underlying value of <see cref="IntervalStartText"/>.
        /// </summary>
        /// <returns>Value.</returns>
        public double GetIntervalStartValue()
        {
            return _intervalStart;
        }

        /// <summary>
        /// Gets the underlying value of <see cref="IntervalEndText"/>.
        /// </summary>
        /// <returns>Value.</returns>
        public double GetIntervalEndValue()
        {
            return _intervalEnd;
        }

        /// <summary>
        /// Attempts to quietly set the underlying value behind <see cref="IntervalStartText"/>.
        /// If the value is between <see cref="MinValidIntervalValue" /> and <see cref="MaxValidIntervalValue" /> inclusive,
        /// the value is changed, <see cref="IntervalStartIsValid"/> is set to true,
        /// and OnPropertyChanged event is fired. No <see cref="PostChanged"/> is fired.
        /// Otherwise <see cref="IntervalStartIsValid"/> is set to false.
        /// </summary>
        /// <param name="d">Value to set.</param>
        public void SetIntervalStartValue(double d)
        {
            if (d >= MinValidIntervalValue && d <= MaxValidIntervalValue)
            {
                _intervalStart = d;
                OnPropertyChanged(nameof(IntervalStartText));

                IntervalStartIsValid = true;
            }
            else
            {
                IntervalStartIsValid = false;
            }
        }

        /// <summary>
        /// Attempts to quietly set the underlying value behind <see cref="IntervalEndText"/>.
        /// If the value is between <see cref="MinValidIntervalValue" /> and <see cref="MaxValidIntervalValue" /> inclusive,
        /// the value is changed, <see cref="IntervalEndIsValid"/> is set to true,
        /// and OnPropertyChanged event is fired. No <see cref="PostChanged"/> is fired.
        /// Otherwise <see cref="IntervalEndIsValid"/> is set to false.
        /// </summary>
        /// <param name="d">Value to set.</param>
        public void SetIntervalEndValue(double d)
        {
            if (d >= MinValidIntervalValue && d <= MaxValidIntervalValue)
            {
                _intervalEnd = d;
                OnPropertyChanged(nameof(IntervalEndText));

                IntervalEndIsValid = true;
            }
            else
            {
                IntervalEndIsValid = false;
            }
        }

        /// <summary>
        /// Triggers event handler for <see cref="PostChanged"/>.
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
