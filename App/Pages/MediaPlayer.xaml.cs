using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Linq;
using Windows.System;
using System.Threading.Tasks;
using FlyleafLib.MediaPlayer;
using Microsoft.UI.Xaml.Controls.Primitives;
using System.Collections.Generic;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Windowing;
using IOCore.Libs;
using IOCore;
using IOApp.Features;
using IOApp.Windows;

namespace IOApp.Pages
{
    internal partial class MediaPlayer : IOPage, StoreManager.IInAppPurchase
    {
        public static MediaPlayer Inst { get; private set; }

        private enum ProgressType
        {
            PlaybackStopped,
            VisiblePlaybackControl
        }

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
                }
                else if (_status == Share.StatusType.Loading)
                {
                    App.CurrentWindow.EnableNavigationViewItems(false);
                }
                else if (_status == Share.StatusType.Loaded)
                {
                    App.CurrentWindow.EnableNavigationViewItems(true);
                }
                else if (_status == Share.StatusType.LoadFailed)
                {
                    App.CurrentWindow.EnableNavigationViewItems(true);
                }
                else if (_status == Share.StatusType.LoadFailed)
                {
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
            }
        }

        public string StatusText => ProtectedResourceLoader.GetString(Share.STATUSES[_status]);

        private bool _isFullScreen;
        public bool IsFullScreen { get => _isFullScreen; set => SetAndNotify(ref _isFullScreen, value); }

        private bool _isPlayable;
        public bool IsPlayable { get => _isPlayable; set => SetAndNotify(ref _isPlayable, value); }

        public IOPlayer Player { get; set; }

        private readonly Utils.Debounce _debouncer = new();

        public RangeObservableCollection<MediaItem> PlaylistMediaItems { get; private set; } = new();

        private bool _addToRecent;

        private IProgress<ProgressType> _progress;

        private StoreManager.Status _licenseStatus = new(false, false);
        public StoreManager.Status LicenseStatus { get => _licenseStatus; set => SetAndNotify(ref _licenseStatus, value); }

        public MediaPlayer()
        {
            InitializeComponent();
            DataContext = this;
            Inst = this;

            InitAllControls();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            App.CurrentWindow.SetTitleBarLoadingVisible(Utils.Any(_status, Share.StatusType.Loading));
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is MediaItem fileItem)
            {
                MainWindow.Inst.NavigateToMediaPlayer(true);
                fileItem.RecoveredInfo.LoadMediaAnalysis();
                Play(fileItem, false);
            }

            (App.CurrentWindow as MainWindow)?.NavigateToMediaPlayer(true);
            App.CurrentWindow.AppWindow.Changed += AppWindow_Changed;

            OnLicensesChanged();
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            Player.Stop();
            PlaylistMediaItems.Clear();

            App.CurrentWindow.SetFullScreen(false);
            (App.CurrentWindow as MainWindow)?.NavigateToMediaPlayer(false);
            App.CurrentWindow.AppWindow.Changed -= AppWindow_Changed;
        }

        public void OnLicensesChanged()
        {
        }

        private void AppWindow_Changed(AppWindow sender, AppWindowChangedEventArgs args)
        {
            if (args.DidPresenterChange && AppWindowTitleBar.IsCustomizationSupported())
            {
                App.CurrentWindow.AppTitleBarVisibility = sender.Presenter.Kind == AppWindowPresenterKind.FullScreen ? Visibility.Collapsed : Visibility.Visible;
                FullScreenToggleButton.IsChecked = sender.Presenter.Kind == AppWindowPresenterKind.FullScreen;
                UpdatePlayerViewport();
            }
        }

        private void InitAllControls()
        {
            Player = IOPlayer.Create();

            Player.PlayEndedOption = IOPlayer.PlayEndedType.AutoPlay;

            foreach (var i in IOPlayer.PLAY_ENDEDS)
            {
                var item = new RadioMenuFlyoutItem()
                {
                    Tag = $"{nameof(IOPlayer.PlayEndedType)}{i.Key}",
                    Icon = new FontIcon() { Glyph = i.Value.Item1 },
                    Text = ProtectedResourceLoader.GetString(i.Value.Item2),
                    IsChecked = i.Key == Player.PlayEndedOption
                };

                item.Click += PlaybackButton_Click;
            }

            Player.SpeedOption = IOPlayer.SpeedType._1;

            foreach (var i in IOPlayer.SPEEDS)
            {
                var item = new RadioMenuFlyoutItem()
                {
                    Tag = $"{nameof(IOPlayer.SpeedType)}{i.Key}",
                    Text = i.Value.Item2,
                    IsChecked = i.Key == Player.SpeedOption
                };

                item.Click += PlaybackButton_Click;
            }

            Notify(nameof(Player));

            Player.PlaybackStopped += Player_PlaybackStopped;

            _progress = new Progress<ProgressType>(type =>
            {
                if (type == ProgressType.PlaybackStopped)
                {
                    if (Player.Status == FlyleafLib.MediaPlayer.Status.Ended)
                    {
                        Player.OnPlaybackStopped();
                        Notify(nameof(Player));

                        //if (ShutdownControl.PlayEnded(Player.CurrentMediaItem == PlaylistMediaItems.LastOrDefault()))
                        //    return;

                        if (Player.PlayEndedOption == IOPlayer.PlayEndedType.AutoPlay)
                            Play("next");
                        else if (Player.PlayEndedOption == IOPlayer.PlayEndedType.Replay)
                            Player.Play(true);
                    }
                }
                else
                {
                    if (!PlayerView.IsCursorHided)
                    {
                        PlaybackHud.Opacity = 0.0;
                        PlayerView.ShowCursor(false);
                    }
                }
            });
        }

        private static void AddToRecent(MediaItem item)
        {
        }

        public void UpdatePlayerViewport()
        {
            if (Player?.CurrentMediaItem == null || !Player.IsVideo) return;

            var size = Player.GetFlyleafVideoResolution();

            var viewportSize = Utils.GetContainSize(size.Width, size.Height, PlayerView.ActualWidth, PlayerView.ActualHeight);
            PlayerViewport.Width = viewportSize.Width;
            PlayerViewport.Height = viewportSize.Height;

            Player.SetZoomAndCenter(1 / App.CurrentWindow.GetScalingFactor(true), new(0, 0));
            Player.Zoom = 100;
        }

        private void VisiblePlaybackControls()
        {
            if (PlayerView.IsCursorHided)
            {
                PlaybackHud.Opacity = 1.0;
                PlayerView.ShowCursor(true);
            }

            _debouncer.Run(() => { _progress.Report(ProgressType.VisiblePlaybackControl); }, 3000);
        }

        private void Player_PlaybackStopped(object sender, PlaybackStoppedArgs e)
        {
            _progress.Report(ProgressType.PlaybackStopped);
        }

        private void PlayerViewport_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            App.CurrentWindow.ToggleFullScreenMode();
        }

        private void PlayerView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdatePlayerViewport();
        }

        private void PlayerView_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            VisiblePlaybackControls();
        }

        private void BackToManagerButton_Click(object sender, RoutedEventArgs e)
        {
            App.CurrentWindow.TryGoBack();
        }

        private void VolumeSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (Player?.Audio == null || Player?.Config?.Player == null) return;
            Player.RaiseVolume();
        }

        private async Task<bool> CheckPlayable(MediaItem item)
        {
            return true;
        }

        public void PlayMultiple(List<MediaItem> items, int startingIndex, bool addToRecent)
        {
            if (items == null || items.Count == 0 || startingIndex < 0 || startingIndex >= items.Count) return;
            
            PlaylistMediaItems.ReplaceRange(items.ToList());
            Play(PlaylistMediaItems.ElementAtOrDefault(startingIndex), addToRecent);
        }

        public void Play(MediaItem item, bool addToRecent)
        {
            if (item == null) return;

            _addToRecent = addToRecent;
            //PlaylistControl.Visibility = PlaylistMediaItems.Count == 0 ? Visibility.Collapsed : Visibility.Visible;

            Player.CurrentMediaItem = item;
            Player.Play(false);

            IsPlayable = true;

            if (_isPlayable)
            {
                Player.Play(false);
                if (addToRecent) AddToRecent(item);
            }

            UpdatePlayerViewport();
            Notify(nameof(Player));
        }

        private void Play(string previous_next)
        {
            var index = PlaylistMediaItems.IndexOf(Player.CurrentMediaItem);
            if (index == -1) return;

            MediaItem item = null;

            if (previous_next == "previous")
            {
                item = index == 0 ?
                    PlaylistMediaItems.LastOrDefault() :
                    PlaylistMediaItems.ElementAtOrDefault(index - 1);
            }
            else if (previous_next == "next")
            {
                if (Player.IsShuffled)
                {
                    var randomIndex = 0;
                    for (var i = 0; i < 3; i++)
                    {
                        randomIndex = Utils.RandomInRange(0, PlaylistMediaItems.Count);
                        if (randomIndex != index) break;
                    }

                    item = PlaylistMediaItems.ElementAtOrDefault(randomIndex);
                }
                else
                {
                    item = index == PlaylistMediaItems.Count - 1 ?
                        PlaylistMediaItems.FirstOrDefault() :
                        PlaylistMediaItems.ElementAtOrDefault(index + 1);
                }
            }

            Play(item, _addToRecent);
        }

        private void TogglePlayOrPause()
        {
            IsPlayable = true;

            if (!_isPlayable) return;

            if (Player.Status == FlyleafLib.MediaPlayer.Status.Ended)
                Player.Play(true);
            else
                Player.TogglePlayPauseResume();

            Notify(nameof(Player));
        }

        private void PlaybackButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Control control) return;

            if (control == BackwardButton)
                Player.SeekBackward();
            else if (control == ForwardButton)
                Player.SeekForward();
            else if (control == PlayButton || control == PauseButton)
                TogglePlayOrPause();
            else if (control == MuteButton)
            {
                Player.Audio.ToggleMute();
                Player.RaiseVolume();
            }

            VisiblePlaybackControls();
        }

        private void PlayerControlToggleButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not ToggleButton toggleButton) return;

            if (toggleButton == FullScreenToggleButton)
                App.CurrentWindow.ToggleFullScreenMode();
        }

        private void KeyboardAccelerator_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
        {
            var e = args.KeyboardAccelerator;

            switch (e.Key)
            {
                case VirtualKey.Escape:
                    if (App.CurrentWindow.IsFullScreen)
                    {
                        App.CurrentWindow.SetFullScreen(false);
                        FullScreenToggleButton.IsChecked = App.CurrentWindow.IsFullScreen;
                    }    
                    else
                        App.CurrentWindow.TryGoBack();
                    break;
                case VirtualKey.F11:
                    App.CurrentWindow.ToggleFullScreenMode();
                    FullScreenToggleButton.IsChecked = App.CurrentWindow.IsFullScreen;
                    break;
                case VirtualKey.Left:
                    Player.SeekBackward();
                    break;
                case VirtualKey.Right:
                    Player.SeekForward();
                    break;
                case VirtualKey.Space:
                    TogglePlayOrPause();
                    break;
            }

            args.Handled = true;
        }

        public void Closed()
        {
            Utils.DeleteFileOrDirectory(AppProfile.Inst.AppTempLocation);
            Utils.DeleteFileOrDirectory(AppProfile.Inst.AppRecoveryLocation);
            _debouncer.Dispose();
            Player.PlaybackStopped -= Player_PlaybackStopped;
        }
    }
}