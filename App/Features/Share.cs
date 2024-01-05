using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.IO.Compression;
using IOCore.Libs;
using IOApp.Configs;

namespace IOApp.Features
{
    internal class Share
    {
        public enum StatusType
        {
            Ready,
            Loading,
            Loaded,
            LoadFailed,
            ProcessInQueue,
            ProcessStart,
            ProcessingOne,
            ProcessingAll,
            Processed,
            ProcessFailed,
            ProcessPausing,
            ProcessPaused,
            ProcessStopping,
            ProcessStopped
        };

        public static readonly Dictionary<StatusType, string> STATUSES = new()
        {
            { StatusType.Ready, "StatusReady" },
            { StatusType.Loading, "StatusLoading" },
            { StatusType.Loaded, "StatusLoaded" },
            { StatusType.LoadFailed, "StatusLoadFailed" },
            { StatusType.ProcessingOne, "StatusProcessingOne" },
            { StatusType.ProcessingAll, "StatusProcessingAll" },
            { StatusType.Processed, "StatusProcessed" },
            { StatusType.ProcessFailed, "StatusProcessFailed" },
            { StatusType.ProcessPausing, "StatusProcessPausing" },
            { StatusType.ProcessPaused, "StatusProcessPaused" },
            { StatusType.ProcessStopping, "StatusProcessStopping" },
            { StatusType.ProcessStopped, "StatusProcessStopped" },
        };

        public struct Argv
        {
            public string OutputFolderPath;
            public bool ExportToOriginalFolder;
            public bool OverwriteExistingOutputFiles;
            public AppTypes.ExportType ExportType;
        }

        public static void EncodeOne(FileItem item)
        {
            if (!Utils.IsExistFileOrDirectory(item.InputFileOrFolderPath))
                throw new IOException();

            string outputTempFilePath = Utils.GetTempFilePath(AppProfile.Inst.AppTempLocation);
            string outputZipFilePath = Utils.GetTempFilePath(AppProfile.Inst.AppTempLocation);

            try
            {
                if (item.FileType == FileUtils.Type.Directory)
                {
                    ZipFile.CreateFromDirectory(item.InputFileOrFolderPath, outputZipFilePath, CompressionLevel.NoCompression, false);
                    CryptographyUtils.EncryptFile(outputZipFilePath, outputTempFilePath, AppProfile.Inst.Password, CryptographyUtils.SALT, CryptographyUtils.ITERATIONS);

                    Utils.DeleteFileOrDirectory(outputZipFilePath);
                }
                else
                    CryptographyUtils.EncryptFile(item.InputFileOrFolderPath, outputTempFilePath, AppProfile.Inst.Password, CryptographyUtils.SALT, CryptographyUtils.ITERATIONS);

                item.InputInfo = new(item.InputFileOrFolderPath);
                item.InputInfo.LoadInfo();
                item.InputInfo.LoadMediaAnalysis();
                item.InputInfo.LoadFormatAndDimentionInfo();

                Footer.Metadata footerMetadata = new(item.FileType, item.OriginalFileOrFolderPath,
                    item.FileType switch
                    {
                        FileUtils.Type.Video or FileUtils.Type.Audio or FileUtils.Type.Image => new(item.InputInfo?.MediaAnalysis?.Format ?? null, item.InputInfo?.Width ?? 0, item.InputInfo?.Height ?? 0),
                        _ => null,

                    });

                footerMetadata.AppendToFile(outputTempFilePath, item.OriginalThumbnail?.ToByteArray() ?? null);

                //

                string outputFilePath = Path.Combine(AppProfile.Inst.AppEncryptedLocation, Guid.NewGuid().ToString());
                File.Move(outputTempFilePath, outputFilePath, true);

                item.EncryptedInfo = new(outputFilePath);

                Utils.DeleteFileOrDirectory(item.InputFileOrFolderPath);
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

        public static void DecodeOne(FileItem item)
        {
            if (!Utils.IsExistFileOrDirectory(item.EncryptedFileOrFolderPath))
                throw new IOException();

            var outputFolderPath = AppProfile.Inst.AppRecoveryLocation;

            var outputRawTempFilePath = Path.GetTempFileName();
            var outputTempFilePath = Path.GetTempFileName();

            Utils.DeleteFileOrDirectory(outputRawTempFilePath);
            Utils.DeleteFileOrDirectory(outputTempFilePath);

            try
            {
                var footer = Footer.CreateFromFile(item.EncryptedFileOrFolderPath) ?? throw new();
                Utils.CreateDirectoryIfNotExist(outputFolderPath);

                File.Copy(item.EncryptedFileOrFolderPath, outputRawTempFilePath);

                Utils.RemoveLatestBytesFromFile(outputRawTempFilePath, footer.GetSize());
                CryptographyUtils.DecryptFile(outputRawTempFilePath, outputTempFilePath, AppProfile.Inst.Password, CryptographyUtils.SALT, CryptographyUtils.ITERATIONS);

                Utils.DeleteFileOrDirectory(outputRawTempFilePath);

                if (footer.FooterMetadata.FileType == FileUtils.Type.Directory)
                {
                    var outputFilePath = Path.Combine(outputFolderPath, Path.GetFileName(footer.FooterMetadata.OriginalName));
                    item.RecoveredInfo = new(outputFilePath);

                    ZipFile.ExtractToDirectory(outputTempFilePath, outputFilePath, true);
                }
                else
                {
                    var outputFilePath = Path.Combine(outputFolderPath, Path.GetFileName(footer.FooterMetadata.OriginalName));
                    item.RecoveredInfo = new(outputFilePath);

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

        public static void ExportFilesOrFolders(
            IEnumerable<FileItem> fileItems, Argv argv,
            Action startAction = null,
            Action<StatusType> endAction = null,
            Action<IEnumerable<FileItem>> itemsAction = null,
            Action<Tuple<FileItem, FileSystemItem.StatusType, Exception>> itemAction = null)
        {
            startAction?.Invoke();

            var locker = new object();
            var coreCount = Environment.ProcessorCount;
            var pathCount = fileItems.Count();

            IProgress<StatusType> endProgress = new Progress<StatusType>(_ =>
            {
                endAction?.Invoke(_);
            });

            IProgress<List<FileItem>> itemsProgress = new Progress<List<FileItem>>(items =>
            {
                lock (locker)
                {
                    itemsAction?.Invoke(items);
                }
            });

            _ = Task.Run(() =>
            {
                try
                {
                    var packages = fileItems.Chunk(GlobalConstants.PACKAGE_SIZE);
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
                            Parallel.ForEach(chunk, item =>
                            {
                                try
                                {
                                    if (argv.ExportToOriginalFolder)
                                        argv.OutputFolderPath = item.OriginalFolderPath;

                                    itemAction?.Invoke(Tuple.Create(item, FileSystemItem.StatusType.Processing, new Exception()));

                                    var outputFileOrFolderPath = Path.Combine(argv.OutputFolderPath, Path.GetFileName(item.OriginalFileOrFolderPath));

                                    if (!argv.OverwriteExistingOutputFiles)
                                    {
                                        if (item.FileType == FileUtils.Type.Directory)
                                            outputFileOrFolderPath = Utils.NextAvailableFolderNameAdvanced(outputFileOrFolderPath);
                                        else
                                            outputFileOrFolderPath = Utils.NextAvailableFileNameAdvanced(outputFileOrFolderPath);
                                    }

                                    if (argv.ExportType == AppTypes.ExportType.ExportLockedCopy)
                                        File.Copy(item.EncryptedFileOrFolderPath, outputFileOrFolderPath, argv.OverwriteExistingOutputFiles);
                                    else
                                    {
                                        DecodeOne(item);
                                        item.RecoveredInfo.LoadInfo();

                                        if (item.FileType == FileUtils.Type.Directory)
                                            Directory.Move(item.RecoveredFileOrFolderPath, outputFileOrFolderPath);
                                        else
                                            File.Move(item.RecoveredFileOrFolderPath, outputFileOrFolderPath, argv.OverwriteExistingOutputFiles);

                                        if (argv.ExportType == AppTypes.ExportType.Export)
                                            item.Destroy();
                                    }

                                    itemAction?.Invoke(Tuple.Create(item, FileSystemItem.StatusType.Processed, new Exception()));

                                    lock (locker)
                                    {
                                        items.Add(item);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    itemAction?.Invoke(Tuple.Create(item, FileSystemItem.StatusType.ProcessFailed, ex));
                                }
                            });
                        }

                        itemsProgress.Report(items);
                    }

                    endProgress.Report(StatusType.Processed);
                }
                catch
                {
                    endProgress.Report(StatusType.ProcessFailed);
                }
            });
        }

        public static void EnsureDirs()
        {
            Utils.CreateDirectoryIfNotExist(AppProfile.Inst.AppEncryptedLocation);
            Utils.CreateDirectoryIfNotExist(AppProfile.Inst.AppRecoveryLocation);
            Utils.CreateDirectoryIfNotExist(AppProfile.Inst.AppTempLocation);
        }

        public static string GetSlugByMediaFormat(FileItem item)
        {
            if (item == null) return null;

            var extension = item.OriginalFileExtension.ToLowerInvariant();

            foreach (var i in Profile.PROMOTION_MEDIA_FORMATS)
            {
                if (MediaUtils.MEDIA_FORMATS[i.Key].Extensions.Contains(extension))
                    return i.Value;
            }

            return null;
        }

        public static string GetSlugByImageFormat(FileItem item)
        {
            if (item == null) return null;

            var extension = item.OriginalFileExtension.ToLowerInvariant();

            foreach (var i in Profile.PROMOTION_IMAGE_FORMATS)
            {
                if (ImageUtils.IMAGE_FORMATS[i.Key].Extensions.Contains(extension))
                    return i.Value;
            }

            return null;
        }

        //public static string GetSlugByExtension(string extension)
        //{
        //    if (string.IsNullOrWhiteSpace(extension)) return null;

        //    extension = extension.ToLowerInvariant();

        //    foreach (var i in Profile.INPUT_EXTENSIONS)
        //        if (i.Contains(extension))
        //            return i;

        //    return null;
        //}
    }
}