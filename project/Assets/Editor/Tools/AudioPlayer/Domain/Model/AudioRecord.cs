using UnityEngine;

namespace YukimaruGames.Editor.Tools
{
    internal sealed class AudioRecord
    {
        internal string Url { get; }
        internal AudioClip AudioClip { get; }

        internal AudioRecord(string url, AudioClip audioClip)
        {
            Url = url;
            AudioClip = audioClip;
        }
    }
}
