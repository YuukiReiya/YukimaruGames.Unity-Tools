#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using YukimaruGames.Editor.Tools.AudioPlayer.Application;

namespace YukimaruGames.Editor.Tools.AudioPlayer.Infrastructure
{
    /// <remarks>
    /// https://github.com/halak/unity-editor-icons
    /// </remarks>
    internal sealed class BuiltInEditorIconRepository : IBuiltInEditorIconRepository,IDisposable
    {
        private Dictionary<string, Texture> _textures;

        internal BuiltInEditorIconRepository()
        {
            Reset();
        }

        ~BuiltInEditorIconRepository()
        {
            IDisposable self = this;
            self.Dispose();
        }
        
        Texture IBuiltInEditorIconRepository.GetIcon(string iconName)
        {
            return GetOrAdd(iconName);
        }

        private Texture GetOrAdd(string iconName)
        {
            if (_textures.TryGetValue(iconName, out var texture))
            {
                if (texture != null && texture.Equals(null))
                {
                    return texture;
                }
                else
                {
                    _textures.Remove(iconName);
                }
            }

            var content = EditorGUIUtility.IconContent(iconName);
            
            {
                // fallback
                if (content == null)
                {
                    const string fallbackIcon = "d_BuildSettings.Broadcom";
                    content = EditorGUIUtility.IconContent(fallbackIcon);
                }

                // throw exception
                //if (content == null) throw new FileNotFoundException("Not found icon image.", iconName);
            }

            texture = content.image;
            _textures.Add(iconName, texture);
            return texture;
        }

        void IDisposable.Dispose()
        {
            Clear();
            GC.SuppressFinalize(this);
        }

        private void Clear()
        {
            _textures?.Clear();
            _textures = null;
        }

        private void Reset()
        {
            Clear();
            _textures = new Dictionary<string, Texture>();
        }
    }
}
#endif
