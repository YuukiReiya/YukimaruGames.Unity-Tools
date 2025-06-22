#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using YukimaruGames.Editor.Tools.AudioPlayer.Domain;

namespace YukimaruGames.Editor.Tools.AudioPlayer.Application
{
    internal interface IAudioClipRepository
    {
        internal IReadOnlyList<AudioClipReference> List { get; } 
        internal void Clear();
        internal bool TryFind(string key, out AudioClip clip);
        internal bool TryFind(int index, out AudioClip clip);
        internal AudioClip Find(string key);
        internal AudioClip Find(int index);
        internal int FindIndex(string key);
        internal bool TryAdd(string key, AudioClip clip);
    }
}
#endif
