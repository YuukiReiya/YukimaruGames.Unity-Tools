#if UNITY_EDITOR

using System;
using UnityEditor;
using UnityEngine;
using System.IO;

namespace YukimaruGames.Editor.Tools
{
    public sealed class ExternalAudioPlayerWindow : EditorWindow
    {
        // ReSharper disable once InconsistentNaming
        private const string kToolName = "External Audio Player";

        // ファイル単位で読み込み、事前にロードが完了したAudioを自由にPlay出来る
        private string _filePath;
        private bool _isLoading;

        [MenuItem("Tools/" + kToolName)]
        public static void ShowWindow()
        {
            GetWindow<ExternalAudioPlayerWindow>(kToolName);
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Select External Audio File:");

            // ファイルパスの入力フィールドとファイル選択ボタン
            using (new EditorGUILayout.HorizontalScope())
            {
                _filePath = EditorGUILayout.TextField("File Path", _filePath);
                DrawBrowseButton();
            }

            DrawLoadButton();
            DrawHelpBox();
            using (new EditorGUILayout.HorizontalScope())
            {
                DrawPlayButton();
                DrawStopButton();
            }

            if (_isLoading)
            {
                Repaint();
            }
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
            using (new EditorGUI.DisabledScope(_isLoading))
            {
                if (!GUILayout.Button("Load Audio File"))
                {
                    return;
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

        private void DrawHelpBox()
        {
            if (!AudioPlayer.CanPlay())
            {
                EditorGUILayout.HelpBox("Load an audio file to enable playback controls.", MessageType.Info);
            }
        }

        private void DrawPlayButton()
        {
            bool isPlay;
            using (new EditorGUILayout.VerticalScope())
            {
                using (new EditorGUI.DisabledScope(!AudioPlayer.CanPlay()))
                {
                    isPlay = GUILayout.Button("Play");
                }

                if (!AudioPlayer.CanPlay())
                {
                    EditorGUILayout.HelpBox("No valid audio file set.", MessageType.Error);
                }
            }

            if (!isPlay)
            {
                return;
            }

            AudioPlayer.Play();
        }

        private void DrawStopButton()
        {
            bool isStop;
            using (new EditorGUILayout.VerticalScope())
            {
                using (new EditorGUI.DisabledScope(!AudioPlayer.IsPlaying()))
                {
                    isStop = GUILayout.Button("Stop");
                }

                if (!AudioPlayer.IsPlaying())
                {
                    EditorGUILayout.HelpBox("No audio playing.", MessageType.Warning);
                }
            }

            if (!isStop)
            {
                return;
            }

            AudioPlayer.Stop();
        }

        private void Load()
        {
            try
            {
                _isLoading = true;
                AudioPlayer.Set(_filePath, UpdateLoadProgress);
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
    }
}
#endif
