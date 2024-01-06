using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Newtonsoft.Json;
using IOCore.Libs;

namespace IOApp.Features
{
    public class Footer
    {
        public static readonly string SIGNATURE = "SE400ProjectCryptography";

        public class Extras
        {
            public TimeSpan Duration { get; set; }
            public TimeSpan StartTime { get; set; }
            public string FormatName { get; set; }
            public string FormatLongName { get; set; }
            public int StreamCount { get; set; }
            public double ProbeScore { get; set; }
            public double BitRate { get; set; }
            public Dictionary<string, string> Tags { get; set; }

            public int Width { get; set; }
            public int Height { get; set; }

            public Extras(FFMpegCore.MediaFormat format, int width, int height)
            {
                Width = width;
                Height = height;

                if (format != null)
                {
                    Duration = format.Duration;
                    StartTime = format.StartTime;
                    FormatName = format.FormatName;
                    FormatLongName = format.FormatLongName;
                    StreamCount = format.StreamCount;
                    ProbeScore = format.ProbeScore;
                    BitRate = format.BitRate;
                    Tags = format.Tags;
                }
            }

            public new string ToString() => JsonConvert.SerializeObject(this);
        }

        public class Metadata
        {
            public int Version = 1;
            public string Platform = "Windows";
            public bool IsFile;
            public FileUtils.Type FileType;
            public string OriginalName;
            public int ThumbnailSize = 0;
            public string Extras;

            public Metadata(FileUtils.Type fileType, string originalName, Extras extra)
            {
                IsFile = fileType != FileUtils.Type.Directory;
                FileType = fileType;
                OriginalName = originalName;
                Extras = (extra ?? new(null, 0, 0)).ToString();
            }

            public void AppendToFile(string filePath, byte[] thumbnailBytes)
            {
                if (thumbnailBytes != null) ThumbnailSize = thumbnailBytes.Length;
                else ThumbnailSize = 0;

                using var fs = new FileStream(filePath, FileMode.Append);
                using var bw = new BinaryWriter(fs);

                var footerMetadataBuffer = Utils.GetJsonByteArrayFromObject(this);

                if (thumbnailBytes != null)
                    bw.Write(thumbnailBytes);

                bw.Write(footerMetadataBuffer);
                bw.Write(BitConverter.GetBytes(footerMetadataBuffer.Length));
                bw.Write(Encoding.UTF8.GetBytes(SIGNATURE));
            }
        }

        public byte[] Signature = Encoding.UTF8.GetBytes(SIGNATURE);
        public int MetaSize;
        public Metadata FooterMetadata;
        public Extras FooterExtra { get; set; }

        [JsonIgnore]
        public byte[] MetaSizeBuffer { get; private set; }

        [JsonIgnore]
        public byte[] FooterMetadataBuffer { get; private set; }

        [JsonIgnore]
        public byte[] FooterThumbnailBuffer { get; private set; }

        public Footer() { }

        private bool LoadSignatureFromFile(FileStream fs)
        {
            fs.Seek(Math.Max(0L, fs.Length - Signature.Length), SeekOrigin.Begin);
            fs.Read(Signature, 0, Signature.Length);

            return SIGNATURE == Encoding.UTF8.GetString(Signature);
        }

        public bool LoadAppIdFromFile(string path)
        {
            using var fs = new FileStream(path, FileMode.Open);
            return LoadSignatureFromFile(fs);
        }

        private bool LoadMetaSizeFromFile(FileStream fs)
        {
            MetaSizeBuffer = new byte[Marshal.SizeOf(MetaSize)];

            fs.Seek(Math.Max(0L, fs.Length - Signature.Length - MetaSizeBuffer.Length), SeekOrigin.Begin);
            fs.Read(MetaSizeBuffer, 0, MetaSizeBuffer.Length);

            MetaSize = BitConverter.ToInt32(MetaSizeBuffer);

            return MetaSize > 0;
        }

        public bool LoadMetaSizeFromFile(string path)
        {
            using var fs = new FileStream(path, FileMode.Open);
            return LoadMetaSizeFromFile(fs);
        }

        private bool LoadMetadataFromFile(FileStream fs)
        {
            FooterMetadataBuffer = new byte[MetaSize];

            fs.Seek(Math.Max(0L, fs.Length - Signature.Length - MetaSizeBuffer.Length - FooterMetadataBuffer.Length), SeekOrigin.Begin);
            var byteRead = fs.Read(FooterMetadataBuffer, 0, FooterMetadataBuffer.Length);

            if (byteRead != FooterMetadataBuffer.Length)
                return false;

            FooterMetadata = JsonConvert.DeserializeObject<Metadata>(Encoding.UTF8.GetString(FooterMetadataBuffer));
            FooterExtra = JsonConvert.DeserializeObject<Extras>(FooterMetadata.Extras);

            return FooterMetadata != null;
        }

        public bool LoadMetadataFromFile(string path)
        {
            using var fs = new FileStream(path, FileMode.Open);
            return LoadMetadataFromFile(fs);
        }

        private bool LoadThumbnailFromFile(FileStream fs)
        {
            if (FooterMetadata.ThumbnailSize == 0)
                return true;

            FooterThumbnailBuffer = new byte[FooterMetadata.ThumbnailSize];

            fs.Seek(Math.Max(0L, fs.Length - Signature.Length - MetaSizeBuffer.Length - FooterMetadataBuffer.Length - FooterThumbnailBuffer.Length), SeekOrigin.Begin);
            var byteRead = fs.Read(FooterThumbnailBuffer, 0, FooterThumbnailBuffer.Length);

            if (byteRead != FooterThumbnailBuffer.Length)
                return false;

            return FooterThumbnailBuffer != null;
        }

        public bool LoadFromFile(string path)
        {
            try
            {
                if (string.IsNullOrEmpty(path)) return false;

                using var fs = new FileStream(path, FileMode.Open);

                if (!LoadSignatureFromFile(fs))
                    return false;

                if (!LoadMetaSizeFromFile(fs))
                    return false;

                if (!LoadMetadataFromFile(fs))
                    return false;

                if (!LoadThumbnailFromFile(fs))
                    return false;

                return true;
            }
            catch { return false; }
        }

        public static bool IsLockedFile(string path) => CreateFromFile(path) != null;

        public long GetSize() => Signature.Length + MetaSizeBuffer.Length + FooterMetadataBuffer.Length + (FooterThumbnailBuffer?.Length ?? 0);

        public static Footer CreateFromFile(string path)
        {
            var footer = new Footer();
            if (footer.LoadFromFile(path))
                return footer;
            return null;
        }
    }
}