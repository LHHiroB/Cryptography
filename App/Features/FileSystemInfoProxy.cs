using FFMpegCore;
using ImageMagick;
using System.IO;
using IOCore.Libs;

namespace IOApp.Features
{
    public class FileSystemInfoProxy
    {
        public string Path { get; set; }
        public FileSystemInfo FileSystemInfo { get; set; }

        //

        public IMediaAnalysis MediaAnalysis { get; private set; }

        public MagickFormat Format { get; set; }
        public int? Width { get; set; }
        public int? Height { get; set; }

        public string MimeType { get; set; }

        //

        public bool IsAnalyzed { get; private set; }

        public FileSystemInfoProxy(string path, bool load = false)
        {
            Path = path;
            if (load) LoadInfo();
        }

        public void LoadInfo()
        {
            if (!Utils.IsExistFileOrDirectory(Path))
                FileSystemInfo = null;
            else
                FileSystemInfo = Utils.IsFilePath(Path).GetValueOrDefault(false) ? new FileInfo(Path) : new DirectoryInfo(Path);
        }

        //

        public void LoadMediaAnalysis()
        {
            if (Path != null && !IsAnalyzed && MediaAnalysis == null)
            {
                IsAnalyzed = true;
                MediaAnalysis = FFMpegUtils.GetMediaAnalysis(Path);
            }
        }

        public void LoadFormatAndDimentionInfo()
        {
            try
            {
                var imageMeta = ImageMagickUtils.GetMagickImageMeta(Path);

                Format = imageMeta.FormatInfo.Format;
                MimeType = imageMeta.FormatInfo.MimeType;

                Width = imageMeta.Width;
                Height = imageMeta.Height;
            }
            catch { }
        }

        public string GetTag(string tag) => (MediaAnalysis?.Format?.Tags.TryGetValue(tag, out string value) ?? false) ? value : null;
    }
}