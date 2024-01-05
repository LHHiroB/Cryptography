#if DEBUG
using System;
using System.Collections.Generic;
using ImageMagick;
using H.Hooks;
using Windows.Win32;
using Windows.Win32.Foundation;
using IOCore.Libs;

namespace IOCore
{
    internal class IODev
    {
        private static readonly Lazy<IODev> lazy = new(() => new());
        public static IODev Inst => lazy.Value;

        private LowLevelKeyboardHook _keyboardHook;
        private IProgress<bool> _progress;

        private IODev()
        {
        }

        public void Init()
        {
            _progress = new Progress<bool>(async reveal =>
            {
                if (!PInvoke.GetWindowRect(new(IOWindow.Inst.HandleIntPtr), out RECT rect)) return;

                var snapshot = Utils.TakeScreenSnapshot(
                    Utils.Round(rect.X + (rect.Width - IOWindow.Inst.Content.ActualSize.X) / 2),
                    Utils.Round(rect.Y + (rect.Height - IOWindow.Inst.Content.ActualSize.Y) / 2) - 4,
                    Utils.Round(IOWindow.Inst.Content.ActualSize.X),
                    Utils.Round(IOWindow.Inst.Content.ActualSize.Y));

                var storageFile = await IOWindow.Inst.PickSaveFileAsync(picker =>
                {
                    picker.SuggestedFileName = "1";
                    picker.FileTypeChoices.Add("Image", new List<string>() { ".png", ".bmp", ".webp", ".gif", ".jpeg", ".emf", ".icon", ".tiff" });
                });

                if (storageFile == null) return;

                snapshot.Save(storageFile.Path);

                //

                var radius = 10;

                var image = new MagickImage(storageFile.Path, new MagickReadSettings()
                {
                    BackgroundColor = MagickColors.Transparent,
                    ColorSpace = ColorSpace.RGB
                });

                using var mask = new MagickImage(MagickColors.Black, image.Width, image.Height);
                mask.Draw(new DrawableRoundRectangle(0, 0, image.Width - 1, image.Height - 1, radius, radius), new DrawableFillColor(MagickColors.White));

                using (var selfMask = image.Clone())
                {
                    selfMask.Alpha(AlphaOption.Extract);
                    selfMask.Opaque(MagickColors.White, MagickColors.Transparent);
                    mask.Composite(selfMask, CompositeOperator.Multiply);
                }

                mask.HasAlpha = false;
                image.HasAlpha = false;
                image.Composite(mask, CompositeOperator.CopyAlpha);

                image.Write(storageFile.Path);

                image.Dispose();

                if (reveal) Utils.RevealInFileExplorer(storageFile.Path);
            });

            _keyboardHook = new()
            {
                IsLeftRightGranularity = true,
                HandleModifierKeys = true,
                Handling = true
            };

            //_keyboardHook.Up += (sender, e) =>
            //{
            //    if (e.Keys.Are(Key.LControl, Key.LShift, Key.S) || e.Keys.Are(Key.LControl, Key.LShift, Key.R))
            //        e.IsHandled = true;
            //};

            _keyboardHook.Down += (sender, e) =>
            {
                if (e.Keys.Are(Key.LControl, Key.LShift, Key.S))
                {
                    _progress.Report(false);
                    //e.IsHandled = true;
                }
                else if (e.Keys.Are(Key.LControl, Key.LShift, Key.R))
                {
                    _progress.Report(true);
                    //e.IsHandled = true;
                }
            };

            _keyboardHook.Start();
        }
    }
}
#endif