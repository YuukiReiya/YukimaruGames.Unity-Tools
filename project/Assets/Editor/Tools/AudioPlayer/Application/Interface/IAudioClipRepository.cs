#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;
using YukimaruGames.Editor.Tools.AudioPlayer.Domain;

namespace YukimaruGames.Editor.Tools.AudioPlayer.Application
{
    internal interface IAudioClipRepository
    {
        internal event Action<string, AudioClip> OnAddElement;
        internal event Action<string> OnRemoveElement;
        internal IReadOnlyList<AudioClipReference> List { get; } 
        internal void Clear();
        internal bool TryFind(string key, out AudioClip clip);
        internal bool TryFind(int index, out AudioClip clip);
        internal AudioClip Find(string key);
        internal AudioClip Find(int index);
        internal int FindIndex(string key);
        internal bool Add(string key, AudioClip clip);
        internal bool Remove(int index);
        internal bool Remove(string key);
    }
}
#endif
