#if UNITY_EDITOR
using UnityEngine;

namespace YukimaruGames.Editor.Tools.AudioPlayer.Domain
{
    internal sealed record PlaylistItem
    {
        internal string Key { get; }
        internal string Url { get; }
        internal string MusicName { get; }
        internal float Volume { get; }
        internal float Length { get; }

        internal PlaylistItem(string key, string url, string musicName, float volume, float length)
        {
            Key = key;
            Url = url;
            MusicName = musicName;
            Volume = volume;
            Length = length;
        }

        internal PlaylistItem(string key, AudioClip clip)
        {
            Key = key;
            Url = null;
            MusicName = clip.name;
            Volume = 1f;
            Length = clip.length;
        }
    }
}
#endif