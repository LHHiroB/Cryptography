using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using Windows.ApplicationModel.Resources;
using WinRT.Interop;
using Windows.ApplicationModel;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;
using System.IO;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Windows.ApplicationModel.DataTransfer;
using Windows.System;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using IOCore.Libs;
using IOCore.Pages;
using System.Linq;
using Windows.Storage;
using System.Threading.Tasks;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;
using IOCore.Controls;

namespace IOCore
{
    public abstract class IOWindow : Window, INotifyPropertyChanged
    {
        internal static IOWindow Inst { get; private set; }

        #region NotifyPropertyChanged & ResourceLoader
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new(propertyName));

        public bool SetAndNotify<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        public void Notify([CallerMemberName] string propertyName = "") => OnPropertyChanged(propertyName);

        protected static readonly ResourceLoader ProtectedResourceLoader = ResourceLoader.GetForViewIndependentUse();
        #endregion

        public enum StatusType
        {
            Init,
            Ready,
        }

        protected StatusType _status;
        public StatusType Status 
        {
            get => _status;
            set
            {
                Backdrop?.ShowLogo(value == StatusType.Init);
                SetAndNotify(ref _status, value);
            }
        }

        public bool IsMaximize { get; set; }

        protected Visibility _appTitleBarVisibility = Visibility.Visible;
        public Visibility AppTitleBarVisibility { get => _appTitleBarVisibility; set => SetAndNotify(ref _appTitleBarVisibility, value); }

        public BitmapImage LargeIcon { get; private set; }
        public BitmapImage SmallIcon { get; private set; }

        public string Name { get; private set; }

        private bool _titleBarLoadingVisible;
        public bool TitleBarLoadingVisible { get => _titleBarLoadingVisible; set => SetAndNotify(ref _titleBarLoadingVisible, value); }

        public Backdrop Backdrop { get; private set; }
        private TeachingTip _teachingTip;

        private bool _isPickerOpened;
        private FileOpenPicker _fileOpenPicker;
        private FileSavePicker _fileSavePicker;
        private FolderPicker _folderPicker;

        private readonly bool _useTempDir;

        public IOWindow(bool useTempDir)
        {
            Inst = this;
            _useTempDir = useTempDir;
        }

        public void Init()
        {
            HandleIntPtr = WindowNative.GetWindowHandle(this);

            _windowId = Win32Interop.GetWindowIdFromWindow(HandleIntPtr);
            _appWindow = AppWindow.GetFromWindowId(_windowId);

            var iconFileName = "icon";

            AppWindow.SetIcon(Path.Combine(Utils.GetAssetsFolderPath(), $"{iconFileName}.ico"));

            var displayName = Marshal.StringToCoTaskMemUni(Package.Current.DisplayName);
            PInvoke.SendMessage(new(HandleIntPtr), PInvoke.WM_SETTEXT, new(0), displayName);
            Marshal.FreeCoTaskMem(displayName);

            LargeIcon = ImageMagickUtils.AppIcon.Load(96, 96);
            Notify(nameof(LargeIcon));
            SmallIcon = ImageMagickUtils.AppIcon.Load(20, 20, $"{iconFileName}.svg");
            Notify(nameof(SmallIcon));

            Name = Package.Current.DisplayName;
            Notify(nameof(Name));

            Utils.SetWindowSize(HandleIntPtr, Meta.WINDOW_INIT_SIZE.Width, Meta.WINDOW_INIT_SIZE.Height);
            IsMaximize = false;

            if (_useTempDir) Utils.CreateFreshDirectory(Meta.TEMP_DIR);

            _newWinProc = new(NewWindowProc);
            _oldWinProc = IOInvoke.SetWindowLongPtr(HandleIntPtr, (int)WINDOW_LONG_PTR_INDEX.GWL_WNDPROC, Marshal.GetFunctionPointerForDelegate(_newWinProc));
        }

        public void Loaded()
        {
            (Content.XamlRoot.Content as Panel).Children.Add(Backdrop = new Backdrop() { LogoIcon = LargeIcon });
            Backdrop.ShowLogo(_status == StatusType.Init);

            LoadFreshTeachingTip(null, null, null);
            _teachingTip.IsOpen = true;
            _teachingTip.IsOpen = false;

            _fileOpenPicker = new() { SuggestedStartLocation = PickerLocationId.Desktop, ViewMode = PickerViewMode.Thumbnail };
            InitializeWithWindow.Initialize(_fileOpenPicker, PInvoke.GetActiveWindow());

            _fileSavePicker = new() { SuggestedStartLocation = PickerLocationId.Desktop, SuggestedFileName = "My file" };
            InitializeWithWindow.Initialize(_fileSavePicker, PInvoke.GetActiveWindow());
            
            _folderPicker = new() { SuggestedStartLocation = PickerLocationId.Desktop, ViewMode = PickerViewMode.Thumbnail };
            InitializeWithWindow.Initialize(_folderPicker, PInvoke.GetActiveWindow());

#if DEBUG
            IODev.Inst.Init();
#endif
        }

        #region Picker

        public async Task<IReadOnlyList<StorageFile>> PickMultipleFilesAsync(Action<FileOpenPicker> configs = null)
        {
            if (_isPickerOpened) return null;

            _isPickerOpened = true;

            configs?.Invoke(_fileOpenPicker);
            var storageFiles = await _fileOpenPicker.PickMultipleFilesAsync().AsTask();

            _fileOpenPicker.FileTypeFilter.Clear();

            _isPickerOpened = false;
            return storageFiles;
        }

        public async Task<StorageFile> PickSaveFileAsync(Action<FileSavePicker> configs = null)
        {
            if (_isPickerOpened) return null;

            _isPickerOpened = true;

            configs?.Invoke(_fileSavePicker);
            var storageFile = await _fileSavePicker.PickSaveFileAsync().AsTask();

            _fileSavePicker.SuggestedFileName = "My file";
            _fileSavePicker.FileTypeChoices.Clear();

            _isPickerOpened = false;
            return storageFile;
        }
        

        public async Task<StorageFolder> PickSingleFolderAsync(Action<FolderPicker> configs = null)
        {
            if (_isPickerOpened) return null;

            _isPickerOpened = true;

            configs?.Invoke(_folderPicker);
            var storageFolder = await _folderPicker.PickSingleFolderAsync().AsTask();

            if (storageFolder != null)
                StorageApplicationPermissions.FutureAccessList.AddOrReplace("PickedFolderToken", storageFolder);

            _isPickerOpened = false;
            return storageFolder;
        }

        #endregion

        #region TeachingTip

        public TeachingTip LoadFreshTeachingTip(object sender, string title, string subTitle)
        {
            if (_teachingTip == null)
            {
                _teachingTip = new()
                {
                    PlacementMargin = new(5),
                    PreferredPlacement = TeachingTipPlacementMode.Auto,
                };

                _teachingTip.ActionButtonClick += TeachingTip_ActionButtonClick;
                _teachingTip.CloseButtonClick += TeachingTip_CloseButtonClick;

                (Content.XamlRoot.Content as Panel).Children.Add(_teachingTip);
            }

            _teachingTip.IsLightDismissEnabled = true;
            _teachingTip.IsOpen = false;

            _teachingTip.Target = sender as FrameworkElement;
            _teachingTip.Title = title;
            _teachingTip.Subtitle = subTitle;

            _teachingTip.ActionButtonContent = ProtectedResourceLoader.GetString("Ok");
            _teachingTip.ActionButtonCommandParameter = null;
            _teachingTip.CloseButtonContent = null;
            _teachingTip.CloseButtonCommandParameter = null;

            return _teachingTip;
        }

        private void TeachingTip_ActionButtonClick(TeachingTip sender, object args)
        {
            (sender.ActionButtonCommandParameter as Action)?.Invoke();
            sender.IsOpen = false;
        }

        private void TeachingTip_CloseButtonClick(TeachingTip sender, object args)
        {
            (sender.CloseButtonCommandParameter as Action)?.Invoke();
            sender.IsOpen = false;
        }

        public void ShowInfoTeachingTip(object sender, string title = null, string subTitle = null)
        {
            if ((sender as FrameworkElement)?.Tag is not string tag) return;

            LoadFreshTeachingTip(sender, title, subTitle);

            if (Meta.DOCS.Any(i => i.Key == tag))
            {
                _teachingTip.ActionButtonContent = ProtectedResourceLoader.GetString("LearnMore");
                _teachingTip.ActionButtonCommandParameter = () => { _ = Launcher.LaunchUriAsync(new(Meta.DOCS[tag])); };
            }

            _teachingTip.IsOpen = true;
        }

        public void ShowMessageTeachingTip(object sender, string title = null, string subTitle = null, Action action = null)
        {
            LoadFreshTeachingTip(sender, title, subTitle);

            _teachingTip.ActionButtonCommandParameter = action;
            _teachingTip.IsOpen = true;
        }

        public void ShowConfirmTeachingTip(object sender, string title = null, string subTitle = null, Action confirmAction = null, Action closeAction = null)
        {
            LoadFreshTeachingTip(sender, title, subTitle);

            _teachingTip.ActionButtonCommandParameter = confirmAction;
            _teachingTip.CloseButtonContent = ProtectedResourceLoader.GetString("Cancel");
            _teachingTip.CloseButtonCommandParameter = closeAction;
            _teachingTip.IsOpen = true;
        }

        #endregion

        #region Abstract

        public abstract void SetTitleBarLoadingVisible(bool isVisible);

        public abstract void SetCurrentNavigationViewItem(NavigationViewItem item, object parameter = null);
        public abstract void SetCurrentNavigationViewItem(string tag, object parameter = null);
        
        public abstract bool TryGoBack();

        public abstract void EnableNavigationViewItems(bool isEnabled);
        public abstract void VisibleNavigationPane(bool isVisible);

        public virtual void HandleArgsData() { }

        #endregion

        public void SetForeground() => PInvoke.SetForegroundWindow((HWND)HandleIntPtr);

        public void Minimize() => PInvoke.ShowWindow(new(HandleIntPtr), SHOW_WINDOW_CMD.SW_MINIMIZE);

        public void Maximize()
        {
            var hWND = new HWND(HandleIntPtr);

            var style = (int)WINDOW_LONG_PTR_INDEX.GWL_STYLE & (int)WINDOW_STYLE.WS_MAXIMIZE;
            if (IOInvoke.GetWindowLongPtr(HandleIntPtr, style) != IntPtr.Zero)
                PInvoke.ShowWindow(hWND, SHOW_WINDOW_CMD.SW_RESTORE);
            else
                PInvoke.ShowWindow(hWND, SHOW_WINDOW_CMD.SW_MAXIMIZE);

            IsMaximize = !IsMaximize;
        }

        public void Restore() => PInvoke.ShowWindow(new HWND(HandleIntPtr), SHOW_WINDOW_CMD.SW_RESTORE);

        public void ToggleFullScreenMode(bool notify = true)
        {
            if (_appWindow.Presenter.Kind == AppWindowPresenterKind.FullScreen)
                _appWindow.SetPresenter(AppWindowPresenterKind.Default);
            else
                _appWindow.SetPresenter(AppWindowPresenterKind.FullScreen);

            if (notify) Notify(nameof(IsFullScreen));
        }

        public void SetFullScreen(bool isFullScreen, bool notify = true)
        {
            if (isFullScreen)
            {
                if (_appWindow.Presenter.Kind != AppWindowPresenterKind.FullScreen)
                {
                    _appWindow.SetPresenter(AppWindowPresenterKind.FullScreen);
                    if (notify) Notify(nameof(IsFullScreen));
                }
            }
            else
            {
                if (_appWindow.Presenter.Kind == AppWindowPresenterKind.FullScreen)
                {
                    _appWindow.SetPresenter(AppWindowPresenterKind.Default);
                    if (notify) Notify(nameof(IsFullScreen));
                }
            }
        }

        public bool IsFullScreen => _appWindow.Presenter.Kind == AppWindowPresenterKind.FullScreen;

        public double GetScalingFactor(bool system) => system ? PInvoke.GetDpiForSystem() / 96.0 : PInvoke.GetDpiForWindow(new(HandleIntPtr)) / 96.0;

        public async void PerformStandardMenuAction(string tag)
        {
            if (tag == "Share")
            {
                var interop = DataTransferManager.As<IDataTransferManagerInterop>();
                var transferManager = DataTransferManager.FromAbi(interop.GetForWindow(HandleIntPtr, Guid.Parse("a5caee9b-8708-49d1-8d36-67d25a8da00c")));

                transferManager.DataRequested += (_, args) =>
                {
                    args.Request.Data = new();
                    args.Request.Data.Properties.Title = Package.Current.DisplayName;
                    args.Request.Data.SetWebLink(new(Meta.URL_WEB_STORE));
                };

                interop.ShowShareUIForWindow(HandleIntPtr);
            }
            else if (tag == "RateUs")
                _ = Launcher.LaunchUriAsync(new(Meta.URL_APP_STORE_REVIEW));
            else if (tag == "Exit")
                Application.Current.Exit();
            else
            {
                try
                {
                    IODialog dialog = tag switch
                    {
                        "Settings" => new SettingsDialog(),
                        "About" => new AboutDialog(),
                        _ => null
                    };

                    await dialog.Dialog(Content.XamlRoot).ShowAsync();
                }
                catch { }
            } 
        }

        //

        [ComImport, Guid("3A3DCD6C-3EAB-43DC-BCDE-45671CE800C8")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        protected interface IDataTransferManagerInterop
        {
            IntPtr GetForWindow([In] IntPtr appWindow, [In] ref Guid riid);
            void ShowShareUIForWindow(IntPtr appWindow);
        }

        //

        public IntPtr HandleIntPtr { get; private set; }

        public WindowId _windowId;
        protected AppWindow _appWindow;

        private WNDPROC _newWinProc;
        private IntPtr _oldWinProc = IntPtr.Zero;

        private LRESULT NewWindowProc(HWND HWND, uint msg, WPARAM wParam, LPARAM lParam)
        {
            switch (msg)
            {
                case PInvoke.WM_GETMINMAXINFO:
                    var scalingFactor = GetScalingFactor(false);

                    var minMaxInfo = Marshal.PtrToStructure<MINMAXINFO>(lParam);
                    minMaxInfo.ptMinTrackSize.X = Utils.Round(Meta.APP_MIN_SIZE.Width * scalingFactor);
                    minMaxInfo.ptMinTrackSize.Y = Utils.Round(Meta.APP_MIN_SIZE.Height * scalingFactor);
                    Marshal.StructureToPtr(minMaxInfo, lParam, true);
                    break;
                case PInvoke.WM_DESTROY:
                    if (_useTempDir) Utils.DeleteFileOrDirectory(Meta.TEMP_DIR);
                    break;
            }

            return PInvoke.CallWindowProc(Marshal.GetDelegateForFunctionPointer<WNDPROC>(_oldWinProc), HWND, msg, new(wParam), new(lParam));
        }
    }
}
