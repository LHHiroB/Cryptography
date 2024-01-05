using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using Windows.ApplicationModel.Resources;
using Windows.System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace IOCore
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public abstract class IODialog : Page, INotifyPropertyChanged
    {
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

        internal static IODialog Inst { get; private set; }

        private static readonly ContentDialog _dialog = new();

        private static bool _isPreviewKeyDownRegistered = false;

        public ContentDialog Dialog(XamlRoot xamlRoot, bool preventEscape = false)
        {
            _dialog.Hide();

            _dialog.XamlRoot = xamlRoot;
            _dialog.Content = this;

            if (_isPreviewKeyDownRegistered)
            {
                _dialog.PreviewKeyDown -= _dialog_PreviewKeyDown;
                _isPreviewKeyDownRegistered = false;
            }

            if (preventEscape)
            {
                _dialog.PreviewKeyDown += _dialog_PreviewKeyDown;
                _isPreviewKeyDownRegistered = true;
            }

            return _dialog;
        }

        private void _dialog_PreviewKeyDown(object sender, KeyRoutedEventArgs e) => e.Handled = e.Key == VirtualKey.Escape;

        public Action OnHide;

        public IODialog()
        {
            Inst = this;
        }

        public void Hide()
        {
            OnHide?.Invoke();
            
            _dialog.Hide();
            _dialog.Content = null;
            OnHide = null;
        }
    }
}
