#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;
using YukimaruGames.Editor.Tools.Extensions;
// ReSharper disable InconsistentNaming

namespace YukimaruGames.Editor.Tools.AudioPlayer.Infrastructure
{
    [InitializeOnLoad]
    internal static class EditorAudioDriverAdapter
    {
        private static IAudioDriver _driver;
        private static GameObject _go;
        private static AudioSource _source;

        private const string kHideGameObjectName = "EditorGlobalAudioPlayer";

        static EditorAudioDriverAdapter()
        {
            EditorApplication.quitting -= OnEditorQuitting;
            EditorApplication.quitting += OnEditorQuitting;
            
            Initialize();
        }
        
        internal static IAudioDriver GetDriver()
        {
            if (_driver == null)
            {
                Initialize();
            }

            return _driver;
        }

        private static void Initialize()
        {
            _go = GameObject.Find(kHideGameObjectName);
            if (_go == null)
            {
                _go = EditorUtility.CreateGameObjectWithHideFlags(
                    kHideGameObjectName,
                    HideFlags.HideAndDontSave,
                    typeof(AudioSource));
            }

            _source = _go.GetComponent<AudioSource>();
            if (_source == null)
            {
                _source = _go.AddComponent<AudioSource>();
            }

            _driver = new UnityAudioDriver(_source);
        }

        private static void OnEditorQuitting()
        {
            // ReSharper disable once SuspiciousTypeConversion.Global
            (_driver as IDisposable)?.Dispose();

            if (_go != null)
            {
                _go.Destroy();
                _go = null;
            }

            _source = null;
            _driver = null;
            EditorApplication.quitting -= OnEditorQuitting;
        }
    }
}
#endif
