using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.ApplicationModel.Resources;

namespace IOCore
{
    public class IOItem : INotifyPropertyChanged
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
    }
}