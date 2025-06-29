#if UNITY_EDITOR
using System;
using System.Threading;
using UnityEditor;
using UnityEngine;
using YukimaruGames.Editor.Tools.AudioPlayer.Application;
using YukimaruGames.Editor.Tools.AudioPlayer.Infrastructure;
using YukimaruGames.Editor.Tools.AudioPlayer.Presenter;
using YukimaruGames.Editor.Tools.AudioPlayer.View;

// ReSharper disable InconsistentNaming

namespace YukimaruGames.Editor.Tools
{
    internal sealed class ExternalAudioPlayerWindow : EditorWindow
    {
        private Vector2 _scrollPosition;
        private LoadFilePanel _loadPanel;
        private VolumeControlPanel _volumePanel;
        private PlaybackControlPanel _playbackPanel;
        private PlaylistPanel _playlistPanel;

        private IAudioPlaybackPresenter _playbackPresenter;
        
        private AudioPlaybackService _playbackService;

        private BuiltInEditorIconRepository _iconRepository;
        private AudioClipLoader _audioClipLoader;
        private PlaylistItemRepository _playlistRepository;

        private string _filePath;
        private bool _isLoading;
        private float _lastUnmuteVolume;

        private const string kToolName = "External Audio Player";

        [MenuItem("Tools/" + kToolName)]
        internal static void ShowWindow()
        {
            GetWindow<ExternalAudioPlayerWindow>(kToolName);
        }

        private void OnEnable()
        {
            SetUp();
        }

        private void OnDisable()
        {
            TearDown();
        }

        private void OnGUI()
        {
            using var scope = new GUILayout.ScrollViewScope(_scrollPosition);
            _scrollPosition = scope.scrollPosition;

            // Reset.
            DrawResetButton();

            // Menu.
            DrawPanel();
        }

        private async void SetUp()
        {
            // フォーカスを外すと音楽再生が止まってしまうためバックグラウンド処理を止めない.
            Application.runInBackground = true;
            
            _iconRepository = new BuiltInEditorIconRepository();
            _audioClipLoader = new AudioClipLoader();
            _playlistRepository = new PlaylistItemRepository(InternalAudioClipRepository.Instance);

            _playbackService = new AudioPlaybackService(
                EditorAudioDriverAdapter.GetDriver(),
                InternalAudioClipRepository.Instance,
                _audioClipLoader,
                _playlistRepository);
            _playbackPresenter = new AudioPlaybackPresenter(_playbackService);

            _loadPanel = new LoadFilePanel(_iconRepository, InternalAudioClipRepository.Instance, _audioClipLoader);
            _volumePanel = new VolumeControlPanel(_iconRepository);
            _playbackPanel = new PlaybackControlPanel(_playbackPresenter, _iconRepository);
            _playlistPanel = new PlaylistPanel(_playbackPresenter, _iconRepository, _playlistRepository);
            
            // テスト.
            IAudioClipRepository repo = InternalAudioClipRepository.Instance;
            var filePath = "C:/Users/yuuki/Desktop/Sound/1／3の純情な感情 covered by 芦澤 サキ (128 kbps).mp3";
            var clip = await ((IAudioClipLoader)_audioClipLoader).LoadAsync(PathUtility.ToUrl(filePath), CancellationToken.None);
            clip.name = "1／3の純情な感情";
            repo.Add(filePath, clip);
            filePath = "C:/Users/yuuki/Desktop/Sound/アスノヨゾラ哨戒班 ／ Startend cover (128 kbps).mp3";
            clip = await ((IAudioClipLoader)_audioClipLoader).LoadAsync(PathUtility.ToUrl(filePath), CancellationToken.None);
            clip.name = "アスノヨゾラ哨戒班";
            repo.Add(filePath, clip);
            filePath = "C:/Users/yuuki/Desktop/Sound/一緒に歌うコラボ放課後オーバーフロウ_256kbps.mp3";
            clip = await ((IAudioClipLoader)_audioClipLoader).LoadAsync(PathUtility.ToUrl(filePath), CancellationToken.None);
            clip.name = "一緒に歌うコラボ放課後オーバーフロウ_256kbps";
            repo.Add(filePath, clip);
        }

        private void TearDown()
        {
            var disposables = new IDisposable[] { _iconRepository, _loadPanel, };
            foreach (var disposer in disposables)
            {
                disposer.Dispose();
            }

            _volumePanel = null;
            _playbackPanel = null;
            _playlistPanel = null;
            _loadPanel = null;
            _iconRepository = null;
        }

        private void DrawResetButton()
        {
            if (!GUILayout.Button("Reset"))
            {
                return;
            }

            InternalAudioPlayer.TearDown();
            InternalAudioPlayer.SetUp();
        }

        private void DrawPanel()
        {
            _loadPanel.Show();
            _volumePanel.Show();
            _playbackPanel.Show();
            _playlistPanel.Show();
        }
    }
}
#endif
