using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Storage.AccessCache;
using WinRT.Interop;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Windows.Win32;
using IOCore;
using IOCore.Libs;
using IOApp.Features;
using IOApp.Pages;

namespace IOApp.Dialogs
{
    internal sealed partial class PreferencesDialog : IODialog
    {
        public static PreferencesDialog Inst { get; private set; }

        private string _errorText = "*";
        public string ErrorText { get => _errorText; set => SetAndNotify(ref _errorText, value); }

        public PreferencesDialog()
        {
            InitializeComponent();
            Inst = this;
            DataContext = this;

            InitAllControls();
        }

        private void InitAllControls()
        {
            StorageFolderPathTextBox.Text = "D:\\WorkSpaceFor_Y3\\NT219 - Cryptography\\Project-Cryptography-NT219\\File Location\\Encrypted";
        }

        private bool IsAccessPasswordChanged()
        {
            var currentPassword = CurrentPasswordBox.Password.Trim();
            var newPassword = NewPasswordBox.Password.Trim();
            var confirmPassword = ConfirmPasswordBox.Password.Trim();

            if (string.IsNullOrWhiteSpace(currentPassword) && string.IsNullOrWhiteSpace(newPassword) && string.IsNullOrWhiteSpace(confirmPassword))
            {
                ErrorText = ProtectedResourceLoader.GetString("PasswordIsNotEmpty");
                return false;
            }

            if (currentPassword == newPassword)
                return false;

            if (newPassword != confirmPassword)
            {
                ErrorText = ProtectedResourceLoader.GetString("PasswordAndConfirmationPasswordDoNotMatch");
                return false;
            }

            if (currentPassword != AppProfile.Inst.Password)
            {
                ErrorText = ProtectedResourceLoader.GetString("IncorrectPassword");
                return false;
            }

            return true;
        }

        private async void ActionButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Control control) return;

            if (control == ResetStoragePathButton)
                StorageFolderPathTextBox.Text = AppProfile.DEFAULT_STORAGE_LOCATION;
            else if (control == BrowseStoragePathButton)
            {
                try
                {
                    var storageFolder = await App.CurrentWindow.PickSingleFolderAsync();

                    if (storageFolder != null)
                    {
                        if (!Utils.IsDirectoryEmpty(storageFolder.Path))
                            throw new(ProtectedResourceLoader.GetString("Features_SelectEmptyFolderMessage"));

                        StorageFolderPathTextBox.Text = storageFolder.Path;
                    }
                }
                catch (Exception ex)
                {
                    App.CurrentWindow.ShowMessageTeachingTip(null, ex.Message);
                }
            }
            else if (control == CopyStoragePathButton)
            {
                Utils.SetTextToClipboard(StorageFolderPathTextBox.Text, false);
                App.CurrentWindow.ShowMessageTeachingTip(null, ProtectedResourceLoader.GetString("Copied"), StorageFolderPathTextBox.Text);
            }
            else if (control == ChangeStoragePathButton)
            {
                var newStoragePath = StorageFolderPathTextBox.Text.Trim();
                if (newStoragePath == AppProfile.Inst.StorageLocation) return;

                AppProfile.Inst.StorageLocation = newStoragePath;
                Share.EnsureDirs();

                var items = Main.Inst.SourceFileItems.ToList();

                IProgress<int> itemProgress = new Progress<int>(index =>
                {
                    App.CurrentWindow.Backdrop.ShowLoading(true, $"{index + 1}/{items.Count}");
                });

                IProgress<bool> progress = new Progress<bool>(_ =>
                {
                    App.CurrentWindow.Backdrop.ShowLoading(false);
                });

                App.CurrentWindow.Backdrop.ShowLoading(true);
                Hide();

                _ = Task.Run(() =>
                {
                    if (newStoragePath == AppProfile.DEFAULT_STORAGE_LOCATION)
                    {
                        Utils.DeleteFileOrDirectory(newStoragePath);
                        Share.EnsureDirs();
                    }

                    for (int i = 0; i < items.Count; i++)
                    {
                        itemProgress.Report(i);

                        var item = items[i];

                        try
                        {
                            var oldEncryptedPath = item.EncryptedFileOrFolderPath;
                            var newEncryptedPath = Path.Combine(AppProfile.Inst.AppEncryptedLocation, item.EncryptedFileOrFolderName);

                            lock (DBManager.Inst.Locker)
                            {
                                var file = AppDbContext.Inst.Files.FirstOrDefault(i => i.Path == item.EncryptedFileOrFolderPath);
                                if (file != null)
                                {
                                    file.Path = newEncryptedPath;
                                    DBManager.Inst.MainDbContext.SaveChanges();
                                }
                            }

                            File.Copy(item.EncryptedFileOrFolderPath, newEncryptedPath, true);
                            item.EncryptedInfo = new(newEncryptedPath);
                            item.EncryptedInfo.LoadInfo();

                            File.Delete(oldEncryptedPath);
                        }
                        catch { }
                    }

                    progress.Report(true);
                });
            }
            else if (control == ChangePasswordButton)
            {
                if (!IsAccessPasswordChanged()) return;

                var items = Main.Inst.SourceFileItems.ToList();

                IProgress<int> itemProgress = new Progress<int>(index =>
                {
                    App.CurrentWindow.Backdrop.ShowLoading(true, $"{index + 1}/{items.Count}");
                });

                IProgress<Tuple<bool, string>> progress = new Progress<Tuple<bool, string>>(result =>
                {
                    if (result.Item1)
                    {
                        ErrorText = "*";
                        AppProfile.Inst.NotifyAll();
                    }
                    else
                    {
                        if (!string.IsNullOrWhiteSpace(result.Item2))
                            ErrorText = ProtectedResourceLoader.GetString(result.Item2);
                    }

                    App.CurrentWindow.Backdrop.ShowLoading(false);
                    Dialog(App.CurrentWindow.Content.XamlRoot);
                });

                var currentPassword = AppProfile.Inst.Password;
                var newPassword = NewPasswordBox.Password.Trim();

                App.CurrentWindow.ShowConfirmTeachingTip(
                    null,
                    ProtectedResourceLoader.GetString("Features_ApplyPreferenceTitle"),
                    ProtectedResourceLoader.GetString("Features_ApplyPreferenceSubtitle"),
                    () =>
                    {
                        App.CurrentWindow.Backdrop.ShowLoading(true);
                        Hide();

                        _ = Task.Run(() =>
                        {
                            try
                            {
                                AppProfile.Inst.Password = newPassword;

                                for (int i = 0; i < items.Count; i++)
                                {
                                    itemProgress.Report(i);

                                    try
                                    {
                                        DecodeOne(items[i], currentPassword);
                                        EncodeOne(items[i], newPassword);
                                    }
                                    catch { }
                                }

                                progress.Report(Tuple.Create(true, string.Empty));
                            }
                            catch (Exception ex)
                            {
                                progress.Report(Tuple.Create(false, ex.Message));
                            }
                        });
                    }
                );
            }
        }

        public void EncodeOne(FileItem item, string password)
        {
            if (!Utils.IsExistFileOrDirectory(item.TempFileOrFolderPath))
                throw new IOException();

            var outputTempFilePath = Utils.GetTempFilePath(null);
            var outputZipFilePath = Utils.GetTempFilePath(null);

            try
            {
                if (item.FileType == FileUtils.Type.Directory)
                {
                    ZipFile.CreateFromDirectory(item.TempFileOrFolderPath, outputZipFilePath, CompressionLevel.NoCompression, false);
                    CryptographyUtils.EncryptFile(outputZipFilePath, outputTempFilePath, password, CryptographyUtils.SALT, CryptographyUtils.ITERATIONS);

                    Utils.DeleteFileOrDirectory(outputZipFilePath);
                }
                else
                    CryptographyUtils.EncryptFile(item.TempFileOrFolderPath, outputTempFilePath, password, CryptographyUtils.SALT, CryptographyUtils.ITERATIONS);

                item.TempInfo.LoadInfo();
                item.TempInfo.LoadMediaAnalysis();
                item.TempInfo.LoadFormatAndDimentionInfo();

                item.Footer.FooterMetadata.AppendToFile(outputTempFilePath, item.OriginalThumbnail?.ToByteArray() ?? null);

                //

                var outputFilePath = Path.Combine(AppProfile.Inst.AppEncryptedLocation, Guid.NewGuid().ToString());
                File.Move(outputTempFilePath, outputFilePath, true);

                lock (DBManager.Inst.Locker)
                {
                    var file = AppDbContext.Inst.Files.FirstOrDefault(i => i.Path == item.EncryptedFileOrFolderPath);
                    if (file != null)
                    {
                        file.Path = outputFilePath;
                        AppDbContext.Inst.SaveChanges();
                    }
                }

                item.EncryptedInfo = new(outputFilePath);
                item.EncryptedInfo.LoadInfo();

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
            if (!Utils.IsExistFileOrDirectory(item.EncryptedFileOrFolderPath))
                throw new IOException();

            var outputFolderPath = AppProfile.Inst.AppTempLocation;

            var outputRawTempFilePath = Utils.GetTempFilePath(outputFolderPath);
            var outputTempFilePath = Utils.GetTempFilePath(outputFolderPath);

            try
            {
                Directory.CreateDirectory(outputFolderPath);

                File.Copy(item.EncryptedFileOrFolderPath, outputRawTempFilePath);

                Utils.RemoveLatestBytesFromFile(outputRawTempFilePath, item.Footer.GetSize());
                CryptographyUtils.DecryptFile(outputRawTempFilePath, outputTempFilePath, password, CryptographyUtils.SALT, CryptographyUtils.ITERATIONS);

                Utils.DeleteFileOrDirectory(outputRawTempFilePath);

                if (item.Footer.FooterMetadata.FileType == FileUtils.Type.Directory)
                {
                    var outputFilePath = Path.Combine(outputFolderPath, Path.GetFileName(outputTempFilePath));
                    item.TempInfo = new(outputFilePath);

                    ZipFile.ExtractToDirectory(outputTempFilePath, outputFilePath, true);
                }
                else
                {
                    var outputFilePath = Path.Combine(outputFolderPath, Path.GetFileName(item.Footer.FooterMetadata.OriginalName));
                    item.TempInfo = new(outputFilePath);

                    File.Move(outputTempFilePath, outputFilePath, true);
                }

                Utils.DeleteFileOrDirectory(outputTempFilePath);
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

        private void EnableAllControls(bool isEnabled)
        {
            CurrentPasswordBox.IsEnabled = isEnabled;
            NewPasswordBox.IsEnabled = isEnabled;
            ConfirmPasswordBox.IsEnabled = isEnabled;

            StorageFolderPathTextBox.IsEnabled = isEnabled;

            ChangePasswordButton.IsEnabled = isEnabled;
            ChangeStoragePathButton.IsEnabled = isEnabled;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Hide();
        }
    }
}