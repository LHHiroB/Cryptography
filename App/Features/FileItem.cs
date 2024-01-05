using System.IO;
using IOCore.Libs;
using System;
using Microsoft.UI.Xaml.Media.Imaging;
using ImageMagick;
using System.Threading.Tasks;
using FFMpegCore;
using System.Linq;

namespace IOApp.Features
{
    internal class FileItem : FileSystemItem
    {
        protected PrivacyType _privacy;
        public PrivacyType Privacy { get => _privacy; set => SetAndNotify(ref _privacy, value); }

        protected Footer _footer;
        public Footer Footer => _footer;

        protected MagickImage _originalThumbnail;
        public MagickImage OriginalThumbnail => _originalThumbnail;

        protected BitmapImage _thumbnail;
        public BitmapImage Thumbnail { get => _thumbnail; set => SetAndNotify(ref _thumbnail, value); }

        private int _width;
        public int Width { get => _width; set => SetAndNotify(ref _width, value); }

        private int _height;
        public int Height { get => _height; set => SetAndNotify(ref _height, value); }

        public string Resolution => $"{Width}x{Height}";

        //

        protected string _originalFileOrFolderPath;
        public string OriginalFileOrFolderPath { get => _originalFileOrFolderPath; set => SetAndNotify(ref _originalFileOrFolderPath, value); }

        public string OriginalFileExtension => Path.GetExtension(OriginalFileOrFolderPath);
        public string OriginalFileOrFolderName => Path.GetFileNameWithoutExtension(OriginalFileOrFolderPath);
        public string OriginalFolderPath => Path.GetDirectoryName(OriginalFileOrFolderPath);

        //

        public FileSystemInfoProxy EncryptedInfo { get; set; }

        public string EncryptedFileOrFolderPath => EncryptedInfo?.Path ?? string.Empty;
        public string EncryptedFileExtension => Path.GetExtension(EncryptedFileOrFolderPath);
        public string EncryptedFileOrFolderName => Path.GetFileNameWithoutExtension(EncryptedFileOrFolderPath);
        public string EncryptedFolderPath => Path.GetDirectoryName(EncryptedFileOrFolderPath);

        public long EncryptedSize
        {
            get
            {
                if (EncryptedInfo?.FileSystemInfo is FileInfo fileInfo)
                    return fileInfo.Length;
                return 0;
            }
        }

        public string EncryptedCreationTimeText => EncryptedInfo?.FileSystemInfo?.CreationTime.ToString("G") ?? "∞";
        public string EncryptedLastWriteTimeText => EncryptedInfo?.FileSystemInfo?.LastWriteTime.ToString("G") ?? "∞";

        //

        public FileSystemInfoProxy TempInfo { get; set; }

        public string TempFileOrFolderPath => TempInfo?.Path ?? string.Empty;

        //

        public FileSystemInfoProxy RecoveredInfo { get; set; }

        public string RecoveredFileOrFolderPath => RecoveredInfo?.Path ?? string.Empty;
        public string RecoveredFileExtension => Path.GetExtension(RecoveredFileOrFolderPath);
        public string RecoveredFileOrFolderName => Path.GetFileName(RecoveredFileOrFolderPath);
        public string RecoveredFolderPath => Path.GetDirectoryName(RecoveredFileOrFolderPath);

        public string StorageMimeType => RecoveredInfo?.MimeType;

        public FileItem(string path) : base(path)
        {
            _originalFileOrFolderPath = path;
        }

        public FileItem(FileEntity file) : base(file)
        {
            EncryptedInfo = new(file.Path);
            EncryptedInfo.LoadInfo();

            _fileType = file.FileType;
            _privacy = file.PrivacyType;
        }

        public void SetFileType(FileUtils.Type fileType)
        {
            _fileType = fileType;
        }

        public void SetIsCorrupted(bool isCorrupted)
        {
            _isCorrupted = isCorrupted;
        }

        public void TogglePrivate(bool? blur)
        {
            if (blur == null)
                Privacy = _privacy == PrivacyType.None ? PrivacyType.Blur : PrivacyType.None;
            else
                Privacy = blur.GetValueOrDefault(false) ? PrivacyType.Blur : PrivacyType.None;

            RenderThumbnailTask(null);

            Notify((nameof(OriginalFileOrFolderName)));
        }

        public void LoadEncryptedBasicInfo()
        {
            EncryptedInfo?.LoadInfo();

            Notify(nameof(EncryptedFileOrFolderPath));

            Notify(nameof(EncryptedFileExtension));
            Notify(nameof(EncryptedFileOrFolderName));
            Notify(nameof(EncryptedFolderPath));

            Notify(nameof(EncryptedSize));
        }

        public void LoadRecoveredBasicInfo()
        {
            RecoveredInfo?.LoadInfo();
            RecoveredInfo.LoadFormatAndDimentionInfo();

            IsExist = RecoveredInfo.FileSystemInfo != null;

            Notify(nameof(RecoveredFileOrFolderPath));

            Notify(nameof(RecoveredFileExtension));
            Notify(nameof(RecoveredFileOrFolderName));
            Notify(nameof(RecoveredFolderPath));

            Notify(nameof(StorageMimeType));
        }

        public void ClearRecovered()
        {
            Utils.DeleteFileOrDirectory(RecoveredFileOrFolderPath);
        }

        public void Destroy()
        {
            lock (DBManager.Inst.Locker)
            {
                var item = AppDbContext.Inst.Files.FirstOrDefault(i => i.Path == EncryptedFileOrFolderPath);
                if (item != null)
                {
                    AppDbContext.Inst.Files.Remove(item);
                    AppDbContext.Inst.SaveChanges();
                }
            }

            Utils.DeleteFileOrDirectory(EncryptedFileOrFolderPath);
            Utils.DeleteFileOrDirectory(RecoveredFileOrFolderPath);
            Utils.DeleteFileOrDirectory(TempFileOrFolderPath);
        }

        public void MoveRecoveredPathToOutputPath()
        {
            File.Move(RecoveredFileOrFolderPath, OutputFileOrFolderPath);
        }

        public bool TryLoadFooter(string path)
        {
            Footer footer = Footer.CreateFromFile(path);

            if (footer != null)
            {
                _footer = footer;
                _isExist = new FileInfo(path) != null;

                _originalFileOrFolderPath = new(_footer.FooterMetadata.OriginalName);

                _fileType = _footer.FooterMetadata.FileType;

                if (_footer.FooterMetadata.ThumbnailSize > 0)
                {
                    _width = _footer.FooterExtra.Width;
                    _height = _footer.FooterExtra.Height;

                    _originalThumbnail = new(_footer.FooterThumbnailBuffer);
                }
            }

            return footer != null;
        }

        public void PrepareThumbnail(string path, int maxWidth, int maxHeight)
        {
            if (_originalThumbnail != null) return;

            var tempPath = Utils.GetTempFilePath(AppProfile.Inst.AppTempLocation);
            var tempPathPng = $"{tempPath}.png";

            try
            {
                if (FileType == FileUtils.Type.Video || FileType == FileUtils.Type.Audio)
                {
                    var mediaAnalysis = FFMpegUtils.GetMediaAnalysis(path);

                    var rotation = mediaAnalysis?.PrimaryVideoStream?.Rotation ?? 0;
                    var width = mediaAnalysis?.PrimaryVideoStream?.Width ?? 1;
                    var height = mediaAnalysis?.PrimaryVideoStream?.Height ?? 1;

                    var correctWidth = rotation is (-90) or 90 ? height : width;
                    var correctHeight = rotation is (-90) or 90 ? width : height;

                    var size = Utils.GetMaxContainSize(correctWidth, correctHeight, maxWidth, maxHeight);

                    var captureTime = TimeSpan.FromMilliseconds(Math.Min((
                        mediaAnalysis?.Duration ?? TimeSpan.FromSeconds(0)).TotalMilliseconds,
                        FileType == FileUtils.Type.Video ? 1250 : 0));

                    if (!FFMpeg.Snapshot(path, tempPathPng, size, captureTime))
                        throw new Exception();
                }
                else if (FileType == FileUtils.Type.Image)
                {
                    using MagickImage image = new(path);
                    var size = Utils.GetMaxContainSize(image.Width, image.Height, maxWidth, maxHeight);

                    image.Resize(size.Width, size.Height);
                    image.Write(tempPathPng);
                }

                _originalThumbnail = new(tempPathPng, new MagickReadSettings()
                {
                    BackgroundColor = MagickColors.Black,
                    ColorSpace = ColorSpace.sRGB,
                });
            }
            catch
            {
            }
            finally
            {
                Utils.DeleteFileOrDirectory(tempPath);
                Utils.DeleteFileOrDirectory(tempPathPng);
            }
        }

        public Task LoadThumbnailIfNotExistTask(Action<bool> action)
        {
            if (_originalThumbnail == null)
                return null;

            var memoryStream = new MemoryStream();

            IProgress<bool> progress = new Progress<bool>(result =>
            {
                if (result)
                {
                    var thumbnail = new BitmapImage();
                    thumbnail.SetSource(memoryStream.AsRandomAccessStream());
                    Thumbnail = thumbnail;
                }

                memoryStream.Dispose();

                action?.Invoke(result);
            });

            return Task.Run(() =>
            {
                try
                {
                    RenderThumbnail(memoryStream);
                    progress.Report(true);
                }
                catch
                {
                    progress.Report(false);
                }
            });
        }

        public Task RenderThumbnailTask(Action<bool> action)
        {
            var memoryStream = new MemoryStream();

            IProgress<bool> process = new Progress<bool>(result =>
            {
                if (result)
                {
                    var thumbnail = new BitmapImage();
                    thumbnail.SetSource(memoryStream.AsRandomAccessStream());
                    Thumbnail = thumbnail;
                }

                memoryStream.Dispose();

                action?.Invoke(result);
            });

            return Task.Run(() =>
            {
                RenderThumbnail(memoryStream);
                process.Report(true);
            });
        }

        protected void RenderThumbnail(MemoryStream memoryStream)
        {
            if (_originalThumbnail == null) return;

            var image = _originalThumbnail.Clone() as MagickImage;

            if (_privacy == PrivacyType.Blur)
                image.Blur(0, 24);

            image.Write(memoryStream, MagickFormat.Bmp);
            memoryStream.Position = 0;
            image.Dispose();
        }
    }
}