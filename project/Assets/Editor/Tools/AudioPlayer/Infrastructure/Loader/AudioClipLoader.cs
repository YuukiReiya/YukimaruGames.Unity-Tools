#if UNITY_EDITOR
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using YukimaruGames.Editor.Tools.AudioPlayer.Application;

namespace YukimaruGames.Editor.Tools.AudioPlayer.Infrastructure
{
    internal sealed class AudioClipLoader : IAudioClipLoader
    {
        private bool _isLoading;
        private Action<float> _onUpdateProgress;


        event Action<float> IAudioClipLoader.OnUpdateProgress
        {
            add => _onUpdateProgress += value;
            remove => _onUpdateProgress -= value;
        }

        bool IAudioClipLoader.IsLoading => _isLoading;

        async Task<AudioClip> IAudioClipLoader.LoadAsync(string url, CancellationToken token)
        {
            try
            {
                _isLoading = true;
                using var request = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.UNKNOWN);
                var e = request.SendWebRequest();

                while (!e.isDone)
                {
                    await Task.Yield();
                    _onUpdateProgress?.Invoke(e.progress);
                }

                _onUpdateProgress?.Invoke(e.progress);
                return request.result is UnityWebRequest.Result.Success
                    ? DownloadHandlerAudioClip.GetContent(request)
                    : null;
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                throw;
            }
            finally
            {
                _isLoading = false;
            }

            return null;
        }
    }
}
#endif
