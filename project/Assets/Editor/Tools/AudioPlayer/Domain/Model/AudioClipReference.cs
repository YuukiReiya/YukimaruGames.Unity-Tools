#if UNITY_EDITOR
using UnityEngine;
// ReSharper disable InconsistentNaming

namespace YukimaruGames.Editor.Tools.AudioPlayer.Domain
{
    [System.Serializable]
    internal sealed class AudioClipReference
    {
        [SerializeField] internal string Key;
        [SerializeField] internal AudioClip AudioClip;

        internal AudioClipReference(string key, AudioClip audioClip)
        {
            Key = key;
            AudioClip = audioClip;
        }
    }
}
#endif
