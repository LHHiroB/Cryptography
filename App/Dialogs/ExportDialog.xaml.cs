using Microsoft.UI.Xaml;
using System.Collections.Generic;
using System;
using System.Linq;
using Microsoft.UI.Xaml.Controls;
using System.IO;
using IOCore;
using IOCore.Types;
using IOCore.Libs;
using IOApp.Features;
using IOApp.Configs;
using IOApp.Pages;

namespace IOApp.Dialogs
{
    internal partial class ExportDialog : IODialog
    {
        public static ExportDialog Inst { get; private set; }

        private Share.StatusType _status;
        public Share.StatusType Status
        {
            get => _status;
            set
            {
                var prevStatus = _status;
                SetAndNotify(ref _status, value);

                App.CurrentWindow.SetTitleBarLoadingVisible(Utils.Any(_status, Share.StatusType.ProcessingOne));

                if (_status == Share.StatusType.Ready)
                {
                }
                else if (_status == Share.StatusType.Loading)
                {
                    App.CurrentWindow.EnableNavigationViewItems(false);
                }
                else if (_status == Share.StatusType.Loaded)
                {
                    App.CurrentWindow.EnableNavigationViewItems(true);
                }
                else if (_status == Share.StatusType.LoadFailed)
                {
                    App.CurrentWindow.EnableNavigationViewItems(true);
                }
                else if (_status == Share.StatusType.ProcessingOne)
                {
                    App.CurrentWindow.EnableNavigationViewItems(true);
                }
                else if (_status == Share.StatusType.ProcessingAll)
                {
                    EnableAllControls(false);

                    _timer.Start();
                }
                else if (_status == Share.StatusType.Processed)
                {
                    EnableAllControls(true);

                    _timer.Stop();
                }
                else if (_status == Share.StatusType.ProcessPausing)
                {
                    EnableAllControls(false);
                }
                else if (_status == Share.StatusType.ProcessPaused)
                {
                    EnableAllControls(false);

                    _timer.Stop();
                }
                else if (_status == Share.StatusType.ProcessStopping)
                {
                    if (prevStatus == Share.StatusType.ProcessPaused)
                        EnableAllControls(true);
                    else
                        EnableAllControls(false);
                }
                else if (_status == Share.StatusType.ProcessStopped)
                {
                    EnableAllControls(true);

                    _timer.Stop();
                }
                else if (_status == Share.StatusType.ProcessFailed)
                {
                    EnableAllControls(true);

                    _timer.Stop();
                }
            }
        }

        private ulong _processTimestamp = 0UL;
        private readonly DispatcherTimer _timer = new() { Interval = TimeSpan.FromSeconds(1) };

        private string _lastRun = "00:00:00";
        public string LastRun { get => _lastRun; set => SetAndNotify(ref _lastRun, value); }

        private string _timeRun = "00:00:00";
        public string TimeRun { get => _timeRun; set => SetAndNotify(ref _timeRun, value); }

        private string _fileCount = "0";
        public string FileCount { get => _fileCount; set => SetAndNotify(ref _fileCount, value); }

        public RangeObservableCollection<Option> ExportOptions { get; private set; } = new();
        public RangeObservableCollection<FileItem> FileItems { get; private set; } = new();

        public ExportDialog(IEnumerable<FileItem> items)
        {
            InitializeComponent();
            Inst = this;
            DataContext = this;

            FileItems.ReplaceRange(items);

            InitAllControls();

            foreach (var i in FileItems)
                i.UpdateAndGetIsAvailable(true, true);
        }

        private void InitAllControls()
        {
            _timer.Tick += (sender, e) =>
            {
                _processTimestamp++;
                TimeRun = TimeSpan.FromSeconds(_processTimestamp).ToString(@"hh\:mm\:ss");
            };

            foreach (var i in AppTypes.EXPORTS)
                ExportOptions.Add(new(true) { Tag = i.Key, Text = ProtectedResourceLoader.GetString(i.Value), IsSelected = false });
            ExportOptions[0].IsSelected = true;

            OverwriteExistingOutputFilesCheckBox.IsChecked = false;
            OriginalOutputCheckBox.IsChecked = true;

            OutputFolderPathTextBox.IsEnabled = false;
            OutputFolderButton.IsEnabled = false;
        }

        private void EnableAllControls(bool enabled)
        {
            ExportButton.IsEnabled = enabled;
            CancelButton.IsEnabled = enabled;
        }

        private void RemoveOneButton_Click(object sender, RoutedEventArgs e)
        {
            if (_status == Share.StatusType.Loading) return;
            if ((sender as FrameworkElement)?.DataContext is not FileItem item) return;

            FileItems.Remove(item);
        }

        private async void OutputFolderButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var storageFolder = await App.CurrentWindow.PickSingleFolderAsync();
             
                if (storageFolder != null)
                    OutputFolderPathTextBox.Text = storageFolder.Path;
            }
            catch { }
        }

        private Share.Argv PrepareArgv()
        {
            return new()
            {
                ExportToOriginalFolder = OriginalOutputCheckBox.IsChecked.GetValueOrDefault(false),
                OutputFolderPath = OutputFolderPathTextBox.Text,
                OverwriteExistingOutputFiles = OverwriteExistingOutputFilesCheckBox.IsChecked.GetValueOrDefault(false),
                ExportType = (AppTypes.ExportType)ExportOptions.FirstOrDefault(i => i.IsSelected)?.Tag,
            };
        }

        public void Export()
        {
            if (FileItems == null) return;

            var argv = PrepareArgv();

            if (!argv.ExportToOriginalFolder && string.IsNullOrWhiteSpace(argv.OutputFolderPath))
            {
                App.CurrentWindow.ShowMessageTeachingTip(null, ProtectedResourceLoader.GetString("OutputFolderMustBeSet"));
                return;
            }

            var itemLock = new object();

            IProgress<Tuple<FileItem, FileSystemItem.StatusType, Exception>> itemProgress = new Progress<Tuple<FileItem, FileSystemItem.StatusType, Exception>>(result =>
            {
                lock (itemLock)
                {
                    var item = result.Item1;
                    var itemStatus = result.Item2;
                    var ex = result.Item3;

                    item.Status = itemStatus;

                    if (itemStatus == FileSystemItem.StatusType.ProcessFailed)
                    {
                        if (ex is IOException)
                            item.ErrorText = ProtectedResourceLoader.GetString("FileOrFolderDoesNotExist");
                        else
                            item.ErrorText = ProtectedResourceLoader.GetString("UnknownError");
                    }
                }
            });

            Share.ExportFilesOrFolders(FileItems, argv,
            () =>
            {
                Status = Share.StatusType.ProcessingAll;

                foreach (var item in FileItems)
                {
                    item.UpdateAndGetIsAvailable(false, true);
                    item.Status = FileSystemItem.StatusType.ProcessInQueue;
                }
            },
            status =>
            {
                Status = status;

                if (Status == Share.StatusType.Processed)
                {
                    App.CurrentWindow.ShowMessageTeachingTip(null, ProtectedResourceLoader.GetString("Features_ProcessCompletedSuccessfully"), ProtectedResourceLoader.GetString("Features_AllInfoWillBeUpdated"), () =>
                    {
                        Hide();
                    });
                }
                else if (Status == Share.StatusType.ProcessFailed)
                    App.CurrentWindow.ShowMessageTeachingTip(null, ProtectedResourceLoader.GetString("UnknownError"));
            },
            items =>
            {
                foreach (var i in items)
                {
                    i.UpdateAndGetIsAvailable(true, true);

                    if (argv.ExportType == AppTypes.ExportType.Export)
                    {
                        Main.Inst.SourceFileItems.Remove(i);
                        Main.Inst.FileItems.Remove(i);
                    }
                }
            },
            item => itemProgress.Report(item));
        }

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            _processTimestamp = 0UL;

            LastRun = DateTime.Now.ToString(@"hh\:mm\:ss");
            TimeRun = TimeSpan.FromSeconds(_processTimestamp).ToString(@"hh\:mm\:ss");

            Export();
        }

        private void OriginalOutputCheckBox_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not CheckBox checkBox) return;

            OutputFolderPathTextBox.IsEnabled = !checkBox.IsChecked.GetValueOrDefault(false);
            OutputFolderButton.IsEnabled = !checkBox.IsChecked.GetValueOrDefault(false);
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Hide();
        }
    }
}