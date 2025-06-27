#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using YukimaruGames.Editor.Tools.AudioPlayer.Application;
using YukimaruGames.Editor.Tools.AudioPlayer.Domain;
using Object = UnityEngine.Object;

namespace YukimaruGames.Editor.Tools.AudioPlayer.Infrastructure
{
    public sealed class AudioClipRepository : IAudioClipRepository
    {
        private readonly List<AudioClipReference> _list = new();

        private Action<string, AudioClip> _onAddElement;

        private Action<string> _onRemoveElement;

        private IAudioClipRepository Self => this;


        event Action<string,AudioClip> IAudioClipRepository.OnAddElement
        {
            add => _onAddElement += value;
            remove => _onAddElement -= value;
        }

        event Action<string> IAudioClipRepository.OnRemoveElement
        {
            add => _onRemoveElement += value;
            remove => _onRemoveElement -= value;
        }

        IReadOnlyList<AudioClipReference> IAudioClipRepository.List => _list;
        
        void IAudioClipRepository.Clear()
        {
            foreach (var reference in _list)
            {
                Destroy(reference.AudioClip);
            }

            _list.Clear();
        }

        bool IAudioClipRepository.TryFind(string key, out AudioClip clip) => (clip = Self.Find(key)) != null;
        bool IAudioClipRepository.TryFind(int index, out AudioClip clip) => (clip = Self.Find(index)) != null;
        AudioClip IAudioClipRepository.Find(string key) => _list.Find(x => x.Key == key)?.AudioClip;
        AudioClip IAudioClipRepository.Find(int index) => index < _list.Count ? _list[index].AudioClip : null;
        int IAudioClipRepository.FindIndex(string key)
        {
            return _list.FindIndex(x => x.Key == key);
        }
        bool IAudioClipRepository.TryAdd(string key, AudioClip clip)
        {
            if (_list.Find(x => x.Key == key) != null)
            {
                return false;
            }

            _onAddElement?.Invoke(key, clip);
            _list.Add(new AudioClipReference(key, clip));
            return true;
        }

        bool IAudioClipRepository.Remove(int index)
        {
            if (0 <= index && index < _list.Count)
            {
                var key = _list[index].Key;
                _list.RemoveAt(index);
                _onRemoveElement?.Invoke(key);
                return true;
            }
            
            return false;
        }

        bool IAudioClipRepository.Remove(string key)
        {
            var index = Self.FindIndex(key);
            if (0 <= index)
            {
                _list.RemoveAt(index);
                _onRemoveElement?.Invoke(key);
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
