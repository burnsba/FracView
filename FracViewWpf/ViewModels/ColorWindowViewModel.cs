using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
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

        /// <summary>
        /// Gets or sets ok button command.
        /// </summary>
        public ICommand CloseCommand { get; set; }

        public ICommand ApplyCommand { get; set; }

        public ICommand CancelCommand { get; set; }

        public ColorWindowViewModel()
        {
            _colorRamp = new ColorRamp();

            Keyframes = new ObservableCollection<ColorKeyframeViewModel>();

            CancelCommand = new RelayCommand<ICloseable>(w => { AnyChanges = false; CloseWindow(w); });
            CloseCommand = new RelayCommand<ICloseable>(w => { AnyChanges = true; CloseWindow(w); });

            ApplyCommand = new CommandHandler(OnColorRampChanged);
        }

        public void LoadColorRamp(ColorRamp cr)
        {
            _colorRamp = cr;

            Keyframes = new ObservableCollection<ColorKeyframeViewModel>(
                _colorRamp.Keyframes.Select((x, i) => new ColorKeyframeViewModel(x) { Index = i })
                );

            foreach (var kf in Keyframes)
            {
                kf.PostChanged += SetAnyChanges;
            }
        }

        public void OnColorRampChanged()
        {
            if (AnyChanges && !object.ReferenceEquals(null, _colorRamp))
            {
                _colorRamp.Keyframes = Keyframes.Select(x => x.ToKeyframe()).ToList();
                ColorRampChanged?.Invoke(this, new ColorRampEventArgs(_colorRamp.Clone()));
            }
        }

        private void SetAnyChanges(object? sender, EventArgs e)
        {
            AnyChanges = true;
        }
    }
}
