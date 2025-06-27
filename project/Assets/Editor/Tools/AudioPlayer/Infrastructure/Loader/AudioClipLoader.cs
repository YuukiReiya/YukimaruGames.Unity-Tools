#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using Debug = UnityEngine.Debug;

namespace YukimaruGames.Editor.Tools
{
    internal static class AudioClipLoader
    {
        private static readonly SemaphoreSlim Semaphore = new(1, 1);

        internal static event Action<float> OnUpdateProgress;
        internal static float LoadProgress { get; private set; }
        internal static Stopwatch Stopwatch { get; private set; }
        internal static bool IsLoading { get; private set; }

        internal static string GetUrl(string localFilePath) =>
            $"file://{localFilePath.Replace("\\", "/")}";

        internal static IEnumerator<(float loadProgress, AudioClip content)> Load(string url)
        {
            try
            {
                Semaphore.Wait(TimeSpan.FromSeconds(5));
                LoadProgress = 0f;

                using var www = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.UNKNOWN);
                www.SendWebRequest();
                var stopwatch = new Stopwatch();
                stopwatch.Start();
                while (!www.isDone)
                {
                    LoadProgress = www.downloadProgress;
                    //OnUpdateProgress?.Invoke(stopwatch.Elapsed, LoadProgress);
                    OnUpdateProgress?.Invoke(LoadProgress);
                    yield return (LoadProgress, null);
                }

                if (www.result is UnityWebRequest.Result.Success)
                {
                    var content = DownloadHandlerAudioClip.GetContent(www);
                    content.name = Path.GetFileNameWithoutExtension(url);
                    content.hideFlags = HideFlags.DontUnloadUnusedAsset;
                    yield return (LoadProgress, content);
                }
            }
            finally
            {
                Semaphore.Release();
            }
        }
        
        internal static async Task<AudioClip> LoadAsync(SynchronizationContext context, string url,
            float timeoutSec)
        {

            try
            {
                await Semaphore.WaitAsync(TimeSpan.FromSeconds(timeoutSec)).ConfigureAwait(true);
                IsLoading = true;
                using var request = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.UNKNOWN);

                var e = request.SendWebRequest();
                Stopwatch ??= new Stopwatch();
                Stopwatch.Restart();
                while (!e.isDone)
                {
                    context?.Post(_ =>
                    {
                        OnUpdateProgress?.Invoke(e.progress);
                    }, null);
                    await Task.Yield();
                }

                LoadProgress = request.downloadProgress;
                context?.Post(_ =>
                {
                    LoadProgress = e.progress;
                    OnUpdateProgress?.Invoke(e.progress);
                }, null);
                
                
                if (request.result is UnityWebRequest.Result.Success)
                {
                    var content = DownloadHandlerAudioClip.GetContent(request);
                    content.name = Path.GetFileNameWithoutExtension(url);
                    return content;
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                throw;
            }
            finally
            {
                IsLoading = false;
                UnityEditor.EditorUtility.ClearProgressBar();
                Semaphore.Release();
            }

            return null;
        }

        internal static async Task<AudioClip> LoadNativeAsync(SynchronizationContext context, string url,
            float timeoutSec,CancellationToken token)
        {
            try
            {
                await Semaphore.WaitAsync(TimeSpan.FromSeconds(timeoutSec), token);
                LoadProgress = 0f;

                Debug.Log($" URL1 : {url} ");

                const int bufferSize = 4096;
                using var stream = new FileStream(url, FileMode.Open, FileAccess.Read, FileShare.Read,
                    bufferSize: bufferSize, useAsync: true);
                var totalSize = stream.Length;
                Debug.Log($" URL2 : {url} Total:{totalSize}");
                if (totalSize == 0)
                {
                    return null;
                }

                var bytes = new List<byte>((int)totalSize);
                var buffer = new byte[bufferSize];

                var bytesReadSoFar = 0L;
                int bufferRead;
                Stopwatch ??= new Stopwatch();
                Stopwatch.Restart();
                do
                {
                    Debug.Log($" Update : {bytesReadSoFar / totalSize}");
                    token.ThrowIfCancellationRequested();

                    bufferRead = await stream.ReadAsync(buffer, 0, buffer.Length, token);

                    if (0 < bufferRead)
                    {
                        bytes.AddRange(buffer.Take(bufferRead));
                        bytesReadSoFar += bufferRead;

                        context?.Post(_ =>
                        {
                            var progress = (float)bytesReadSoFar / totalSize;
                            OnUpdateProgress?.Invoke(progress);
                        }, null);
                    }
                } while (0 < bufferRead);

                OnUpdateProgress?.Invoke(1f);

                return null;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                throw;
            }
            finally
            {
                Semaphore.Release();
            }

            return null;
        }
    }
}
#endif
