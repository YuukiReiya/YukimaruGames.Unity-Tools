#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;
using YukimaruGames.Editor.Tools.AudioPlayer.Application;
using YukimaruGames.Editor.Tools.AudioPlayer.Domain;
using YukimaruGames.Editor.Tools.AudioPlayer.Infrastructure;

namespace YukimaruGames.Editor.Tools
{
    internal sealed class InternalAudioClipRepository : IAudioClipRepository
    {
        private static InternalAudioClipRepository _instance;
        internal static InternalAudioClipRepository Instance => _instance ??= new InternalAudioClipRepository();

        private readonly IAudioClipRepository _repository;

        internal InternalAudioClipRepository()
        {
            _repository = new AudioClipRepository();
        }

        event Action<string, AudioClip> IAudioClipRepository.OnAddElement
        {
            add => _repository.OnAddElement += value;
            remove => _repository.OnAddElement -= value;
        }

        event Action<string> IAudioClipRepository.OnRemoveElement
        {
            add => _repository.OnRemoveElement += value;
            remove => _repository.OnRemoveElement -= value;
        }

        IReadOnlyList<AudioClipReference> IAudioClipRepository.List => _repository.List;
        
        void IAudioClipRepository.Clear() => _repository.Clear();
        bool IAudioClipRepository.TryFind(string key, out AudioClip clip) => _repository.TryFind(key, out clip);
        bool IAudioClipRepository.TryFind(int index, out AudioClip clip) => _repository.TryFind(index, out clip);
        AudioClip IAudioClipRepository.Find(string key) => _repository.Find(key);
        AudioClip IAudioClipRepository.Find(int index) => _repository.Find(index);
        int IAudioClipRepository.FindIndex(string key) => _repository.FindIndex(key);
        bool IAudioClipRepository.Add(string key, AudioClip clip) => _repository.Add(key, clip);
        bool IAudioClipRepository.Remove(int index) => _repository.Remove(index);
        bool IAudioClipRepository.Remove(string key) => _repository.Remove(key);
    }
}
#endif