#if UNITY_EDITOR
using System;
using UnityEngine;
// ReSharper disable InconsistentNaming

namespace YukimaruGames.Editor.Tools.AudioPlayer.Domain
{
    internal sealed class AudioClipReference : IEquatable<AudioClipReference>
    {
        internal readonly string Key;
        internal readonly AudioClip AudioClip;

        internal AudioClipReference(string key, AudioClip audioClip)
        {
            Key = key;
            AudioClip = audioClip;
        }

        public bool Equals(AudioClipReference other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Key == other.Key && Equals(AudioClip, other.AudioClip);
        }

        public override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj) || obj is AudioClipReference other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Key, AudioClip);
        }

        public static bool operator ==(AudioClipReference left, AudioClipReference right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(AudioClipReference left, AudioClipReference right)
        {
            return !Equals(left, right);
        }
    }
}
#endif
