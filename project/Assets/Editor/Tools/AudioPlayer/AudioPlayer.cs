#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using UnityEngine;

namespace YukimaruGames.Editor.Tools
{
    public static class AudioPlayer
    {
        private static AudioClip _clip;
        private static readonly Dictionary<string, AudioClip> AudioClips = new();

        public static bool CanPlay() => _clip != null;
        
        public static void Set(string filePath, Action<float> onUpdateProgress)
        {
            SafeStop();
            
            var url = AudioClipLoader.GetUrl(filePath);

            if (AudioClips.TryGetValue(url, out _clip)) return;
            
            AudioClipLoader.OnUpdateProgress += onUpdateProgress;
            var e = AudioClipLoader.Load(url);
            while (e.MoveNext())
            {
                var current = e.Current;
                    
                if (current == null) continue;
                    
                AudioClips.TryAdd(url, current);
                _clip = current;        
                break;
            }
            AudioClipLoader.OnUpdateProgress -= onUpdateProgress;
        }

        public static void Play()
        {
            if (_clip == null)
            {
                throw new InvalidOperationException($"Audio clip is null.");
            }

            InternalAudioUtilProxy.PlayPreviewClip(_clip);
        }

        public static void Stop()
        {
            if (_clip == null)
            {
                throw new InvalidOperationException($"Audio clip is null.");
            }
            InternalAudioUtilProxy.StopAllPreviewClips();
        }

        private static void SafeStop()
        {
            if (IsPlaying())
            {
                Stop();
            }
        }

        public static bool IsPlaying() => _clip != null && InternalAudioUtilProxy.IsPreviewClipPlaying(_clip);
    }
}
#endif
