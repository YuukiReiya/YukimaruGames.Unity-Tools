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

        IReadOnlyList<AudioClipReference> IAudioClipRepository.List => _repository.List;
        
        void IAudioClipRepository.Clear() => _repository.Clear();
        bool IAudioClipRepository.TryFind(string key, out AudioClip clip) => _repository.TryFind(key, out clip);
        bool IAudioClipRepository.TryFind(int index, out AudioClip clip) => _repository.TryFind(index, out clip);
        AudioClip IAudioClipRepository.Find(string key) => _repository.Find(key);
        AudioClip IAudioClipRepository.Find(int index) => _repository.Find(index);
        int IAudioClipRepository.FindIndex(string key) => _repository.FindIndex(key);
        bool IAudioClipRepository.TryAdd(string key, AudioClip clip) => _repository.TryAdd(key, clip);
    }
}
