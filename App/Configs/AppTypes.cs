using System;
using System.Collections.Generic;
using System.Drawing;
using IOCore;
using IOCore.Libs;

namespace IOApp.Configs
{
    internal class AppTypes
    {
        public enum FileType
        {
            All,
            Video,
            Audio,
            Image,
            Document,
            Pdf,
        }

        public class FileTypeRecord
        {
            public bool IsSupported;
            public string[] Extensions;

            public FileTypeRecord(bool isSupported, string[] extensions)
            {
                IsSupported = isSupported;
                Extensions = extensions;
            }
        }

        public static readonly Dictionary<FileType, string> FILES = new()
        {
            { FileType.All, "All" },
            { FileType.Video, "Video" },
            { FileType.Audio, "Audio" },
            { FileType.Image, "Image" },
        };

        //

        #region Resolution

        public enum Resolution
        {
            Auto,
            FourK_2160p_3840,
            FourK_2160p_4090,
            TwoK_1440p,
            Hd_1080p,
            Hd_720p,
            Sd_480p,
            Sd_360p,
            Sd_240p
        }

        public static readonly Dictionary<Resolution, ResolutionRecord> RESOLUTIONS = new()
        {
            { Resolution.Auto,             new("Auto", 0, 0) },
            { Resolution.FourK_2160p_3840, new("4K 2160p • 3840x2160", 3840, 2160) },
            { Resolution.FourK_2160p_4090, new("4K 2160p • 4096x2160", 4096, 2160) },
            { Resolution.TwoK_1440p,       new("2K 1440p • 2560x1440", 2560, 1440) },
            { Resolution.Hd_1080p,         new("HD 1080p • 1920x1080", 1920, 1080) },
            { Resolution.Hd_720p,          new("HD 720p • 1280x720", 1280, 720) },
            { Resolution.Sd_480p,          new("SD 480p • 720x480", 720,480 ) },
            { Resolution.Sd_360p,          new("SD 360p • 480x360", 480,360 ) },
            { Resolution.Sd_240p,          new("SD 240p • 352x240", 352,240 ) }
        };

        public static readonly Resolution[] ResolutionArray = (Resolution[])Enum.GetValues(typeof(Resolution));

        public struct ResolutionRecord
        {
            public string Name;
            public int Width;
            public int Height;

            public readonly Size Size => new(Width, Height);

            public ResolutionRecord(string name, int width, int height)
            {
                Name = name;
                Width = width;
                Height = height;
            }
        }

        #endregion

        #region FrameRate

        public enum FrameRate
        {
            Auto,
            _8,
            _10,
            _12,
            _15,
            _20,
            _24,
            _30,
            _50,
            _60
        }

        public static readonly Dictionary<FrameRate, FrameRateRecord> FRAME_RATES = new()
        {
            { FrameRate.Auto, new("Auto", 0.0) },
            { FrameRate._8,   new("8",    8.0) },
            { FrameRate._10,  new("10",   10.0) },
            { FrameRate._12,  new("12",   12.0) },
            { FrameRate._15,  new("15",   15.0) },
            { FrameRate._20,  new("20",   20.0) },
            { FrameRate._24,  new("24",   24.0) },
            { FrameRate._30,  new("30",   30.0) },
            { FrameRate._50,  new("50",   50.0) },
            { FrameRate._60,  new("60",   60.0) },
        };

        public static readonly FrameRate[] FrameRateArray = (FrameRate[])Enum.GetValues(typeof(FrameRate));

        public struct FrameRateRecord
        {
            public string Name;
            public double FrameRate;

            public FrameRateRecord(string name, double frameRate)
            {
                Name = name;
                FrameRate = frameRate;
            }
        }

        #endregion

        #region VideoCodec

        public enum VideoCodec
        {
            Auto,
            H264,
            HEVC_H265,
            MPEG4,
            //XVID,
            //DIVX,
            //MJPEG,
            //WMV9,
            //VP8,
            VP9,
            VC1,
            MPEG2,
            //MSMPEG2,
            //MSMPEG3,
            THEORA,
            FLV,
        }

        public static readonly Dictionary<VideoCodec, VideoCodecRecord> VIDEO_CODECS = new()
        {
            { VideoCodec.Auto,      new("Auto", null) },
            { VideoCodec.H264,      new("H264", "libopenh264") },
            { VideoCodec.HEVC_H265, new("HEVC/H265", "libx265") },
            { VideoCodec.MPEG4,     new("MPEG-4", "mpeg4") },
            //{ VideoCodec.XVID,     new("XVID" },
            //{ VideoCodec.DIVX,     new("DIVX" },
            //{ VideoCodec.MJPEG,    new("MJPEG" },
            //{ VideoCodec.WMV9,     new("WMV9" },
            //{ VideoCodec.VP8,      new("VP8" },
            { VideoCodec.VP9,       new("VP9", "libvpx-vp9") },
            { VideoCodec.VC1,       new("VC-1", "vc1") },
            { VideoCodec.MPEG2,     new("MPEG2", "mpeg2video") },
            //{ VideoCodec.VC_MSMPEG2,  new("MSMPEG2" },
            //{ VideoCodec.VC_MSMPEG3,  new("MSMPEG3" },
            { VideoCodec.THEORA,    new("THEORA", "libtheora") },
            { VideoCodec.FLV,       new("FLV", "flv") },
        };

        public static readonly VideoCodec[] VideoCodecArray = (VideoCodec[])Enum.GetValues(typeof(VideoCodec));

        public struct VideoCodecRecord
        {
            public string Name;
            public string VideoCodec;

            public VideoCodecRecord(string name, string videoCodec)
            {
                Name = name;
                VideoCodec = videoCodec;
            }
        }

        #endregion

        #region VideoBitrate

        public enum VideoBitrate
        {
            Auto,
            _1000,
            _1500,
            _2500,
            _4000,
            _5000,
            _7500,
            _8000,
            _12000,
            _16000,
            _24000,
        }

        public static readonly Dictionary<VideoBitrate, VideoBitrateRecord> VIDEO_BITRATES = new()
        {
            { VideoBitrate.Auto,   new("Auto",       0) },
            { VideoBitrate._1000,  new("1000 kbps",  1000) },
            { VideoBitrate._1500,  new("1500 kbps",  1500) },
            { VideoBitrate._2500,  new("2500 kbps",  2500) },
            { VideoBitrate._4000,  new("4000 kbps",  4000) },
            { VideoBitrate._5000,  new("5000 kbps",  5000) },
            { VideoBitrate._7500,  new("7500 kbps",  7500) },
            { VideoBitrate._8000,  new("8000 kbps",  8000) },
            { VideoBitrate._12000, new("12000 kbps", 12000) },
            { VideoBitrate._16000, new("16000 kbps", 16000) },
            { VideoBitrate._24000, new("24000 kbps", 24000) },
        };

        public static readonly VideoBitrate[] VideoBitrateArray = (VideoBitrate[])Enum.GetValues(typeof(VideoBitrate));

        public struct VideoBitrateRecord
        {
            public string Name;
            public long VideoBitrate;

            public VideoBitrateRecord(string name, long videoBitrate)
            {
                Name = name;
                VideoBitrate = videoBitrate;
            }
        }

        #endregion

        #region Channel

        public enum Channel
        {
            Auto,
            _1,
            _2,
            _6,
            _8
        }

        public static readonly Dictionary<Channel, ChannelRecord> CHANNELS = new()
        {
            { Channel.Auto, new("Auto",       0) },
            { Channel._1,   new("1 (Mono)",   1) },
            { Channel._2,   new("2 (Stereo)", 2) },
            { Channel._6,   new("6 (5.1)",    6) },
            { Channel._8,   new("8 (7.1)",    8) },
        };

        public static readonly Channel[] ChannelArray = (Channel[])Enum.GetValues(typeof(Channel));

        public struct ChannelRecord
        {
            public string Name;
            public int Channel;

            public ChannelRecord(string name, int channel)
            {
                Name = name;
                Channel = channel;
            }
        }

        #endregion

        #region SampleRate

        public enum SampleRate
        {
            Auto,
            _22050,
            _32000,
            _44100,
            _48000
        }

        public static readonly Dictionary<SampleRate, SampleRateRecord> SAMPLE_RATES = new()
        {
            { SampleRate.Auto,   new("Auto",     0) },
            { SampleRate._22050, new("22050 Hz", 22050) },
            { SampleRate._32000, new("32000 Hz", 32000) },
            { SampleRate._44100, new("44100 Hz", 44100) },
            { SampleRate._48000, new("48000 Hz", 48000) },
        };

        public static readonly SampleRate[] SampleRateArray = (SampleRate[])Enum.GetValues(typeof(SampleRate));

        public struct SampleRateRecord
        {
            public string Name;
            public int SamepleRate;

            public SampleRateRecord(string name, int samepleRate)
            {
                Name = name;
                SamepleRate = samepleRate;
            }
        }

        #endregion

        #region AudioCodec

        public enum AudioCodec
        {
            Auto,
            AAC,
            AC3,
            ALAC,
            FLAC,
            MP2,
            MP3,
            OPUS,
            PCM_ALAW,
            PCM_MULAW,
            PCM_S8,
            PCM_S16LE,
            PCM_S16BE,
            PCM_S24LE,
            PCM_S24BE,
            PCM_S32LE,
            PCM_S32BE,
            PCM_S64LE,
            PCM_U8,
            PCM_U16LE,
            PCM_U24LE,
            PCM_U32LE,
            PCM_F16LE,
            PCM_F24LE,
            PCM_F32LE,
            PCM_F32BE,
            PCM_F64LE,
            PCM_F64BE,
            //WMA9,
            //WMA8,
            SPEEX,
            VORBIS,
            //OGG,
            WMA2,
            WMA1,
            //AD_PCM,
            WV
        }

        public static readonly Dictionary<AudioCodec, AudioCodecRecord> AUDIO_CODECS = new()
        {
            { AudioCodec.Auto,      new("Auto",       null) },
            { AudioCodec.AAC,       new("AAC",        "aac") },
            { AudioCodec.AC3,       new("AC3",        "ac3") },
            { AudioCodec.ALAC,      new("ALAC",       "alac") },
            { AudioCodec.MP2,       new("MP2",        "mp2") },
            { AudioCodec.MP3,       new("MP3",        "libmp3lame") },
            { AudioCodec.OPUS,      new("OPUS",       "libopus") },
            { AudioCodec.PCM_ALAW,  new("PCM A-law",  "pcm_alaw") }, 
            { AudioCodec.PCM_MULAW, new("PCM mu-law", "pcm_mulaw") },
            { AudioCodec.PCM_S8,    new("PCM S8",     "pcm_s8") },
            { AudioCodec.PCM_S16LE, new("PCM S16LE",  "pcm_s16le") },
            { AudioCodec.PCM_S16BE, new("PCM S16BE",  "pcm_s16be") },
            { AudioCodec.PCM_S24LE, new("PCM S24LE",  "pcm_s24le") },
            { AudioCodec.PCM_S24BE, new("PCM S24BE",  "pcm_s24be") },
            { AudioCodec.PCM_S32LE, new("PCM S32LE",  "pcm_s32le") },
            { AudioCodec.PCM_S32BE, new("PCM S32BE",  "pcm_s32be") },
            { AudioCodec.PCM_S64LE, new("PCM S64LE",  "pcm_s64le") },
            { AudioCodec.PCM_U8,    new("PCM U8",     "pcm_u8") },
            { AudioCodec.PCM_U16LE, new("PCM U16LE",  "pcm_u16le") },
            { AudioCodec.PCM_U24LE, new("PCM U24LE",  "pcm_u24le") },
            { AudioCodec.PCM_U32LE, new("PCM U32LE",  "pcm_u32le") },
            { AudioCodec.PCM_F16LE, new("PCM F16LE",  "pcm_f16le") },
            { AudioCodec.PCM_F24LE, new("PCM F24LE",  "pcm_f24le") },
            { AudioCodec.PCM_F32LE, new("PCM F32LE",  "pcm_f32le") },
            { AudioCodec.PCM_F32BE, new("PCM F32BE",  "pcm_f32be") },
            { AudioCodec.PCM_F64LE, new("PCM F64LE",  "pcm_f64le") },
            { AudioCodec.PCM_F64BE, new("PCM F64BE",  "pcm_f64be") },
            { AudioCodec.FLAC,      new("FLAC",       "flac") },
            //{ AudioCodec.WMA9,    new("WMA9", ) },
            //{ AudioCodec.WMA8,    new("WMA8", ) },
            { AudioCodec.SPEEX,     new("SPEEX",      "libspeex") },
            { AudioCodec.VORBIS,    new("VORBIS",     "libvorbis") },
            //{ AudioCodec.OGG,     new("OGG", ) },
            { AudioCodec.WMA1,      new("WMA1",       "wmav1") },
            { AudioCodec.WMA2,      new("WMA2",       "wmav2") },
            //{ AudioCodec.AD_PCM,  new("AD PCM" ) },
        };

        public static readonly AudioCodec[] AudioCodecArray = (AudioCodec[])Enum.GetValues(typeof(AudioCodec));

        public struct AudioCodecRecord
        {
            public string Name;
            public string AudioCodec;

            public AudioCodecRecord(string name, string audioCodec)
            {
                Name = name;
                AudioCodec = audioCodec;
            }
        }

        #endregion

        #region AudioBitrate

        public enum AudioBitrate
        {
            Auto,
            _64,
            _96,
            _128,
            _192,
            _320
        }

        public static readonly AudioBitrate[] AudioBitrateArray = (AudioBitrate[])Enum.GetValues(typeof(AudioBitrate));

        public struct AudioBitrateRecord
        {
            public string Name;
            public long AudioBitrate;

            public AudioBitrateRecord(string name, long audioBitrate)
            {
                Name = name;
                AudioBitrate = audioBitrate;
            }
        }

        public static readonly Dictionary<AudioBitrate, AudioBitrateRecord> AUDIO_BITRATES = new()
        {
            { AudioBitrate.Auto,   new("Auto", 0) },
            { AudioBitrate._64,  new("64 kbps", 64) },
            { AudioBitrate._96,  new("96 kbps", 96) },
            { AudioBitrate._128, new("128 kbps", 128) },
            { AudioBitrate._192, new("192 kbps", 192) },
            { AudioBitrate._320, new("320 kbps", 320) },
        };

        #endregion

        public struct VideoContainerRecord
        {
            public Resolution[] Resolutions;

            public FrameRate[] FrameRates;
            public VideoCodec[] VideoCodecs;
            public VideoBitrate[] VideoBitrates;

            public Channel[] Channels;
            public SampleRate[] SampleRates;
            public AudioCodec[] AudioCodecs;
            public AudioBitrate[] AudioBitrates;

            public VideoContainerRecord(Resolution[] resolutions, FrameRate[] frameRates, VideoCodec[] videoCodecs, VideoBitrate[] videoBitrates, Channel[] channels, SampleRate[] sampleRates, AudioCodec[] audioCodecs, AudioBitrate[] audioBitrates)
            {
                Resolutions = resolutions;
                FrameRates = frameRates;
                VideoCodecs = videoCodecs;
                VideoBitrates = videoBitrates;
                Channels = channels;
                SampleRates = sampleRates;
                AudioCodecs = audioCodecs;
                AudioBitrates = audioBitrates;
            }
        }

        public struct AudioContainerRecord
        {
            public Channel[] Channels;
            public SampleRate[] SampleRates;
            public AudioCodec[] AudioCodecs;
            public AudioBitrate[] AudioBitrates;

            public AudioContainerRecord(Channel[] channels, SampleRate[] sampleRates, AudioCodec[] audioCodecs, AudioBitrate[] audioBitrates)
            {
                Channels = channels;
                SampleRates = sampleRates;
                AudioCodecs = audioCodecs;
                AudioBitrates = audioBitrates;
            }
        }

        public static readonly Dictionary<MediaUtils.FormatFamily, VideoContainerRecord> VIDEO_CONTAINERS = new()
        {
            { MediaUtils.FormatFamily.V_Mp4, new(
                ResolutionArray,
                FrameRateArray,
                new[] { VideoCodec.Auto, VideoCodec.H264 },
                VideoBitrateArray,
                ChannelArray,
                SampleRateArray,
                new[] { AudioCodec.Auto, AudioCodec.AAC, AudioCodec.AC3 },
                AudioBitrateArray
                ) },
            { MediaUtils.FormatFamily.V_Mov, new(
                ResolutionArray,
                FrameRateArray,
                new[] { VideoCodec.Auto, VideoCodec.H264 },
                VideoBitrateArray,
                ChannelArray,
                SampleRateArray,
                new[] { AudioCodec.Auto, AudioCodec.AAC },
                AudioBitrateArray
                ) },
            { MediaUtils.FormatFamily.V_Mkv, new(
                ResolutionArray,
                FrameRateArray,
                new[] { VideoCodec.Auto, VideoCodec.H264 },
                VideoBitrateArray,
                ChannelArray,
                SampleRateArray,
                new[] { AudioCodec.Auto, AudioCodec.AAC, AudioCodec.AC3, AudioCodec.MP3, AudioCodec.OPUS, AudioCodec.PCM_S16LE, AudioCodec.FLAC },
                AudioBitrateArray
                ) },
            { MediaUtils.FormatFamily.V_Avi, new(
                ResolutionArray,
                FrameRateArray,
                new[] { VideoCodec.Auto, VideoCodec.H264 },
                VideoBitrateArray,
                new[] { Channel.Auto, Channel._1, Channel._2 },
                SampleRateArray,
                new[] { AudioCodec.Auto, AudioCodec.MP3 },
                AudioBitrateArray
                ) },
            { MediaUtils.FormatFamily.V_Wmv, new(
                ResolutionArray,
                FrameRateArray,
                new[] { VideoCodec.Auto, VideoCodec.MPEG4 },
                VideoBitrateArray,
                new[] { Channel.Auto, Channel._1, Channel._2 },
                SampleRateArray,
                new[] { AudioCodec.Auto, AudioCodec.WMA2, AudioCodec.WMA1 },
                AudioBitrateArray
                ) },
            { MediaUtils.FormatFamily.V_Webm, new(
                ResolutionArray,
                FrameRateArray,
                new[] { VideoCodec.Auto, VideoCodec.VP9 },
                VideoBitrateArray,
                ChannelArray,
                new[] { SampleRate.Auto, SampleRate._48000 },
                new[] { AudioCodec.Auto, AudioCodec.OPUS, AudioCodec.VORBIS },
                new[] { AudioBitrate.Auto, AudioBitrate._64, AudioBitrate._96, AudioBitrate._128, AudioBitrate._192 }
                ) },
            { MediaUtils.FormatFamily.V_Mxf, new(
                ResolutionArray,
                new[] { FrameRate.Auto, FrameRate._24, FrameRate._30, FrameRate._50, FrameRate._60 },
                new[] { VideoCodec.Auto, VideoCodec.MPEG2 },
                VideoBitrateArray,
                ChannelArray,
                new[] { SampleRate.Auto, SampleRate._48000 },
                new[] { AudioCodec.Auto, AudioCodec.PCM_S16LE },
                AudioBitrateArray
                ) },
            { MediaUtils.FormatFamily.V_M4v, new(
                ResolutionArray,
                FrameRateArray,
                new[] { VideoCodec.Auto, VideoCodec.H264 },
                VideoBitrateArray,
                ChannelArray,
                SampleRateArray,
                new[] { AudioCodec.Auto, AudioCodec.AAC },
                AudioBitrateArray
                ) },
            { MediaUtils.FormatFamily.V_Asf, new (
                ResolutionArray,
                FrameRateArray,
                new[] { VideoCodec.Auto, VideoCodec.MPEG4 },
                VideoBitrateArray,
                new[] { Channel.Auto, Channel._1, Channel._2 },
                SampleRateArray,
                new[] { AudioCodec.Auto, AudioCodec.WMA2, AudioCodec.WMA1 },
                AudioBitrateArray
                ) },
            { MediaUtils.FormatFamily.V_Vob, new(
                ResolutionArray,
                FrameRateArray,
                new[] { VideoCodec.Auto, VideoCodec.MPEG2 },
                VideoBitrateArray,
                new[] { Channel.Auto, Channel._1, Channel._2 },
                SampleRateArray,
                new[] { AudioCodec.Auto, AudioCodec.AC3, AudioCodec.MP2 },
                new[] { AudioBitrate.Auto, AudioBitrate._64, AudioBitrate._96, AudioBitrate._128 }
                ) },
            { MediaUtils.FormatFamily.V_Ogv, new(
                ResolutionArray,
                FrameRateArray,
                new[] { VideoCodec.Auto, VideoCodec.THEORA },
                VideoBitrateArray,
                new[] { Channel.Auto, Channel._2, Channel._6, Channel._8 },
                new[] { SampleRate.Auto, SampleRate._32000, SampleRate._44100, SampleRate._48000 },
                new[] { AudioCodec.Auto, AudioCodec.VORBIS },
                AudioBitrateArray
                ) },
            { MediaUtils.FormatFamily.V_Flv, new(
                ResolutionArray,
                FrameRateArray,
                new[] { VideoCodec.Auto, VideoCodec.H264 },
                VideoBitrateArray,
                new[] { Channel.Auto, Channel._1, Channel._2 },
                SampleRateArray,
                new[] { AudioCodec.Auto, AudioCodec.MP3 },
                AudioBitrateArray
                ) },
        };

        public static readonly Dictionary<MediaUtils.FormatFamily, AudioContainerRecord> AUDIO_CONTAINERS = new()
        {
            { MediaUtils.FormatFamily.A_Aac, new(
                ChannelArray,
                SampleRateArray,
                new[] { AudioCodec.AAC },
                AudioBitrateArray
                ) },
            { MediaUtils.FormatFamily.A_Ac3, new(
                new[] { Channel.Auto, Channel._1, Channel._2, Channel._6 },
                new[] { SampleRate.Auto, SampleRate._32000, SampleRate._44100, SampleRate._48000 },
                new[] { AudioCodec.AC3 },
                AudioBitrateArray
                ) },
            { MediaUtils.FormatFamily.A_Aiff, new(
                ChannelArray,
                SampleRateArray,
                new[] { AudioCodec.PCM_F32BE, AudioCodec.PCM_F64BE, AudioCodec.PCM_ALAW, AudioCodec.PCM_MULAW, AudioCodec.PCM_U8, AudioCodec.PCM_S8, AudioCodec.PCM_S16LE, AudioCodec.PCM_S16BE, AudioCodec.PCM_S24BE, AudioCodec.PCM_S32BE },
                AudioBitrateArray
                ) },
            { MediaUtils.FormatFamily.A_Caf, new(
                ChannelArray,
                SampleRateArray,
                new[] { AudioCodec.ALAC, AudioCodec.PCM_S8, AudioCodec.PCM_S16BE, AudioCodec.PCM_S24BE, AudioCodec.PCM_S32BE, AudioCodec.PCM_F32BE, AudioCodec.PCM_F64BE },
                AudioBitrateArray
                ) },
            { MediaUtils.FormatFamily.A_Flac, new(
                ChannelArray,
                SampleRateArray,
                new[] { AudioCodec.FLAC },
                AudioBitrateArray
                ) },
            { MediaUtils.FormatFamily.A_M4a, new(
                ChannelArray,
                SampleRateArray,
                new[] { AudioCodec.AAC },
                AudioBitrateArray
                ) },
            { MediaUtils.FormatFamily.A_M4b, new(
                ChannelArray,
                SampleRateArray,
                new[] { AudioCodec.AAC },
                AudioBitrateArray
                ) },
            { MediaUtils.FormatFamily.A_Mp2, new(
                new[] { Channel.Auto, Channel._1, Channel._2 },
                SampleRateArray,
                new[] { AudioCodec.MP2 },
                new[] { AudioBitrate.Auto, AudioBitrate._64, AudioBitrate._96 }
                ) },
            { MediaUtils.FormatFamily.A_Mp3, new(
                new[] { Channel.Auto, Channel._1, Channel._2 },
                SampleRateArray,
                new[] { AudioCodec.MP3 },
                AudioBitrateArray
                ) },
            { MediaUtils.FormatFamily.A_Ogg, new(
                new[] { Channel.Auto, Channel._2 },
                new[] { SampleRate.Auto, SampleRate._32000, SampleRate._44100, SampleRate._48000 },
                new[] { AudioCodec.VORBIS, AudioCodec.FLAC },
                AudioBitrateArray
                ) },
            { MediaUtils.FormatFamily.A_Wav, new(
                ChannelArray,
                SampleRateArray,
                new[] {
                    AudioCodec.PCM_S16LE,
                    AudioCodec.PCM_S24LE,
                    AudioCodec.PCM_S32LE,
                },
                AudioBitrateArray
                ) },
            { MediaUtils.FormatFamily.A_Wma, new(
                new[] { Channel.Auto, Channel._1, Channel._2 },
                SampleRateArray,
                new[] { AudioCodec.WMA1, AudioCodec.WMA2 },
                AudioBitrateArray
                ) },
        };

        //

        public enum SortType
        {
            LastModified,
            A2Z
        }

        public static readonly Dictionary<SortType, Tuple<string, string>> SORTS = new()
        {
            { SortType.LastModified, Tuple.Create("Features_SortLastPlayed", "Features_SortLastModified") },
            { SortType.A2Z, Tuple.Create("Features_SortA2Z", "Features_SortA2Z") }
        };

        public enum ExportType
        {
            Export,
            ExportUnlockedCopy,
            ExportLockedCopy,
        }

        public static readonly Dictionary<ExportType, string> EXPORTS = new()
        {
            { ExportType.Export, "Features_ExportItemToDestination"},
            { ExportType.ExportUnlockedCopy, "Features_ExportUnlockedCopyItemToDestination"},
            { ExportType.ExportLockedCopy, "Features_ExportLockedCopyItemToDestination"},
        };

        public class Metadata : IOItem
        {
            public static readonly Tuple<string, string>[] METADATAS =
            {
                new("Info_MetadataExif", "Types_RemoveExifMetadata"),
                new("Info_MetadataXmp", "Types_RemoveXmpMetadata"),
                new("Info_MetadataIptc", "Types_RemoveIptcMetadata")
            };

            public string Text { get; set; }
            public string Tag { get; set; }

            private bool _isChecked = false;
            public bool IsChecked { get => _isChecked; set => SetAndNotify(ref _isChecked, value); }

            private bool _isEnabled = false;
            public bool IsEnabled { get => _isEnabled; set => SetAndNotify(ref _isEnabled, value); }
        }
    }
}