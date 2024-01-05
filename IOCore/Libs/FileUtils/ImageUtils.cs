using System.Collections.Generic;
using System.Linq;
using ImageMagick;

namespace IOCore.Libs
{
    public class ImageFormat : FileFormat
    {
        public bool IsRaw { get; private set; }
        public string Container { get; private set; }
        public string[] Extensions { get; private set; }
        public MagickFormat[] Formats { get; private set; }

        public string Extension => Extensions.FirstOrDefault();
        public MagickFormat Format => Formats.FirstOrDefault();

        public ImageFormat(string name, FileUtils.Type type, bool isRaw, string[] extensions, MagickFormat[] formats) : base(name, type)
        {
            IsRaw = isRaw;
            Extensions = extensions;
            Formats = formats;
        }

        public ImageFormat(ImageFormat imageFormat) : base(imageFormat.Name, imageFormat.Type)
        {
            IsRaw = imageFormat.IsRaw;
            Extensions = imageFormat.Extensions.Clone() as string[];
            Formats = imageFormat.Formats.Clone() as MagickFormat[];
        }
    }

    public class ImageUtils
    {
        public enum FormatFamily
        {
            #region Standard
            Avif,
            Eps,
            Heic,
            Mpo,
            Psd,
            Qoi,
            Sfw,
            Svg,
            Tga,
            Tiff,
            Webp,
            Xpm,

            Bmp,
            Gif,
            Ico,
            Jpg,
            Png,

            Png8,    // Output only

            Pbm,
            Pcx,
            Wbmp,
            #endregion

            #region Raw
            Arw,
            Cr2,
            Dcr,
            Dng,
            Erf,
            Mef,
            Nef,
            Orf,
            Pef,
            Raf,
            Raw,
            Rw2,
            #endregion
        }

        public static readonly Dictionary<FormatFamily, ImageFormat> IMAGE_FORMATS = new()
        {
            #region Standard
            { FormatFamily.Avif, new("AVIF",                FileUtils.Type.Image, false, new[] { ".avif" },                        new[] { MagickFormat.Avif }) },
            { FormatFamily.Eps,  new("EPS",                 FileUtils.Type.Image, false, new[] { ".eps" },                         new[] { MagickFormat.Epi, MagickFormat.Eps, MagickFormat.Eps2, MagickFormat.Eps3, MagickFormat.Epsf, MagickFormat.Epsi, MagickFormat.Ept, MagickFormat.Ept2, MagickFormat.Ept3 }) },
            { FormatFamily.Heic, new("HEIC",                FileUtils.Type.Image, false, new[] { ".heic", ".heif" },               new[] { MagickFormat.Heic, MagickFormat.Heif }) },
            { FormatFamily.Mpo,  new("MPO",                 FileUtils.Type.Image, false, new[] { ".mpo" },                         new[] { MagickFormat.Mpo }) },
            { FormatFamily.Psd,  new("PSD",                 FileUtils.Type.Image, false, new[] { ".psb", ".psd" },                 new[] { MagickFormat.Psb, MagickFormat.Psd }) },
            { FormatFamily.Qoi,  new("QOI",                 FileUtils.Type.Image, false, new[] { ".qoi" },                         new[] { MagickFormat.Qoi }) },
            { FormatFamily.Sfw,  new("Seattle FilmWorks",   FileUtils.Type.Image, false, new[] { ".pwp", ".sfw" },                 new[] { MagickFormat.Pwp, MagickFormat.Sfw }) },
            { FormatFamily.Svg,  new("SVG",                 FileUtils.Type.Image, false, new[] { ".svg", ".svgz" },                new[] { MagickFormat.Svg, MagickFormat.Svgz }) },
            { FormatFamily.Tga,  new("TGA",                 FileUtils.Type.Image, false, new[] { ".icb", ".tga", ".vda", ".vst" }, new[] { MagickFormat.Icb, MagickFormat.Tga, MagickFormat.Vda, MagickFormat.Vst }) },
            { FormatFamily.Tiff, new("TIFF",                FileUtils.Type.Image, false, new[] { ".tif", ".tiff" },                new[] { MagickFormat.Tif, MagickFormat.Tiff, MagickFormat.Tiff64, MagickFormat.Ptif }) },
            { FormatFamily.Webp, new("WEBP",                FileUtils.Type.Image, false, new[] { ".webp" },                        new[] { MagickFormat.WebP }) },
            { FormatFamily.Xpm,  new("XPM",                 FileUtils.Type.Image, false, new[] { ".xbm", ".xpm" },                 new[] { MagickFormat.Xbm, MagickFormat.Xpm }) },

            { FormatFamily.Bmp,  new("BMP",                 FileUtils.Type.Image, false, new[] { ".bmp", ".rle", ".dib" },         new[] { MagickFormat.Bmp, MagickFormat.Rle, MagickFormat.Dib }) },
            { FormatFamily.Gif,  new("GIF",                 FileUtils.Type.Image, false, new[] { ".gif" },                         new[] { MagickFormat.Gif, MagickFormat.Gif87 }) },
            { FormatFamily.Ico,  new("ICO",                 FileUtils.Type.Image, false, new[] { ".ico" },                         new[] { MagickFormat.Ico }) },
            { FormatFamily.Jpg,  new("JPG",                 FileUtils.Type.Image, false, new[] { ".jpg", ".jpeg", ".jpe" },        new[] { MagickFormat.Jpg, MagickFormat.Jpeg, MagickFormat.Jpe, MagickFormat.Mat }) },
            { FormatFamily.Png,  new("PNG",                 FileUtils.Type.Image, false, new[] { ".png" },                         new[] { MagickFormat.Png }) },

            { FormatFamily.Png8, new("PNG8",                FileUtils.Type.Image, false, new[] { ".png" },                         new[] { MagickFormat.Png8 }) }, // Output Only

            { FormatFamily.Pbm,  new("PBM",                 FileUtils.Type.Image, false, new[] { ".pbm", ".pgm", ".ppm", ".pnm", ".pfm", ".pam" }, new[] { MagickFormat.Pbm, MagickFormat.Pgm, MagickFormat.Ppm, MagickFormat.Pnm, MagickFormat.Pfm, MagickFormat.Pam }) },
            { FormatFamily.Pcx,  new("PCX",                 FileUtils.Type.Image, false, new[] { ".pcx" },                         new[] { MagickFormat.Pcx }) },
            { FormatFamily.Wbmp, new("WBMP",                FileUtils.Type.Image, false, new[] { ".wbm", ".wbmp" },                new[] { MagickFormat.Wbmp }) },
            #endregion

            #region Raw
            { FormatFamily.Arw,  new("ARW",  FileUtils.Type.Image, true,  new[] { ".arw", ".srf", ".sr2" },         new[] { MagickFormat.Arw, MagickFormat.Srf, MagickFormat.Sr2 }) },
            { FormatFamily.Cr2,  new("CR2",  FileUtils.Type.Image, true,  new[] { ".cr2", ".cr3", ".crw" },         new[] { MagickFormat.Cr2, MagickFormat.Cr3, MagickFormat.Crw }) },
            { FormatFamily.Dcr,  new("DCR",  FileUtils.Type.Image, true,  new[] { ".dcr", ".kdc", ".k25" },         new[] { MagickFormat.Dcr, MagickFormat.Kdc, MagickFormat.K25 }) },
            { FormatFamily.Dng,  new("DNG",  FileUtils.Type.Image, true,  new[] { ".dng" },                         new[] { MagickFormat.Dng }) },
            { FormatFamily.Erf,  new("ERF",  FileUtils.Type.Image, true,  new[] { ".erf" },                         new[] { MagickFormat.Erf }) },
            { FormatFamily.Mef,  new("MEF",  FileUtils.Type.Image, true,  new[] { ".mef" },                         new[] { MagickFormat.Mef }) },
            { FormatFamily.Nef,  new("NEF",  FileUtils.Type.Image, true,  new[] { ".nef", ".nrw" },                 new[] { MagickFormat.Nef, MagickFormat.Nrw }) },
            { FormatFamily.Orf,  new("ORF",  FileUtils.Type.Image, true,  new[] { ".orf" },                         new[] { MagickFormat.Orf }) },
            { FormatFamily.Pef,  new("PEF",  FileUtils.Type.Image, true,  new[] { ".pef" },                         new[] { MagickFormat.Pef }) },
            { FormatFamily.Raf,  new("RAF",  FileUtils.Type.Image, true,  new[] { ".raf" },                         new[] { MagickFormat.Raf }) },
            { FormatFamily.Raw,  new("RAW",  FileUtils.Type.Image, true,  new[] { ".raw" },                         new[] { MagickFormat.Raw }) },
            { FormatFamily.Rw2,  new("RW2",  FileUtils.Type.Image, true,  new[] { ".rw2" },                         new[] { MagickFormat.Rw2 }) },
            #endregion
        };

        public static bool IsFormatFamily(MagickFormat magickFormat, FormatFamily formatFamily) => IMAGE_FORMATS[formatFamily].Formats.Any(i => i == magickFormat);
        public static bool IsAnyFormatFamilies(MagickFormat magickFormat, params FormatFamily[] formatFamilies) => formatFamilies.Any(ff => IsFormatFamily(magickFormat, ff));
        public static bool IsVectorFamily(MagickFormat magickFormat) => IsAnyFormatFamilies(magickFormat, FormatFamily.Eps, FormatFamily.Svg);
    }
}