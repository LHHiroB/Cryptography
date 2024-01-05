using System.Collections.Generic;
using System.Linq;

namespace IOCore.Libs
{
    public class MediaFormat : FileFormat
    {
        public string Container { get; private set; }
        public string[] Extensions { get; private set; }

        public string Extension => Extensions.FirstOrDefault();

        public MediaFormat(string name, string container, FileUtils.Type type, string[] extensions) : base(name, type)
        {
            Container = container;
            Extensions = extensions;
        }

        public MediaFormat(MediaFormat mediaFormat) : base(mediaFormat.Name, mediaFormat.Type)
        {
            Container = mediaFormat.Container;
            Extensions = mediaFormat.Extensions.Clone() as string[];
        }
    }

    public class MediaUtils
    {
        public enum FormatFamily
        {
            #region Video
            V_3gp,
            V_Asf,
            V_Avi,
            V_Dat,
            V_Flv,
            V_Hevc,
            V_M2ts,
            V_M4v,
            V_Mkv,
            V_Mov,
            V_Mp4,
            V_Mxf,
            V_Vob,
            V_Ogg,
            V_Ogv,
            V_Rm,
            V_Swf,
            V_Ts,
            V_Mpeg,
            V_Webm,
            V_Wmv,
            #endregion

            #region Audio
            // Lossy
            A_Aac,
            A_Ac3,
            A_Amr,
            A_Mp2,
            A_Mp3,
            A_Ogg,
            A_Opus,
            A_Oga,
            A_Spx,
            // Lossless
            A_Alac,
            //A_Cda,
            A_Flac,
            A_M4a,
            A_M4b,
            A_M4p,
            A_M4r,
            A_Mlp,
            A_Tta,
            A_Ra,
            A_Voc,
            A_Wv,
            // Uncompressed
            A_Aif,
            A_Aifc,
            A_Aiff,
            A_Au,
            A_Dsd,
            A_Dsf,
            A_Dts,
            A_Pcm,
            A_Wav,
            // Container
            A_Caf,
            //
            A_Weba,
            A_Wma,
            #endregion
        }

        public static readonly Dictionary<FormatFamily, MediaFormat> MEDIA_FORMATS = new()
        {
            #region Video
            { FormatFamily.V_3gp,  new("3GP",                       "3gp",  FileUtils.Type.Video, new[] { ".3gp", ".3g2", ".3gpp", ".3gp2", ".3gpp2" }             ) },
            { FormatFamily.V_Asf,  new("ASF",                       "asf",  FileUtils.Type.Video, new[] { ".asf" }                                                 ) },
            { FormatFamily.V_Avi,  new("AVI",                       "avi",  FileUtils.Type.Video, new[] { ".avi" }                                                 ) },
            { FormatFamily.V_Dat,  new("DAT",                       "dat",  FileUtils.Type.Video, new[] { ".dat" }                                                 ) },
            { FormatFamily.V_Flv,  new("Flash Video",               "flv",  FileUtils.Type.Video, new[] { ".f4v", ".flv" }                                         ) },
            { FormatFamily.V_Hevc, new("HEVC",                      "hevc", FileUtils.Type.Video, new[] { ".hevc" }                                                ) },
            { FormatFamily.V_M2ts, new("M2TS",                      "m2ts", FileUtils.Type.Video, new[] { ".m2ts", ".mts" }                                        ) },
            { FormatFamily.V_M4v,  new("M4V",                       "m4v",  FileUtils.Type.Video, new[] { ".m4v" }                                                 ) },
            { FormatFamily.V_Mkv,  new("MKV",                       "mkv",  FileUtils.Type.Video, new[] { ".mkv" }                                                 ) },
            { FormatFamily.V_Mov,  new("Movie",                     "mov",  FileUtils.Type.Video, new[] { ".mov", ".qt" }                                          ) },
            { FormatFamily.V_Mp4,  new("MP4",                       "mp4",  FileUtils.Type.Video, new[] { ".mp4", ".m4a", ".m4p", ".m4b", ".m4r" }                 ) },
            { FormatFamily.V_Mpeg, new("MPEG",                      "mpeg", FileUtils.Type.Video, new[] { ".mpeg", ".mpg" }                                        ) },
            { FormatFamily.V_Mxf,  new("MXF",                       "mxf",  FileUtils.Type.Video, new[] { ".mxf" }                                                 ) },
            { FormatFamily.V_Ogg,  new("OGG",                       "ogg",  FileUtils.Type.Video, new[] { ".ogg", ".ogv", ".oga", ".ogx", ".ogm", ".spx", ".opus" }) },
            { FormatFamily.V_Ogv,  new("OGV",                       "ogv",  FileUtils.Type.Video, new[] { ".ogv" }                                                 ) },
            { FormatFamily.V_Rm,   new("RealMedia",                 "rm",   FileUtils.Type.Video, new[] { ".rm" }                                                  ) },
            { FormatFamily.V_Swf,  new("ShockWave Flash",           "swf",  FileUtils.Type.Video, new[] { ".swf" }                                                 ) },
            { FormatFamily.V_Ts,   new("TS",                        "ts",   FileUtils.Type.Video, new[] { ".ts", ".tsv", ".tsa", ".m2t" }                          ) },
            { FormatFamily.V_Vob,  new("DVD Video Object File",     "vob",  FileUtils.Type.Video, new[] { ".vob" }                                                 ) },
            { FormatFamily.V_Webm, new("WEBM",                      "webm", FileUtils.Type.Video, new[] { ".webm" }                                                ) },
            { FormatFamily.V_Wmv,  new("WMV",                       "wmv",  FileUtils.Type.Video, new[] { ".wmv", ".wm" }                                          ) },
            #endregion

            #region Audio
            { FormatFamily.A_Aac,  new("AAC",                       "aac",  FileUtils.Type.Audio, new[] { ".aac" }                                                 ) },
            { FormatFamily.A_Ac3,  new("AC3",                       "ac3",  FileUtils.Type.Audio, new[] { ".ac3" }                                                 ) },
            { FormatFamily.A_Aiff, new("AIFF",                      "aiff", FileUtils.Type.Audio, new[] { ".aiff", ".aifc", ".aif" }                               ) },
            { FormatFamily.A_Amr,  new("AMR",                       "amr",  FileUtils.Type.Audio, new[] { ".amr" }                                                 ) },
            { FormatFamily.A_Au,   new("AU",                        "au",   FileUtils.Type.Audio, new[] { ".au" }                                                  ) },
            { FormatFamily.A_Caf,  new("CAF",                       "caf",  FileUtils.Type.Audio, new[] { ".caf" }                                                 ) },
            //{ FormatFamily.A_Cda,  new("CD Digital Audio",        "cda",  FileUtils.Type.Audio, new[] { ".cda", ".cdda" }                                        ) }, // FFmpeg cannot read
            { FormatFamily.A_Dsd,  new("DSD Audio",                 "dsd",  FileUtils.Type.Audio, new[] { ".dff", ".dsf" }                                         ) },
            { FormatFamily.A_Dts,  new("DTS",                       "dts",  FileUtils.Type.Audio, new[] { ".dts" }                                                 ) },
            { FormatFamily.A_Flac, new("FLAC",                      "flac", FileUtils.Type.Audio, new[] { ".flac" }                                                ) },
            { FormatFamily.A_M4a,  new("M4A",                       "m4a",  FileUtils.Type.Audio, new[] { ".m4a" }                                                 ) },
            { FormatFamily.A_M4b,  new("M4B",                       "m4b",  FileUtils.Type.Audio, new[] { ".m4b" }                                                 ) },
            { FormatFamily.A_M4p,  new("M4P",                       "m4p",  FileUtils.Type.Audio, new[] { ".m4p" }                                                 ) },
            { FormatFamily.A_M4r,  new("M4R",                       "m4r",  FileUtils.Type.Audio, new[] { ".m4r" }                                                 ) },
            { FormatFamily.A_Mlp,  new("Meridian Lossless Packing", "mlp",  FileUtils.Type.Audio, new[] { ".mlp" }                                                 ) }, // Bluray :: Dolby TrueHD
            { FormatFamily.A_Mp2,  new("MP2",                       "mp2",  FileUtils.Type.Audio, new[] { ".mp2" }                                                 ) },
            { FormatFamily.A_Mp3,  new("MP3",                       "mp3",  FileUtils.Type.Audio, new[] { ".mp3" }                                                 ) },
            { FormatFamily.A_Ogg,  new("OGG",                       "ogg",  FileUtils.Type.Audio, new[] { ".ogg", ".oga", ".opus", ".spx" }                        ) },
            { FormatFamily.A_Opus, new("OPUS",                      "opus", FileUtils.Type.Audio, new[] { ".opus" }                                                ) },
            { FormatFamily.A_Tta,  new("TTA",                       "tta",  FileUtils.Type.Audio, new[] { ".tta" }                                                 ) },
            { FormatFamily.A_Voc,  new("VOC",                       "voc",  FileUtils.Type.Audio, new[] { ".voc" }                                                 ) },
            { FormatFamily.A_Wav,  new("WAV",                       "wav",  FileUtils.Type.Audio, new[] { ".wav" }                                                 ) },
            { FormatFamily.A_Weba, new("WEBA",                      "weba", FileUtils.Type.Audio, new[] { ".weba" }                                                ) },
            { FormatFamily.A_Wma,  new("WMA",                       "wma",  FileUtils.Type.Audio, new[] { ".wma" }                                                 ) },
            { FormatFamily.A_Wv,   new("WV",                        "wv",   FileUtils.Type.Audio, new[] { ".wv" }                                                  ) },
            #endregion
        };

        public static readonly Dictionary<FormatFamily, MediaFormat> VIDEO_FORMATS;
        public static readonly Dictionary<FormatFamily, MediaFormat> AUDIO_FORMATS;
        public static readonly Dictionary<string, string> CONTAINERS;

        static MediaUtils()
        {
            VIDEO_FORMATS = new();
            AUDIO_FORMATS = new();
            CONTAINERS = new();

            foreach (var m in MEDIA_FORMATS)
            {
                if (m.Value.Type == FileUtils.Type.Video)
                    VIDEO_FORMATS.Add(m.Key, m.Value);
                else if (m.Value.Type == FileUtils.Type.Audio)
                    AUDIO_FORMATS.Add(m.Key, m.Value);

                foreach (var e in m.Value.Extensions)
                    if (!CONTAINERS.ContainsKey(e))
                        CONTAINERS.Add(e, m.Value.Container);
            }
        }
    }
}