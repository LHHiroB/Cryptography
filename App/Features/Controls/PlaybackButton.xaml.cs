using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls.Primitives;
using System;
using IOCore;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace IOApp.Features
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class PlaybackButton : IOUserControl
    {
        private static readonly Type _type = typeof(PlaybackButton);

        public PlaybackButton()
        {
            InitializeComponent();
        }

        public string Icon { get => (string)GetValue(_iconProperty); set => SetValue(_iconProperty, value); }
        private static readonly DependencyProperty _iconProperty = DependencyProperty.Register(nameof(Icon), typeof(string), _type, new(null, (DependencyObject d,
            DependencyPropertyChangedEventArgs e) => (d as dynamic).Notify(nameof(Icon))));

        public FlyoutBase Flyout { get => (FlyoutBase)GetValue(_flyoutProperty); set => SetValue(_flyoutProperty, value); }
        public static readonly DependencyProperty _flyoutProperty = DependencyProperty.Register(nameof(Flyout), typeof(FlyoutBase), _type, new(default));

        public new Visibility Visibility { get => (Visibility)GetValue(_visibilityProperty); set => SetValue(_visibilityProperty, value); }
        private static readonly DependencyProperty _visibilityProperty = DependencyProperty.Register(nameof(Visibility), typeof(Visibility), _type, new(Visibility.Visible,
            (DependencyObject d, DependencyPropertyChangedEventArgs e) => (d as UIElement).Visibility = (Visibility)e.NewValue));

        //
        
        public event RoutedEventHandler Click;

        private void XButton_Click(object sender, RoutedEventArgs e)
        {
            Click?.Invoke(this, e);
        }
    }
}