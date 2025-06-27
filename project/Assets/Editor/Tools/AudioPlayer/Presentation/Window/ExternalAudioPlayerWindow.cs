#if UNITY_EDITOR
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using YukimaruGames.Editor.Tools.AudioPlayer.Infrastructure;
using YukimaruGames.Editor.Tools.AudioPlayer.View;
using Debug = UnityEngine.Debug;

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

        private BuiltInEditorIconRepository _iconRepository;

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

        private void SetUp()
        {
            _iconRepository = new BuiltInEditorIconRepository();
            _loadPanel = new LoadFilePanel(_iconRepository, InternalAudioClipRepository.Instance);
            _volumePanel = new VolumeControlPanel(_iconRepository);
            _playbackPanel = new PlaybackControlPanel(_iconRepository);
            _playlistPanel = new PlaylistPanel(_iconRepository, InternalAudioClipRepository.Instance);
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
            using (new EditorGUI.DisabledScope(InternalAudioPlayer.GetCurrentClip() == null))
            {
                _playbackPanel.Show();
            }

            _playlistPanel.Show();
        }
    }
}
#endif
