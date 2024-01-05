using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace IOCore.Libs
{
    public class FileFormat
    {
        public string Name { get; private set; }
        public FileUtils.Type Type { get; private set; }

        public bool IsSupported { get; set; }
        public object Extra { get; set; }

        public FileFormat(string name, FileUtils.Type type)
        {
            Name = name;
            Type = type;
        }
    }

    public class FileUtils
    {
        public enum Type
        {
            Unknown,
            Image,
            Video,
            Audio,
            Document,
            Directory,
        }

        public static readonly Dictionary<Type, string[]> EXTENSIONS;

        static FileUtils()
        {
            EXTENSIONS = new()
            {
                { Type.Image, ImageUtils.IMAGE_FORMATS.Select(i => i.Value.Extensions).SelectMany(i => i).Distinct().ToArray() },
                { Type.Video, MediaUtils.VIDEO_FORMATS.Select(i => i.Value.Extensions).SelectMany(i => i).Distinct().ToArray() },
                { Type.Audio, MediaUtils.AUDIO_FORMATS.Select(i => i.Value.Extensions).SelectMany(i => i).Distinct().ToArray() },
                { Type.Document, DocumentUtils.DOCUMENT_FORMATS.Select(i => i.Value.Extensions).SelectMany(i => i).Distinct().ToArray() },
            };

        }
        public static ImageFormat CreateFormat(ImageUtils.FormatFamily format, bool isSupported, object extra = null) => new(ImageUtils.IMAGE_FORMATS[format]) { IsSupported = isSupported, Extra = extra };
        public static MediaFormat CreateFormat(MediaUtils.FormatFamily format, bool isSupported, object extra = null) => new(MediaUtils.MEDIA_FORMATS[format]) { IsSupported = isSupported, Extra = extra };
        public static DocumentFormat CreateFormat(DocumentUtils.FormatFamily format, bool isSupported, object extra = null) => new(DocumentUtils.DOCUMENT_FORMATS[format]) { IsSupported = isSupported, Extra = extra };

        public static KeyValuePair<ImageUtils.FormatFamily, ImageFormat> CreateFormatPair(ImageUtils.FormatFamily format, bool isSupported, object extra = null) => new(format, new(ImageUtils.IMAGE_FORMATS[format]) { IsSupported = isSupported, Extra = extra });
        public static KeyValuePair<MediaUtils.FormatFamily, MediaFormat> CreateFormatPair(MediaUtils.FormatFamily format, bool isSupported, object extra = null) => new(format, new(MediaUtils.MEDIA_FORMATS[format]) { IsSupported = isSupported, Extra = extra });
        public static KeyValuePair<DocumentUtils.FormatFamily, DocumentFormat> CreateFormatPair(DocumentUtils.FormatFamily format, bool isSupported, object extra = null) => new(format, new(DocumentUtils.DOCUMENT_FORMATS[format]) { IsSupported = isSupported, Extra = extra });

        //

        public static bool Is(string extension, Type type) => EXTENSIONS[type].Contains(extension.ToLowerInvariant());

        public static Type GetType(string path)
        {
            if (!Utils.IsExistFileOrDirectory(path))
                throw new IOException();

            if (Directory.Exists(path))
                return Type.Directory;

            string extension = Path.GetExtension(path).ToLowerInvariant();

            foreach (var i in EXTENSIONS)
                if (i.Value.Contains(extension))
                    return i.Key;

            return Type.Unknown;
        }

        public static Type GetTypeByExtension(string extension)
        {
            extension = extension.ToLowerInvariant();

            foreach (var i in EXTENSIONS)
                if (i.Value.Contains(extension))
                    return i.Key;

            return Type.Unknown;
        }    
    }
}