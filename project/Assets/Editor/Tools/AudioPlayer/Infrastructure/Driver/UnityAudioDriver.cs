#if UNITY_EDITOR
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace YukimaruGames.Editor.Tools.AudioPlayer.Infrastructure
{
    internal sealed class UnityAudioDriver : IAudioDriver,IDisposable
    {
        private readonly AudioSource _source;
        private Action<AudioClip> _onClipChanged;
        private Action<bool> _onLoopChanged;
        private Action<float> _onVolumeChanged;
        private Action<float> _onTimeUpdated;
        private Action _onPlayStarted;
        private Action _onPlayStopped;
        private Action _onPlayPaused;
        private Action _onPlayResumed;
        private Action _onPlaybackFinished;
        private CancellationTokenSource _cts;
        private bool _disposed;

        bool IAudioDriver.IsPlaying => _source.isPlaying;

        AudioClip IAudioDriver.CurrentClip
        {
            get => _source.clip;
            set
            {
                var current = _source.clip;
                if (current?.Equals(value) ?? false) return;

                _source.clip = value;
                _onClipChanged?.Invoke(value);
            }
        }

        event Action<AudioClip> IAudioDriver.OnClipChanged
        {
            add => _onClipChanged += value;
            remove => _onClipChanged -= value;
        }

        event Action<bool> IAudioDriver.OnLoopChanged
        {
            add => _onLoopChanged += value;
            remove => _onLoopChanged -= value;
        }

        event Action<float> IAudioDriver.OnVolumeChanged
        {
            add => _onVolumeChanged += value;
            remove => _onVolumeChanged -= value;
        }

        event Action<float> IAudioDriver.OnTimeUpdated
        {
            add => _onTimeUpdated += value;
            remove => _onTimeUpdated -= value;
        }

        event Action IAudioDriver.OnPlayStarted
        {
            add => _onPlayStarted += value;
            remove => _onPlayStarted -= value;
        }

        event Action IAudioDriver.OnPlayStopped
        {
            add => _onPlayStopped += value;
            remove => _onPlayStopped -= value;
        }

        event Action IAudioDriver.OnPlayPaused
        {
            add => _onPlayPaused += value;
            remove => _onPlayPaused -= value;
        }

        event Action IAudioDriver.OnPlayResumed
        {
            add => _onPlayResumed += value;
            remove => _onPlayResumed -= value;
        }

        event Action IAudioDriver.OnPlaybackFinished
        {
            add => _onPlaybackFinished += value;
            remove => _onPlaybackFinished -= value;
        }
        
        bool IAudioDriver.Loop
        {
            get => _source.loop;
            set
            {
                var current = _source.loop;
                if (current == value) return;
                _source.loop = value;
                _onLoopChanged?.Invoke(value);
            }
        }

        float IAudioDriver.Volume
        {
            get => _source.volume;
            set
            {
                var current = _source.volume;
                if (Mathf.Approximately(current, value)) return;

                _source.volume = value;
                _onVolumeChanged?.Invoke(value);
            }
        }

        float IAudioDriver.Time
        {
            get => _source.time;
            set
            {
                if (_source.clip == null)
                {
                    return;
                }
                
                var current = _source.time;
                if (Mathf.Approximately(current, value)) return;

                _source.time = value;
                _onTimeUpdated?.Invoke(value);
            }
        }

        internal UnityAudioDriver(AudioSource source)
        {
            _source = source;
        }

        ~UnityAudioDriver()
        {
            Dispose();
        }

        void IAudioDriver.Play()
        {
            _source.Play();
            _onPlayStarted?.Invoke();

            _cts?.Cancel();
            _cts?.Dispose();
            _cts ??= new CancellationTokenSource();
            PlaybackMonitorAsync(_cts.Token);
        }

        void IAudioDriver.Stop()
        {
            _source.Stop();
            _onPlayStopped?.Invoke();
        }

        void IAudioDriver.Pause()
        {
            _source.Pause();
            _onPlayPaused?.Invoke();
        }

        void IAudioDriver.Resume()
        {
            _source.UnPause();
            _onPlayResumed?.Invoke();
        }

        private async void PlaybackMonitorAsync(CancellationToken token)
        {
            while (true)
            {
                var lastTime = _source?.time ?? 0;
                await Task.Yield();

                if (token.IsCancellationRequested) return;

                if ((_source?.isPlaying ?? false) && _source.clip != null)
                {
                    var elapsed = Time.deltaTime;
                    if ((_source?.clip?.length ?? float.MaxValue) <= (lastTime + elapsed))
                    {
                        _onPlaybackFinished?.Invoke();
                    }
                }
            }
        }

        void IDisposable.Dispose()
        {
            Dispose();
            GC.SuppressFinalize(this);
        }

        internal void Dispose()
        {
            if (_disposed) return;

            _cts?.Cancel();
            _cts?.Dispose();
            
            _onClipChanged = null;
            _onLoopChanged = null;
            _onPlaybackFinished = null;
            _onVolumeChanged = null;
            _onTimeUpdated = null;
            _onPlayStarted = null;
            _onPlayStopped = null;
            _onPlayPaused = null;
            _onPlayResumed = null;
            
            _cts = null;
            _disposed = true;
        }
    }
}
#endif
