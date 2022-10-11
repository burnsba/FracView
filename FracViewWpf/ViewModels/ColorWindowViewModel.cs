using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FracView.Gfx;
using FracViewWpf.Mvvm;

namespace FracViewWpf.ViewModels
{
    public class ColorWindowViewModel : WindowViewModelBase
    {
        private ColorRamp _colorRamp;

        public event EventHandler<ColorRampEventArgs>? ColorRampChanged;

        public bool AnyChanges { get; set; } = false;

        public ColorWindowViewModel()
        {
            _colorRamp = new ColorRamp();
        }

        public void LoadColorRamp(ColorRamp cr)
        {
            _colorRamp = cr;
        }

        public void OnColorRampChanged()
        {
            if (AnyChanges)
            {
                ColorRampChanged?.Invoke(this, new ColorRampEventArgs(_colorRamp.Clone()));
            }
        }
    }
}
