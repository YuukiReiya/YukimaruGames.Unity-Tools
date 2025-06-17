#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;



namespace YukimaruGames.Editor.Tools.Extensions
{
    internal static class EditorGUILayoutExtensions
    {
        internal static void DrawHorizontalLine(Color color, float height = 1)
        {
            var rect = EditorGUILayout.GetControlRect(false, height);
            EditorGUI.DrawRect(rect, color);
        }
    }
}
#endif
