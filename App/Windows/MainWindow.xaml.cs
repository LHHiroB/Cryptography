using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using Windows.System;
using IOCore.Libs;
using IOCore;
using IOApp.Configs;
using IOApp.Features;
using IOApp.Pages;
using IOApp.Dialogs;

namespace IOApp.Windows
{
    internal partial class MainWindow : IOWindow, StoreManager.IInAppPurchase
    {
        public static MainWindow Inst { get; private set; }

        public Visibility PromotionAppVisibility { get; private set; } = Visibility.Collapsed;

        public MainWindow() : base(false)
        {
            InitializeComponent();
            (Content as FrameworkElement).DataContext = this;
            Inst = this;
            Init();

            ExtendsContentIntoTitleBar = true;
            SetTitleBar(AppTitleBar);
        }

        private void Loaded(object sender, RoutedEventArgs e)
        {
            Loaded();

            DBManager.Inst.InitAsync(new AppDbContext(), new Progress<bool>(async _ =>
            {
                if (PageNavigationView.IsLoaded)
                {
                    Status = StatusType.Ready;

                    Backdrop.ShowMask(true);

                    await new AccessDialog().Dialog(Content.XamlRoot).ShowAsync();

                    Backdrop.ShowMask(false);

                    SetCurrentNavigationViewItem(typeof(Main).ToString());
                }
            }));
        }

        public override void HandleArgsData()
        {
            base.HandleArgsData();
			
            Restore();
            SetForeground();
        }

        public void OnLicensesChanged()
        {
            ImageViewer.Inst?.OnLicensesChanged();
            MediaPlayer.Inst?.OnLicensesChanged();
        }

        public void NavigateToMediaPlayer(bool to)
        {
            if (to)
            {
                AppTitleBar.Visibility = Visibility.Visible;
                Toolbar.Visibility = Visibility.Collapsed;
            }
            else
            {
                AppTitleBar.Visibility = Visibility.Visible;
                Toolbar.Visibility = Visibility.Visible;
            }

            Utils.SetThreadExecutionState(to, to);
        }

        private async void MenuButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.Tag is not string tag) return;

            if (tag == "PrivacySettings")
                await new PreferencesDialog().Dialog(Content.XamlRoot).ShowAsync();
            else
                PerformStandardMenuAction(tag);
        }

        //

        private void NavigationView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            SetCurrentNavigationViewItem(args.SelectedItemContainer as NavigationViewItem);
        }

        private void Window_Activated(object sender, WindowActivatedEventArgs args)
        {
        }

        private void Window_Closed(object sender, WindowEventArgs args)
        {
            try
            {
                Utils.DeleteFileOrDirectory(AppProfile.Inst.AppTempLocation);
                Utils.DeleteFileOrDirectory(AppProfile.Inst.AppRecoveryLocation);
            }
            catch { }
        }

        //

        public override void SetTitleBarLoadingVisible(bool isVisible)
        {
            TitleBarLoadingVisible = isVisible;
        }

        public string GetCurrentNavigationViewItemTag()
        {
            if ((PageNavigationView.SelectedItem as NavigationViewItem).Tag is not string tag) return null;
            return tag;
        }

        public override void SetCurrentNavigationViewItem(NavigationViewItem item, object parameter = null)
        {
            if (item?.Tag is not string tag) return;

            PageNavigationView.SelectionChanged -= NavigationView_SelectionChanged;

            _ = ContentFrame.Navigate(Type.GetType(tag), parameter);
            PageNavigationView.Header = null;
            PageNavigationView.SelectedItem = item;

            PageNavigationView.SelectionChanged += NavigationView_SelectionChanged;
        }

        public override void SetCurrentNavigationViewItem(string tag, object parameter = null)
        {
            for (int i = 0; i < PageNavigationView.MenuItems.Count; i++)
            {
                if ((PageNavigationView.MenuItems[i] as FrameworkElement).Tag as string == tag)
                {
                    SetCurrentNavigationViewItem(PageNavigationView.MenuItems[i] as NavigationViewItem, parameter);
                    return;
                }
            }

            for (int i = 0; i < PageNavigationView.FooterMenuItems.Count; i++)
            {
                if ((PageNavigationView.FooterMenuItems[i] as FrameworkElement).Tag as string == tag)
                {
                    SetCurrentNavigationViewItem(PageNavigationView.FooterMenuItems[i] as NavigationViewItem, parameter);
                    return;
                }
            }
        }

        public override bool TryGoBack()
        {
            if (ContentFrame.CanGoBack)
            {
                ContentFrame.GoBack();
                return true;
            }
            return false;
        }

        public override void EnableNavigationViewItems(bool isEnabled)
        {
            for (int i = 0; i < PageNavigationView.MenuItems.Count; i++)
                (PageNavigationView.MenuItems[i] as Control).IsEnabled = isEnabled;

            for (int i = 0; i < PageNavigationView.FooterMenuItems.Count; i++)
                (PageNavigationView.FooterMenuItems[i] as Control).IsEnabled = isEnabled;
        }

        public override void VisibleNavigationPane(bool isVisible) => PageNavigationView.IsPaneVisible = isVisible;
    }
}
