using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System;
using Windows.Foundation;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using Windows.System;
using IOApp.Features;
using IOCore.Libs;
using IOCore;

namespace IOApp.Pages
{
    internal partial class ImageViewer : IOPage, StoreManager.IInAppPurchase
    {
        public static ImageViewer Inst { get; private set; }

        private Share.StatusType _status;
        public Share.StatusType Status
        {
            get => _status;
            set
            {
                SetAndNotify(ref _status, value);
                Notify(nameof(StatusText));

                App.CurrentWindow.SetTitleBarLoadingVisible(Utils.Any(_status, Share.StatusType.Loading));

                if (_status == Share.StatusType.Ready)
                {
                    App.CurrentWindow.Backdrop.ShowLoading(false);
                }
                else if (_status == Share.StatusType.Loading)
                {
                    App.CurrentWindow.Backdrop.ShowLoading(true);

                    App.CurrentWindow.EnableNavigationViewItems(false);
                }
                else if (_status == Share.StatusType.Loaded)
                {
                    App.CurrentWindow.Backdrop.ShowLoading(false);

                    App.CurrentWindow.EnableNavigationViewItems(true);
                }
                else if (_status == Share.StatusType.LoadFailed)
                {
                    App.CurrentWindow.Backdrop.ShowLoading(false);

                    App.CurrentWindow.EnableNavigationViewItems(true);
                }
                else if (_status == Share.StatusType.LoadFailed)
                {
                    App.CurrentWindow.Backdrop.ShowLoading(false);

                    App.CurrentWindow.EnableNavigationViewItems(true);
                }
                else if (_status == Share.StatusType.Processed)
                {
                    App.CurrentWindow.EnableNavigationViewItems(true);
                }
                else if (_status == Share.StatusType.ProcessFailed)
                {
                    App.CurrentWindow.EnableNavigationViewItems(true);
                }

                EnableAllControlButtons();
            }
        }

        public string StatusText => ProtectedResourceLoader.GetString(Share.STATUSES[_status]);

        private FileItem _currentItem;
        public FileItem CurrentItem { get => _currentItem; set => SetAndNotify(ref _currentItem, value); }

        private BitmapImage _currentBitmapImage;

        private bool _dragging = false;
        private Point _lastMousePoint;

        private string _scaleText = "100%";
        public string ScaleText { get => _scaleText; set => SetAndNotify(ref _scaleText, value); }

        public ImageViewer()
        {
            InitializeComponent();
            Inst = this;
            DataContext = this;

            InitAllControls();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            App.CurrentWindow.SetTitleBarLoadingVisible(Utils.Any(_status, Share.StatusType.Loading));
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is FileItem fileItem)
            {
                CurrentItem = fileItem;
                _currentItem.RecoveredInfo.LoadFormatAndDimentionInfo();

                LoadImage();
            }
        }

        public void OnLicensesChanged()
        {
        }

        public void LoadImage()
        {
            Status = Share.StatusType.Loading;

            try
            {
                PreviewImage.Source = _currentBitmapImage = ImageMagickUtils.LoadBitmapImage(_currentItem.RecoveredFileOrFolderPath);

                var scaleFactor = GetAdjustedZoomFactor();
                PreviewScrollViewer.ChangeView(null, null, scaleFactor);
                ScaleText = $"{Utils.Round(scaleFactor * 100)}%";

                Status = Share.StatusType.Loaded;
            }
            catch
            {
                Status = Share.StatusType.LoadFailed;
            }
        }

        private float GetAdjustedZoomFactor()
        {
            if (_currentBitmapImage == null) return 1.0F;

            double zoomFactor = 1.0;

            if (_currentBitmapImage.PixelHeight > PreviewContainer.ActualHeight || _currentBitmapImage.PixelWidth > PreviewContainer.ActualWidth)
                zoomFactor = Math.Min(PreviewContainer.ActualHeight / _currentBitmapImage.PixelHeight, PreviewContainer.ActualWidth / _currentBitmapImage.PixelWidth);

            return (float)zoomFactor;
        }

        private void PreviewBox_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (_currentBitmapImage != null)
            {
                var scaleFactor = GetAdjustedZoomFactor();
                PreviewScrollViewer.ChangeView(null, null, scaleFactor);
                ScaleText = $"{Utils.Round(scaleFactor * 100)}%";
            }
        }

        private void KeyboardAccelerator_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
        {
            if (Utils.Any(_status, Share.StatusType.Loading)) return;
            var e = args.KeyboardAccelerator;

            if (e.Modifiers == VirtualKeyModifiers.Control)
            {
                int roundedScrollViewerScaleFactor;

                switch (e.Key)
                {
                    case VirtualKey.Number0:
                    case VirtualKey.NumberPad0:
                        PreviewScrollViewer.ChangeView(null, null, GetAdjustedZoomFactor());
                        break;
                    case VirtualKey.Subtract:
                        roundedScrollViewerScaleFactor = Utils.Round(PreviewScrollViewer.ZoomFactor * 100);

                        roundedScrollViewerScaleFactor -= roundedScrollViewerScaleFactor % 10 == 0 ? 10 : (roundedScrollViewerScaleFactor % 10);
                        if (roundedScrollViewerScaleFactor >= Utils.Round(PreviewScrollViewer.MinZoomFactor * 100))
                            PreviewScrollViewer.ChangeView(null, null, roundedScrollViewerScaleFactor / 100.0F);
                        break;
                    case VirtualKey.Add:
                        roundedScrollViewerScaleFactor = Utils.Round(PreviewScrollViewer.ZoomFactor * 100);

                        roundedScrollViewerScaleFactor = (roundedScrollViewerScaleFactor + 10) / 10 * 10;
                        if (roundedScrollViewerScaleFactor <= Utils.Round(PreviewScrollViewer.MaxZoomFactor * 100))
                            PreviewScrollViewer.ChangeView(null, null, roundedScrollViewerScaleFactor / 100.0F);
                        break;
                }

                args.Handled = true;
            }
        }

        #region PREVIEW_SCROLL_VIEWER

        private void PreviewScrollViewer_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (_dragging)
            {
                var point = e.GetCurrentPoint(PreviewScrollViewer);

                double deltaX = point.Position.X - _lastMousePoint.X;
                double deltaY = point.Position.Y - _lastMousePoint.Y;

                PreviewScrollViewer.ScrollToHorizontalOffset(PreviewScrollViewer.HorizontalOffset - deltaX);
                PreviewScrollViewer.ScrollToVerticalOffset(PreviewScrollViewer.VerticalOffset - deltaY);

                _lastMousePoint = point.Position;
            }
        }

        private void PreviewScrollViewer_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            _dragging = false;
        }

        private void PreviewScrollViewer_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            var point = e.GetCurrentPoint(PreviewScrollViewer);

            if (point.Properties.IsLeftButtonPressed)
            {
                _dragging = true;
                _lastMousePoint = point.Position;
            }
        }

        private void PreviewScrollViewer_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            _dragging = false;
        }

        private void PreviewScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            ScaleText = $"{Math.Round(PreviewScrollViewer.ZoomFactor * 100)}%";

            if (e.IsIntermediate) return;
        }

        private void PreviewScrollViewer_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            var roundedScaleFactor = Utils.Round(PreviewScrollViewer.ZoomFactor * 100);
            var scaleFactor = roundedScaleFactor < 200 ? 2.0F : GetAdjustedZoomFactor();

            PreviewScrollViewer.ChangeView(PreviewScrollViewer.HorizontalOffset, PreviewScrollViewer.VerticalOffset, scaleFactor);
            ScaleText = $"{Utils.Round(scaleFactor * 100)}%";
        }

        #endregion

        #region INIT_MAKE_DEFAULT_VALUE

        private void InitAllControls()
        {
        }

        #endregion

        private void EnableControlButton(Control control)
        {
            var processing = Utils.Any(_status, Share.StatusType.Loading);
        }

        private void EnableAllControlButtons()
        {
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Utils.DeleteFileOrDirectory(AppProfile.Inst.AppTempLocation);
            Utils.DeleteFileOrDirectory(AppProfile.Inst.AppRecoveryLocation);
            App.CurrentWindow.TryGoBack();
        }
    }
}
