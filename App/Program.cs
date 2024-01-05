using System;
using System.Linq;
using System.Threading;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.Windows.AppLifecycle;
using WinRT;
using System.Threading.Tasks;
using Windows.Win32;
using Windows.Win32.Foundation;

namespace IOApp
{
    public class Program
    {
        [STAThread]
        static int Main()
        {
            IOApp.InitMeta();

            ComWrappersSupport.InitializeComWrappers();
                
            var instanceKey = App.EvaluateInstance(AppInstance.GetCurrent().GetActivatedEventArgs());
            var instance = AppInstance.FindOrRegisterForKey(instanceKey);

            if (instance.IsCurrent)
            {
                instance.Activated += OnActivated;

                Application.Start((p) =>
                {
                    SynchronizationContext.SetSynchronizationContext(
                        new DispatcherQueueSynchronizationContext(
                            DispatcherQueue.GetForCurrentThread()
                        )
                    );

                    _ = new App();
                });
            }
            else
                RedirectActivationTo(AppInstance.GetCurrent().GetActivatedEventArgs(), instance);

            return 0;
        }

        public static void RedirectActivationTo(AppActivationArguments args, AppInstance keyInstance)
        {
            var redirectEventHandle = PInvoke.CreateEvent(null, true, false, (string)null);

            Task.Run(() =>
            {
                keyInstance.RedirectActivationToAsync(args).AsTask().Wait();
                PInvoke.SetEvent(redirectEventHandle);
            });

            _ = PInvoke.CoWaitForMultipleObjects(0, 0xFFFFFFFF, new HANDLE[] { new(redirectEventHandle.DangerousGetHandle()) }, out uint handleIndex);
        }

        public static void RedirectActivationTo(string key) => RedirectActivationTo(AppInstance.GetCurrent().GetActivatedEventArgs(), AppInstance.GetInstances().FirstOrDefault(i => i.Key == key));

        public static void OnActivated(object sender, AppActivationArguments args)
        {
            _ = args.Kind;
            App.Inst?.InitialArgsProgress?.Report(args);
        }
    }
}
