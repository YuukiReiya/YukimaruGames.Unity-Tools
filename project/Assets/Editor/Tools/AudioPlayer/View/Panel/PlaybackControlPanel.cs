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
        private readonly IAudioPlaybackPresenter _presenter;
        private readonly IBuiltInEditorIconRepository _iconRepository;

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

        internal PlaybackControlPanel(IAudioPlaybackPresenter presenter,IBuiltInEditorIconRepository iconRepository)
        {
            _presenter = presenter;
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
            using (new EditorGUI.DisabledScope(_presenter.CurrentPlayingMusic == null || _presenter.IsPlaying))
            {
                if (GUILayout.Button(_playButtonContentLazy.Value, _buttonStyleLazy.Value,
                        GUILayout.Height(kButtonHeight), GUILayout.ExpandWidth(true)))
                {
                    _presenter.Play();
                }
            }

            using (new EditorGUI.DisabledScope(!_presenter.IsPlaying))
            {
                if (GUILayout.Button(_pauseButtonContentLazy.Value, _buttonStyleLazy.Value,
                        GUILayout.Height(kButtonHeight), GUILayout.ExpandWidth(true)))
                {
                    _presenter.Pause();
                }

                if (GUILayout.Button(_stopButtonContentLazy.Value, _buttonStyleLazy.Value,
                        GUILayout.Height(kButtonHeight), GUILayout.ExpandWidth(true)))
                {
                    _presenter.Stop();
                }
            }
        }

        private void DrawNavigationPanel()
        {
            using var scope = new EditorGUILayout.HorizontalScope();

            var isLoop = _presenter.Loop;

            if (GUILayout.Button(_prevButtonContentLazy.Value, _buttonStyleLazy.Value,
                    GUILayout.Height(kButtonHeight), GUILayout.ExpandWidth(true)))
            {
                // ひとつ前の再生.
                var list = _presenter.List;
                var index = list.FindIndex(x => x.Key == _presenter.CurrentPlayingMusic.Key);
                index = 0 < index ? --index : list.Count - 1;
                _presenter.NextPlayRequest = list[index];
                _presenter.Play();
            }

            _presenter.Loop = GUILayout.Toggle(isLoop, _loopButtonContentLazy.Value, _buttonStyleLazy.Value,
                GUILayout.Height(kButtonHeight), GUILayout.ExpandWidth(true));

            _presenter.Shuffle = GUILayout.Toggle(_presenter.Shuffle, _shuffleButtonContentLazy.Value,
                _buttonStyleLazy.Value, GUILayout.Height(kButtonHeight), GUILayout.ExpandWidth(true));

            if (GUILayout.Button(_nextButtonContentLazy.Value, _buttonStyleLazy.Value,
                    GUILayout.Height(kButtonHeight), GUILayout.ExpandWidth(true)))
            {
                // 次の再生.
                var list = _presenter.List;
                var index = list.FindIndex(x => x.Key == _presenter.CurrentPlayingMusic.Key);
                index = index + 1 < list.Count ? ++index : 0;
                _presenter.NextPlayRequest = list[index];
                _presenter.Play();
            }
        }

        private void DrawSeekBar()
        {
            using var scope = new GUILayout.VerticalScope();
            var item = _presenter.CurrentPlayingMusic;
            var isActive = item != null;
            var time = _presenter.Time;
            var length = item?.Length ?? 0f;
            var name = item?.MusicName ?? string.Empty;

            EditorGUILayout.Space(kHeaderSpace);
            using (new EditorGUI.DisabledScope(!isActive))
            {
                DrawPlaybackPosition(time, length);
                _presenter.Time = TimeSlider(time, length);
                if (isActive) DrawClipName(name);
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

        private void DrawClipName(string name)
        {
            EditorGUILayout.LabelField(new GUIContent(name, _iconRepository.GetIcon(kAudioClipIcon)), _clipNameStyleLazy.Value);
        }
    }
}
#endif