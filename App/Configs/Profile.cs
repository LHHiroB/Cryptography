using System;
using System.Collections.Generic;
using System.Linq;
using IOCore.Libs;

namespace IOApp.Configs
{
    internal class Profile
    {
        private static readonly Dictionary<AppTypes.FileType, AppTypes.FileTypeRecord> _INPUT_FILES = new()
        {
            {AppTypes.FileType.Image,    new(true,  FileUtils.EXTENSIONS[FileUtils.Type.Image]) },
            {AppTypes.FileType.Video,    new(true,  FileUtils.EXTENSIONS[FileUtils.Type.Video]) },
            {AppTypes.FileType.Audio,    new(true,  FileUtils.EXTENSIONS[FileUtils.Type.Audio]) },
            {AppTypes.FileType.Document, new(true,  FileUtils.EXTENSIONS[FileUtils.Type.Document]) },
            {AppTypes.FileType.Pdf,      new(true,  new[]{".pdf"}) },
        };

        public static bool FullSupport => !_INPUT_FILES.Any(i => !i.Value.IsSupported);

        //

        public static readonly string[] INPUT_EXTENSIONS;

        static Profile()
        {
            INPUT_EXTENSIONS = FullSupport ?
              new[] { "*" } :
              _INPUT_FILES.Where(i => i.Value.IsSupported).Select(i => i.Value.Extensions).SelectMany(i => i).Distinct().ToArray();
        }

        public static string GetInputExtensionsTextByGroupFamily()
        {
            const int LENGTH = 20;

            var extensions = INPUT_EXTENSIONS;

            List<string> extTexts = new();
            while (extensions.Length > 0)
            {
                extTexts.Add(string.Join(", ", extensions.Skip(0).Take(LENGTH).ToArray()));
                extensions = extensions.Skip(LENGTH).ToArray();
            }

            return string.Join("\n", extTexts);
        }

        public static bool IsAcceptedInputExtension(string extension)
        {
            if (string.IsNullOrWhiteSpace(extension)) return false;
            return INPUT_EXTENSIONS.Contains(extension);
        }

        //

        public static readonly Dictionary<MediaUtils.FormatFamily, string> PROMOTION_MEDIA_FORMATS = new()
        {
            #region Video
            { MediaUtils.FormatFamily.V_Dat,  "dat-to" },
            { MediaUtils.FormatFamily.V_Flv,  "flv-to" },
            { MediaUtils.FormatFamily.V_Hevc, "hevc-to" },
            { MediaUtils.FormatFamily.V_Mxf,  "mxf-to" },
            { MediaUtils.FormatFamily.V_Ogg,  "ogg-to" },
            { MediaUtils.FormatFamily.V_Ogv,  "ogv-to" },
            { MediaUtils.FormatFamily.V_Rm,   "universal-video-audio-converter" },
            { MediaUtils.FormatFamily.V_Swf,  "swf-to" },
            { MediaUtils.FormatFamily.V_Ts,   "ts-to" },
            { MediaUtils.FormatFamily.V_Vob,  "vob-to" },
            { MediaUtils.FormatFamily.V_Webm, "webm-to" },

            { MediaUtils.FormatFamily.V_3gp,  "3gp-to" },
            { MediaUtils.FormatFamily.V_Asf,  "asf-to" },
            { MediaUtils.FormatFamily.V_Avi,  "avi-to" },
            { MediaUtils.FormatFamily.V_M2ts, "m2ts-to" },
            { MediaUtils.FormatFamily.V_M4v,  "universal-video-audio-converter" },
            { MediaUtils.FormatFamily.V_Mkv,  "mkv-to" },
            { MediaUtils.FormatFamily.V_Mov,  "mov-to" },
            { MediaUtils.FormatFamily.V_Mp4,  "mp4-to" },
            { MediaUtils.FormatFamily.V_Mpeg, "mpeg-to" },
            { MediaUtils.FormatFamily.V_Wmv,  "wmv-to" },
            #endregion

            #region Audio
            { MediaUtils.FormatFamily.A_Ac3,  "ac3-to" },
            { MediaUtils.FormatFamily.A_Amr,  "amr-to" },
            { MediaUtils.FormatFamily.A_Caf,  "caf-to" },
            { MediaUtils.FormatFamily.A_Dts,  "dts-to" },
            { MediaUtils.FormatFamily.A_Dsd,  "dsd-to" },
            { MediaUtils.FormatFamily.A_M4b,  "m4b-to" },
            { MediaUtils.FormatFamily.A_M4p,  "m4p-to" },
            { MediaUtils.FormatFamily.A_M4r,  "m4r-to" },
            { MediaUtils.FormatFamily.A_Mlp,  "mlp-to" },
            { MediaUtils.FormatFamily.A_Ogg,  "ogg-to-mp3" },
            { MediaUtils.FormatFamily.A_Opus, "opus-to" },
            { MediaUtils.FormatFamily.A_Tta,  "tta-to" },
            { MediaUtils.FormatFamily.A_Voc,  "voc-to" },
            { MediaUtils.FormatFamily.A_Weba, "weba-to" },

            { MediaUtils.FormatFamily.A_Aac,  "aac-to" },
            { MediaUtils.FormatFamily.A_Aiff, "aiff-to" },
            { MediaUtils.FormatFamily.A_Au,   "au-to" },
            { MediaUtils.FormatFamily.A_Flac, "flac-to" },
            { MediaUtils.FormatFamily.A_M4a,  "m4a-to" },
            { MediaUtils.FormatFamily.A_Mp3,  "universal-video-audio-converter" },
            { MediaUtils.FormatFamily.A_Mp2,  "mp2-to" },
            { MediaUtils.FormatFamily.A_Wav,  "wav-to" },
            { MediaUtils.FormatFamily.A_Wma,  "wma-to" },
            { MediaUtils.FormatFamily.A_Wv,   "wv-to" }
            #endregion
        };

        public static readonly Dictionary<ImageUtils.FormatFamily, string> PROMOTION_IMAGE_FORMATS = new()
        {
            #region Standard
            { ImageUtils.FormatFamily.Avif, "avif-to" },
            { ImageUtils.FormatFamily.Eps,  "eps-to" },
            { ImageUtils.FormatFamily.Heic, "heic-to" },
            { ImageUtils.FormatFamily.Mpo,  "mpo-to" },
            { ImageUtils.FormatFamily.Psd,  "psd-to" },
            { ImageUtils.FormatFamily.Qoi,  "qoi-to" },
            { ImageUtils.FormatFamily.Sfw,  "sfw-to" },
            { ImageUtils.FormatFamily.Svg,  "svg-to" },
            { ImageUtils.FormatFamily.Tga,  "tga-to" },
            { ImageUtils.FormatFamily.Tiff, "tiff-to" },
            { ImageUtils.FormatFamily.Webp, "webp-to" },
            { ImageUtils.FormatFamily.Xpm,  "xpm-to" },

            { ImageUtils.FormatFamily.Bmp,  "universal-image-converter" },
            { ImageUtils.FormatFamily.Gif,  "universal-image-converter" },
            { ImageUtils.FormatFamily.Ico,  "icon-maker-studio" },
            { ImageUtils.FormatFamily.Jpg,  "universal-image-converter" },
            { ImageUtils.FormatFamily.Png,  "universal-image-converter" },

            { ImageUtils.FormatFamily.Pbm,  "universal-image-converter" },
            { ImageUtils.FormatFamily.Pcx,  "universal-image-converter" },
            { ImageUtils.FormatFamily.Wbmp, "universal-image-converter" },
            #endregion

            #region Raw
            { ImageUtils.FormatFamily.Arw,  "arw-to" },
            { ImageUtils.FormatFamily.Cr2,  "cr2-to" },
            { ImageUtils.FormatFamily.Dcr,  "dcr-to" },
            { ImageUtils.FormatFamily.Dng,  "dng-to" },
            { ImageUtils.FormatFamily.Erf,  "erf-to" },
            { ImageUtils.FormatFamily.Mef,  "mef-to" },
            { ImageUtils.FormatFamily.Nef,  "nef-to" },
            { ImageUtils.FormatFamily.Orf,  "orf-to" },
            { ImageUtils.FormatFamily.Pef,  "pef-to" },
            { ImageUtils.FormatFamily.Raf,  "raf-to" },
            { ImageUtils.FormatFamily.Raw,  "raw-to" },
            { ImageUtils.FormatFamily.Rw2,  "rw2-to" },
            #endregion
        };
    }
}
