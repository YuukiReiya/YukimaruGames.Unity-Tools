#if UNITY_EDITOR
using System;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("YukimaruGames.Editor.Tools.AudioPlayer.Core")]

namespace YukimaruGames.Editor.Tools.Extensions
{
    
    internal static class FormatExtensions
    {
        /// <summary>
        /// 秒から時刻フォーマットに直す.
        /// </summary>
        /// <param name="seconds">秒</param>
        /// <returns>00:00:00(00:00)形式</returns>
        internal static string TimeFormated(this float seconds)
        {
            var timeSpan = TimeSpan.FromSeconds(seconds);
            return 1 <= timeSpan.TotalHours
                ? $"{timeSpan.Hours:D2}:{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}"
                : $"{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
        }
    }
}
#endif
