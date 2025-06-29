#if UNITY_EDITOR
using System;
using System.Threading;
using YukimaruGames.Editor.Tools.AudioPlayer.Domain;

namespace YukimaruGames.Editor.Tools.AudioPlayer.Application
{
    public sealed class AudioPlaybackService : IAudioPlaybackService
    {
        private readonly IAudioDriver _driver;
        private readonly IAudioClipRepository _audioClipRepository;
        private readonly IAudioClipLoader _loader;
        private readonly IPlaylistItemRepository _playlistItemRepository;

        private AudioPlaybackState _state;
        private PlaylistItem _currentPlaylistItem;
        private bool _loop;
        private bool _disposed;

        private Action<AudioPlaybackState> _onPlaybackStateChanged;
        private Action<PlaylistItem> _onMusicChanged;
        private Action<bool> _onLoopChanged;
        private Action<float> _onVolumeChangedProxy;
        private Action<float> _onTimeChangedProxy;
        private Action _onPlaybackFinishedProxy;

        internal TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(5f);

        AudioPlaybackState IAudioPlaybackService.State => _state;

        PlaylistItem IAudioPlaybackService.CurrentPlayingItem => _currentPlaylistItem;

        event Action<PlaylistItem> IAudioPlaybackService.OnMusicChanged
        {
            add => _onMusicChanged += value;
            remove => _onMusicChanged -= value;
        }

        event Action<AudioPlaybackState> IAudioPlaybackService.OnPlaybackStateChanged
        {
            add => _onPlaybackStateChanged += value;
            remove => _onPlaybackStateChanged -= value;
        }

        event Action<bool> IAudioPlaybackService.OnLoopChanged
        {
            add => _onLoopChanged += value;
            remove => _onLoopChanged -= value;
        }

        event Action<float> IAudioPlaybackService.OnVolumeChanged
        {
            add => _onVolumeChangedProxy += value;
            remove => _onVolumeChangedProxy -= value;
        }

        event Action<float> IAudioPlaybackService.OnPlaybackTimeUpdated
        {
            add => _onTimeChangedProxy += value;
            remove => _onTimeChangedProxy -= value;
        }

        event Action IAudioPlaybackService.OnPlaybackFinished
        {
            add => _onPlaybackFinishedProxy += value;
            remove => _onPlaybackFinishedProxy -= value;
        }

        bool IAudioPlaybackService.Loop
        {
            get => _loop;
            set
            {
                var current = _loop;
                if (current == value) return;

                _loop = value;
                _onLoopChanged?.Invoke(value);
            }
        }

        float IAudioPlaybackService.Volume
        {
            get => _driver.Volume;
            set => _driver.Volume = value;
        }

        float IAudioPlaybackService.Time
        {
            get => _driver.CurrentClip != null ? _driver.Time : 0f;
            set => _driver.Time = value;
        }

        internal AudioPlaybackService(IAudioDriver driver,IAudioClipRepository audioClipRepository,IAudioClipLoader loader,IPlaylistItemRepository playlistItemRepository)
        {
            _driver = driver;
            _audioClipRepository = audioClipRepository;
            _playlistItemRepository = playlistItemRepository;
            _loader = loader;
            
            _state = AudioPlaybackState.Stopped;
            _disposed = false;
            
            SetUp();
        }
        
        void IAudioPlaybackService.Play()
        {
            if (_driver.CurrentClip == null)
            {
                throw new InvalidOperationException(
                    "Cannot play audio. No PlaylistItem has been set. Call the Set method with a valid PlaylistItem before attempting to play.");
            }

            _state = AudioPlaybackState.Playing;
            _driver.Play();
        }

        void IAudioPlaybackService.Stop()
        {
            _state = AudioPlaybackState.Stopped;
            _driver.Stop();
        }

        void IAudioPlaybackService.Pause()
        {
            if (_state != AudioPlaybackState.Playing)
            {
                return;
            }
            
            _state = AudioPlaybackState.Paused;
            _driver.Pause();
        }

        void IAudioPlaybackService.Resume()
        {
            if (_state != AudioPlaybackState.Paused)
            {
                return;
            }
            
            _state = AudioPlaybackState.Playing;
            _driver.Resume();
        }

        async void IAudioPlaybackService.Set(PlaylistItem item)
        {
            if (item == null)
            {
                _driver.CurrentClip = null;
                _currentPlaylistItem = null;
                _onMusicChanged?.Invoke(null);
                return;
            }
            
            if (_currentPlaylistItem?.Key == item.Key) return;

            _playlistItemRepository.Add(item);
            
            var audioClip = _audioClipRepository.Find(item.Key);
            if (audioClip == null)
            {
                if (string.IsNullOrWhiteSpace(item.Url))
                {
                    throw new ArgumentException($"The URL for audio item with key '{item.Key}' cannot be null, empty, or white space. URL: '{item.Url}'", nameof(item.Url));
                }

                using var cts = new CancellationTokenSource(Timeout);
                audioClip = await _loader.LoadAsync(item.Url, cts.Token);
                
                if (audioClip != null)
                {
                    _audioClipRepository.Add(item.Key, audioClip);
                }
                else
                {
                    return;
                }
            }

            _driver.CurrentClip = audioClip;
            _currentPlaylistItem = item;
            _onMusicChanged?.Invoke(item);
        }

        private void OnVolumeChanged(float volume) => _onVolumeChangedProxy?.Invoke(volume);
        private void OnTimeChanged(float time) => _onTimeChangedProxy?.Invoke(time);

        private void OnPlaybackPlaying() => _onPlaybackStateChanged?.Invoke(AudioPlaybackState.Playing);
        private void OnPlaybackStopped() => _onPlaybackStateChanged?.Invoke(AudioPlaybackState.Stopped);
        private void OnPlaybackPaused() => _onPlaybackStateChanged?.Invoke(AudioPlaybackState.Paused);
        private void OnPlaybackResumed() => _onPlaybackStateChanged?.Invoke(AudioPlaybackState.Playing);

        private void OnPlaybackFinished()
        {
            _onPlaybackFinishedProxy?.Invoke();
            
            if (_loop)
            {
                (this as IAudioPlaybackService).Play();
            }
            else
            {
                (this as IAudioPlaybackService).Stop();
                (this as IAudioPlaybackService).Time = 0f;
            }
        } 
        
        private void OnAddPlaylist(PlaylistItem item)
        {
            if (_currentPlaylistItem != null) return;
            
            // 設定
            (this as IAudioPlaybackService).Set(item);
        }

        private void OnRemovePlaylist(PlaylistItem item)
        {
            if (_currentPlaylistItem.Key != item.Key) return;
            (this as IAudioPlaybackService).Set(null);
        }
        
        private void SetUp()
        {
            _driver.OnPlayStarted += OnPlaybackPlaying;
            _driver.OnPlayStopped += OnPlaybackStopped;
            _driver.OnPlayPaused += OnPlaybackPaused;
            _driver.OnPlayResumed += OnPlaybackResumed;
            _driver.OnPlaybackFinished += OnPlaybackFinished;
            _driver.OnTimeUpdated += OnTimeChanged;
            _driver.OnVolumeChanged += OnVolumeChanged;

            _playlistItemRepository.OnAddElement += OnAddPlaylist;
            _playlistItemRepository.OnRemoveElement += OnRemovePlaylist;
        }

        private void TearDown()
        {
            _driver.OnPlayStarted -= OnPlaybackPlaying;
            _driver.OnPlayStopped -= OnPlaybackStopped;
            _driver.OnPlayPaused -= OnPlaybackPaused;
            _driver.OnPlayResumed -= OnPlaybackResumed;
            _driver.OnPlaybackFinished -= OnPlaybackFinished;
            _driver.OnTimeUpdated -= OnTimeChanged;
            _driver.OnVolumeChanged -= OnVolumeChanged;
            
            _playlistItemRepository.OnAddElement -= OnAddPlaylist;
            _playlistItemRepository.OnRemoveElement -= OnRemovePlaylist;

            _onMusicChanged = null;
            _onPlaybackStateChanged = null;
            _onLoopChanged = null;
            _onVolumeChangedProxy = null;
            _onTimeChangedProxy = null;
            _onPlaybackFinishedProxy = null;
        }
        
        void IDisposable.Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool releaseManaged)
        {
            if (_disposed) return;

            if (releaseManaged)
            {
                // managed resources here.
               TearDown();
            }
            
            // unmanaged resources here.

            _disposed = true;
        }

        ~AudioPlaybackService()
        {
            Dispose(false);
        }
    }
}
#endif