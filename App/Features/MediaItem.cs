using System;

namespace IOApp.Features
{
    internal class MediaItem : FileItem
    {
        private bool _isPlaying;
        public bool IsPlaying { get => _isPlaying; set => SetAndNotify(ref _isPlaying, value); }

        public TimeSpan InputDuration => RecoveredInfo?.MediaAnalysis?.Duration ?? TimeSpan.FromSeconds(0);

        public bool IsVideo => RecoveredInfo?.MediaAnalysis?.PrimaryVideoStream != null;

        public int Rotation => RecoveredInfo?.MediaAnalysis?.PrimaryVideoStream?.Rotation ?? 0;
        public int MediaWidth => RecoveredInfo?.MediaAnalysis?.PrimaryVideoStream?.Width ?? 1;
        public int MediaHeight => RecoveredInfo?.MediaAnalysis?.PrimaryVideoStream?.Height ?? 1;

        public int CorrectWidth => Rotation == -90 || Rotation == 90 ? MediaHeight : MediaWidth;
        public int CorrectHeight => Rotation == -90 || Rotation == 90 ? MediaWidth : MediaHeight;

        //

        public uint FlyleafInitRotation 
        {
            get
            {
                if (Rotation == -90) return 180;
                else if (Rotation == -180) return 180;
                else if (Rotation == -270) return 180;
                return (uint)Rotation;
            }
        }

        public int FlyleafInitWidth => FlyleafInitRotation == 90 || FlyleafInitRotation == 270 ? CorrectHeight : CorrectWidth;
        public int FlyleafInitHeight => FlyleafInitRotation == 90 || FlyleafInitRotation == 270 ? CorrectWidth : CorrectHeight;

        public static MediaItem FromFSItem(FileItem inputItem)
        {
            var item = new MediaItem(inputItem.RecoveredFileOrFolderPath)
            {
                RecoveredInfo = inputItem.RecoveredInfo,
                FileType = inputItem.FileType
            };

            return item;
        }

        public MediaItem(string path) : base(path)
        {
        }

        public MediaItem(FileEntity file) : base(file)
        {
        }
    }
}