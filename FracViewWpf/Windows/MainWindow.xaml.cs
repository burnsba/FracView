using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using FracViewWpf.ViewModels;

namespace FracViewWpf.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainWindowViewModel _vm = null;

        public MainWindow(MainWindowViewModel vm)
        {
            InitializeComponent();

            _vm = vm;

            _vm.GetParentDisplayGridImageWidth = () => this.ImageGridContainer.ActualWidth;
            _vm.GetParentDisplayGridImageHeight = () => this.ImageGridContainer.ActualHeight;

            DataContext = _vm;
        }

        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            _vm.RecomputeImageScreenDimensions();
        }
    }
}
