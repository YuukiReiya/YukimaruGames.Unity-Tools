#if UNITY_EDITOR
namespace YukimaruGames.Editor.Tools.AudioPlayer.Domain
{
    internal enum AudioPlaybackState
    {
        Stopped = 0,
        Paused = 1,
        Playing = 2,
        Error = -1,
    }
}
#endif
