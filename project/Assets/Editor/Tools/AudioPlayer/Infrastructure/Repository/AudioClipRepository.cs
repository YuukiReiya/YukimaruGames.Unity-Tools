#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using YukimaruGames.Editor.Tools.AudioPlayer.Application;
using YukimaruGames.Editor.Tools.AudioPlayer.Domain;

namespace YukimaruGames.Editor.Tools.AudioPlayer.Infrastructure
{
    public sealed class AudioClipRepository : IAudioClipRepository
    {
        private readonly List<AudioClipReference> _list = new();
        private readonly Dictionary<string, AudioClip> _dic = new();

        private IAudioClipRepository Self => this;

        IReadOnlyList<AudioClipReference> IAudioClipRepository.List => _list;
        
        void IAudioClipRepository.Clear()
        {
            foreach (var reference in _list)
            {
                Destroy(reference.AudioClip);
            }

            _list.Clear();

            foreach (var kvp in _dic)
            {
                Destroy(kvp.Value);
            }

            _dic.Clear();
        }

        bool IAudioClipRepository.TryFind(string key, out AudioClip clip) => (clip = Self.Find(key)) != null;
        bool IAudioClipRepository.TryFind(int index, out AudioClip clip) => (clip = Self.Find(index)) != null;
        AudioClip IAudioClipRepository.Find(string key) => _dic.GetValueOrDefault(key, null);
        AudioClip IAudioClipRepository.Find(int index) => index < _list.Count ? _list[index].AudioClip : null;
        int IAudioClipRepository.FindIndex(string key)
        {
            if (_dic.TryGetValue(key, out _))
            {
                return _list.FindIndex(x => x.Key == key);
            }

            return -1;
        }
        bool IAudioClipRepository.TryAdd(string key, AudioClip clip)
        {
            if (_dic.TryAdd(key, clip))
            {
                _list.Add(new AudioClipReference(key, clip));
                return true;
            }

            return false;
        }

        private static void Destroy(AudioClip audioClip)
        {
            if (audioClip == null)
            {
                return;
            }
            
            if (EditorApplication.isPlaying)
            {
                Object.DestroyImmediate(audioClip);
            }
            else
            {
                Object.Destroy(audioClip);
            }
        }
    }
}
#endif
