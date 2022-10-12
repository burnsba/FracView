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

        public int Index { get; set; }
        public double IntervalStart { get; set; }
        public double IntervalEnd { get; set; }

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
                OnIntervalStartChanged();
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
                OnIntervalEndChanged();
            }
        }

        public event EventHandler<EventArgs>? IntervalStartChanged;
        public event EventHandler<EventArgs>? IntervalEndChanged;

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
    }
}
