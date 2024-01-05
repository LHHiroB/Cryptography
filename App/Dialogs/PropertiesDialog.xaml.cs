using Microsoft.UI.Xaml;
using System.Linq;
using System.Collections.Generic;
using IOCore;
using IOCore.Libs;
using IOApp.Features;
using System.IO;

namespace IOApp.Dialogs
{
    internal partial class PropertiesDialog : IODialog
    {
        public class ItemRecord
        {
            public string Title { get; private set; }
            public string Value { get; private set; }

            public ItemRecord(string title, string value)
            {
                Title = title;
                Value = value;
            }
        }

        public static PropertiesDialog Inst { get; private set; }

        public RangeObservableCollection<ItemRecord> Items { get; private set; } = new();

        public PropertiesDialog(FileItem item)
        {
            InitializeComponent();
            DataContext = this;
            Inst = this;

            //Items.ReplaceRange(new List<ItemRecord>()
            //{
            //    new(ProtectedResourceLoader.GetString("Name"),                      Path.GetFileName(item.OriginalFileOrFolderPath)),
            //    new(ProtectedResourceLoader.GetString("Location"),                  Path.GetDirectoryName(item.OriginalFileOrFolderPath)),
            //    new(ProtectedResourceLoader.GetString("Features_FileType"),         item.FileExtension),
            //    new(ProtectedResourceLoader.GetString("Features_Size"),             Utils.GetReadableByteSizeText(item.FileSize)),
            //    new(ProtectedResourceLoader.GetString("Features_Album"),            item.Album),
            //    new(ProtectedResourceLoader.GetString("Features_Artists"),          item.Artist),
            //    new(ProtectedResourceLoader.GetString("Features_Duration"),         item.Duration.ToString(@"hh\:mm\:ss")),
            //    new(ProtectedResourceLoader.GetString("Features_Genre"),            item.Genre),
            //    new(ProtectedResourceLoader.GetString("Features_AudioChannels"),    item.ChannelText),
            //    new(ProtectedResourceLoader.GetString("Features_AudioBitrate"),     item.AudioBitrateText),
            //});

            //if (item.IsVideo)
            //{
            //    Items.AddRange(new List<ItemRecord>()
            //    {
            //        new(ProtectedResourceLoader.GetString("Features_Resolution"),   item.ResolutionText),
            //        new(ProtectedResourceLoader.GetString("Features_FrameRate"),    item.FrameRateText),
            //    });
            //}
        }

        private void CopyButton_Click(object sender, RoutedEventArgs e) => Utils.SetTextToClipboard(string.Join("\n", Items.Select(i => $"{i.Title}: {i.Value}")), false);
        private void CloseButton_Click(object sender, RoutedEventArgs e) => Hide();
    }
}

//<? xml version = "1.0" encoding = "utf-8" ?>
//< iocore : IODialog
//    xmlns: iocore = "using:IOCore"
//    x: Class = "IOApp.Dialogs.PropertiesDialog"
//    xmlns = "http://schemas.microsoft.com/winfx/2006/xaml/presentation"
//    xmlns: x = "http://schemas.microsoft.com/winfx/2006/xaml"
//    xmlns: d = "http://schemas.microsoft.com/expression/blend/2008"
//    xmlns: mc = "http://schemas.openxmlformats.org/markup-compatibility/2006"
//    xmlns: coreLibs = "using:IOCore.Libs"
//    xmlns: coreControls = "using:IOCore.Controls"
//    mc: Ignorable = "d"
//    NavigationCacheMode = "Enabled"
//>
//    < iocore:IODialog.Resources >
//        < coreLibs:FilePathConverter x:Key = "FilePathConverter" />
//        < coreLibs:EnumComparisonToBoolConverter x:Key = "EnumComparisonToBoolConverter" />
//        < coreLibs:FileSizeToSizeStringConverter x:Key = "FileSizeToSizeStringConverter" />
//        < coreLibs:TimeSpanToTimeStringConverter x:Key = "TimeSpanToTimeStringConverter" />

//        < Style x: Key = "ValueTextBlockStyle" TargetType = "TextBlock" >
//            < Setter Property = "FontSize" Value = "14" />
//            < Setter Property = "FontWeight" Value = "SemiBold" />
//            < Setter Property = "TextWrapping" Value = "Wrap" />
//        </ Style >
//    </ iocore:IODialog.Resources >

//    < Grid Width = "450" DataContext = "{Binding FileItem}" >
//        < StackPanel Padding = "15" Spacing = "5" >
//            < StackPanel Margin = "0,15,0,0" Grid.Column = "1" >
//                < TextBlock Text = "{Binding OriginalFileOrFolderPath, ConverterParameter=fileName, Converter={StaticResource FilePathConverter}}" FontSize = "14" FontWeight = "Bold" />
//                < TextBlock Margin = "0,3,0,0" Style = "{StaticResource ValueTextBlockStyle}" Text = "{Binding OriginalFileOrFolderPath}" />
//            </ StackPanel >


//            < !--File-- >
//            < StackPanel Visibility = "{Binding FileType, ConverterParameter=Document|Unknown|Directory, Converter={StaticResource EnumComparisonToBoolConverter}}" >
//                < Grid RowDefinitions = "Auto,Auto"  ColumnDefinitions = "Auto,Auto" ColumnSpacing = "200" RowSpacing = "15" >
//                    < !--1-- >
//                    < StackPanel >
//                        < TextBlock Text = "{coreLibs:SRE K=Features_Size}" FontSize = "14" />
//                        < TextBlock Style = "{StaticResource ValueTextBlockStyle}" Text = "{Binding EncryptedSize, Converter={StaticResource FileSizeToSizeStringConverter}}" />
//                    </ StackPanel >
//                    < !--2-- >
//                    < StackPanel Grid.Column = "1" >
//                        < TextBlock Text = "{coreLibs:SRE K=Features_FileType}" FontSize = "14" />
//                        < TextBlock Style = "{StaticResource ValueTextBlockStyle}" Text = "{coreLibs:SRE K=File}" Visibility = "{Binding FileType, ConverterParameter=Unknown|Document, Converter={StaticResource EnumComparisonToBoolConverter}}" FontSize = "14" />
//                        < TextBlock Style = "{StaticResource ValueTextBlockStyle}" Text = "{coreLibs:SRE K=Folder}" Visibility = "{Binding FileType, ConverterParameter=Directory, Converter={StaticResource EnumComparisonToBoolConverter}}" FontSize = "14" />
//                    </ StackPanel >
//                    < !--3-- >
//                    < StackPanel Grid.Row = "1" Visibility = "{Binding FileType, ConverterParameter=Unknown|Document, Converter={StaticResource EnumComparisonToBoolConverter}}" >
//                        < TextBlock Text = "{coreLibs:SRE K=Features_Extension}" FontSize = "14" />
//                        < TextBlock Style = "{StaticResource ValueTextBlockStyle}" Text = "{Binding OriginalFileOrFolderPath, ConverterParameter=extension, Converter={StaticResource FilePathConverter}}" FontSize = "14" />
//                    </ StackPanel >
//                </ Grid >
//            </ StackPanel >


//            < !--Image-- >
//            < StackPanel Visibility = "{Binding FileType, ConverterParameter=Image, Converter={StaticResource EnumComparisonToBoolConverter}}" >
//                < Grid RowDefinitions = "Auto,Auto"  ColumnDefinitions = "Auto,Auto" ColumnSpacing = "200" RowSpacing = "15" >
//                    < !--1-- >
//                    < StackPanel >
//                        < TextBlock Text = "{coreLibs:SRE K=Features_Size}" FontSize = "14" />
//                        < TextBlock Style = "{StaticResource ValueTextBlockStyle}" Text = "{Binding EncryptedSize, Converter={StaticResource FileSizeToSizeStringConverter}}" />
//                    </ StackPanel >
//                    < !--2-- >
//                    < StackPanel Grid.Column = "1" >
//                        < TextBlock Text = "{coreLibs:SRE K=Features_FileType}" FontSize = "14" />
//                        < TextBlock Style = "{StaticResource ValueTextBlockStyle}" Text = "{coreLibs:SRE K=Image}" FontSize = "14" />
//                    </ StackPanel >
//                    < !--3-- >
//                    < StackPanel Grid.Row = "1" >
//                        < TextBlock Text = "{coreLibs:SRE K=Features_Dimension}" FontSize = "14" />
//                        < TextBlock Style = "{StaticResource ValueTextBlockStyle}" Text = "{Binding Resolution}" FontSize = "14" />
//                    </ StackPanel >
//                    < !--4-- >
//                    < StackPanel Grid.Row = "1" Grid.Column = "1" >
//                        < TextBlock Text = "{coreLibs:SRE K=Features_Extension}" FontSize = "14" />
//                        < TextBlock Style = "{StaticResource ValueTextBlockStyle}" Text = "{Binding OriginalFileOrFolderPath, ConverterParameter=extension, Converter={StaticResource FilePathConverter}}" FontSize = "14" />
//                    </ StackPanel >
//                </ Grid >
//            </ StackPanel >


//            < !--Video-- >
//            < StackPanel Visibility = "{Binding FileType, ConverterParameter=Video, Converter={StaticResource EnumComparisonToBoolConverter}}" >
//                < Grid RowDefinitions = "Auto,Auto" ColumnDefinitions = "Auto,Auto" ColumnSpacing = "200" RowSpacing = "15" >
//                    < !--1-- >
//                    < StackPanel >
//                        < TextBlock Text = "{coreLibs:SRE K=Features_Size}" FontSize = "14" />
//                        < TextBlock Style = "{StaticResource ValueTextBlockStyle}" Text = "{Binding EncryptedSize, Converter={StaticResource FileSizeToSizeStringConverter}}" />
//                    </ StackPanel >
//                    < !--2-- >
//                    < StackPanel Grid.Column = "1" >
//                        < TextBlock Text = "{coreLibs:SRE K=Features_FileType}" FontSize = "14" />
//                        < TextBlock Style = "{StaticResource ValueTextBlockStyle}" Text = "Video" FontSize = "14" />
//                    </ StackPanel >
//                    < !--3-- >
//                    < StackPanel Grid.Row = "1" >
//                        < TextBlock Text = "{coreLibs:SRE K=Features_Duration}" FontSize = "14" />
//                        < TextBlock Style = "{StaticResource ValueTextBlockStyle}" Text = "{Binding Footer.FooterExtra.Duration, Converter={StaticResource TimeSpanToTimeStringConverter}}" FontSize = "14" />
//                    </ StackPanel >
//                    < !--4-- >
//                    < StackPanel Grid.Row = "1" Grid.Column = "1" >
//                        < TextBlock Text = "{coreLibs:SRE K=Features_Extension}" FontSize = "14" />
//                        < TextBlock Style = "{StaticResource ValueTextBlockStyle}" Text = "{Binding OriginalFileOrFolderPath, ConverterParameter=extension, Converter={StaticResource FilePathConverter}}" FontSize = "14" />
//                    </ StackPanel >
//                </ Grid >
//            </ StackPanel >

//            < !--Audio-- >
//            < StackPanel Visibility = "{Binding FileType, ConverterParameter=Audio, Converter={StaticResource EnumComparisonToBoolConverter}}" >
//                < Grid RowDefinitions = "Auto,Auto" ColumnDefinitions = "Auto,Auto" ColumnSpacing = "200" RowSpacing = "15" >
//                    < !--1-- >
//                    < StackPanel >
//                        < TextBlock Text = "{coreLibs:SRE K=Features_Size}" FontSize = "14" />
//                        < TextBlock Style = "{StaticResource ValueTextBlockStyle}" Text = "{Binding EncryptedSize, Converter={StaticResource FileSizeToSizeStringConverter}}" />
//                    </ StackPanel >
//                    < !--2-- >
//                    < StackPanel Grid.Column = "1" >
//                        < TextBlock Text = "{coreLibs:SRE K=Features_FileType}" FontSize = "14" />
//                        < TextBlock Style = "{StaticResource ValueTextBlockStyle}" Text = "{coreLibs:SRE K=Audio}" FontSize = "14" />
//                    </ StackPanel >
//                    < !--3-- >
//                    < StackPanel Grid.Row = "1" >
//                        < TextBlock Text = "{coreLibs:SRE K=Features_Duration}" FontSize = "14" />
//                        < TextBlock Style = "{StaticResource ValueTextBlockStyle}" Text = "{Binding Footer.FooterExtra.Duration, Converter={StaticResource TimeSpanToTimeStringConverter}}"  FontSize = "14" />
//                    </ StackPanel >
//                    < !--4-- >
//                    < StackPanel Grid.Row = "1" Grid.Column = "1" >
//                        < TextBlock Text = "{coreLibs:SRE K=Features_Extension}" FontSize = "14" />
//                        < TextBlock Style = "{StaticResource ValueTextBlockStyle}" Text = "{Binding OriginalFileOrFolderPath, ConverterParameter=extension, Converter={StaticResource FilePathConverter}}" FontSize = "14" />
//                    </ StackPanel >
//                </ Grid >
//            </ StackPanel >
//        </ StackPanel >

//        < coreControls:IconButton Icon = "&#xE711;" IsSquare="True" Corner="None" Variant="Text" Click="CloseButton_Click"
//                                 VerticalAlignment="Top" HorizontalAlignment="Right"/>
//    </Grid>
//</iocore:IODialog >