using Microsoft.UI.Xaml.Controls;
using System.ComponentModel;
using Windows.ApplicationModel.Resources;
using System.Runtime.CompilerServices;
using System.Collections.Generic;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace IOCore
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public abstract class IOPage : Page, INotifyPropertyChanged
    {
        internal static IOPage Inst { get; private set; }

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

        public IOPage()
        {
            Inst = this;
        } 
    }
}
