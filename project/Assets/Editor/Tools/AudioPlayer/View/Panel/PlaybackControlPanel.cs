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

        private const string kHeaderName = "Playback Control";
        private const float kButtonHeight = 30f;
        private const string kPlayTime = "Play Time";
        private const float kSliderHeight = 16f;

        private const string kPlay = "Play";
        private const string kPause = "Pause";
        private const string kStop = "Stop";
        private const string kLoop = "Repeat";
        private const string kPrev = "";
        private const string kNext = "";
        private const string kShuffle = "Shuffle";
        
        private const string kPlayIcon = "d_PlayButton";
        private const string kPauseIcon = "d_PauseButton";
        private const string kStopIcon = "d_StopButton";
        private const string kLoopIcon = "d_preAudioLoopOff";
        private const string kPrevIcon = "d_Animation.PrevKey";
        private const string kNextIcon = "d_Animation.NextKey";
        private const string kShuffleIcon = "d_UnityEditor.Graphs.AnimatorControllerTool";

        internal PlaybackControlPanel(IBuiltInEditorIconRepository iconRepository)
        {
            _iconRepository = iconRepository;
        }
        
        internal void Show()
        {
            EditorGUILayout.LabelField(kHeaderName, EditorStyles.boldLabel);

            using var layoutScope = new EditorGUILayout.VerticalScope(EditorStyles.helpBox);
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
                if (GUILayout.Button(new GUIContent(kPlay, GetIcon(kPlayIcon)), _buttonStyleLazy.Value,
                        GUILayout.Height(kButtonHeight), GUILayout.ExpandWidth(true)))
                {
                    InternalAudioPlayer.Play();
                }
            }

            using (new EditorGUI.DisabledScope(!InternalAudioPlayer.IsPlaying()))
            {
                if (GUILayout.Button(new GUIContent(kPause, GetIcon(kPauseIcon)), _buttonStyleLazy.Value,
                        GUILayout.Height(kButtonHeight), GUILayout.ExpandWidth(true)))
                {
                    InternalAudioPlayer.Pause();
                }

                if (GUILayout.Button(new GUIContent(kStop, GetIcon(kStopIcon)), _buttonStyleLazy.Value,
                        GUILayout.Height(kButtonHeight), GUILayout.ExpandWidth(true)))
                {
                    InternalAudioPlayer.Stop();
                }
            }
        }

        private void DrawNavigationPanel()
        {
            using var scope = new EditorGUILayout.HorizontalScope();
            
            if (GUILayout.Button(new GUIContent(kPrev, GetIcon(kPrevIcon)), _buttonStyleLazy.Value,
                    GUILayout.Height(kButtonHeight), GUILayout.ExpandWidth(true)))
            {
                InternalAudioPlayer.Play();
            }

            var isLoop = InternalAudioPlayer.Loop;
            InternalAudioPlayer.Loop = GUILayout.Toggle(isLoop,new GUIContent(kLoop, GetIcon(kLoopIcon)), _buttonStyleLazy.Value,
                    GUILayout.Height(kButtonHeight), GUILayout.ExpandWidth(true));

            _useShuffle = GUILayout.Toggle(_useShuffle, new GUIContent(kShuffle, GetIcon(kShuffleIcon)),
                _buttonStyleLazy.Value, GUILayout.Height(kButtonHeight), GUILayout.ExpandWidth(true));
            
            if (GUILayout.Button(new GUIContent(kNext, GetIcon(kNextIcon)), _buttonStyleLazy.Value,
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

            using (new EditorGUI.DisabledScope(!hasClip))
            {
                //EditorGUILayout.LabelField(kPlayTime, EditorStyles.label);
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
            EditorGUILayout.LabelField($"{length.TimeFormated()}", _timeLabelStyleLazy.Value);
        }

        private void DrawClipName(AudioClip clip)
        {
            EditorGUILayout.LabelField($"{clip.name}", _clipNameStyleLazy.Value);
        }

        private Texture GetIcon(string iconName) => _iconRepository.GetIcon(iconName);
    }
}
#endif