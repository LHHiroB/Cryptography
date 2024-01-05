using FlyleafLib;
using FlyleafLib.MediaPlayer;
using System;
using System.Collections.Generic;
using Windows.Graphics;

namespace IOApp.Features
{
    internal partial class IOPlayer : Player
    {
        public enum PlayEndedType
        {
            AutoPlay,
            Replay,
            Stop
        }

        public static readonly Dictionary<PlayEndedType, Tuple<string, string>> PLAY_ENDEDS = new()
        {
            { PlayEndedType.AutoPlay, Tuple.Create("\uEC57", "Features_AutoPlay") },
            { PlayEndedType.Replay,   Tuple.Create("\uEF3B", "Features_Replay") },
            { PlayEndedType.Stop,     Tuple.Create("\uE71A", "Features_Stop") },
        };

        public enum SpeedType
        {
            _0_125,
            _0_25,
            _0_5,
            _0_75,
            _1,
            _1_25,
            _1_5,
            _1_75,
            _2,
            _2_5,
            _3,
            _4,
            _5
        }

        public static readonly Dictionary<SpeedType, Tuple<bool, string, double>> SPEEDS = new()
        {
            { SpeedType._0_125, Tuple.Create(false, "0.125x", 0.125) },
            { SpeedType._0_25,  Tuple.Create(true,  "0.25x", 0.25) },
            { SpeedType._0_5,   Tuple.Create(true,  "0.5x", 0.5) },
            { SpeedType._0_75,  Tuple.Create(true,  "0.75x", 0.75) },
            { SpeedType._1,     Tuple.Create(true,  "1x", 1.0) },
            { SpeedType._1_25,  Tuple.Create(true,  "1.25x", 1.25) },
            { SpeedType._1_5,   Tuple.Create(true,  "1.5x", 1.5) },
            { SpeedType._1_75,  Tuple.Create(true,  "1.75x", 1.75) },
            { SpeedType._2,     Tuple.Create(true,  "2x", 2.0) },
            { SpeedType._2_5,   Tuple.Create(false, "2.5x", 2.5) },
            { SpeedType._3,     Tuple.Create(false, "3x", 3.0) },
            { SpeedType._4,     Tuple.Create(false, "4x", 4.0) },
            { SpeedType._5,     Tuple.Create(false, "5x", 5.0) },
        };

        private MediaItem _previousMediaItem;
        private MediaItem _currentMediaItem;
        public MediaItem CurrentMediaItem 
        {
            get => _currentMediaItem;
            set 
            {
                _previousMediaItem = _currentMediaItem;
                _currentMediaItem = value;

                Raise(nameof(IsVideo));
            }
        }

        public int VolumeLevel => Audio.Volume switch
        {
            0 => 0,
            < 30 => 1,
            < 60 => 2,
            _ => 3
        };

        public string VolumeIcon => Audio.Mute ? "\uE74F" : VolumeLevel switch
        {
            0 => "\uE992",
            1 => "\uE993",
            2 => "\uE994",
            _ => "\uE995"
        };

        public void RaiseVolume()
        {
            Raise(nameof(VolumeLevel));
            Raise(nameof(VolumeIcon));
        }

        private PlayEndedType _playEndedOption;
        public PlayEndedType PlayEndedOption
        {
            get => _playEndedOption;
            set
            {
                Set(ref _playEndedOption, value);
                Raise(nameof(PlayEndedIcon));
                Raise(nameof(PlayEndedText));
            }
        }
        public string PlayEndedIcon => PLAY_ENDEDS[_playEndedOption].Item1;
        public string PlayEndedText => PLAY_ENDEDS[_playEndedOption].Item2;

        private SpeedType _speedOption;
        public SpeedType SpeedOption { get => _speedOption; set => Set(ref _speedOption, value); }

        private bool _isShuffled;
        public bool IsShuffled { get => _isShuffled; set => Set(ref _isShuffled, value); }

        public bool IsVideo => _currentMediaItem?.IsVideo ?? false;

        public static IOPlayer Create()
        {
            var config = new Config();
            config.Video.BackgroundColor = System.Windows.Media.Colors.Black;
            config.Player.VolumeMax = 100;

            var player = new IOPlayer(config);
            player.Audio.Volume = 75;

            return player;
        }

        private IOPlayer(Config config) : base(config)
        {
        }

        internal SizeInt32 GetFlyleafVideoResolution()
        {
            var size = new SizeInt32(_currentMediaItem.FlyleafInitWidth, _currentMediaItem.FlyleafInitHeight);

            var delta = Math.Abs((long)Rotation - (long)CurrentMediaItem.FlyleafInitRotation);

            if (delta == 90 || delta == 270)
            {
                size.Width = CurrentMediaItem.FlyleafInitHeight;
                size.Height = CurrentMediaItem.FlyleafInitWidth;
            }

            return size;
        }

        public void Play(bool replay)
        {
            if (_previousMediaItem != null)
                _previousMediaItem.IsPlaying = false;
            _currentMediaItem.IsPlaying = true;

            if (!replay)
                Rotation = _currentMediaItem.FlyleafInitRotation;

            Open(_currentMediaItem.RecoveredFileOrFolderPath);
            Play();
        }

        public void TogglePlayPauseResume()
        {
            if (CanPlay)
            {
                if (IsPlaying)
                    Pause();
                else
                {
                    if (Status == Status.Ended)
                        SeekAccurate(0);
                    Play();
                }
            }

            CurrentMediaItem.IsPlaying = IsPlaying;
        }

        public void OnPlaybackStopped()
        {
            if (Status == Status.Ended)
            {
                if (_currentMediaItem != null)
                    _currentMediaItem.IsPlaying = false;
            }
        }
    }
}
