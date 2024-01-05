using Microsoft.UI.Xaml;
using ImageMagick;
using IOCore;
using IOCore.Libs;

namespace IOApp
{
    internal class IOApp
    {
        internal static void InitMeta()
        {
#if DEBUG
            StoreManager.DevLicense = StoreManager.License.Default;
#endif
        }

        internal static void InitUi(Application app)
        {
            Meta.WINDOW_INIT_SIZE = new(1280, 720 + 32);

            ThemeManager.THEMES[ThemeManager.Theme.Dark].IsOn = true;
            ThemeManager.THEMES[ThemeManager.Theme.Light].IsOn = true;

            ThemeManager.Init(app);
            LanguageManager.Init();
        }

        internal static void InitExt()
        {
            ResourceLimits.LimitMemory(new Percentage(90));

            FFMpegCore.GlobalFFOptions.Configure(new FFMpegCore.FFOptions { BinaryFolder = Meta.EXTERNAL_DIR, TemporaryFilesFolder = Meta.TEMP_DIR });

            FlyleafLib.Engine.Start(new()
            {
                FFmpegPath = Meta.EXTERNAL_DIR,
                FFmpegDevices = false,    // Prevents loading avdevice/avfilter dll files. Enable it only if you plan to use dshow/gdigrab etc.
                FFmpegLogLevel = FlyleafLib.FFmpegLogLevel.Quiet,
                LogLevel = FlyleafLib.LogLevel.Quiet,
                //LogOutput         = ":console",
                //LogOutput         = @"C:\Flyleaf\Logs\flyleaf.log",                
                //PluginsPath       = @"C:\Flyleaf\Plugins",
                UIRefresh = false,    // Required for Activity, BufferedDuration, Stats in combination with Config.Player.Stats = true
                UIRefreshInterval = 250,      // How often (in ms) to notify the UI
                UICurTimePerSecond = true,     // Whether to notify UI for CurTime only when it's second changed or by UIRefreshInterval
            });

            Features.Share.EnsureDirs();
        }
    }
}