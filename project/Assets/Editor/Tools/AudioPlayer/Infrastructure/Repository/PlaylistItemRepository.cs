#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;
using YukimaruGames.Editor.Tools.AudioPlayer.Application;
using YukimaruGames.Editor.Tools.AudioPlayer.Domain;

namespace YukimaruGames.Editor.Tools.AudioPlayer.Infrastructure
{
    internal sealed class PlaylistItemRepository : IPlaylistItemRepository, IDisposable
    {
        private readonly IAudioClipRepository _audioClipRepository;
        private readonly List<PlaylistItem> _list = new();
        private bool _disposed;

        private Action<PlaylistItem> _onAddItem;
        private Action<PlaylistItem> _onRemoveItem;

        IReadOnlyList<PlaylistItem> IPlaylistItemRepository.List => _list;

        internal PlaylistItemRepository(IAudioClipRepository audioClipRepository)
        {
            _audioClipRepository = audioClipRepository;
            
            SetUp();
        }

        ~PlaylistItemRepository()
        {
            Dispose(false);
        }

        event Action<PlaylistItem> IPlaylistItemRepository.OnAddElement
        {
            add => _onAddItem += value;
            remove => _onAddItem -= value;
        }

        event Action<PlaylistItem> IPlaylistItemRepository.OnRemoveElement
        {
            add => _onRemoveItem += value;
            remove => _onRemoveItem -= value;
        }

        void IPlaylistItemRepository.Clear()
        {
            _list.Clear();
        }

        bool IPlaylistItemRepository.Add(PlaylistItem item)
        {
            if (Find(item.Key) != null) return false;

            _list.Add(item);
            _onAddItem?.Invoke(item);
            return true;
        }

        bool IPlaylistItemRepository.Remove(string key)
        {
            var item = Find(key);
            if (item == null) return false;

            _onRemoveItem?.Invoke(item);
            return _list.Remove(item);
        }

        PlaylistItem IPlaylistItemRepository.Find(string key) => Find(key);

        private PlaylistItem Find(string key)
        {
            return _list.Find(x => x.Key == key);
        }

        private void OnAddClip(string key, AudioClip clip) =>
            (this as IPlaylistItemRepository).Add(new PlaylistItem(key, clip));

        private void OnRemoveClip(string key) => (this as IPlaylistItemRepository).Remove(key);

        private void SetUp()
        {
            _audioClipRepository.OnAddElement += OnAddClip;
            _audioClipRepository.OnRemoveElement += OnRemoveClip;
        }

        private void TearDown()
        {
            _audioClipRepository.OnAddElement -= OnAddClip;
            _audioClipRepository.OnRemoveElement -= OnRemoveClip;

            _onAddItem = null;
            _onRemoveItem = null;
        }

        void IDisposable.Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        internal void Dispose(bool disposable)
        {
            if (_disposed) return;

            if (disposable)
            {
                TearDown();
            }

            _disposed = true;
        }
    }
}
#endif
