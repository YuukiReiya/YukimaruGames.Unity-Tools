#if UNITY_EDITOR
using System;
using YukimaruGames.Editor.Tools.AudioPlayer.Application;
using YukimaruGames.Editor.Tools.AudioPlayer.Domain;

namespace YukimaruGames.Editor.Tools.AudioPlayer.Presenter
{
    internal sealed class AudioPlaybackPresenter : IAudioPlaybackPresenter
    {
        private readonly IAudioPlaybackService _service;
        private bool _shuffle;

        private Action<bool> _onShuffleChanged;
        private Action _onClickNextMusic;
        private Action _onClickPreviousMusic;
        private PlaylistItem _nextPlayRequest;

        PlaylistItem IAudioPlaybackPresenter.CurrentPlayingMusic
        {
            get => _service.CurrentPlayingItem;
            set => _service.Set(value);
        }

        PlaylistItem IAudioPlaybackPresenter.NextPlayRequest
        {
            get => _nextPlayRequest;
            set => _nextPlayRequest = value;
        }

        bool IAudioPlaybackPresenter.IsPlaying => _service.State == AudioPlaybackState.Playing;

        bool IAudioPlaybackPresenter.Loop
        {
            get => _service.Loop;
            set => _service.Loop = value;
        }

        bool IAudioPlaybackPresenter.Shuffle
        {
            get => _shuffle;
            set
            {
                var current = _shuffle;
                if (current == value) return;
                
                _shuffle = value;
                _onShuffleChanged?.Invoke(value);
            } 
        }

        float IAudioPlaybackPresenter.Time
        {
            get => _service.Time;
            set => _service.Time = value;
        }

        float IAudioPlaybackPresenter.Volume
        {
            get => _service.Volume;
            set => _service.Volume = value;
        }

        internal AudioPlaybackPresenter(IAudioPlaybackService service)
        {
            _service = service;
        }

        event Action<PlaylistItem> IAudioPlaybackPresenter.OnMusicChanged
        {
            add => _service.OnMusicChanged += value;
            remove => _service.OnMusicChanged -= value;
        }

        event Action<AudioPlaybackState> IAudioPlaybackPresenter.OnPlaybackStateChanged
        {
            add => _service.OnPlaybackStateChanged += value;
            remove => _service.OnPlaybackStateChanged -= value;
        }

        event Action<float> IAudioPlaybackPresenter.OnPlaybackTimeUpdated
        {
            add => _service.OnPlaybackTimeUpdated += value;
            remove => _service.OnPlaybackTimeUpdated -= value;
        }

        event Action IAudioPlaybackPresenter.OnPlaybackFinished
        {
            add => _service.OnPlaybackFinished += value;
            remove => _service.OnPlaybackFinished -= value;
        }

        event Action<float> IAudioPlaybackPresenter.OnVolumeChanged
        {
            add => _service.OnVolumeChanged += value;
            remove => _service.OnVolumeChanged -= value;
        }

        event Action<bool> IAudioPlaybackPresenter.OnShuffleChanged
        {
            add => _onShuffleChanged += value;
            remove => _onShuffleChanged -= value;
        }

        event Action IAudioPlaybackPresenter.OnClickNextMusic
        {
            add => _onClickNextMusic += value;
            remove => _onClickNextMusic -= value;
        }

        event Action IAudioPlaybackPresenter.OnClickPreviousMusic
        {
            add => _onClickPreviousMusic += value;
            remove => _onClickPreviousMusic -= value;
        }

        void IAudioPlaybackPresenter.Play()
        {
            if (_nextPlayRequest != null)
            {
                _service.Set(_nextPlayRequest);
            }
            _service.Play();  
        } 
        void IAudioPlaybackPresenter.Stop() => _service.Stop();
        void IAudioPlaybackPresenter.Pause() => _service.Pause();
        void IAudioPlaybackPresenter.Resume() => _service.Resume();
    }
}
#endif
