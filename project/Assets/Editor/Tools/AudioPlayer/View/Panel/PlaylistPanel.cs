#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using YukimaruGames.Editor.Tools.AudioPlayer.Application;
using YukimaruGames.Editor.Tools.AudioPlayer.Domain;
using YukimaruGames.Editor.Tools.Extensions;
// ReSharper disable InconsistentNaming

namespace YukimaruGames.Editor.Tools
{
    internal sealed class PlaylistPanel : IDisposable
    {
        private Vector2 _scrollPosition;
        private readonly IAudioClipRepository _audioClipRepository;

        private readonly List<PlaylistItem> _list = new();
        private readonly ReorderableList _reorderableList;

        private readonly Lazy<GUIStyle> _musicNameStyleLazy = new(() => new GUIStyle(EditorStyles.largeLabel) { alignment = TextAnchor.MiddleLeft, contentOffset = new Vector2(0, 0),fontStyle = FontStyle.Bold,fontSize = 16});
        private readonly Lazy<GUIStyle> _volumeStyleLazy = new(() => new GUIStyle(EditorStyles.label) { alignment = TextAnchor.MiddleRight});
        private readonly Lazy<GUIStyle> _lengthStyleLazy = new(() => new GUIStyle(EditorStyles.label) { alignment = TextAnchor.MiddleRight });

        private readonly Lazy<GUIContent> _musicNameContentLazy;
        private readonly Lazy<GUIContent> _playButtonContentLazy;
        private readonly Lazy<GUIContent> _deleteButtonContentLazy;
        private readonly Lazy<GUIContent> _lengthContentLazy;
        private readonly Lazy<GUIContent> _volumeContentLazy;

        private readonly Color _lineColor = new(0.15f, 0.15f, 0.15f);
        private readonly Color _focusedColor = new(0.33f, 0.33f, 0.33f, 0.9f);
        private readonly Color _activeColor = new(0.1f, 0.4f, 0.6f, 0.9f);

        private const float kPadding = 2f;

        internal PlaylistPanel(IBuiltInEditorIconRepository iconRepository,IAudioClipRepository audioClipRepository)
        {
            _audioClipRepository = audioClipRepository;
            _musicNameContentLazy = new Lazy<GUIContent>(() => new GUIContent(string.Empty, iconRepository.GetIcon("d_AudioClip On Icon")));
            _playButtonContentLazy = new Lazy<GUIContent>(() => new GUIContent(string.Empty, iconRepository.GetIcon("d_PlayButton")));
            _deleteButtonContentLazy = new Lazy<GUIContent>(() => new GUIContent(string.Empty, iconRepository.GetIcon("d_TreeEditor.Trash")));
            _lengthContentLazy = new Lazy<GUIContent>(() => new GUIContent(string.Empty, iconRepository.GetIcon("UnityEditor.AnimationWindow")));
            _volumeContentLazy = new Lazy<GUIContent>(() => new GUIContent(string.Empty, iconRepository.GetIcon("d_GameViewAudio On")));
            
            _reorderableList = new ReorderableList(_list, typeof(PlaylistItem), true, false, false, false)
            {
                drawElementCallback = DrawItem,
                drawElementBackgroundCallback = DrawItemBackground,
                elementHeight = 40,
            };
            
            _list.Clear();
            _list.AddRange(_audioClipRepository.List.Select(x => new PlaylistItem(x.Key, x.AudioClip)));

            _list.Add(new PlaylistItem("dummy", "dummy_music_name", 1f, 60f));
            _list.Add(new PlaylistItem("dummy2", "dummy_music_name", 0.9f, 3600f));
            _list.Add(new PlaylistItem("dummy3", "dummy_music_name", 0.01f, 3600f*60));
            
            _audioClipRepository.OnAddElement += OnAddElement;
            _audioClipRepository.OnRemoveElement += OnRemoveElement;
        }

        ~PlaylistPanel()
        {
            IDisposable disposer = this;
            disposer.Dispose();
        }
        
        void IDisposable.Dispose()
        {
            // TODO マネージリソースをここで解放します
            _audioClipRepository.OnAddElement -= OnAddElement;
            _audioClipRepository.OnRemoveElement -= OnRemoveElement;
            
            _reorderableList.drawElementBackgroundCallback -= DrawItemBackground;
            _reorderableList.drawElementCallback -= DrawItem;
            GC.SuppressFinalize(this);
        }

        internal void Show()
        {
            using var layoutScope = new EditorGUILayout.VerticalScope(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Playlist", EditorStyles.boldLabel);
            DrawScrollList();
        }

        private void DrawScrollList()
        {
            using var scrollScope = new EditorGUILayout.ScrollViewScope(_scrollPosition);
            _scrollPosition = scrollScope.scrollPosition;
            _reorderableList.DoLayoutList();
        }
        
        private void DrawItemBackground(Rect rect, int index, bool isActive, bool isFocused)
        {
            const float thickness = 1f;
            rect.height -= thickness;
            
            // Focus.
            if (isFocused)
            {
                EditorGUI.DrawRect(rect, _activeColor);
            }
            // Active.
            else if (isActive)
            {
                EditorGUI.DrawRect(rect, _focusedColor);
            }
            
            // 下線.
            var underLineRect = new Rect
            {
                x = rect.x,
                y = rect.y + rect.height,
                width = rect.width,
                height = thickness,
            };
            EditorGUI.DrawRect(underLineRect, _lineColor);
        }

        private void DrawItem(Rect rect, int index, bool isActive, bool isFocused)
        {
            var item = _list[index];

            //  曲名.
            var musicNameRect = new Rect(rect);
            MaterializeMusicNameRect(ref musicNameRect);
            _musicNameContentLazy.Value.text = item.MusicName;
            GUI.Label(musicNameRect, _musicNameContentLazy.Value, _musicNameStyleLazy.Value);

            // 削除ボタン.
            var deleteButtonRect = new Rect(rect);
            MaterializeDeleteButtonRect(ref deleteButtonRect);
            GUI.Button(deleteButtonRect, _deleteButtonContentLazy.Value); 

            // 再生ボタン.
            var playButtonRect = new Rect(deleteButtonRect);
            MaterializePlayButtonRect(ref playButtonRect);
            GUI.Button(playButtonRect, _playButtonContentLazy.Value);

            // 音量.
            var volumeRect = new Rect(playButtonRect);
            MaterializeVolumeRect(ref volumeRect);
            _volumeContentLazy.Value.text = item.Volume.ToString("P0");
            GUI.Label(volumeRect, _volumeContentLazy.Value, _volumeStyleLazy.Value);

            // 再生時間.
            var lengthRect = new Rect(volumeRect);
            MaterializeLengthRect(ref lengthRect);
            _lengthContentLazy.Value.text = item.Length.TimeFormated();
            GUI.Label(lengthRect, _lengthContentLazy.Value, _lengthStyleLazy.Value);
        }

        private void MaterializeMusicNameRect(ref Rect rect)
        {
            rect.y += kPadding;
        }

        private void MaterializeLengthRect(ref Rect rect)
        {
            rect.width += 20f;
            rect.x -= rect.width + kPadding;
        }
        
        private void MaterializeVolumeRect(ref Rect rect)
        {
            rect.x -= rect.width + kPadding;
        }

        private void MaterializePlayButtonRect(ref Rect rect)
        {
            rect.x -= rect.width + kPadding;
        }
        
        private void MaterializeDeleteButtonRect(ref Rect rect)
        {
            const float width = 50f;
            rect.height -= 6f;
            rect.position = new(rect.x + rect.width - width + 4f, rect.y);
            rect.y += kPadding;
            rect.width = width;
        }
        
        private void OnAddElement(string key, AudioClip content)
        {
            _list.Add(new PlaylistItem(key, content));
        }

        private void OnRemoveElement(string key)
        {
            var index = _list.FindIndex(x => x.Key == key);
            if (index < 0)
            {
                _list.RemoveAt(index);
            }
        }
    }
}
#endif