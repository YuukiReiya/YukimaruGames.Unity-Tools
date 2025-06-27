#if UNITY_EDITOR
using System.Collections.Generic;
using YukimaruGames.Editor.Tools.AudioPlayer.Domain;

namespace YukimaruGames.Editor.Tools.AudioPlayer.Application
{
    internal interface IPlaylistItemRepository
    {
        IReadOnlyList<PlaylistItem> List { get; }
        internal PlaylistItem Find(int index);
        internal bool Add(PlaylistItem item);
    }
}
#endif
