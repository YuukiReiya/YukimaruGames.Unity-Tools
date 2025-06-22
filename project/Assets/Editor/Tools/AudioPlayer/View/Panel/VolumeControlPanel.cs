#if UNITY_EDITOR

using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using YukimaruGames.Editor.Tools.AudioPlayer.Application;

// ReSharper disable InconsistentNaming

namespace YukimaruGames.Editor.Tools
{
    public sealed class VolumeControlPanel
    {
        private float _lastUnmuteVolume = InternalAudioPlayer.Volume;
        private readonly Lazy<GUIStyle> _currentVolumeStyleLazy = new(() => new GUIStyle(EditorStyles.boldLabel)
        {
            alignment = TextAnchor.MiddleCenter
        });

        private readonly Lazy<GUIContent> _headerContentLazy;
        private readonly Lazy<GUIContent> _muteContentLazy;
        private readonly Lazy<GUIContent> _unmuteContentLazy;
        private readonly Lazy<GUIContent> _presetContentLazy;
        private readonly Lazy<GUIContent> _volumeContentLazy;
        
        private const float kItemHeightSpace = 5f;
        private const float kMuteLabelWidth = 80f;
        private const string kCurrentVolume = "Current Volume: ";
        
        private static readonly float[] Presets = { 0f, 0.25f, 0.5f, 0.75f, 1f };
        private static readonly float[] FineTunes = { 0.01f, 0.05f, 0.1f };

        internal VolumeControlPanel(IBuiltInEditorIconRepository iconRepository)
        {
            _headerContentLazy = new Lazy<GUIContent>(() => new GUIContent("Volume Control", iconRepository.GetIcon("d_AudioImporter Icon")));
            _muteContentLazy = new Lazy<GUIContent>(() => new GUIContent("Mute", iconRepository.GetIcon("d_GameViewAudio")));
            _unmuteContentLazy = new Lazy<GUIContent>(() => new GUIContent("Unmute", iconRepository.GetIcon("d_GameViewAudio On")));
            _presetContentLazy = new Lazy<GUIContent>(() => new GUIContent("Presets", iconRepository.GetIcon("Preset.Context")));
            _volumeContentLazy = new Lazy<GUIContent>(() => new GUIContent("Volume", iconRepository.GetIcon("Audio Mixer")));
        }
        
        public void Show()
        {
            using var layoutScope = new EditorGUILayout.VerticalScope(EditorStyles.helpBox);
            EditorGUILayout.LabelField(_headerContentLazy.Value, EditorStyles.boldLabel);
            
            EditorGUILayout.Space(kItemHeightSpace);
            DrawMuteButton();
            EditorGUILayout.Space(kItemHeightSpace);
            DrawVolumeSlider();
            EditorGUILayout.Space(kItemHeightSpace);
            DrawVolumePresets();
            EditorGUILayout.Space(kItemHeightSpace);
            DrawFineTunePanel();
        }

        private void DrawMuteButton()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(_muteContentLazy.Value,GUILayout.Width(kMuteLabelWidth));
                
                const float threshold = 0.001f;

                var isMuted = InternalAudioPlayer.Volume < threshold;
                if (GUILayout.Button((isMuted ? _unmuteContentLazy : _muteContentLazy).Value))
                {
                    if (isMuted)
                    {
                        InternalAudioPlayer.Volume = _lastUnmuteVolume;
                    }
                    else
                    {
                        _lastUnmuteVolume = InternalAudioPlayer.Volume;
                        InternalAudioPlayer.Volume = 0f;
                    }
                }
            }
        }

        private void DrawVolumeSlider()
        {
            var volume = EditorGUILayout.Slider(_volumeContentLazy.Value, InternalAudioPlayer.Volume, 0f, 1f);
            InternalAudioPlayer.Volume = volume;
        }

        private void DrawVolumePresets()
        {
            EditorGUILayout.LabelField(_presetContentLazy.Value, EditorStyles.boldLabel);
            using var scope = new EditorGUILayout.HorizontalScope();

            foreach (var value in Presets)
            {
                if (GUILayout.Button(value.ToString("P0")))
                {
                    InternalAudioPlayer.Volume = value;
                }
            }
        }

        private void DrawFineTunePanel()
        {
            using var scope = new EditorGUILayout.HorizontalScope();
            foreach (var value in FineTunes.OrderByDescending(v => v))
            {
                if (GUILayout.Button($"-{value:P0}"))
                {
                    InternalAudioPlayer.Volume = Mathf.Max(0f, InternalAudioPlayer.Volume - value);
                }
            }

            GUILayout.FlexibleSpace();
            EditorGUILayout.LabelField($"{kCurrentVolume} {InternalAudioPlayer.Volume:P1}", _currentVolumeStyleLazy.Value);
            GUILayout.FlexibleSpace();

            foreach (var value in FineTunes.OrderBy(v => v))
            {
                if (GUILayout.Button($"+{value:P0}"))
                {
                    InternalAudioPlayer.Volume = Mathf.Min(InternalAudioPlayer.Volume + value, 1f);
                }
            }
        }
    }
}
#endif
