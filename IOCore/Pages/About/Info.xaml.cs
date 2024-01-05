using Microsoft.UI.Xaml;
using System;
using Windows.System;
using Windows.ApplicationModel;
using CommunityToolkit.WinUI.Helpers;
using IOCore.Libs;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace IOCore.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Info : IOPage
    {
        public new static Info Inst { get; private set; }

        public class Item
        {
            public string Name { get; set; }
            public string Icon { get; set; }
            public string Title { get; set; }
            public string Description { get; set; }
            public string Link { get; set; }

            public Item(string name, string icon, string title, string description, string link)
            {
                Name = name;
                Icon = icon;
                Title = title;
                Description = description;
                Link = link;
            }
        }

        public string AppName { get; private set; }
        public string Version { get; private set; }
        public string Credit { get; private set; }
        public Uri PolicyUri { get; private set; }

        public RangeObservableCollection<Item> Items { get; private set; } = new();

        public string WebsiteDescription { get; private set; }
        public string Website { get; private set; }

        public Info()
        {
            InitializeComponent();
            Inst = this;
            DataContext = this;

            AppIconImage.Source = ImageMagickUtils.AppIcon.Load(96, 96);
            AppIconImage.Width = AppIconImage.Height = 96;

            AppName = Package.Current.DisplayName;
            Notify(nameof(AppName));

            Version = $"v{Package.Current.Id.Version.ToFormattedString(3)}";
            Notify(nameof(Version));

            Credit = string.Format(ProtectedResourceLoader.GetString("About_Credit"), DateTime.Now.Year, Package.Current.PublisherDisplayName);
            Notify(nameof(Credit));

            PolicyUri = new(Meta.URL_IO_PRIVACY);
            Notify(nameof(PolicyUri));

            Items.AddRange(new Item[]
            {
                new("leaveAnIdea",
                    "\uE913",
                    ProtectedResourceLoader.GetString("About_ShareValuableIdeas"),
                    ProtectedResourceLoader.GetString("About_ShareValuableIdeasDescription"),
                    Meta.URL_APP_STORE_REVIEW),
                new("publisher",
                    "\uE8F9",
                    ProtectedResourceLoader.GetString("About_DiscoverMoreApps"),
                    ProtectedResourceLoader.GetString("About_DiscoverMoreAppsDescription"),
                    Meta.URL_APP_STORE_PUBLISHER),
            });

            WebsiteDescription = ProtectedResourceLoader.GetString("About_OurWebsiteDescription");
            Notify(nameof(WebsiteDescription));

            Website = Meta.URL_IO;
            Notify(nameof(Website));
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is not Item item) return;
            _ = Launcher.LaunchUriAsync(new(item.Link));
        }
    }
}