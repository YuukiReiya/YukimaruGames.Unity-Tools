#if UNITY_EDITOR
using System;
using System.IO;
using UnityEngine;

namespace YukimaruGames.Editor.Tools
{
    public static class PathUtility
    {
        public static string ToUrl(string localFilePath)
        {
            if (string.IsNullOrWhiteSpace(localFilePath))
            {
                throw new ArgumentException("Local file path is null or empty. Cannot convert to URL.",
                    nameof(localFilePath));
            }

            string absolutePath;
            try
            {
                absolutePath = Path.GetFullPath(localFilePath);
            }
            catch (Exception e)
            {
                Debug.LogError($"Invalid file path format: '{localFilePath}'. Error: {e.Message}");
                return null;
            }
            
            try
            {
                var uri = new Uri(absolutePath);
                return uri.AbsoluteUri; 
            }
            catch (UriFormatException ex)
            {
                Debug.LogError($"Failed to convert path '{absolutePath}' to URL. Invalid URI format: {ex.Message}");
                return null;
            }
            catch (Exception ex)
            {
                Debug.LogError($"An unexpected error occurred during URL conversion for path: '{absolutePath}'. Error: {ex.Message}");
                return null;
            }
        }
    }
}
#endif