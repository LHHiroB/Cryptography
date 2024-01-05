using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using Windows.System;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace IOCore.Controls
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SocialIconButton : IOUserControl
    {
        private static readonly Type _type = typeof(SocialIconButton);

        public enum SocialType
        {
            Facebook,
            LinkedIn,
            X,
        };

        private static readonly Dictionary<SocialType, string[]> SOCIALS = new()
        {
            {SocialType.Facebook,   new[]{"\u0046", "\u0066", "Facebook", Meta.URL_SOCIAL_FACEBOOK } },
            {SocialType.LinkedIn,   new[]{"\u0049", "\u0069", "LinkedIn", Meta.URL_SOCIAL_LINKEDIN } },
            {SocialType.X,          new[]{"\u004c", "\u0062", "X",        Meta.URL_SOCIAL_X } }
        };

        private readonly Brush _defaultBackground;

        public SocialIconButton()
        {
            InitializeComponent();
            _defaultBackground = XButton.Background;
        }

        public SocialType Social { get => (SocialType)GetValue(_socialProperty); set => SetValue(_socialProperty, value); }
        private static readonly DependencyProperty _socialProperty = DependencyProperty.Register(nameof(Social), typeof(SocialType), _type, new(null,
            (d, e) => (d as dynamic).Notify(nameof(Icon))));

        public int Model { get => (int)GetValue(_modelProperty); set => SetValue(_modelProperty, value); }
        private static readonly DependencyProperty _modelProperty = DependencyProperty.Register(nameof(Model), typeof(int), _type, new(null,
            (d, e) => (d as dynamic).Notify(nameof(Icon))));

        public ButtonTypes.SizeOption Size { get => (ButtonTypes.SizeOption)GetValue(_sizeProperty); set => SetValue(_sizeProperty, value); }
        public static readonly DependencyProperty _sizeProperty = DependencyProperty.Register(nameof(Size), typeof(ButtonTypes.SizeOption), _type, new(ButtonTypes.SizeOption.Designed));

        public double ConstantScale { get => (double)GetValue(_constantScaleProperty); set => SetValue(_constantScaleProperty, value); }
        public static readonly DependencyProperty _constantScaleProperty = DependencyProperty.Register(nameof(ConstantScale), typeof(double), _type, new(1.0));

        public ButtonTypes.VariantOption Variant { get => (ButtonTypes.VariantOption)GetValue(_variantProperty); set => SetValue(_variantProperty, value); }
        public static readonly DependencyProperty _variantProperty = DependencyProperty.Register(nameof(Variant), typeof(bool), _type, new(ButtonTypes.VariantOption.Default));

        public ButtonTypes.CornerOption Corner { get => (ButtonTypes.CornerOption)GetValue(_cornerProperty); set => SetValue(_cornerProperty, value); }
        public static readonly DependencyProperty _cornerProperty = DependencyProperty.Register(nameof(Corner), typeof(ButtonTypes.CornerOption), _type, new(ButtonTypes.CornerOption.Default));

        public bool IsSquare { get => (bool)GetValue(_isSquareProperty); set => SetValue(_isSquareProperty, value); }
        public static readonly DependencyProperty _isSquareProperty = DependencyProperty.Register(nameof(IsSquare), typeof(bool), _type, new(false));

        public new Visibility Visibility { get => (Visibility)GetValue(_visibilityProperty); set => SetValue(_visibilityProperty, value); }
        private static readonly DependencyProperty _visibilityProperty = DependencyProperty.Register(nameof(Visibility), typeof(Visibility), _type, new(Visibility.Visible,
            (d, e) => (d as UIElement).Visibility = (Visibility)e.NewValue));

        //

        public string Icon => SOCIALS[Social][Model];
        public string Text => SOCIALS[Social][2];

        public double ButtonWidth => ButtonTypes.GetSize(IsSquare ? (double)ButtonTypes.SizeOption.Designed : 48.0, Size, ConstantScale);
        public double ButtonHeight => ButtonTypes.GetSize((double)ButtonTypes.SizeOption.Designed, Size, ConstantScale);

        public double IconSize => ButtonTypes.GetIconSize(Size, ConstantScale) * 1.5;

        public Thickness ButtonBorderThickness => ButtonTypes.GetBorderThickness(Variant);
        public Brush ButtonBackground => ButtonTypes.GetBackground(Variant, _defaultBackground);

        public CornerRadius ButtonCornerRadius => ButtonTypes.GetCornerRadius(Size, Corner, ConstantScale);

        //

        private void XButton_Click(object sender, RoutedEventArgs e)
        {
            var teachingTip = IOWindow.Inst.LoadFreshTeachingTip(sender,
                string.Format(ProtectedResourceLoader.GetString("Social_FollowUsOn"), SOCIALS[Social][2]),
                ProtectedResourceLoader.GetString("Social_FollowUsOnDescription")
            );

            teachingTip.ActionButtonContent = ProtectedResourceLoader.GetString("Follow");
            teachingTip.ActionButtonCommandParameter = () => { _ = Launcher.LaunchUriAsync(new(SOCIALS[Social][3])); };

            teachingTip.IsOpen = true;
        }
    }
}