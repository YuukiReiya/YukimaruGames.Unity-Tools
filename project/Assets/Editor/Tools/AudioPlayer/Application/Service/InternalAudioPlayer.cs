#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace YukimaruGames.Editor.Tools
{
    [InitializeOnLoad]
    internal static class InternalAudioPlayer
    {
        // ReSharper disable once InconsistentNaming
        private const string kHideGameObjectName = "EditorGlobalAudioPlayer";

        private static GameObject _go;
        private static AudioSource _source;
        internal static readonly List<AudioRecord> AudioClips = new();

        internal static event Action OnSetUp;
        internal static event Action OnTearDown;
        internal static event Action<float> OnUpdateLoadProgress;
        internal static event Action<float> OnUpdateVolume;
        private static event Action<float> OnUpdateTime; 

        private static GameObject Go
        {
            get
            {
                if (_go != null && !_go.Equals(null))
                {
                    return _go;
                }

                return _go = GetOrCreateInstance();
            }
        }

        private static AudioSource Source
        {
            get
            {
                if (_source != null && !_source.Equals(null))
                {
                    return _source;
                }

                return _source = GetOrReattach();
            }
        }

        internal static float Volume
        {
            get => Source.volume;
            set
            {
                OnUpdateVolume?.Invoke(value);
                Source.volume = value;
            }
        }

        internal static float Time
        {
            get => HasClip() ? Source.time : 0f;
            set
            {
                OnUpdateTime?.Invoke(value);
                var clip = GetCurrentClip();
                if (clip != null)
                {
                    Source.time = Mathf.Clamp(value, 0f, clip.length);
                }
            }
        }

        static InternalAudioPlayer()
        {
            EditorApplication.update -= OnEditorUpdate;
            EditorApplication.update += OnEditorUpdate;

            EditorApplication.quitting -= OnEditorQuit;
            EditorApplication.quitting += OnEditorQuit;

            SetUp();
        }

        private static void OnEditorUpdate()
        {
            
        }

        private static void OnEditorQuit()
        {
            EditorApplication.update -= OnEditorUpdate;
            EditorApplication.quitting -= OnEditorQuit;
            
            TearDown();
        }

        internal static void SetUp()
        {
            _go = GetOrCreateInstance();
            _source = GetOrReattach();

            // フォーカスを外すと音楽再生が止まってしまうためバックグラウンド処理を止めない.
            Application.runInBackground = true;
            
            OnSetUp?.Invoke();
        }

        internal static void TearDown()
        {
            if (Source.isPlaying)
            {
                Source.Stop();
            }

            if (Application.isPlaying)
            {
                UnityEngine.Object.Destroy(_go);
            }
            else
            {
                UnityEngine.Object.DestroyImmediate(_go);
            }
            _source = null;
            OnTearDown?.Invoke();
        }

        private static GameObject GetOrCreateInstance()
        {
            var go = GameObject.Find(kHideGameObjectName);
            if (go != null)
            {
                return go;
            }

            return EditorUtility.CreateGameObjectWithHideFlags(kHideGameObjectName, HideFlags.HideAndDontSave,
                typeof(AudioSource));
        }

        private static AudioSource GetOrReattach()
        {
            var component = Go.GetComponent<AudioSource>();
            return component != null ? component : Go.AddComponent<AudioSource>();
        }

        internal static void Play()
        {
            AudioClipValidThrowIfNeed();
            Source.Play();
        }

        internal static void Stop()
        {
            AudioClipValidThrowIfNeed();
            Source.Stop();
        }

        internal static bool HasClip() => Source.clip != null && !Source.clip.Equals(null);

        internal static AudioClip GetCurrentClip() => HasClip() ? Source.clip : null;
        
        internal static bool IsPlaying() =>
            HasClip() && Source.isPlaying;
        
        internal static AudioClip Load(string filePath)
        {
            var url = AudioClipLoader.GetUrl(filePath);

            var record = AudioClips.Find(x => x.Url == url);
            if (record != null)
            {
                return record.AudioClip;
            }

            AudioClip clip = null;
            AudioClipLoader.OnUpdateProgress += OnUpdateLoadProgress;
            using var e = AudioClipLoader.Load(url); 
            while (e.MoveNext())
            {
                var current = e.Current;

                if (current == null) continue;

                AudioClips.Add(new AudioRecord(url, current));
                Source.clip = current;
                clip = current;
                break;
            }
            AudioClipLoader.OnUpdateProgress -= OnUpdateLoadProgress;
            return clip;
        }

        internal static void Pause() => Source.Pause();
        internal static void Resume() => Source.UnPause();
        
        internal static void Set(string filePath)
        {
            if (Source.isPlaying)
            {
                Source.Stop();
            }

            var clip = Load(filePath);
            if (clip == null)
            {
                return;
            }

            Source.clip = clip;
        }

        private static void AudioClipValidThrowIfNeed()
        {
            if (!HasClip())
            {
                throw new InvalidOperationException("Audio clip is null.");
            }
        }
    }
}
#endif
