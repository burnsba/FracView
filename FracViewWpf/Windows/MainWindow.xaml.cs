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
using FracViewWpf.Constants;
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
        private readonly Dispatcher _dispatcher;
        private readonly MainWindowViewModel _vm;

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

            // After algorithm is full computed, reset the ui zoom.
            _vm.AfterRunCompleted += ClearZoom;

            DataContext = _vm;
        }

        /// <summary>
        /// Sets the main image container to be the size specified by algorithm run settings.
        /// </summary>
        private void AdjustImageSize()
        {
            MainDisplayImage.Height = (double)_vm.StepHeight;
            MainDisplayImage.Width = (double)_vm.StepWidth;
        }

        /// <summary>
        /// When the main window is resized, update size of main image container, and update
        /// image position stats.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            AdjustImageSize();
            UpdateImagePositionStats();
        }

        /// <summary>
        /// Mouse wheel event, captured from main image container.
        /// This allows zooming in and out.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Image_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            e.Handled = true;

            AdjustImagePixelZoom(e.Delta);
            UpdateImagePositionStats();
        }

        /// <summary>
        /// Helper method to resize main UI basde on mousewheel event.
        /// This changes the scale factor on the main image display and adjusts
        /// the container scrollviewer accordingly.
        /// </summary>
        /// <param name="delta">Mouse wheel event scroll delta.</param>
        private void AdjustImagePixelZoom(double delta)
        {
            var change = false;

            // scroll up
            if (delta > 0)
            {
                _imageZoomLittleScrollIndex++;
                if (_imageZoomLittleScrollIndex >= Views.ScrollValues.Count)
                {
                    _imageZoomLittleScrollIndex = 0;
                    _imageZoomBigScrollIndex++;
                }

                // You can always zoom in farther
                change = true;
            }
            // scroll down
            else if (delta < 0)
            {
                // Can only zoom out if not already at max zoom.
                _imageZoomLittleScrollIndex--;
                if (_imageZoomLittleScrollIndex < 0)
                {
                    _imageZoomLittleScrollIndex = Views.ScrollValues.Count - 1;

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
            // Reset zoom event.
            else if (delta == 0 || double.IsNaN(delta))
            {
                _imageZoomLittleScrollIndex = 0;
                _imageZoomBigScrollIndex = 0;
                change = true;
            }

            if (change)
            {
                /**
                 * This section handles the actual zoom/rescale event.
                 * 
                 * Scaling is normalized based on Views.ScrollValues. The current scale amount is a "big zoom" factor,
                 * which is the number of times the Views.ScrollValues collection has wrapped, and then a "small zoom"
                 * factor, which is the current index into the Views.ScrollValues collection.
                 * 
                 * The process is to find the current center pixel, scale the image, find the new center pixel
                 * location, then adjust the scroll viewer so that the original center pixel location
                 * is unchanged.
                 */
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
                    // Scrollview content blows up according to scale, so normalize back to
                    // to a base offset.

                    startPixelWidth /= startScale;
                    startPixelHeight /= startScale;

                    startPixelLeft = MainDisplayImageScrollViewer.ContentHorizontalOffset / startScale;
                    startPixelTop = MainDisplayImageScrollViewer.ContentVerticalOffset / startScale;
                }

                // Current (pre-zoom) center pixel location
                double startPixelCenterX = startPixelLeft + (startPixelWidth / 2);
                double startPixelCenterY = startPixelTop + (startPixelHeight / 2);

                // Calculate new zoom amount.
                double bigScaleFactor = 1;
                for (int i = 0; i < _imageZoomBigScrollIndex; i++)
                {
                    bigScaleFactor *= 10.0;
                }

                var scalex = bigScaleFactor * Views.ScrollValues[_imageZoomLittleScrollIndex];
                var scaley = bigScaleFactor * Views.ScrollValues[_imageZoomLittleScrollIndex];

                // Save new zoom into view model.
                _vm.UiScale = scalex;

                // Build wpf transform.
                var transform = new ScaleTransform
                {
                    ScaleX = scalex,
                    ScaleY = scaley
                };

                MainDisplayImage.LayoutTransform = transform;

                // Adjust scrollview to maintain the point in the center.
                // https://stackoverflow.com/a/6461667/1462295
                var startPoint = startTransform.Transform(new Point(startPixelCenterX, startPixelCenterY));
                var endPoint = transform.Transform(new Point(startPixelCenterX, startPixelCenterY));

                // What are these magic constants, who knows.
                if (startScale == 1 && scalex == 1.1)
                {
                    endPoint.X -= 12.2;
                }
                else if (startScale == 1.1 && scalex == 1.21)
                {
                    endPoint.Y -= 6;
                }

                // Find the amount the center pixel has shifted due to change in scale.
                var shift = endPoint - startPoint;

                // Adjust current scrollviewer offset to maintain center pixel.
                MainDisplayImageScrollViewer.ScrollToVerticalOffset(MainDisplayImageScrollViewer.VerticalOffset + shift.Y);
                MainDisplayImageScrollViewer.ScrollToHorizontalOffset(MainDisplayImageScrollViewer.HorizontalOffset + shift.X);
            }
        }

        /// <summary>
        /// Ignore mouse wheel events on the parent scrollview container.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainDisplayImageScrollViewer_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            e.Handled = true;
            return;
        }

        /// <summary>
        /// On mouse down, begin capturing mouse position to pan image.
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

        /// <summary>
        /// Scroll view mouse move event. If currently dragging, then shift the image
        /// according to mouse movement.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainDisplayImageScrollViewer_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            Point imageMousePosition = new(
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

        /// <summary>
        /// Finalize mouse drag.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainDisplayImageScrollViewer_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            MainDisplayImage.ReleaseMouseCapture();
        }

        /// <summary>
        /// Helper method, pass through relevant UI information to display stats.
        /// </summary>
        private void UpdateImagePositionStats()
        {
            ScrollScaleInfo scrollInfo = new(
                MainDisplayImageScrollViewer.ContentHorizontalOffset,
                MainDisplayImageScrollViewer.ExtentWidth,
                MainDisplayImageScrollViewer.ContentVerticalOffset,
                MainDisplayImageScrollViewer.ExtentHeight,
                MainDisplayImageScrollViewer.DesiredSize.Width,
                MainDisplayImageScrollViewer.DesiredSize.Height
                );

            _vm.UpdateImagePositionStats(scrollInfo);
        }

        /// <summary>
        /// Helper method, pass through relevant UI information to display stats.
        /// </summary>
        private void UpdateMousePositionStats(Point position)
        {
            _vm.UpdateMousePositionStats(position);
        }

        /// <summary>
        /// Helper method, pass through relevant UI information to display stats.
        /// </summary>
        private void MainDisplayImageScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            ScrollScaleInfo scrollInfo = new(
                MainDisplayImageScrollViewer.ContentHorizontalOffset,
                MainDisplayImageScrollViewer.ExtentWidth,
                MainDisplayImageScrollViewer.ContentVerticalOffset,
                MainDisplayImageScrollViewer.ExtentHeight,
                MainDisplayImageScrollViewer.DesiredSize.Width,
                MainDisplayImageScrollViewer.DesiredSize.Height
                );

            _vm.UpdateImagePositionStats(scrollInfo);
        }

        /// <summary>
        /// Reset zoom back to base level, clear associated variables, and update UI.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ClearZoom(object? sender, EventArgs e)
        {
            _dispatcher.BeginInvoke(() =>
            {
                _imageZoomLittleScrollIndex = 0;
                _imageZoomBigScrollIndex = 0;
                _vm.UiScale = 1.0;

                var transform = new ScaleTransform
                {
                    ScaleX = 1.0,
                    ScaleY = 1.0
                };

                MainDisplayImage.LayoutTransform = transform;

                AdjustImageSize();
                UpdateImagePositionStats();
            });
        }

        /// <summary>
        /// When the main window closes, close any other opened windows.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Workspace.Instance.CloseWindows<ErrorWindow>();
            Workspace.Instance.CloseWindows<ColorWindow>();
        }
    }
}
