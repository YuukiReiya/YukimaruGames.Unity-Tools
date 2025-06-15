#if UNITY_EDITOR
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using UnityEngine;

namespace YukimaruGames.Editor.Tools
{
    /// <see cref=" https://qiita.com/Rijicho_nl/items/c7e6a5cb9cf56e52588a "/>
    internal static class InternalAudioUtilProxy
    {
        private static readonly Lazy<Type> AudioUtilType =
            new(typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.AudioUtil"));

        private static readonly ConcurrentDictionary<string, Func<object[], object>> CompiledMethods = new();

        // ReSharper disable once InconsistentNaming
        private const BindingFlags kBindingFlags =
            BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.GetField |
            BindingFlags.GetProperty | BindingFlags.InvokeMethod;

        public static void PlayPreviewClip(AudioClip clip) => Call(nameof(PlayPreviewClip), clip, 0, false);
        public static void StopAllPreviewClips() => Call(nameof(StopAllPreviewClips));
        public static void PausePreviewClip() => Call(nameof(PausePreviewClip));
        public static bool IsPreviewClipPlaying(AudioClip clip) => Call<bool, AudioClip>(nameof(IsPreviewClipPlaying), clip);
        public static float GetClipPosition(AudioClip clip) => Call<float, AudioClip>(nameof(GetClipPosition), clip);
        // ないかも？
        public static void SetClipPosition(AudioClip clip, float position) =>
            Call(nameof(SetClipPosition), clip, position);
        public static int GetClipSamplePosition(AudioClip clip) =>
            Call<int, AudioClip>(nameof(GetClipSamplePosition), clip);
        public static void SetClipSamplePosition(AudioClip clip, int position) =>
            Call(nameof(SetClipSamplePosition), clip, position);
        
        private static Func<object[], object> GetOrCompile(string methodName) =>
            CompiledMethods.GetOrAdd(methodName, m =>
            {
                var mi2Method = AudioUtilType.Value.GetMethod(m, kBindingFlags);

                var args = Expression.Parameter(typeof(object[]), "args");
                var parameters = mi2Method!.GetParameters()
                    .Select((p, idx) =>
                        (Expression)Expression.Convert(Expression.ArrayIndex(args, Expression.Constant(idx)),
                            p.ParameterType))
                    .ToArray();

                var lambda = Expression.Lambda<Func<object[], object>>(
                    mi2Method.ReturnType == typeof(void)
                        ? Expression.Block(Expression.Call(null, mi2Method, parameters),
                            Expression.Call(null, mi2Method, parameters),
                            Expression.Constant(null, typeof(object))
                        )
                        : Expression.Convert(
                            Expression.Call(null, mi2Method, parameters),
                            typeof(object)),
                    args);
                return lambda.Compile();
            });

        private static TResult Call<TResult>(string methodName) => (TResult)GetOrCompile(methodName).Invoke(null);
        private static TResult Call<TResult, T1>(string methodName, T1 arg1) =>
            (TResult)GetOrCompile(methodName).Invoke(new object[] { arg1 });
        private static TResult Call<TResult, T1, T2>(string methodName, T1 arg1, T2 arg2) =>
            (TResult)GetOrCompile(methodName).Invoke(new object[] { arg1, arg2 });
        private static TResult Call<TResult, T1, T2, T3>(string methodName, T1 arg1, T2 arg2, T3 arg3) =>
            (TResult)GetOrCompile(methodName).Invoke(new object[] { arg1, arg2, arg3 });

        private static void Call(string methodName) => GetOrCompile(methodName).Invoke(null);
        private static void Call<T1>(string methodName, T1 arg1) =>
            GetOrCompile(methodName).Invoke(new object[] { arg1 });
        private static void Call<T1, T2>(string methodName, T1 arg1, T2 arg2) =>
            GetOrCompile(methodName).Invoke(new object[] { arg1, arg2 });
        private static void Call<T1, T2, T3>(string methodName, T1 arg1, T2 arg2, T3 arg3) =>
            GetOrCompile(methodName).Invoke(new object[] { arg1, arg2, arg3 });
    }
}
#endif
