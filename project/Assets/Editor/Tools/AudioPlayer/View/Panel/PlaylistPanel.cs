#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using YukimaruGames.Editor.Tools.AudioPlayer.Application;
using YukimaruGames.Editor.Tools.AudioPlayer.Domain;
using YukimaruGames.Editor.Tools.Extensions;
// ReSharper disable InconsistentNaming

namespace YukimaruGames.Editor.Tools.AudioPlayer.View
{
    internal sealed class PlaylistPanel : IDisposable
    {
        private Vector2 _scrollPosition;
        private bool _disposed;

        private readonly IAudioPlaybackPresenter _presenter;
        private readonly IPlaylistItemRepository _repository;
        private readonly List<PlaylistItem> _list = new();
        private readonly Queue<PlaylistItem> _deleteQueue = new();
        private readonly ReorderableList _reorderableList;

        private readonly Lazy<GUIStyle> _musicNameStyleLazy = new(() => new GUIStyle(EditorStyles.largeLabel) { alignment = TextAnchor.MiddleLeft, contentOffset = new Vector2(0, 0),fontStyle = FontStyle.Bold,fontSize = 16});
        private readonly Lazy<GUIStyle> _volumeStyleLazy = new(() => new GUIStyle(EditorStyles.label) { alignment = TextAnchor.MiddleRight});
        private readonly Lazy<GUIStyle> _lengthStyleLazy = new(() => new GUIStyle(EditorStyles.label) { alignment = TextAnchor.MiddleRight });

        private readonly Lazy<GUIContent> _musicNameContentLazy;
        private readonly Lazy<GUIContent> _playButtonContentLazy;
        private readonly Lazy<GUIContent> _pauseButtonContentLazy;
        private readonly Lazy<GUIContent> _deleteButtonContentLazy;
        private readonly Lazy<GUIContent> _lengthContentLazy;
        private readonly Lazy<GUIContent> _volumeContentLazy;

        private readonly Color _lineColor = new(0.15f, 0.15f, 0.15f);
        private readonly Color _focusedColor = new(0.33f, 0.33f, 0.33f, 0.9f);
        private readonly Color _activeColor = new(0.1f, 0.4f, 0.6f, 0.9f);

        private const float kPadding = 2f;

        internal PlaylistPanel(IAudioPlaybackPresenter presenter, IBuiltInEditorIconRepository iconRepository, IPlaylistItemRepository repository)
        {
            _presenter = presenter;
            _repository = repository;
            _musicNameContentLazy = new Lazy<GUIContent>(() => new GUIContent(string.Empty, iconRepository.GetIcon("d_AudioClip On Icon")));
            _playButtonContentLazy = new Lazy<GUIContent>(() => new GUIContent(string.Empty, iconRepository.GetIcon("d_PlayButton")));
            _pauseButtonContentLazy = new Lazy<GUIContent>(() => new GUIContent(string.Empty, iconRepository.GetIcon("d_PauseButton")));
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
            _list.AddRange(repository.List);

            _repository.OnAddElement += OnAddElement;
            _repository.OnRemoveElement += OnRemoveElement;
        }

        ~PlaylistPanel()
        {
            (this as IDisposable).Dispose();
        }
        
        void IDisposable.Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        internal void Dispose(bool disposable)
        {
            if (_disposed) return;

            if (disposable)
            {
                _repository.OnAddElement -= OnAddElement;
                _repository.OnRemoveElement -= OnRemoveElement;
            
                _reorderableList.drawElementBackgroundCallback -= DrawItemBackground;
                _reorderableList.drawElementCallback -= DrawItem;
            }
            
            _disposed = true;
        }
        
        internal void Show()
        {
            using var layoutScope = new EditorGUILayout.VerticalScope(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Playlist", EditorStyles.boldLabel);
            DrawScrollList();
            
            // ReorderableListの描画が終わってから描画要素を削除.
            DeleteIfNeeded();
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

            // 削除ボタン.
            var deleteButtonRect = new Rect(rect);
            MaterializeDeleteButtonRect(ref deleteButtonRect);
            if (GUI.Button(deleteButtonRect, _deleteButtonContentLazy.Value))
            {
                _deleteQueue.Enqueue(item);
            } 

            // 再生ボタン / 一時停止ボタン.
            var isPlaying = _presenter.CurrentPlayingMusic.Key == item.Key && _presenter.IsPlaying;
            var playButtonRect = new Rect(deleteButtonRect);
            MaterializePlayButtonRect(ref playButtonRect);

            if (isPlaying)
            {
                if (GUI.Button(playButtonRect, _pauseButtonContentLazy.Value))
                {
                    _presenter.Pause();
                }
            }
            else
            {
                if (GUI.Button(playButtonRect, _playButtonContentLazy.Value))
                {
                    _presenter.CurrentPlayingMusic = item;
                    _presenter.Play();
                }
            }
            
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
            
            //  曲名.
            var musicNameRect = new Rect(rect);
            MaterializeMusicNameRect(ref musicNameRect, in lengthRect);
            _musicNameContentLazy.Value.text = item.MusicName;
            GUI.Label(musicNameRect, _musicNameContentLazy.Value, _musicNameStyleLazy.Value);
        }

        private void MaterializeMusicNameRect(ref Rect rect,in Rect argRect1)
        {
            rect.y += kPadding;
            rect.xMax = argRect1.x;
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

        private void DeleteIfNeeded()
        {
            using var e = _deleteQueue.GetEnumerator(); 
            while (e.MoveNext())
            {
                var current = e.Current;
                _list.Remove(current);
            }
        }
        
        private void OnAddElement(PlaylistItem item)
        {
            _list.Add(item);
        }

        private void OnRemoveElement(PlaylistItem item)
        {
            _list.Remove(item);
        }
    }
}
#endif