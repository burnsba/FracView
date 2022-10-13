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

        public int Index { get; set; }

        public double IntervalStart
        {
            get => _intervalStart;

            set
            {
                _intervalStart = value;
                OnPropertyChanged(nameof(IntervalStart));
                OnIntervalStartChanged();
                OnPostChanged();
            }
        }

        public double IntervalEnd
        {
            get => _intervalEnd;

            set
            {
                _intervalEnd = value;
                OnPropertyChanged(nameof(IntervalEnd));
                OnIntervalEndChanged();
                OnPostChanged();
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
            IntervalStart = keyframe.IntervalStart;
            IntervalEnd = keyframe.IntervalEnd;

            _valueStart = System.Windows.Media.Color.FromArgb(255, keyframe.ValueStart.Red, keyframe.ValueStart.Green, keyframe.ValueStart.Blue);
            _valueEnd = System.Windows.Media.Color.FromArgb(255, keyframe.ValueEnd.Red, keyframe.ValueEnd.Green, keyframe.ValueEnd.Blue);
        }

        public Keyframe<SKColor, double> ToKeyframe()
        {
            return new Keyframe<SKColor, double>()
            {
                IntervalStart = IntervalStart,
                IntervalEnd = IntervalEnd,
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
            return $"{IntervalStart}->{IntervalEnd}; {start}->{end}";
        }
    }
}
