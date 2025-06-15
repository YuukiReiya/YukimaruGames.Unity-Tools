#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;

namespace YukimaruGames.Editor.Tools
{
    internal static class AudioClipLoader
    {
        private static readonly SemaphoreSlim Semaphore = new(1, 1);

        internal static event Action<float> OnUpdateProgress; 
        
        private static float LoadProgress { get; set; }

        internal static string GetUrl(string localFilePath) =>
            $"file://{localFilePath.Replace("\\", "/")}";

        internal static IEnumerator<AudioClip> Load(string url)
        {
            try
            {
                Semaphore.Wait(TimeSpan.FromSeconds(5));
                LoadProgress = 0f;
            
                using var www = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.UNKNOWN);
                www.SendWebRequest();
                while (!www.isDone)
                {
                    LoadProgress = www.downloadProgress;
                    OnUpdateProgress?.Invoke(LoadProgress);
                    yield return null;
                }
                
                if (www.result is UnityWebRequest.Result.Success)
                {
                    yield return DownloadHandlerAudioClip.GetContent(www);
                }
            }
            finally
            {
                Semaphore.Release();
            }
        }
    }
}
#endif
