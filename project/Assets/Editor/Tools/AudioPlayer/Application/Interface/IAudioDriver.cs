#if UNITY_EDITOR
using System;
using UnityEngine;

namespace YukimaruGames.Editor.Tools
{
    internal interface IAudioDriver
    {
        internal AudioClip CurrentClip { get; set; }
        internal bool IsPlaying { get; }

        internal event Action<AudioClip> OnClipChanged;
        internal event Action<bool> OnLoopChanged;
        internal event Action<float> OnVolumeChanged;
        internal event Action<float> OnTimeUpdated;
        internal event Action OnPlayStarted;
        internal event Action OnPlayStopped;
        internal event Action OnPlayPaused;
        internal event Action OnPlayResumed;
        internal event Action OnPlaybackFinished;
        
        internal bool Loop { get; set; }
        internal float Volume { get; set; }
        internal float Time { get; set; }

        internal void Play();
        internal void Stop();
        internal void Pause();
        internal void Resume();
    }
}
#endif
