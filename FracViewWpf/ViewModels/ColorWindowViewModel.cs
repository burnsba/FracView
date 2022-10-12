using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FracView.Gfx;
using FracViewWpf.Mvvm;
using SkiaSharp;

namespace FracViewWpf.ViewModels
{
    public class ColorWindowViewModel : WindowViewModelBase
    {
        private ColorRamp _colorRamp;

        public event EventHandler<ColorRampEventArgs>? ColorRampChanged;

        public bool AnyChanges { get; set; } = false;

        public ObservableCollection<ColorKeyframeViewModel> Keyframes { get; set; }

        public ColorWindowViewModel()
        {
            _colorRamp = new ColorRamp();

            Keyframes = new ObservableCollection<ColorKeyframeViewModel>();
        }

        public void LoadColorRamp(ColorRamp cr)
        {
            _colorRamp = cr;

            Keyframes = new ObservableCollection<ColorKeyframeViewModel>(
                _colorRamp.Keyframes.Select(x => new ColorKeyframeViewModel(x))
                );
        }

        public void OnColorRampChanged()
        {
            if (AnyChanges && !object.ReferenceEquals(null, _colorRamp))
            {
                _colorRamp.Keyframes = Keyframes.Select(x => x.ToKeyframe()).ToList();
                ColorRampChanged?.Invoke(this, new ColorRampEventArgs(_colorRamp.Clone()));
            }
        }
    }
}
