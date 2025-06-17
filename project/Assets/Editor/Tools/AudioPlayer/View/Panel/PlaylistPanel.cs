#if UNITY_EDITOR
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace YukimaruGames.Editor.Tools
{
    internal sealed class PlaylistPanel
    {
        private Vector2 _scrollPosition;
        private readonly IReadOnlyList<AudioRecord> _records;
        
        
        private const string kPlaylist = "Playlist";

        internal PlaylistPanel(IReadOnlyList<AudioRecord> records)
        {
            _records = records;
        }
        
        internal void Show()
        {
            EditorGUILayout.LabelField(kPlaylist, EditorStyles.boldLabel);
            using var layoutScope = new EditorGUILayout.VerticalScope(EditorStyles.helpBox);
            DrawScrollList();
        }

        private void DrawScrollList()
        {
            using var scrollScope = new EditorGUILayout.ScrollViewScope(_scrollPosition);
            foreach (var record in _records)
            {
                DrawItem(record);
            }
        }

        private void DrawItem(AudioRecord record)
        {
            var name = Path.GetFileNameWithoutExtension(record.Url);
            EditorGUILayout.LabelField(name, GUI.skin.textField);
        }
    }
}
#endif