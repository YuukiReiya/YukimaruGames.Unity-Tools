#if UNITY_EDITOR

using System.Linq;
using UnityEditor;
using UnityEngine;
// ReSharper disable InconsistentNaming

namespace YukimaruGames.Editor.Tools
{
    public sealed class VolumeControlPanel
    {
        private float _lastUnmuteVolume = InternalAudioPlayer.Volume;
        private GUIStyle _currentVolumeStyle;
        private GUIStyle CurrentVolumeStyle
        {
            get
            {
                if (_currentVolumeStyle == null || _currentVolumeStyle.Equals(null))
                {
                    _currentVolumeStyle = new GUIStyle(EditorStyles.boldLabel)
                    {
                        alignment = TextAnchor.MiddleCenter,
                    };
                }
                return _currentVolumeStyle;
            }
        }
        
        private const string kHeaderName = "Volume Control";
        private const float kItemHeightSpace = 5f;
        private const string kMute = "Mute";
        private const string kUnmute = "Unmute";
        private const float kMuteLabelWidth = 80f;
        private const string kVolume = "Volume";
        private const string kPresets = "Presets";
        private const string kCurrentVolume = "Current Volume: ";

        private static readonly float[] Presets = { 0f, 0.25f, 0.5f, 0.75f, 1f };
        private static readonly float[] FineTunes = { 0.01f, 0.05f, 0.1f };

        public void Show()
        {
            using var layoutScope = new EditorGUILayout.VerticalScope(EditorStyles.helpBox);
            EditorGUILayout.LabelField(kHeaderName, EditorStyles.boldLabel);
            
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
                EditorGUILayout.LabelField(kMute,GUILayout.Width(kMuteLabelWidth));

                const float threshold = 0.001f;

                var isMuted = InternalAudioPlayer.Volume < threshold;
                if (GUILayout.Button(isMuted ? kUnmute : kMute))
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
            var volume = EditorGUILayout.Slider(kVolume, InternalAudioPlayer.Volume, 0f, 1f);
            InternalAudioPlayer.Volume = volume;
        }

        private void DrawVolumePresets()
        {
            EditorGUILayout.LabelField(kPresets, EditorStyles.boldLabel);
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
            EditorGUILayout.LabelField($"{kCurrentVolume} {InternalAudioPlayer.Volume:P1}", CurrentVolumeStyle);
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
