#if UNITY_EDITOR
using System;
using UnityEngine;
using YukimaruGames.Editor.Tools.AudioPlayer.Domain;

namespace YukimaruGames.Editor.Tools.AudioPlayer.Application
{
    internal interface IAudioPlaybackPresenter
    {
        internal PlaylistItem CurrentPlayingMusic { get; set; }
        internal PlaylistItem NextPlayRequest { get; set; }
        internal bool IsPlaying { get; }
        internal bool Loop { get; set; }
        internal bool Shuffle { get; set; }
        internal float Time { get; set; }
        internal float Volume { get; set; }
        
        internal event Action<PlaylistItem> OnMusicChanged;
        internal event Action<AudioPlaybackState> OnPlaybackStateChanged;
        internal event Action<float> OnPlaybackTimeUpdated;
        internal event Action OnPlaybackFinished;
        internal event Action<float> OnVolumeChanged;
        internal event Action<bool> OnShuffleChanged;

        internal event Action OnClickNextMusic;
        internal event Action OnClickPreviousMusic;

        internal void Play();
        internal void Stop();
        internal void Pause();
        internal void Resume();
    }
}
#endif