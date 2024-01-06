using ColorCode.Common;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using IOApp.Features;
using IOApp.Configs;
using IOApp.Dialogs;
using IOCore;
using IOCore.Libs;
using IOCore.Types;

namespace IOApp.Pages
{
    internal partial class Main : IOPage
    {
        public static Main Inst { get; private set; }

        private Share.StatusType _status;
        public Share.StatusType Status
        {
            get => _status;
            set
            {
                var prevStatus = _status;

                SetAndNotify(ref _status, value);
                Notify(nameof(StatusText));

                App.CurrentWindow.SetTitleBarLoadingVisible(_status == Share.StatusType.Loading);

                if (_status == Share.StatusType.Ready)
                {
                    App.CurrentWindow.Backdrop.ShowLoading(false);
                }
                else if (_status == Share.StatusType.Loading)
                {
                    App.CurrentWindow.Backdrop.ShowLoading(true);

                    App.CurrentWindow.EnableNavigationViewItems(false);
                }
                else if (_status == Share.StatusType.Loaded)
                {
                    App.CurrentWindow.Backdrop.ShowLoading(false);

                    App.CurrentWindow.EnableNavigationViewItems(true);
                }
                else if (_status == Share.StatusType.LoadFailed)
                {
                    App.CurrentWindow.Backdrop.ShowLoading(false);

                    App.CurrentWindow.EnableNavigationViewItems(true);
                }
                else if (_status == Share.StatusType.ProcessingOne)
                {
                    App.CurrentWindow.Backdrop.ShowLoading(true);

                    App.CurrentWindow.EnableNavigationViewItems(true);
                }
                else if (_status == Share.StatusType.ProcessingAll)
                {
                    App.CurrentWindow.Backdrop.ShowLoading(true);

                    EnableAllControls(false);

                    foreach (var i in FileItems)
                        i.IsEnabled = false;
                }
                else if (_status == Share.StatusType.Processed)
                {
                    App.CurrentWindow.Backdrop.ShowLoading(false);

                    EnableAllControls(true);

                    foreach (var i in FileItems)
                        i.IsEnabled = true;
                }
                else if (_status == Share.StatusType.ProcessPausing)
                {
                    EnableAllControls(false);
                }
                else if (_status == Share.StatusType.ProcessPaused)
                {
                    EnableAllControls(false);
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

                    foreach (var i in FileItems)
                        i.IsEnabled = true;
                }
                else if (_status == Share.StatusType.ProcessFailed)
                {
                    App.CurrentWindow.Backdrop.ShowLoading(false);

                    EnableAllControls(true);

                    foreach (var i in FileItems)
                        i.IsEnabled = true;
                }

                EnableAllControls(_status != Share.StatusType.Loading);
            }
        }

        public string StatusText => ProtectedResourceLoader.GetString(Share.STATUSES[_status]);

        //
        public RangeObservableCollection<Option> FilterItems { get; private set; } = new();

        private string _fileCountText = string.Empty;
        public string FileCountText { get => _fileCountText; set => SetAndNotify(ref _fileCountText, value); }

        private string _fileCount = "0";
        public string FileCount { get => _fileCount; set => SetAndNotify(ref _fileCount, value); }

        public List<FileItem> SourceFileItems = new();
        public RangeObservableCollection<FileItem> FileItems { get; private set; } = new();

        public List<FileItem> ExternalFileItems = new();

        private readonly Utils.Debounce _debouncer = new();

        private string _inputTypes = string.Empty;
        public string InputTypes { get => _inputTypes; set => SetAndNotify(ref _inputTypes, value); }

        public bool FullSupport { get; private set; }

        public Main()
        {
            InitializeComponent();
            Inst = this;
            DataContext = this;

            FullSupport = Profile.FullSupport;
            Notify(nameof(FullSupport));

            InitAllControls();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            App.CurrentWindow.SetTitleBarLoadingVisible(_status == Share.StatusType.Loading);
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            _debouncer.Dispose();
        }

        public void AddFiles(IEnumerable<string> paths)
        {
            if (_status == Share.StatusType.Loading) return;
			Status = Share.StatusType.Loading;

            var hasCorrupted = false;

            var locker = new object();
            var coreCount = Environment.ProcessorCount;
            var pathCount = paths.Count();

            App.CurrentWindow.Backdrop.ShowLoading(true, $"1/{pathCount}");

            ExternalFileItems.Clear();

            IProgress<Share.StatusType> progress = new Progress<Share.StatusType>(async status =>
            {
                if (ExternalFileItems.Count != 0)
                    await new UnlockDialog().Dialog(App.CurrentWindow.Content.XamlRoot, true).ShowAsync();

                if (status == Share.StatusType.Loaded)
                {
                    if (hasCorrupted)
                        App.CurrentWindow.ShowMessageTeachingTip(null, string.Empty, ProtectedResourceLoader.GetString("LoadCorruptedSomeFiles"));

                    ApplyFilter();
                }

                ExternalFileItems.Clear();

                Status = status;
            });

            IProgress<List<FileItem>> itemProgress = new Progress<List<FileItem>>(items =>
            {
				lock (locker)
                {
                    SourceFileItems.AddRange(items);
                    App.CurrentWindow.Backdrop.ShowLoading(true, $"{SourceFileItems.Count}/{pathCount}");
				}
            });

            _ = Task.Run(() =>
            {
                try
                {
                    var packages = paths.Chunk(GlobalConstants.PACKAGE_SIZE);
                    var items = new List<FileItem>();

                    foreach (var package in packages)
                    {
                        var pathChunksPerPackage = package.Chunk(coreCount);

                        lock (locker)
                        {
                            items.Clear();
                        }

                        foreach (var pathChunk in pathChunksPerPackage)
                        {
                            Parallel.ForEach(pathChunk, path =>
                            {
                                try
                                {
                                    if (!Profile.FullSupport)
                                    {
                                        if (!Utils.IsFilePath(path).GetValueOrDefault(false) || !Profile.IsAcceptedInputExtension(Path.GetExtension(path)))
                                            return;
                                    }

                                    var item = new FileItem(path);

                                    var footerLoaded = item.TryLoadFooter(item.InputFileOrFolderPath);
                                    if (!footerLoaded)
                                    {
                                        if (item.FileType != FileUtils.Type.Directory)
                                        {
                                            item.FileType = FileUtils.GetTypeByExtension(item.InputFileExtension);
                                            item.PrepareThumbnail(item.InputFileOrFolderPath, 256, 256);
                                        }

                                        Share.EncodeOne(item);

                                        item.TryLoadFooter(item.EncryptedFileOrFolderPath);
                                        item.LoadEncryptedBasicInfo();
                                    }
                                    else
                                    {
                                        var outputRawTempFilePath = Utils.GetTempFilePath(AppProfile.Inst.AppTempLocation);
                                        var outputTempFilePath = Utils.GetTempFilePath(AppProfile.Inst.AppTempLocation);

                                        try
                                        {
                                            var originalName = item.Footer.FooterMetadata.OriginalName;

                                            Utils.CreateDirectoryIfNotExist(AppProfile.Inst.AppTempLocation);

                                            File.Copy(item.InputFileOrFolderPath, outputRawTempFilePath);
                                            Utils.RemoveLatestBytesFromFile(outputRawTempFilePath, item.Footer.GetSize());

                                            var outputFileOrFolderPath = Path.Combine(AppProfile.Inst.AppTempLocation, Path.GetFileName(item.Footer.FooterMetadata.OriginalName));

                                            CryptographyUtils.DecryptFile(outputRawTempFilePath, outputTempFilePath, AppProfile.Inst.Password, CryptographyUtils.SALT, CryptographyUtils.ITERATIONS);

                                            var outputFilePath = Path.Combine(AppProfile.Inst.AppEncryptedLocation, Guid.NewGuid().ToString());

                                            File.Copy(item.InputFileOrFolderPath, outputFilePath);

                                            item.InputInfo = new(originalName);
                                            item.EncryptedInfo = new(outputFilePath);

                                            item.TryLoadFooter(outputFilePath);
                                            item.LoadEncryptedBasicInfo();
                                        }
                                        catch (Exception)
                                        {
                                            ExternalFileItems.Add(item);
                                            return;
                                        }
                                        finally
                                        {
                                            Utils.DeleteFileOrDirectory(outputRawTempFilePath);
                                            Utils.DeleteFileOrDirectory(outputTempFilePath);
                                        }
                                    }

                                    lock (DBManager.Inst.Locker)
                                    {
                                        AppDbContext.Inst.Files.Add(new(item.EncryptedFileOrFolderPath)
                                        {
                                            FileType = item.FileType,
                                            PrivacyType = item.Privacy,
                                        });

                                        AppDbContext.Inst.SaveChanges();
                                    }

                                    if (!string.IsNullOrWhiteSpace(item.EncryptedFileOrFolderPath))
                                        lock (locker)
                                        {
                                            items.Add(item);
                                        }
                                }
                                catch (Exception)
                                {
                                    hasCorrupted = true;
                                }
                            });
                        }

                        itemProgress.Report(items);
                    }

                    progress.Report(Share.StatusType.Loaded);
                }
                catch
                {
                    progress.Report(Share.StatusType.LoadFailed);
                }
            });
        }

        public void LoadFiles(IEnumerable<FileEntity> files)
        {
            if (files == null || !files.Any()) return;

            if (_status == Share.StatusType.Loading) return;
            Status = Share.StatusType.Loading;

            var hasCorrupted = false;
			
            var locker = new object();
            var coreCount = Environment.ProcessorCount;
            var pathCount = files.Count();

            App.CurrentWindow.Backdrop.ShowLoading(true, $"1/{pathCount}");

            IProgress<Share.StatusType> progress = new Progress<Share.StatusType>(status =>
            {
                if (status == Share.StatusType.Loaded)
				{
                    if (hasCorrupted)
                        App.CurrentWindow.ShowMessageTeachingTip(null, string.Empty, ProtectedResourceLoader.GetString("LoadCorruptedSomeFiles"));

                    ApplyFilter();
				}

                Status = status;
            });

            IProgress<List<FileItem>> itemProgress = new Progress<List<FileItem>>(items =>
            {
                lock (locker)
                {
                    SourceFileItems.AddRange(items);
                    App.CurrentWindow.Backdrop.ShowLoading(true, $"{SourceFileItems.Count}/{pathCount}");
                }
            });

            _ = Task.Run(() =>
            {
                try
                {
                    var packages = files.Chunk(GlobalConstants.PACKAGE_SIZE);
                    var items = new List<FileItem>();

                    foreach (var package in packages)
                    {
                        var chunkPerPackages = package.Chunk(coreCount);

                        lock (locker)
                        {
                            items.Clear();
                        }

                        foreach (var chunk in chunkPerPackages)
                        {
                            Parallel.ForEach(chunk, file =>
                            {
                                try
                                {
                                    var item = new FileItem(file);
                                    item.EncryptedInfo?.LoadInfo();
                                    item.TryLoadFooter(item.EncryptedFileOrFolderPath);
                                    item.SetIsCorrupted(item.Footer == null);

                                    lock (locker)
                                    {
                                        items.Add(item);
                                    }
                                }
                                catch
								{
                                    hasCorrupted = true;
								}
                            });
                        }

                        itemProgress.Report(items);
                    }

                    progress.Report(Share.StatusType.Loaded);
                }
                catch
                {
                    progress.Report(Share.StatusType.LoadFailed);
                }
            });
        }

        private async void OpenInputFilesPicker()
        {
            try
            {
                var storageFiles = await App.CurrentWindow.PickMultipleFilesAsync(picker =>
                {
                    foreach (var i in Profile.INPUT_EXTENSIONS)
                        picker.FileTypeFilter.Add(i);

                    if (Profile.FullSupport)
                        picker.FileTypeFilter.Add("*");
                });

                if (storageFiles != null && storageFiles.Count > 0)
                    AddFiles(storageFiles.Select(i => i.Path).ToList());
            }
            catch { }
        }

        private async void OpenInputFolderPicker()
        {
            try
            {
                var storageFolder = await App.CurrentWindow.PickSingleFolderAsync();

                if (storageFolder != null)
                    AddFiles(new List<string>() { storageFolder.Path });
            }
            catch { }
        }

        private void InputFilesButton_Click(object sender, RoutedEventArgs e)
        {
            OpenInputFilesPicker();
        }

        private void InputFolderButton_Click(object sender, RoutedEventArgs e)
        {
            OpenInputFolderPicker();
        }

        #region DRAP_DROP_FILES

        private void Input_DragEnter(object sender, DragEventArgs e)
        {
            e.DragUIOverride.IsGlyphVisible = false;
            (sender as FrameworkElement).Opacity = 0.6;
        }

        private void Input_DragLeave(object sender, DragEventArgs e)
        {
            (sender as FrameworkElement).Opacity = 1.0;
        }

        private void Input_DragOver(object sender, DragEventArgs e)
        {
            e.AcceptedOperation = DataPackageOperation.Copy;
            e.DragUIOverride.Caption = ProtectedResourceLoader.GetString("PlusCopy");
        }

        private async void Input_Drop(object sender, DragEventArgs e)
        {
            if (e.Data != null)
            {
                (sender as FrameworkElement).Opacity = 1.0;
                return;
            }

            if (e.DataView.Contains(StandardDataFormats.StorageItems))
            {
                var storageItems = await e.DataView.GetStorageItemsAsync();
                if (storageItems.Count > 0)
                    AddFiles(storageItems.Select(i => i.Path).ToList());
            }

            (sender as FrameworkElement).Opacity = 1.0;
        }

        #endregion

        private void InitAllControls()
        {
            InputTypes = Profile.GetInputExtensionsTextByGroupFamily() + $", *";

            foreach (var i in AppTypes.FILES)
                FilterItems.Add(new(true) { Tag = i.Key, Text = ProtectedResourceLoader.GetString(i.Value) });
            FilterItems[0].IsSelected = true;

            foreach (var i in AppTypes.SORTS)
                SortComboBox.Items.Add(new ComboBoxItem { Tag = i.Key, Content = ProtectedResourceLoader.GetString(i.Value.Item2) });
            SortComboBox.SelectedIndex = 0;

            void fileItemsCollectionChangedAction()
            {
                if (SourceFileItems.Count > 0)
                {
                    WelcomeBox.Visibility = Visibility.Collapsed;
                    WorkBox.Visibility = Visibility.Visible;
                }
                else
                {
                    WelcomeBox.Visibility = Visibility.Visible;
                    WorkBox.Visibility = Visibility.Collapsed;
                }

                FileCount = FileItems.Count.ToString();
                FileCountText = $"{FileItems.Count} {ProtectedResourceLoader.GetString(FileItems.Count >= 2 ? "Files" : "File").ToLowerInvariant()}";
            };

            FileItems.CollectionChanged += (object sender, NotifyCollectionChangedEventArgs e) => fileItemsCollectionChangedAction();
            fileItemsCollectionChangedAction();

            LoadFiles(AppDbContext.Inst.Files);
        }

        private void FileGridView_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
        {
            if (!args.InRecycleQueue)
            {
                if (args?.Item is not FileItem item) return;

                try
                {
                    item.LoadThumbnailIfNotExistTask(null);
                }
                catch { }
            }
        }

        private async void FileControl_OnPlay(object sender, EventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is not FileItem item) return;

            var hasException = false;

            if (!Utils.IsExistFileOrDirectory(item.RecoveredFileOrFolderPath))
            {
                Status = Share.StatusType.ProcessingOne;
                
                await Task.Run(() =>
                {
                    try
                    {
                        Share.DecodeOne(item);
                        item.RecoveredInfo.LoadFormatAndDimentionInfo();
                    }
                    catch
                    {
                        hasException = true;
                    }
                });
            }
            
            Status = Share.StatusType.Processed;

            if (hasException)
                App.CurrentWindow.ShowMessageTeachingTip(null, ProtectedResourceLoader.GetString("LoadCorruptedFile"), null, null);
            else
            {
                if (item.FileType == FileUtils.Type.Image)
                    App.CurrentWindow.SetCurrentNavigationViewItem(typeof(ImageViewer).ToString(), item);
                else if (Utils.Any(item.FileType, FileUtils.Type.Video, FileUtils.Type.Audio))
                    App.CurrentWindow.SetCurrentNavigationViewItem(typeof(MediaPlayer).ToString(), MediaItem.FromFSItem(item));
                else
                    Utils.OpenFileWithDefaultApp(item.RecoveredFileOrFolderPath);
            }
        }

        private void FileControl_OnBlur(object sender, EventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is not FileItem item) return;
            item.TogglePrivate(null);

            lock (DBManager.Inst.Locker)
            {
                var file = AppDbContext.Inst.Files.FirstOrDefault(f => f.Path == item.EncryptedFileOrFolderPath);
                file.PrivacyType = item.Privacy;

                AppDbContext.Inst.SaveChanges();
            }
        }

        private void FileControl_OnRemove(object sender, EventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is not FileItem item) return;

            lock (DBManager.Inst.Locker)
            {
                var file = AppDbContext.Inst.Files.FirstOrDefault(i => i.Path == item.EncryptedFileOrFolderPath);
                if (file != null)
                {
                    try
                    {
                        AppDbContext.Inst.Files.Remove(file);
                        AppDbContext.Inst.SaveChanges();

                        SourceFileItems.Remove(item);
                        FileItems.Remove(item);
                    }
                    catch { }
                }
            }
        }

        private void FileControl_OnDelete(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is not FileItem item) return;

            App.CurrentWindow.ShowConfirmTeachingTip(null, ProtectedResourceLoader.GetString("Features_ConfirmPermanentlyDelete"), null,
                () =>
                {
                    SourceFileItems.Remove(item);
                    item.Destroy();
                    ApplyFilter();
                });
        }

        public void ApplyFilter()
        {
            var filter = FilterItems.FirstOrDefault(i => i.IsSelected);
            if (filter == null || SortComboBox.SelectedItem == null) return;

            var filterTag = (AppTypes.FileType)filter.Tag;

            Status = Share.StatusType.Loading;

            FileItems.ReplaceRange(
                filterTag switch
                {
                    AppTypes.FileType.Video => SourceFileItems.Where(i => i.FileType == FileUtils.Type.Video),
                    AppTypes.FileType.Audio => SourceFileItems.Where(i => i.FileType == FileUtils.Type.Audio),
                    AppTypes.FileType.Image => SourceFileItems.Where(i => i.FileType == FileUtils.Type.Image),
                    _ => SourceFileItems
                }
            );

            var sortTag = (AppTypes.SortType)(SortComboBox.SelectedItem as ComboBoxItem).Tag;

            if (sortTag == AppTypes.SortType.A2Z)
                FileItems.SortStable((a, b) => string.Compare(a.OriginalFileOrFolderName, b.OriginalFileOrFolderName));
            else if (sortTag == AppTypes.SortType.LastModified)
                FileItems.SortStable((a, b) => DateTime.Compare(b.EncryptedInfo.FileSystemInfo.LastWriteTime, a.EncryptedInfo.FileSystemInfo.LastWriteTime));

            Status = Share.StatusType.Loaded;
        }

        private void MediaFilter_SelectionChanged(object sender, TappedRoutedEventArgs e)
        {
            ApplyFilter();
        }

        private void ActionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilter();
        }

        private void EnableAllControls(bool isEnabled)
        {
            WelcomeBoxAddFolderButton.IsEnabled = isEnabled;
        }

        private async void OutSide_DragItemsStarting(object sender, DragItemsStartingEventArgs e)
        {
            var storageItems = new List<IStorageItem>();

            foreach (var i in e.Items.Cast<FileItem>())
            {
                Share.DecodeOne(i);
                var selectedFileItem = await StorageFile.GetFileFromPathAsync(i.RecoveredFileOrFolderPath);
                storageItems.Add(selectedFileItem);
            }

            e.Data.SetStorageItems(storageItems);
            e.Data.RequestedOperation = DataPackageOperation.Move;
        }

        private async void ActionButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.Tag is not string tag) return;

            var selectedItems = FileGridView.SelectedItems.Select(i => i as FileItem).ToList();

            if (tag == "ExportSelected" && selectedItems.Count > 0)
                await new ExportDialog(selectedItems).Dialog(App.CurrentWindow.Content.XamlRoot, true).ShowAsync();
            else if (tag == "PermanentlyDeleteSelected" && selectedItems.Count > 0)
            {
                App.CurrentWindow.ShowConfirmTeachingTip(null, ProtectedResourceLoader.GetString("ClearSelectedItems"), ProtectedResourceLoader.GetString("CannotBeUndone"), () =>
                {
                    foreach (var item in selectedItems)
                    {
                        SourceFileItems.Remove(item);
                        item.Destroy();
                    }

                    ApplyFilter();
                });
            }
        }

        public void Closed()
        {
            _debouncer.Dispose();
        }
    }
}