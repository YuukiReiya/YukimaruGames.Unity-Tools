#if UNITY_EDITOR
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using YukimaruGames.Editor.Tools.AudioPlayer.Application;
using File = UnityEngine.Windows.File;

// ReSharper disable InconsistentNaming

namespace YukimaruGames.Editor.Tools.AudioPlayer.View
{
    internal sealed class LoadFilePanel : IDisposable
    {
        private readonly IAudioClipLoader _loader;
        private readonly IAudioClipRepository _repository;
        private string _filePath;
        private bool _isLoading;

        internal event Action<float> OnUpdateLoadProgress;
        private readonly TimeSpan kTimeout = TimeSpan.FromSeconds(5f);
        private readonly Lazy<GUIContent> _headerContentLazy;
        private readonly Lazy<GUIContent> _inputFieldContentLazy;
        private readonly Lazy<GUIContent> _browseButtonContentLazy;
        private readonly Lazy<GUIContent> _loadButtonContentLazy;
        
        private const string kBrowseOpenFileTitle = "Select Audio File";
        private const string kFileExtensions = "mp3,wav";
        private const float kBrowseButtonWidth = 100f;

        internal LoadFilePanel(IBuiltInEditorIconRepository iconRepository,IAudioClipRepository audioClipRepository,IAudioClipLoader audioClipLoader)
        {
            _loader = audioClipLoader;
            _repository = audioClipRepository;
            _headerContentLazy =  new Lazy<GUIContent>(() => new GUIContent("Select External Audio File:", iconRepository.GetIcon("d_Profiler.FileAccess")));
            _inputFieldContentLazy = new Lazy<GUIContent>(() => new GUIContent("File Path"));
            _loadButtonContentLazy = new Lazy<GUIContent>(() => new GUIContent("Load Audio File", iconRepository.GetIcon("Download-Available")));
            _browseButtonContentLazy = new Lazy<GUIContent>(() => new GUIContent("Browse", iconRepository.GetIcon("Folder")));
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
            EditorGUILayout.LabelField(_headerContentLazy.Value, EditorStyles.boldLabel);
        }

        private void DrawInputField()
        {
            _filePath = EditorGUILayout.TextField(_inputFieldContentLazy.Value, _filePath);
        }

        private void DrawBrowseButton()
        {
            if (!GUILayout.Button(_browseButtonContentLazy.Value, GUILayout.Width(kBrowseButtonWidth)))
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
                var clicked = GUILayout.Button(_loadButtonContentLazy.Value);
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
                EditorUtility.DisplayDialog("Error", "Please select a valid audio file.", "OK");
            }
        }

        private async void Load()
        {
            var url = PathUtility.ToUrl(_filePath);
            var key = _filePath;
            var clipName = Path.GetFileNameWithoutExtension(_filePath);

            if (_repository.TryFind(key, out _))
            {
                return;
            }

            await LoadAsync(key, clipName, url);
        }

        private async Task LoadAsync(string key, string clipName, string url)
        {
            try
            {
                _isLoading = true;

                using var cts = new CancellationTokenSource(kTimeout);
                _loader.OnUpdateProgress += UpdateLoadProgress;
                var clip = await _loader.LoadAsync(url, cts.Token);

                if (clip != null)
                {
                    clip.name = clipName;
                    _repository.Add(key, clip);
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                throw;
            }
            finally
            {
                _isLoading = false;
                _loader.OnUpdateProgress -= UpdateLoadProgress;
                EditorUtility.ClearProgressBar();
            }
        }

        private void UpdateLoadProgress(float progress)
        {
            EditorUtility.DisplayProgressBar(
                "Loading...",
                $"{progress*100:F0}/100%",
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
