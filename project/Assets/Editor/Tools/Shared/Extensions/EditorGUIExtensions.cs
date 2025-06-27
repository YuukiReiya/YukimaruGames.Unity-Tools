#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace YukimaruGames.Editor.Tools
{
    internal static class EditorGUIExtensions
    {
        internal static void DrawOutlineRect(Rect rect, float thickness, Color color)
        {
            EditorGUI.DrawRect(new Rect(rect.x, rect.y, rect.width, thickness), color);                // 上.
            EditorGUI.DrawRect(new Rect(rect.x, rect.y + rect.height, rect.width, thickness), color);  // 下.
            EditorGUI.DrawRect(new Rect(rect.x, rect.y, thickness, rect.height), color);                // 左.
            EditorGUI.DrawRect(new Rect(rect.x + rect.width, rect.y, thickness, rect.height), color);   // 右.
        }

    }
}
#endif
