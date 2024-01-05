using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.System;
using Microsoft.UI.Xaml.Input;
using Microsoft.Windows.AppLifecycle;
using IOApp.Features;
using IOCore;
using IOApp.Configs;
using IOCore.Libs;

namespace IOApp.Dialogs
{
    internal sealed partial class AccessDialog : IODialog
    {
        public static AccessDialog Inst { get; private set; }

        public bool IsInited { get; set; }

        private string _errorText = "*";
        public string ErrorText { get => _errorText; set => SetAndNotify(ref _errorText, value); }

        public AccessDialog()
        {
            InitializeComponent();
            Inst = this;
            DataContext = this;

            IsInited = !string.IsNullOrWhiteSpace(AppProfile.Inst.Password);
            Notify(nameof(IsInited));

            ApplyButton.Text = IsInited ? ProtectedResourceLoader.GetString("Unlock") : ProtectedResourceLoader.GetString("Apply");
        }

        private void Apply()
        {
            if (string.IsNullOrWhiteSpace(InitialPasswordBox.Password))
                ErrorText = ProtectedResourceLoader.GetString("Features_EmptyPasswordError");
            else if (InitialPasswordBox.Password.Trim() != InitialConfirmedPasswordBox.Password.Trim())
                ErrorText = ProtectedResourceLoader.GetString("Features_PasswordsNotMatchedError");
            else
            {
                AppProfile.Inst.Password = InitialPasswordBox.Password.Trim();
                Hide();
            }
        }

        private void Unlock()
        {
            if (PasswordBox.Password.Trim() == AppProfile.Inst.Password)
                Hide();
            else
                ErrorText = ProtectedResourceLoader.GetString("Features_WrongPasswordError");
        }

        private void ActionButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.Tag is not string tag) return;

            if (tag == "Apply")
            {
                if (IsInited) Unlock();
                else Apply();
            }
            else if (tag == "LearnMore")
                _ = Launcher.LaunchUriAsync(new(Constants.URL_IO_HOW_TO_USE_FAQ));
            else if (tag == "ResetApplication")
                App.CurrentWindow.ShowConfirmTeachingTip(sender, ProtectedResourceLoader.GetString("Features_ResetApplication"), ProtectedResourceLoader.GetString("Features_ResetMessage"),
                    () =>
                    {
                        lock (DBManager.Inst.Locker)
                        {
                            DBManager.Inst.MainDbContext.Database.EnsureDeleted();
                            DBManager.Inst.MainDbContext.Database.EnsureCreated();
                        }

                        AppProfile.Inst.Reset();

                        IOCore.Libs.Utils.DeleteFileOrDirectory(AppProfile.Inst.StorageLocation);

                        AppInstance.Restart(string.Empty);
                    });
        }

        private void KeyboardAccelerator_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
        {
            if (args.KeyboardAccelerator.Key == VirtualKey.Enter)
            {
                if (IsInited) Unlock();
                else Apply();
            }
        }
    }
}