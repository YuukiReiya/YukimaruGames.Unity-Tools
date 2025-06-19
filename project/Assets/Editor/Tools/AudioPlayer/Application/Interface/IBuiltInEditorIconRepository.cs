#if UNITY_EDITOR
using UnityEngine;

namespace YukimaruGames.Editor.Tools.AudioPlayer.Application
{
    internal interface IBuiltInEditorIconRepository
    {
        internal Texture GetIcon(string iconName);
    }
}
#endif
