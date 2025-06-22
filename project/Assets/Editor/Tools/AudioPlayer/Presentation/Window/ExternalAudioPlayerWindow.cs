#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;
using System.IO;
using YukimaruGames.Editor.Tools.AudioPlayer.Application;
using YukimaruGames.Editor.Tools.AudioPlayer.Infrastructure;
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

        private BuiltInEditorIconRepository _iconRepository;

        private string _filePath;
        private bool _isLoading;
        private float _lastUnmuteVolume;
        
        private const string kToolName = "External Audio Player";

        private IAudioClipRepository AudioClipRepository => InternalAudioClipRepository.Instance;

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
            EditorGUILayout.LabelField("Select External Audio File:", EditorStyles.boldLabel);

            // ファイルパスの入力フィールドとファイル選択ボタン
            using (new EditorGUILayout.HorizontalScope())
            {
                _filePath = EditorGUILayout.TextField("File Path", _filePath);
                DrawBrowseButton();
            }

            DrawLoadButton();
            
            //EditorGUILayout.Space(5);
            DrawResetButton();
            //EditorGUILayout.Space(5);
            
            // Info.
            DrawMusicInfo();
            
            // Menu.
            DrawPanel();

            // if (_isLoading)
            // {
            //     Repaint();
            // }
        }

        private void SetUp()
        {
            InternalAudioPlayer.OnUpdateLoadProgress += UpdateLoadProgress;

            _iconRepository = new BuiltInEditorIconRepository();
            _loadPanel = new LoadFilePanel(InternalAudioClipRepository.Instance);
            _volumePanel = new VolumeControlPanel(_iconRepository);
            _playbackPanel = new PlaybackControlPanel(_iconRepository);
            _playlistPanel = new PlaylistPanel(InternalAudioPlayer.AudioClips);
        }

        private void TearDown()
        {
            InternalAudioPlayer.OnUpdateLoadProgress -= UpdateLoadProgress;

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
        
        private void DrawBrowseButton()
        {
            if (!GUILayout.Button("Browse", GUILayout.Width(60)))
            {
                return;
            }

            var path = EditorUtility.OpenFilePanel("Select Audio File", "", "mp3,wav,ogg,aiff");
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            _filePath = path;
        }

        private void DrawLoadButton()
        {
            using (new EditorGUILayout.VerticalScope())
            {
                using (new EditorGUI.DisabledScope(_isLoading))
                {
                    var isLoad = GUILayout.Button("Load Audio File");
                    if (!InternalAudioPlayer.HasClip())
                    {
                        EditorGUILayout.HelpBox("Load an audio file to enable playback controls.", MessageType.Info);
                    }

                    if (!isLoad)
                    {
                        return;
                    }
                }
            }

            if (!string.IsNullOrEmpty(_filePath) && File.Exists(_filePath))
            {
                Load();
            }
            else
            {
                EditorUtility.DisplayDialog("Error", "Please select a valid audio file.", "OK");
            }
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

        private void Load()
        {
            try
            {
                _isLoading = true;
                InternalAudioPlayer.Set(_filePath);
                
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            finally
            {
                _isLoading = false;
                EditorUtility.ClearProgressBar();

            }
        }

        private void UpdateLoadProgress(float progress) =>
            EditorUtility.DisplayProgressBar(
                "Loading...",
                $"{progress * 100:F1}/100.0%",
                progress);

        private static string FormatTime(float seconds)
        {
            var timeSpan = TimeSpan.FromSeconds(seconds);
            return 1 <= timeSpan.TotalHours
                ? $"{timeSpan.TotalHours:D2}:{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}"
                : $"{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
        }
    }
}
#endif
