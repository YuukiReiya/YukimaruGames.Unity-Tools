#if UNITY_EDITOR
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Windows;
using YukimaruGames.Editor.Tools.AudioPlayer.Application;

// ReSharper disable InconsistentNaming

namespace YukimaruGames.Editor.Tools.AudioPlayer.View
{
    internal sealed class LoadFilePanel : IDisposable
    {
        private readonly IAudioClipRepository _repository;
        private string _filePath;
        private bool _isLoading;

        internal event Action<float> OnUpdateLoadProgress;
        
        private const string kHeaderName = "Select External Audio File:";
        private const string kFilePath = "File Path";
        private const string kBrowseButtonName = "Browse";
        private const string kBrowseOpenFileTitle = "Select Audio File";
        private const string kFileExtensions = "mp3,wav";
        private const string kLoadButtonName = "Load Audio File";
        private const float kBrowseButtonWidth = 60f;
        private const string kErrorDialogTitle = "Error";
        private const string kErrorDialogMessage = "Please select a valid audio file.";
        private const string kErrorDialogConfirmButtonName = "OK";
        private const string kProgressBarTitle = "Loading...";
        private const string kIcon = "d_Project";//d_FolderOpened Icon

        internal LoadFilePanel(IAudioClipRepository repository)
        {
            _repository = repository;
        }

        ~LoadFilePanel()
        {
            IDisposable self = this;
            self.Dispose();
        }

        internal void Show()
        {
            using var scope = new EditorGUILayout.VerticalScope(EditorStyles.helpBox);
            DrawHeader();
            using (new GUILayout.HorizontalScope())
            {
                DrawInputField();
                DrawBrowseButton();
            }

            DrawLoadFileButton();
        }

        private void DrawHeader()
        {
            EditorGUILayout.LabelField(kHeaderName, EditorStyles.boldLabel);
        }

        private void DrawInputField()
        {
            _filePath = EditorGUILayout.TextField(kFilePath, _filePath);
        }

        private void DrawBrowseButton()
        {
            if (!GUILayout.Button(kBrowseButtonName, GUILayout.Width(kBrowseButtonWidth)))
            {
                return;
            }

            var path = EditorUtility.OpenFilePanel(kBrowseOpenFileTitle, string.Empty, kFileExtensions);
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            _filePath = path;
        }

        private void DrawLoadFileButton()
        {
            using (new EditorGUI.DisabledScope(_isLoading))
            {
                var clicked = GUILayout.Button(kLoadButtonName);
                if (!clicked)
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
                EditorUtility.DisplayDialog(kErrorDialogTitle, kErrorDialogMessage, kErrorDialogConfirmButtonName);
            }
        }

        private async void Load()
        {
            var url = AudioClipLoader.GetUrl(_filePath);
            var key = _filePath;

            if (_repository.TryFind(key, out _))
            {
                return;
            }

//#pragma warning disable CS4014 // この呼び出しは待機されなかったため、現在のメソッドの実行は呼び出しの完了を待たずに続行されます
            await LoadAsync(key, url);
//#pragma warning restore CS4014 // この呼び出しは待機されなかったため、現在のメソッドの実行は呼び出しの完了を待たずに続行されます
        }

        private async Task LoadAsync(string key, string url)
        {
            try
            {
                _isLoading = true;
                AudioClipLoader.OnUpdateProgress += UpdateLoadProgress;
                // var clip = await AudioClipLoader.LoadAsync(
                //     SynchronizationContext.Current,
                //     url, 5f);

                var clip = await AudioClipLoader.LoadAsync(
                    SynchronizationContext.Current,
                    url, 5f);

                if (clip != null)
                {
                    _repository.TryAdd(key, clip);
                }

                var msg = clip != null
                    ? "成功"
                    : "失敗";
                Debug.Log(msg);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                throw;
            }
            finally
            {
                _isLoading = false;
                AudioClipLoader.OnUpdateProgress -= UpdateLoadProgress;
                EditorUtility.ClearProgressBar();
            }
        }

        internal void OnUpdate()
        {
            if (!_isLoading) return;

            // var elapsed = AudioClipLoader.Stopwatch.Elapsed;
            // var progress = AudioClipLoader.LoadProgress;
            // UpdateLoadProgress(elapsed, progress);
            // OnUpdateLoadProgress?.Invoke(progress);
        }

        //private void UpdateLoadProgress(TimeSpan timeSpan,float progress)
        private void UpdateLoadProgress(float progress)
        {
            Debug.Log($"OnUpdate : {progress} from LoadFilePanel");
            EditorUtility.DisplayProgressBar(
                kProgressBarTitle,
                //$"{progress:P0}/100% Elapsed : {timeSpan.TotalSeconds}s",
                $"{progress:F0} / 100% ",
                progress);
            OnUpdateLoadProgress?.Invoke(progress);
        }

        private void Clear()
        {
            _isLoading = false;
            _filePath = null;
            OnUpdateLoadProgress = null;
        }

        void IDisposable.Dispose()
        {
            Clear();
            GC.SuppressFinalize(this);
        }
    }
}
#endif
