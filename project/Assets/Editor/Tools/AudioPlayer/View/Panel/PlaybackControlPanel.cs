#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;
using YukimaruGames.Editor.Tools.AudioPlayer.Application;
using YukimaruGames.Editor.Tools.Extensions;

// ReSharper disable InconsistentNaming

namespace YukimaruGames.Editor.Tools.AudioPlayer.View
{
    internal sealed class PlaybackControlPanel
    {
        private readonly IBuiltInEditorIconRepository _iconRepository;
        private bool _useShuffle;

        private readonly Lazy<GUIStyle> _buttonStyleLazy = new(() => new GUIStyle(GUI.skin.button));
        private readonly Lazy<GUIStyle> _timeLabelStyleLazy = new(() => new GUIStyle(EditorStyles.label)
        {
            padding = new RectOffset(5, 5, 0, 2),
            margin = new RectOffset(0, 0, 0, 0),
            wordWrap = true,

        });
        private readonly Lazy<GUIStyle> _clipNameStyleLazy = new(() => new GUIStyle(EditorStyles.label)
        {
            alignment = TextAnchor.MiddleCenter,
        });

        private readonly Lazy<GUIContent> _headerContentLazy;
        private readonly Lazy<GUIContent> _playButtonContentLazy;
        private readonly Lazy<GUIContent> _pauseButtonContentLazy;
        private readonly Lazy<GUIContent> _stopButtonContentLazy;
        private readonly Lazy<GUIContent> _prevButtonContentLazy;
        private readonly Lazy<GUIContent> _nextButtonContentLazy;
        private readonly Lazy<GUIContent> _loopButtonContentLazy;
        private readonly Lazy<GUIContent> _shuffleButtonContentLazy;

        private const float kButtonHeight = 30f;
        private const float kHeaderSpace = 6f;
        private const float kSliderHeight = 16f;
        private const string kAudioClipIcon = "d_AudioClip On Icon";

        internal PlaybackControlPanel(IBuiltInEditorIconRepository iconRepository)
        {
            _iconRepository = iconRepository;
            _headerContentLazy = new Lazy<GUIContent>(() => new GUIContent("Playback Menu", iconRepository.GetIcon("d_BuildSettings.Windows.Small")));
            _playButtonContentLazy = new Lazy<GUIContent>(() => new GUIContent("Play", iconRepository.GetIcon("d_PlayButton")));
            _pauseButtonContentLazy = new Lazy<GUIContent>(() => new GUIContent("Pause", iconRepository.GetIcon("d_PauseButton")));
            _stopButtonContentLazy = new Lazy<GUIContent>(() => new GUIContent("Stop", iconRepository.GetIcon("d_StopButton")));
            _loopButtonContentLazy = new Lazy<GUIContent>(() => new GUIContent("Repeat", iconRepository.GetIcon("d_preAudioLoopOff")));
            _shuffleButtonContentLazy = new Lazy<GUIContent>(() => new GUIContent("Shuffle", iconRepository.GetIcon("d_UnityEditor.Graphs.AnimatorControllerTool")));
            _prevButtonContentLazy = new Lazy<GUIContent>(() => new GUIContent(string.Empty, iconRepository.GetIcon("d_Animation.PrevKey")));
            _nextButtonContentLazy = new Lazy<GUIContent>(() => new GUIContent(string.Empty, iconRepository.GetIcon("d_Animation.NextKey")));
        }

        internal void Show()
        {
            using var layoutScope = new EditorGUILayout.VerticalScope(EditorStyles.helpBox);
            EditorGUILayout.LabelField(_headerContentLazy.Value, EditorStyles.boldLabel);

            DrawSeekBar();
            EditorGUILayout.Space(10);
            DrawButtonsPanel();
        }

        private void DrawButtonsPanel()
        {
            using var scope = new EditorGUILayout.VerticalScope();
            DrawPlaybackButtons();
            DrawNavigationPanel();
        }

        private void DrawPlaybackButtons()
        {
            using var scope = new EditorGUILayout.HorizontalScope();
            using (new EditorGUI.DisabledScope(InternalAudioPlayer.IsPlaying()))
            {
                if (GUILayout.Button(_playButtonContentLazy.Value, _buttonStyleLazy.Value,
                        GUILayout.Height(kButtonHeight), GUILayout.ExpandWidth(true)))
                {
                    InternalAudioPlayer.Play();
                }
            }

            using (new EditorGUI.DisabledScope(!InternalAudioPlayer.IsPlaying()))
            {
                if (GUILayout.Button(_pauseButtonContentLazy.Value, _buttonStyleLazy.Value,
                        GUILayout.Height(kButtonHeight), GUILayout.ExpandWidth(true)))
                {
                    InternalAudioPlayer.Pause();
                }

                if (GUILayout.Button(_stopButtonContentLazy.Value, _buttonStyleLazy.Value,
                        GUILayout.Height(kButtonHeight), GUILayout.ExpandWidth(true)))
                {
                    InternalAudioPlayer.Stop();
                }
            }
        }

        private void DrawNavigationPanel()
        {
            using var scope = new EditorGUILayout.HorizontalScope();

            if (GUILayout.Button(_prevButtonContentLazy.Value, _buttonStyleLazy.Value,
                    GUILayout.Height(kButtonHeight), GUILayout.ExpandWidth(true)))
            {
                InternalAudioPlayer.Play();
            }

            var isLoop = InternalAudioPlayer.Loop;
            InternalAudioPlayer.Loop = GUILayout.Toggle(isLoop, _loopButtonContentLazy.Value, _buttonStyleLazy.Value,
                GUILayout.Height(kButtonHeight), GUILayout.ExpandWidth(true));

            _useShuffle = GUILayout.Toggle(_useShuffle, _shuffleButtonContentLazy.Value,
                _buttonStyleLazy.Value, GUILayout.Height(kButtonHeight), GUILayout.ExpandWidth(true));

            if (GUILayout.Button(_nextButtonContentLazy.Value, _buttonStyleLazy.Value,
                    GUILayout.Height(kButtonHeight), GUILayout.ExpandWidth(true)))
            {
                InternalAudioPlayer.Play();
            }
        }

        private void DrawSeekBar()
        {
            using var scope = new GUILayout.VerticalScope();
            var clip = InternalAudioPlayer.GetCurrentClip();
            var hasClip = clip != null;
            var length = hasClip ? clip.length : 0f;
            var time = InternalAudioPlayer.Time;

            EditorGUILayout.Space(kHeaderSpace);
            using (new EditorGUI.DisabledScope(!hasClip))
            {
                DrawPlaybackPosition(time, length);
                InternalAudioPlayer.Time = TimeSlider(time, length);
                if (hasClip) DrawClipName(clip);
            }
        }

        private float TimeSlider(float time,float length)
        {
            using var scope = new EditorGUILayout.HorizontalScope();
            var rect = EditorGUILayout.GetControlRect(false, kSliderHeight, GUILayout.ExpandWidth(true));
            return GUI.HorizontalSlider(rect, time, 0f, length);
        }

        private void DrawPlaybackPosition(float time, float length)
        {
            using var scope = new EditorGUILayout.HorizontalScope();
            EditorGUILayout.LabelField($"{time.TimeFormated()}", _timeLabelStyleLazy.Value);
            GUILayout.FlexibleSpace();
            EditorGUILayout.LabelField($"{(Mathf.Approximately(length, 0f) ? "--:--" : length.TimeFormated())}", _timeLabelStyleLazy.Value);
        }

        private void DrawClipName(AudioClip clip)
        {
            EditorGUILayout.LabelField(new GUIContent(clip.name, _iconRepository.GetIcon(kAudioClipIcon)), _clipNameStyleLazy.Value);
        }
    }
}
#endif