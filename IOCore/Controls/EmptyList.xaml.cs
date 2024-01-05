using Microsoft.UI.Xaml;
using System;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace IOCore.Controls
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public partial class EmptyList : IOUserControl
    {
        private static readonly Type _type = typeof(EmptyList);

        public EmptyList()
        {
            InitializeComponent();
        }

        public string Icon { get => (string)GetValue(_iconProperty); set => SetValue(_iconProperty, value); }
        private static readonly DependencyProperty _iconProperty = DependencyProperty.Register(nameof(Icon), typeof(string), _type, new("\uEA37",
            (d, e) => (d as dynamic).Notify(nameof(Icon))));

        public string Text { get => (string)GetValue(_textProperty); set => SetValue(_textProperty, value); }
        private static readonly DependencyProperty _textProperty = DependencyProperty.Register(nameof(Text), typeof(string), _type, new(ProtectedResourceLoader.GetString("NothingInList"),
            (d, e) => (d as dynamic).Notify(nameof(Text))));

        public new Visibility Visibility { get => (Visibility)GetValue(_visibilityProperty); set => SetValue(_visibilityProperty, value); }
        private static readonly DependencyProperty _visibilityProperty = DependencyProperty.Register(nameof(Visibility), typeof(Visibility), _type, new(Visibility.Visible,
            (d, e) => (d as UIElement).Visibility = (Visibility)e.NewValue));
    }
}