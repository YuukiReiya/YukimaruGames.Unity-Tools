#if UNITY_EDITOR
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace YukimaruGames.Editor.Tools.AudioPlayer.Application
{
    internal interface IAudioClipLoader
    {
        internal event Action<float> OnUpdateProgress;
        internal bool IsLoading { get; }
        internal Task<AudioClip> LoadAsync(string url, CancellationToken token);
    }
}
#endif
