#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using YukimaruGames.Editor.Tools.AudioPlayer.Domain;

namespace YukimaruGames.Editor.Tools.AudioPlayer.Application
{
    internal interface IPlaylistItemRepository
    {
        internal IReadOnlyList<PlaylistItem>List { get; }
        internal event Action<PlaylistItem> OnAddElement;
        internal event Action<PlaylistItem> OnRemoveElement;
        internal void Clear();
        internal bool Add(PlaylistItem item);
        internal bool Remove(string key);
        internal PlaylistItem Find(string key);
    }
}
#endif
