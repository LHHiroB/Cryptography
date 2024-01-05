using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media.Imaging;
using System;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace IOCore.Controls
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public partial class Backdrop : IOUserControl
    {
        private static readonly Type _type = typeof(Backdrop);

        public Backdrop()
        {
            InitializeComponent();
        }

        public BitmapImage LogoIcon { get => (BitmapImage)GetValue(_logoIconProperty); set => SetValue(_logoIconProperty, value); }
        private static readonly DependencyProperty _logoIconProperty = DependencyProperty.Register(nameof(LogoIcon), typeof(BitmapImage), _type, new(null,
            (d, e) => (d as dynamic).Notify(nameof(LogoIcon))));

        public string LoadingText { get => (string)GetValue(_loadingTextProperty); set => SetValue(_loadingTextProperty, value); }
        private static readonly DependencyProperty _loadingTextProperty = DependencyProperty.Register(nameof(LoadingText), typeof(string), _type, new(null,
            (d, e) => (d as dynamic).Notify(nameof(LoadingText))));

        //

        public bool IsMaskVisible { get => (bool)GetValue(_isMaskVisibleProperty); set => SetValue(_isMaskVisibleProperty, value); }
        private static readonly DependencyProperty _isMaskVisibleProperty = DependencyProperty.Register(nameof(IsMaskVisible), typeof(bool), _type, new(false,
            (d, e) => (d as dynamic).Notify(nameof(IsMaskVisible))));

        public bool IsLoadingVisible { get => (bool)GetValue(_isLoadingVisibleProperty); set => SetValue(_isLoadingVisibleProperty, value); }
        private static readonly DependencyProperty _isLoadingVisibleProperty = DependencyProperty.Register(nameof(IsLoadingVisible), typeof(bool), _type, new(false,
            (d, e) => (d as dynamic).Notify(nameof(IsLoadingVisible))));

        public bool IsLogoVisible { get => (bool)GetValue(_isLogoVisibleProperty); set => SetValue(_isLogoVisibleProperty, value); }
        private static readonly DependencyProperty _isLogoVisibleProperty = DependencyProperty.Register(nameof(IsLogoVisible), typeof(bool), _type, new(false,
            (d, e) => (d as dynamic).Notify(nameof(IsLogoVisible))));

        //

        public void ShowMask(bool isVisible) => IsMaskVisible = isVisible;

        public void ShowLoading(bool isVisible, string text = null)
        {
            IsLoadingVisible = isVisible;
            LoadingText = text;
        }

        public void ShowLogo(bool isVisible) => IsLogoVisible = isVisible;
    }
}