#if UNITY_EDITOR
using UnityEngine;

namespace YukimaruGames.Editor.Tools.Extensions
{
    public static class ObjectExtensions
    {
        public static void Destroy(this Object self)
        {
            if (Application.isPlaying)
            {
                Object.Destroy(self);
            }
            else
            {
                Object.DestroyImmediate(self);
            }
        }
    }
}
#endif
