using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using IOCore;
using IOApp.Dialogs;
using System.Collections.Generic;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace IOApp.Features
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    internal partial class FileItemView : IOUserControl
    {
        private static readonly Type _type = typeof(FileItemView);

        public FileItemView()
        {
            InitializeComponent();
        }

        //

        public delegate void EventHandler(object sender, EventArgs e);

        public event EventHandler OnPlay;
        public event EventHandler OnBlur;
        public event RoutedEventHandler OnDelete;
        public event EventHandler OnRemove;

        private async void MenuItemButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.Tag is not string tag) return;
            if ((sender as FrameworkElement)?.DataContext is not FileItem item) return;

            if (tag == "Play")
                OnPlay?.Invoke(sender, EventArgs.Empty);
            else if (tag == "Blur")
                OnBlur?.Invoke(sender, EventArgs.Empty);
            else if (tag == "Properties")
                await new PropertiesDialog(item).Dialog(App.CurrentWindow.Content.XamlRoot).ShowAsync();
            else if (tag == "Export")
                await new ExportDialog(new List<FileItem>() { item }).Dialog(App.CurrentWindow.Content.XamlRoot, true).ShowAsync();
            else if (tag == "PermanentlyDelete")
                OnDelete?.Invoke(sender, e);
            else if (tag == "Remove")
                OnRemove?.Invoke(sender, EventArgs.Empty);
        }

        private void MenuItemButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.Tag is not string tag) return;
            if ((sender as FrameworkElement)?.DataContext is not FileItem item) return;

            if (tag == "Play")
                OnPlay?.Invoke(sender, EventArgs.Empty);
        }
    }
}