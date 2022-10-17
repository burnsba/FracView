using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Xml.Linq;
using FracViewWpf.Dto;
using FracViewWpf.Mvvm;
using FracViewWpf.ViewModels;

namespace FracViewWpf.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, ICloseable
    {
        private Dispatcher _dispatcher;
        private MainWindowViewModel _vm;

        private int _imageZoomLittleScrollIndex = 0;
        private int _imageZoomBigScrollIndex = 0;

        private Point _panMousePoint;
        private double _verticalPanOffset = 1;
        private double _horizontalPanOffset = 1;

        public MainWindow(MainWindowViewModel vm)
        {
            _dispatcher = Dispatcher.CurrentDispatcher;

            InitializeComponent();

            _vm = vm;

            _vm.AfterRunCompleted += ClearZoom;

            DataContext = _vm;
        }

        private void AdjustImageSize()
        {
            MainDisplayImage.Height = (double)_vm.StepHeight;
            MainDisplayImage.Width = (double)_vm.StepWidth;
        }

        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            AdjustImageSize();

            UpdateImagePositionStats();
        }

        private void Image_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            e.Handled = true;
            var mousePosition = e.GetPosition(MainDisplayImage);

            AdjustImagePixelZoom(e.Delta, mousePosition);

            UpdateImagePositionStats();
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
                double startPixelWidth = MainDisplayImageScrollViewer.DesiredSize.Width
                    - System.Windows.SystemParameters.VerticalScrollBarWidth;
                double startPixelHeight = MainDisplayImageScrollViewer.DesiredSize.Height
                    - System.Windows.SystemParameters.HorizontalScrollBarHeight;
                double startPixelTop = MainDisplayImageScrollViewer.ContentVerticalOffset;
                double startPixelLeft = MainDisplayImageScrollViewer.ContentHorizontalOffset;
                double startScale = _vm.UiScale;
                var startTransform = new ScaleTransform(startScale, startScale);

                if (startScale > 1)
                {
                    startPixelWidth /= startScale;
                    startPixelHeight /= startScale;

                    startPixelLeft = MainDisplayImageScrollViewer.ContentHorizontalOffset / startScale;
                    startPixelTop = MainDisplayImageScrollViewer.ContentVerticalOffset / startScale;
                }

                double startPixelCenterX = startPixelLeft + (startPixelWidth / 2);
                double startPixelCenterY = startPixelTop + (startPixelHeight / 2);

                ///////

                double bigScaleFactor = 1;
                for (int i = 0; i < _imageZoomBigScrollIndex; i++)
                {
                    bigScaleFactor *= 10.0;
                }

                var scalex = bigScaleFactor * Views.Constants.ScrollValues[_imageZoomLittleScrollIndex];
                var scaley = bigScaleFactor * Views.Constants.ScrollValues[_imageZoomLittleScrollIndex];

                _vm.UiScale = scalex;

                var transform = new ScaleTransform();

                transform.ScaleX = scalex;
                transform.ScaleY = scaley;

                MainDisplayImage.LayoutTransform = transform;

                // Adjust scrollview to maintain the point in the center.
                // https://stackoverflow.com/a/6461667/1462295
                var startPoint = startTransform.Transform(new Point(startPixelCenterX, startPixelCenterY));
                var endPoint = transform.Transform(new Point(startPixelCenterX, startPixelCenterY));

                if (startScale == 1 && scalex == 1.1)
                {
                    endPoint.X -= 12.2;
                }
                else if (startScale == 1.1 && scalex == 1.21)
                {
                    endPoint.Y -= 6;
                }

                var shift = endPoint - startPoint;

                MainDisplayImageScrollViewer.ScrollToVerticalOffset(MainDisplayImageScrollViewer.VerticalOffset + shift.Y);
                MainDisplayImageScrollViewer.ScrollToHorizontalOffset(MainDisplayImageScrollViewer.HorizontalOffset + shift.X);
            }
        }

        private void MainDisplayImageScrollViewer_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            e.Handled = true;
            return;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// https://stackoverflow.com/a/42288914/1462295
        /// </remarks>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainDisplayImageScrollViewer_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MainDisplayImage.CaptureMouse();
            _panMousePoint = e.GetPosition(MainDisplayImageScrollViewer);
            _verticalPanOffset = MainDisplayImageScrollViewer.VerticalOffset;
            _horizontalPanOffset = MainDisplayImageScrollViewer.HorizontalOffset;
        }

        private void MainDisplayImageScrollViewer_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            Point scrollerMousePosition = new Point(
                e.GetPosition(MainDisplayImageScrollViewer).X,
                e.GetPosition(MainDisplayImageScrollViewer).Y);

            Point imageMousePosition = new Point(
                e.GetPosition(MainDisplayImage).X,
                e.GetPosition(MainDisplayImage).Y
                );

            UpdateMousePositionStats(imageMousePosition);

            if (MainDisplayImage.IsMouseCaptured)
            {
                double newOffset;

                newOffset = _verticalPanOffset + (_panMousePoint.Y - e.GetPosition(MainDisplayImageScrollViewer).Y);
                MainDisplayImageScrollViewer.ScrollToVerticalOffset(newOffset);

                newOffset = _horizontalPanOffset + (_panMousePoint.X - e.GetPosition(MainDisplayImageScrollViewer).X);
                MainDisplayImageScrollViewer.ScrollToHorizontalOffset(newOffset);

                UpdateImagePositionStats();
            }
        }

        private void MainDisplayImageScrollViewer_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            MainDisplayImage.ReleaseMouseCapture();
        }

        private void UpdateImagePositionStats()
        {
            ScrollScaleInfo scrollInfo = new ScrollScaleInfo(
                MainDisplayImageScrollViewer.ContentHorizontalOffset,
                MainDisplayImageScrollViewer.ExtentWidth,
                MainDisplayImageScrollViewer.ContentVerticalOffset,
                MainDisplayImageScrollViewer.ExtentHeight,
                MainDisplayImageScrollViewer.DesiredSize.Width,
                MainDisplayImageScrollViewer.DesiredSize.Height
                );

            _vm.UpdateImagePositionStats(scrollInfo);
        }

        private void UpdateMousePositionStats(Point position)
        {
            ScrollScaleInfo scrollInfo = new ScrollScaleInfo(
                MainDisplayImageScrollViewer.ContentHorizontalOffset,
                MainDisplayImageScrollViewer.ExtentWidth,
                MainDisplayImageScrollViewer.ContentVerticalOffset,
                MainDisplayImageScrollViewer.ExtentHeight,
                MainDisplayImageScrollViewer.DesiredSize.Width,
                MainDisplayImageScrollViewer.DesiredSize.Height
                );

            _vm.UpdateMousePositionStats(position, scrollInfo);
        }

        private void MainDisplayImageScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            ScrollScaleInfo scrollInfo = new ScrollScaleInfo(
                MainDisplayImageScrollViewer.ContentHorizontalOffset,
                MainDisplayImageScrollViewer.ExtentWidth,
                MainDisplayImageScrollViewer.ContentVerticalOffset,
                MainDisplayImageScrollViewer.ExtentHeight,
                MainDisplayImageScrollViewer.DesiredSize.Width,
                MainDisplayImageScrollViewer.DesiredSize.Height
                );

            _vm.UpdateImagePositionStats(scrollInfo);
        }

        private void ClearZoom(object? sender, EventArgs e)
        {
            _dispatcher.BeginInvoke(() =>
            {
                _imageZoomLittleScrollIndex = 0;
                _imageZoomBigScrollIndex = 0;
                _vm.UiScale = 1.0;

                var transform = new ScaleTransform();

                transform.ScaleX = 1.0;
                transform.ScaleY = 1.0;

                MainDisplayImage.LayoutTransform = transform;

                AdjustImageSize();
                UpdateImagePositionStats();
            });
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Workspace.Instance.CloseWindows<ErrorWindow>();
            Workspace.Instance.CloseWindows<ColorWindow>();
        }
    }
}
