using System;
using YukimaruGames.Editor.Tools.AudioPlayer.Domain;

namespace YukimaruGames.Editor.Tools
{
    internal interface IAudioPlaybackService : IDisposable
    {
        internal AudioPlaybackState State { get; }
        internal PlaylistItem CurrentPlayingItem { get; }
        internal event Action<PlaylistItem> OnMusicChanged;
        internal event Action<AudioPlaybackState> OnPlaybackStateChanged;
        internal event Action<bool> OnLoopChanged;
        internal event Action<float> OnVolumeChanged;
        internal event Action<float> OnPlaybackTimeUpdated;
        internal event Action OnPlaybackFinished;
        bool Loop { get; set; }
        float Volume { get; set; }
        float Time { get; set; }
        void Play();
        void Stop();
        void Pause();
        void Resume();
        void Set(PlaylistItem item);
    }
}
