using System.IO;
using System.Collections.Generic;
using IOCore;
using IOCore.Libs;

namespace IOApp.Features
{
    internal abstract class FileSystemItem : IOItem
    {
        public enum StatusType
        {
            Ready,
            ProcessInQueue,
            Processing,
            Processed,
            ProcessFailed,
            ProcessPaused,
            ProcessStopped
        };

        public enum InfoType
        {
            Input,
            Storage,
            Recovered,
            Output,
        };

        public enum PrivacyType
        {
            None,
            Blur
        };

        public static readonly Dictionary<StatusType, string> STATUSES = new()
        {
            { StatusType.Ready, "Item_StatusReady" },
            { StatusType.ProcessInQueue, "Item_StatusProcessInQueue"},
            { StatusType.Processing, "Item_StatusProcessing"},
            { StatusType.Processed, "Item_StatusProcessed"},
            { StatusType.ProcessFailed, "Item_StatusProcessFailed"},
            { StatusType.ProcessPaused, "Item_StatusProcessPaused"},
            { StatusType.ProcessStopped, "Item_StatusProcessStopped"}
        };

        public void RaisePropertyChanged(string name)
        {
            Notify(new(name));
        }

        //

        protected StatusType _status;
        public StatusType Status
        {
            get => _status;
            set
            {
                SetAndNotify(ref _status, value);
                Notify(nameof(StatusText));
            }
        }

        public string StatusText => ProtectedResourceLoader.GetString(STATUSES[_status]);

        protected FileUtils.Type _fileType;
        public FileUtils.Type FileType { get => _fileType; set => SetAndNotify(ref _fileType, value); }
     
        protected bool _isEnabled;
        public bool IsEnabled { get => _isEnabled; set => SetAndNotify(ref _isEnabled, value); }

        protected bool _isExist = true;
        public bool IsExist { get => _isExist; set => SetAndNotify(ref _isExist, value); }

        protected bool _isCorrupted;
        public bool IsCorrupted { get => _isCorrupted; set => SetAndNotify(ref _isCorrupted, value); }

        protected string _errorText = string.Empty;
        public string ErrorText { get => _errorText; set => SetAndNotify(ref _errorText, value); }

        //

        public FileSystemInfoProxy InputInfo { get; set; }

        public string InputFileOrFolderPath => InputInfo?.Path ?? string.Empty;
        public string InputFileExtension => Path.GetExtension(InputFileOrFolderPath);
        public string InputFileOrFolderName => Path.GetFileNameWithoutExtension(InputFileOrFolderPath);
        public string InputFolderPath => Path.GetDirectoryName(InputFileOrFolderPath);

        public long InputSize
        {
            get
            {
                if (InputInfo?.FileSystemInfo is FileInfo fileInfo)
                    return fileInfo.Length;
                return 0;
            }
        }

        public string InputCreationTimeText => InputInfo?.FileSystemInfo?.CreationTime.ToString("G") ?? "∞";
        public string InputLastWriteTimeText => InputInfo?.FileSystemInfo?.LastWriteTime.ToString("G") ?? "∞";

        //

        protected string _outputFileOrFolderPath;
        public string OutputFileOrFolderPath { get => _outputFileOrFolderPath; set => SetAndNotify(ref _outputFileOrFolderPath, value); }

        public string OutputFileExtension => Path.GetExtension(OutputFileOrFolderPath);
        public string OutputFileOrFolderName => Path.GetFileNameWithoutExtension(OutputFileOrFolderPath);
        public string OutputFolderPath => Path.GetDirectoryName(OutputFileOrFolderPath);

        //

        public bool IsAvailable { get; set; }

        public bool UpdateAndGetIsAvailable(bool isAvailable, bool raise)
        {
            IsAvailable = isAvailable;

            if (raise)
                Notify(nameof(IsAvailable));

            return IsAvailable;
        }

        //

        public FileSystemItem(string path)
        {
            InputInfo = new(path);
            _fileType = FileUtils.GetType(path);
            _status = StatusType.Ready;
            _isEnabled = true;
        }

        public FileSystemItem(FileEntity file)
        {
            _fileType = file.FileType;
            _status = StatusType.Ready;
            _isEnabled = true;
        }

        public void LoadBasicInfo()
        {
            InputInfo?.LoadInfo();

            IsExist = InputInfo.FileSystemInfo != null;

            Notify(nameof(InputFileOrFolderPath));
            Notify(nameof(InputFileOrFolderName));
            Notify(nameof(InputSize));
            Notify(nameof(InputCreationTimeText));
            Notify(nameof(InputLastWriteTimeText));
        }

        public void SetError(string error, bool raised)
        {
            _errorText = error;

            if (raised)
                Notify(nameof(ErrorText));
        }
    }
}