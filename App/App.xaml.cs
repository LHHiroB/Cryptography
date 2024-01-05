using System;
using System.Linq;
using Microsoft.Windows.AppLifecycle;
using Microsoft.UI.Xaml;
using IOApp.Windows;
using IOCore;

namespace IOApp
{
    public partial class App : Application
    {
        public static readonly string[] WINDOW_NAMES =
        {
            nameof(MainWindow),
        };

        public static IOWindow GetWindow(string name) => name switch
        {
            nameof(MainWindow) => MainWindow.Inst,
            _ => null
        };

        public enum CommandType
        {
            Undefined,
        }

        public class AppInfo
        {
            public Type WindowType;
            public bool IsReusable;
            public CommandType Command;
            public string[] Params;
        }

        public static App Inst { get; private set; }
        
        private Window _window;
        public AppInfo Info { get; private set; }

        public IProgress<AppActivationArguments> InitialArgsProgress;

        public App()
        {
            InitializeComponent();
            Inst = this;

            Info = CreateAppInfoFromArgs(AppInstance.GetCurrent().GetActivatedEventArgs());

            InitialArgsProgress = new Progress<AppActivationArguments>(args =>
            {
                if (CurrentWindow == null) return;

                // HACK: Avoiding exception when redirecting to instances other than MainWindow
                if (AppInstance.GetCurrent().Key.EndsWith(nameof(MainWindow)))
                    Info = CreateAppInfoFromArgs(args);

                foreach (var i in WINDOW_NAMES)
                    GetWindow(i)?.HandleArgsData();
            });

            IOApp.InitUi(Current);
            IOApp.InitExt();
        }

        public AppInfo CreateAppInfoFromArgs(AppActivationArguments args)
        {
            var info = new AppInfo
            {
                WindowType = typeof(MainWindow),
                Command = CommandType.Undefined,
                Params = Array.Empty<string>(),
                IsReusable = true,
            };

            return info;
        }

        public static IOWindow CurrentWindow => GetWindow(WINDOW_NAMES.FirstOrDefault(i => AppInstance.GetCurrent().Key == Meta.IO_APP_ID + i));

        public static string EvaluateInstance(AppActivationArguments args)
        {
            _ = args.Kind;
            
            string instanceKey = null;

            instanceKey ??= Meta.IO_APP_ID + nameof(MainWindow);

            return instanceKey;
        }

        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
#if INIT
            IOInit.Init();
            Current.Exit();
#else
			_window = new MainWindow();

            _window.Activate();
#endif
        }
    }
}