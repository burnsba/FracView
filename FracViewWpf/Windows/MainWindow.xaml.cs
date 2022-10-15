using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.Xml;
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
using System.Xml.Linq;
using FracViewWpf.Mvvm;
using FracViewWpf.ViewModels;

namespace FracViewWpf.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, ICloseable
    {
        private MainWindowViewModel _vm = null;

        private int _imageZoomLittleScrollIndex = 0;
        private int _imageZoomBigScrollIndex = 0;

        private Point _panMousePoint;
        private double _verticalPanOffset = 1;
        private double _horizontalPanOffset = 1;

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

        private void Image_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            e.Handled = true;
            var mousePosition = e.GetPosition(MainDisplayImage);

            AdjustImagePixelZoom(e.Delta, mousePosition);
        }

        private void AdjustImagePixelZoom(double delta, Point position)
        {
            var change = false;

            // scroll up
            if (delta > 0)
            {
                _imageZoomLittleScrollIndex++;
                if (_imageZoomLittleScrollIndex >= Views.Constants.ScrollValues.Count)
                {
                    _imageZoomLittleScrollIndex = 0;
                    _imageZoomBigScrollIndex++;
                }

                change = true;
            }
            // scroll down
            else if (delta < 0)
            {
                _imageZoomLittleScrollIndex--;
                if (_imageZoomLittleScrollIndex < 0)
                {
                    _imageZoomLittleScrollIndex = Views.Constants.ScrollValues.Count - 1;

                    _imageZoomBigScrollIndex--;
                    if (_imageZoomBigScrollIndex < 0)
                    {
                        _imageZoomLittleScrollIndex = 0;
                        _imageZoomBigScrollIndex = 0;
                    }
                    else
                    {
                        change = true;
                    }
                }
                else
                {
                    change = true;
                }
            }
            else if (delta == 0 || double.IsNaN(delta))
            {
                _imageZoomLittleScrollIndex = 0;
                _imageZoomBigScrollIndex = 0;
                change = true;
            }

            if (change)
            {
                double bigScaleFactor = 1;
                for (int i = 0; i < _imageZoomBigScrollIndex; i++)
                {
                    bigScaleFactor *= 10.0;
                }

                var scalex = bigScaleFactor * Views.Constants.ScrollValues[_imageZoomLittleScrollIndex];
                var scaley = bigScaleFactor * Views.Constants.ScrollValues[_imageZoomLittleScrollIndex];

                var scrollStartOffsetX = MainDisplayImageScrollViewer.ContentHorizontalOffset;
                var scrollStartOffsetY = MainDisplayImageScrollViewer.ContentVerticalOffset;

                var transform = new ScaleTransform();

                transform.ScaleX = scalex;
                transform.ScaleY = scaley;

                MainDisplayImage.LayoutTransform = transform;
                /*
                var deltaScrollX = Math.Abs(scrollStartOffsetX - scrollStartOffsetX * scalex);
                var deltaScrollY = Math.Abs(scrollStartOffsetY - scrollStartOffsetY * scaley);

                if (position.X > (MainDisplayImageScrollViewer.ActualWidth / 2))
                {
                    MainDisplayImageScrollViewer.ScrollToHorizontalOffset(scrollStartOffsetX + deltaScrollX);
                }
                else
                {
                    MainDisplayImageScrollViewer.ScrollToHorizontalOffset(scrollStartOffsetX - deltaScrollX);
                }

                if (position.Y > (MainDisplayImageScrollViewer.ActualHeight / 2))
                {
                    MainDisplayImageScrollViewer.ScrollToVerticalOffset(scrollStartOffsetY + deltaScrollY);
                }
                else
                {
                    MainDisplayImageScrollViewer.ScrollToVerticalOffset(scrollStartOffsetY - deltaScrollY);
                }
                */

                if (delta > 0)
                {
                    MainDisplayImageScrollViewer.ScrollToHorizontalOffset(scrollStartOffsetX * 1.1);
                    MainDisplayImageScrollViewer.ScrollToVerticalOffset(scrollStartOffsetY * 1.1);
                }
                else
                {
                    MainDisplayImageScrollViewer.ScrollToHorizontalOffset(scrollStartOffsetX * 0.9);
                    MainDisplayImageScrollViewer.ScrollToVerticalOffset(scrollStartOffsetY * 0.9);
                }
            }

            if (_imageZoomLittleScrollIndex == 0 && _imageZoomBigScrollIndex == 0)
            {
                MainDisplayImageScrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden;
                MainDisplayImageScrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
            }
            else
            {
                MainDisplayImageScrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Visible;
                MainDisplayImageScrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;
            }
        }

        private void MainDisplayImageScrollViewer_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            e.Handled = true;
            return;
        }

        private void MainDisplayImageScrollViewer_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MainDisplayImage.CaptureMouse();
            _panMousePoint = e.GetPosition(MainDisplayImageScrollViewer);
            _verticalPanOffset = MainDisplayImageScrollViewer.VerticalOffset;
            _horizontalPanOffset = MainDisplayImageScrollViewer.HorizontalOffset;
        }

        private void MainDisplayImageScrollViewer_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (MainDisplayImage.IsMouseCaptured)
            {
                double newOffset;

                newOffset = _verticalPanOffset + (_panMousePoint.Y - e.GetPosition(MainDisplayImageScrollViewer).Y);
                MainDisplayImageScrollViewer.ScrollToVerticalOffset(newOffset);

                newOffset = _horizontalPanOffset + (_panMousePoint.X - e.GetPosition(MainDisplayImageScrollViewer).X);
                MainDisplayImageScrollViewer.ScrollToHorizontalOffset(newOffset);
            }
        }

        private void MainDisplayImageScrollViewer_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            MainDisplayImage.ReleaseMouseCapture();
        }
    }
}
