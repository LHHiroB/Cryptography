using Microsoft.UI.Xaml;
using System;
using System.IO;
using System.Linq;
using System.IO.Compression;
using System.Collections.Specialized;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.UI.Xaml.Data;
using IOCore;
using IOCore.Libs;
using IOApp.Features;
using IOApp.Pages;

namespace IOApp.Dialogs
{
    internal class ItemToOpacityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string culture)
        {
            if (value is not bool v) return 0.6;
            return v ? 1.0 : 0.6;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string culture) => throw new NotImplementedException();
    }

    internal partial class UnlockDialog : IODialog
    {
        public static UnlockDialog Inst { get; private set; }

        private Share.StatusType _status;
        public Share.StatusType Status
        {
            get => _status;
            set
            {
                var prevStatus = _status;

                SetAndNotify(ref _status, value);

                App.CurrentWindow.SetTitleBarLoadingVisible(_status == Share.StatusType.Loading);

                if (_status == Share.StatusType.Ready)
                {
                    App.CurrentWindow.EnableNavigationViewItems(true);
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

                    _timer.Stop();
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

                EnableAllControls(_status != Share.StatusType.Loading);
                EnableAllControls(_status != Share.StatusType.ProcessingAll);
            }
        }

        public string StatusText => ProtectedResourceLoader.GetString(Share.STATUSES[_status]);

        public RangeObservableCollection<FileItem> FileItems { get; private set; } = new();

        private ulong _processTimestamp = 0UL;
        private readonly DispatcherTimer _timer = new() { Interval = TimeSpan.FromSeconds(1) };

        //

        private string _lastRun = "00:00:00";
        public string LastRun { get => _lastRun; set => SetAndNotify(ref _lastRun, value); }

        private string _timeRun = "00:00:00";
        public string TimeRun { get => _timeRun; set => SetAndNotify(ref _timeRun, value); }

        private string _fileCount = "0";
        public string FileCount { get => _fileCount; set => SetAndNotify(ref _fileCount, value); }

        private string _inputTypes = string.Empty;
        public string InputTypes { get => _inputTypes; set => SetAndNotify(ref _inputTypes, value); }

        public UnlockDialog()
        {
            InitializeComponent();
            Inst = this;
            DataContext = this;

            FileItems.Clear();
            FileItems.AddRange(Main.Inst.ExternalFileItems.ToList());

            foreach (var item in FileItems)
            {
                item.LoadBasicInfo();
                item.UpdateAndGetIsAvailable(true, true);
            }

            InitAllControls();
        }

        private void RemoveOneButton_Click(object sender, RoutedEventArgs e)
        {
            if (Utils.Any(_status, Share.StatusType.Loading, Share.StatusType.ProcessingAll)) return;
            if ((sender as FrameworkElement).DataContext is not FileItem item) return;

            FileItems.Remove(item);
        }

        private void InitAllControls()
        {
            void fileItemsCollectionChangedAction()
            {
                if (_status != Share.StatusType.Loading)
                {
                    ProcessAllButton.Visibility = Visibility.Visible;
                }

                FileCount = FileItems.Count.ToString();

                if (FileItems.Count > 0)
                {
                    FileListView.Visibility = Visibility.Visible;
                }
                else
                {
                    FileListView.Visibility = Visibility.Collapsed;
                }
            }

            FileItems.CollectionChanged += (object sender, NotifyCollectionChangedEventArgs e) => fileItemsCollectionChangedAction();
            fileItemsCollectionChangedAction();

            FileItems.CollectionChanged += (object sender, NotifyCollectionChangedEventArgs e) => EnableAllControls(FileItems.Count > 0);

            _timer.Tick += (object sender, object e) =>
            {
                _processTimestamp++;
                TimeRun = TimeSpan.FromSeconds(_processTimestamp).ToString(@"hh\:mm\:ss");
            };
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Hide();
        }

        public void EncodeOne(FileItem item)
        {
            if (!Utils.IsExistFileOrDirectory(item.TempFileOrFolderPath))
                throw new IOException();

            string outputTempFilePath = Utils.GetTempFilePath(AppProfile.Inst.AppTempLocation);
            string outputZipFilePath = Utils.GetTempFilePath(AppProfile.Inst.AppTempLocation);

            try
            {
                if (item.FileType == FileUtils.Type.Directory)
                {
                    ZipFile.CreateFromDirectory(item.TempFileOrFolderPath, outputZipFilePath, CompressionLevel.NoCompression, false);
                    CryptographyUtils.EncryptFile(outputZipFilePath, outputTempFilePath, AppProfile.Inst.Password, CryptographyUtils.SALT, CryptographyUtils.ITERATIONS);

                    Utils.DeleteFileOrDirectory(outputZipFilePath);
                }
                else
                    CryptographyUtils.EncryptFile(item.TempFileOrFolderPath, outputTempFilePath, AppProfile.Inst.Password, CryptographyUtils.SALT, CryptographyUtils.ITERATIONS);

                item.TempInfo = new(item.TempFileOrFolderPath);
                item.TempInfo.LoadInfo();
                item.TempInfo.LoadMediaAnalysis();
                item.TempInfo.LoadFormatAndDimentionInfo();

                Footer.Metadata footerMetadata = new(item.FileType, item.OriginalFileOrFolderPath,
                    item.FileType switch
                    {
                        FileUtils.Type.Video or FileUtils.Type.Audio or FileUtils.Type.Image => new(item.TempInfo?.MediaAnalysis?.Format ?? null, item.TempInfo?.Width ?? 0, item.TempInfo?.Height ?? 0),
                        _ => null,
                    });

                footerMetadata.AppendToFile(outputTempFilePath, item.OriginalThumbnail?.ToByteArray() ?? null);

                //

                string outputFilePath = Path.Combine(AppProfile.Inst.AppEncryptedLocation, Guid.NewGuid().ToString());
                File.Copy(outputTempFilePath, outputFilePath, true);

                item.EncryptedInfo = new(outputFilePath);

                Utils.DeleteFileOrDirectory(item.TempFileOrFolderPath);
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                Utils.DeleteFileOrDirectory(outputTempFilePath);
                Utils.DeleteFileOrDirectory(outputZipFilePath);
            }
        }

        public void DecodeOne(FileItem item, string password)
        {
            if (!Utils.IsExistFileOrDirectory(item.InputFileOrFolderPath))
                throw new IOException();

            var outputFolderPath = AppProfile.Inst.AppTempLocation;

            var outputRawTempFilePath = Utils.GetTempFilePath(AppProfile.Inst.AppTempLocation);
            var outputTempFilePath = Utils.GetTempFilePath(AppProfile.Inst.AppTempLocation);

            try
            {
                var footer = Footer.CreateFromFile(item.InputFileOrFolderPath) ?? throw new();
                File.Copy(item.InputFileOrFolderPath, outputRawTempFilePath);

                Utils.RemoveLatestBytesFromFile(outputRawTempFilePath, footer.GetSize());
                CryptographyUtils.DecryptFile(outputRawTempFilePath, outputTempFilePath, password, CryptographyUtils.SALT, CryptographyUtils.ITERATIONS);

                var outputFilePath = Path.Combine(outputFolderPath, Path.GetFileName(footer.FooterMetadata.OriginalName));
                item.TempInfo = new(outputFilePath);

                if (footer.FooterMetadata.FileType == FileUtils.Type.Directory)
                    ZipFile.ExtractToDirectory(outputTempFilePath, outputFilePath, true);
                else
                    File.Move(outputTempFilePath, outputFilePath, true);
            }
            catch (ApplicationException)
            {
                throw new IncorrectPasswordException();
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                Utils.DeleteFileOrDirectory(outputRawTempFilePath);
                Utils.DeleteFileOrDirectory(outputTempFilePath);
            }
        }

        public void ReloadOne(FileItem item)
        {
            if (item.FileType != FileUtils.Type.Directory)
            {
                if (FileUtils.EXTENSIONS[FileUtils.Type.Image].Contains(item.InputFileExtension))
                    item.SetFileType(FileUtils.Type.Image);
                else if (FileUtils.EXTENSIONS[FileUtils.Type.Video].Contains(item.InputFileExtension))
                    item.SetFileType(FileUtils.Type.Video);
                else if (FileUtils.EXTENSIONS[FileUtils.Type.Audio].Contains(item.InputFileExtension))
                    item.SetFileType(FileUtils.Type.Audio);
                else if (FileUtils.EXTENSIONS[FileUtils.Type.Document].Contains(item.InputFileExtension))
                    item.SetFileType(FileUtils.Type.Document);
                else
                    item.SetFileType(FileUtils.Type.Unknown);

                item.PrepareThumbnail(item.TempFileOrFolderPath, 256, 256);
            }

            EncodeOne(item);

            item.TryLoadFooter(item.EncryptedFileOrFolderPath);
            item.EncryptedInfo?.LoadInfo();

            lock (DBManager.Inst.Locker)
            {
                FileEntity file = new(item.EncryptedFileOrFolderPath)
                {
                    FileType = item.FileType,
                    PrivacyType = item.Privacy,
                };

                AppDbContext.Inst.Files.Add(file);
                AppDbContext.Inst.SaveChanges();
            }
        }

        private void EnableAllControls(bool isEnabled)
        {
            InputPasswordBox.IsEnabled = isEnabled;
            ProcessAllButton.IsEnabled = isEnabled;
        }

        public void ImportFilesOrFolders(Action startAction = null, Action<Share.StatusType> endAction = null, Action<List<FileItem>> itemAction = null)
        {
            startAction?.Invoke();

            foreach (var i in FileItems)
                i.Status = FileSystemItem.StatusType.ProcessInQueue;

            var itemLocker = new object();
            var itemsLocker = new object();
            var password = InputPasswordBox.Password.Trim();
            var coreCount = Environment.ProcessorCount;

            IProgress<Share.StatusType> endProgress = new Progress<Share.StatusType>(s =>
            {
                endAction?.Invoke(s);
            });

            IProgress<Tuple<FileItem, FileSystemItem.StatusType, Exception>> itemProgress = new Progress<Tuple<FileItem, FileSystemItem.StatusType, Exception>>(result =>
            {
                lock (itemLocker)
                {
                    var item = result.Item1;
                    var itemStatus = result.Item2;
                    var ex = result.Item3;

                    item.Status = itemStatus;

                    if (itemStatus == FileSystemItem.StatusType.Processed)
                    {
                        Utils.DeleteFileOrDirectory(item.InputFileOrFolderPath);
                    }
                    else if (itemStatus == FileSystemItem.StatusType.ProcessFailed)
                    {
                        if (ex is IOException)
                            item.ErrorText = ProtectedResourceLoader.GetString("FileOrFolderDoesNotExist");
                        else
                            item.ErrorText = ProtectedResourceLoader.GetString("UnknownError");
                    }
                }
            });

            IProgress<List<FileItem>> itemsProgress = new Progress<List<FileItem>>(items =>
            {
                lock (itemsLocker)
                {
                    itemAction?.Invoke(items);
                }
            });

            _ = Task.Run(() =>
            {
                try
                {
                    var packages = FileItems.Chunk(GlobalConstants.PACKAGE_SIZE);
                    var items = new List<FileItem>();

                    foreach (var package in packages)
                    {
                        var itemChunksPerPackage = package.Chunk(coreCount);

                        lock (itemsLocker)
                        {
                            items.Clear();
                        }

                        foreach (var itemChunk in itemChunksPerPackage)
                        {
                            _ = Parallel.ForEach(itemChunk, item =>
                            {
                                try
                                {
                                    itemProgress.Report(Tuple.Create(item, FileSystemItem.StatusType.Processing, new Exception()));

                                    DecodeOne(item, password);
                                    ReloadOne(item);

                                    itemProgress.Report(Tuple.Create(item, FileSystemItem.StatusType.Processed, new Exception()));
                                }
                                catch (Exception ex)
                                {
                                    itemProgress.Report(Tuple.Create(item, FileSystemItem.StatusType.ProcessFailed, ex));
                                }

                                items.Add(item);
                            });
                        }

                        itemsProgress.Report(items);
                    }

                    endProgress.Report(Share.StatusType.Processed);
                }
                catch
                {
                    endProgress.Report(Share.StatusType.ProcessFailed);
                }
            });
        }

        private void ProcessAllButton_Click(object sender, RoutedEventArgs e)
        {
            ImportFilesOrFolders(
                () =>
                {
                    Status = Share.StatusType.ProcessingAll;

                    foreach (var item in FileItems)
                        item.UpdateAndGetIsAvailable(false, true);
                },
                status =>
                {
                    var processedItems = FileItems.Where(i => !string.IsNullOrEmpty(i.EncryptedFileOrFolderPath) && string.IsNullOrEmpty(i.ErrorText)).ToList();

                    if (processedItems.Count != FileItems.Count)
                    {
                        foreach (var item in processedItems)
                        {
                            FileItems.Remove(item);
                            Utils.DeleteFileOrDirectory(item.InputFileOrFolderPath);
                        }
                    }
                    else
                    {
                        Main.Inst.ApplyFilter();
                        Hide();
                    }

                    Status = status;
                },
                items =>
                {
                    var processedItems = items.Where(i => !string.IsNullOrEmpty(i.EncryptedFileOrFolderPath) && string.IsNullOrEmpty(i.ErrorText)).ToList();

                    foreach (var item in items)
                        item.UpdateAndGetIsAvailable(true, true);

                    Main.Inst.SourceFileItems.AddRange(processedItems);
                }
            );
        }
    }
}