using System;
using UnityEditor;
using UnityEngine;
using YukimaruGames.Editor.Tools.Extensions;

// ReSharper disable InconsistentNaming

namespace YukimaruGames.Editor.Tools
{
    public sealed class PlaybackControlPanel : IDisposable
    {
        private Texture _playIconTexture;
        private Texture _pauseIconTexture;
        private Texture _stopIconTexture;
        private Texture _loopIconTexture;
        private GUIStyle _activeButtonStyle;
        private GUIStyle _timeLabelStyle;
        
        private Texture PlayIcon
        {
            get
            {
                if (_playIconTexture == null || _playIconTexture.Equals(null))
                {
                    _playIconTexture = null;
                    _playIconTexture = EditorGUIUtility.IconContent("d_PlayButton").image;
                }

                return _playIconTexture;
            }
        }

        private Texture PauseIcon
        {
            get
            {
                if (_pauseIconTexture == null || _pauseIconTexture.Equals(null))
                {
                    _pauseIconTexture = null;
                    _pauseIconTexture = EditorGUIUtility.IconContent("d_PauseButton").image;
                }

                return _pauseIconTexture;
            }
        }

        private Texture StopIcon
        {
            get
            {
                if (_stopIconTexture == null || _stopIconTexture.Equals(null))
                {
                    _stopIconTexture = null;
                    _stopIconTexture = EditorGUIUtility.IconContent("d_StopButton").image;
                }

                return _stopIconTexture;
            }
        }

        private Texture LoopIcon
        {
            get
            {
                if (_loopIconTexture == null || _loopIconTexture.Equals(null))
                {
                    _loopIconTexture = null;
                    _loopIconTexture = EditorGUIUtility.IconContent("d_preAudioLoopOff").image;
                }

                return _loopIconTexture;
            }
        } 

        private GUIStyle ActiveButtonStyle
        {
            get
            {
                if (_activeButtonStyle == null || _activeButtonStyle.Equals(null))
                {
                    _activeButtonStyle = new GUIStyle(GUI.skin.button);
                }

                return _activeButtonStyle;
            }
        }

        private GUIStyle TimeLabelStyle
        {
            get
            {
                if (_timeLabelStyle == null || _timeLabelStyle.Equals(null))
                {
                    _timeLabelStyle = new GUIStyle(EditorStyles.label)
                    {
                        padding = new RectOffset(5, 5, 0, 2),
                        margin = new RectOffset(0,0,0,0),
                        wordWrap = true,
                        
                    };
                }

                return _timeLabelStyle;
            }
        }

        private const string kHeaderName = "Playback Control";
        private const float kButtonHeight = 30f;
        private const string kPlay = "Play";
        private const string kPause = "Pause";
        private const string kStop = "Stop";
        private const string kPlayTime = "Play Time";
        private const float kSliderHeight = 16f;
        
        public void Show()
        {
            EditorGUILayout.LabelField(kHeaderName, EditorStyles.boldLabel);

            using var layoutScope = new EditorGUILayout.VerticalScope(EditorStyles.helpBox);
            DrawSeekBar();
            EditorGUILayout.Space(10);
            DrawButtonsPanel();
        }

        private void DrawButtonsPanel()
        {
            using var scope = new EditorGUILayout.HorizontalScope();
            using (new EditorGUI.DisabledScope(InternalAudioPlayer.IsPlaying()))
            {
                if (GUILayout.Button(new GUIContent(kPlay, PlayIcon), ActiveButtonStyle,
                        GUILayout.Height(kButtonHeight), GUILayout.ExpandWidth(true)))
                {
                    InternalAudioPlayer.Play();
                }
            }

            using (new EditorGUI.DisabledScope(!InternalAudioPlayer.IsPlaying()))
            {
                if (GUILayout.Button(new GUIContent(kPause, PauseIcon), ActiveButtonStyle,
                        GUILayout.Height(kButtonHeight), GUILayout.ExpandWidth(true)))
                {
                    InternalAudioPlayer.Pause();
                }

                if (GUILayout.Button(new GUIContent(kStop, StopIcon), ActiveButtonStyle,
                        GUILayout.Height(kButtonHeight), GUILayout.ExpandWidth(true)))
                {
                    InternalAudioPlayer.Stop();
                }
            }
        }

        private void DrawSeekBar()
        {
            using var scope = new GUILayout.VerticalScope();
            var clip = InternalAudioPlayer.GetCurrentClip();
            var hasClip = clip != null;
            var length = hasClip ? clip.length : 0f;
            var time = InternalAudioPlayer.Time;

            using (new EditorGUI.DisabledScope(!InternalAudioPlayer.IsPlaying()))
            {
                EditorGUILayout.LabelField(kPlayTime, EditorStyles.label);
                InternalAudioPlayer.Time = TimeSlider(time, length);
                DrawPlaybackPosition(time, length);
            }

            // using (new EditorGUILayout.VerticalScope())
            // {
            //     using var scope = new EditorGUILayout.HorizontalScope();
            //     var clip = InternalAudioPlayer.GetCurrentClip();
            //     var hasClip = clip != null;
            //     var length = hasClip ? clip.length : 0f;
            //     var time = InternalAudioPlayer.Time;
            //
            //     // var itemNameStyle = new GUIStyle(EditorStyles.label)
            //     // {
            //     //     alignment = TextAnchor.MiddleRight,
            //     //     padding = new RectOffset(5, 5, 2, 2),
            //     //     wordWrap = true,
            //     // };
            //
            //     var height = EditorGUIUtility.singleLineHeight;
            //     //EditorGUILayoutExtensions.DrawHorizontalLine(Color.white, height);
            //
            //     using (new EditorGUI.DisabledScope(!hasClip))
            //     {
            //         //EditorGUILayout.LabelField($"{time.TimeFormated()}", itemNameStyle);
            //         //GUILayout.FlexibleSpace();
            //
            //         var sliderRect = EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight,
            //             GUILayout.ExpandWidth(true));
            //         //var value = EditorGUI.Slider(sliderRect, time, 0f, length);
            //         var value = GUI.Slider(sliderRect, time, 1f, 0f, length, GUI.skin.horizontalSlider,
            //             GUI.skin.horizontalSliderThumb, true, 0);
            //
            //         //GUILayout.FlexibleSpace();
            //         var diff = Mathf.Abs(value - time);
            //         if (kThreshold <= diff)
            //         {
            //             InternalAudioPlayer.Time = value;
            //         }
            //
            //         scope.Dispose();
            //         using var scope_ = new EditorGUILayout.HorizontalScope();
            //         EditorGUILayout.LabelField($"{time.TimeFormated()}", itemNameStyle);
            //         GUILayout.FlexibleSpace();
            //         EditorGUILayout.LabelField($"{length.TimeFormated()}", itemNameStyle);
            //     }
            // }
            //EditorGUILayoutExtensions.DrawHorizontalLine(Color.white, height);
        }

        private float TimeSlider(float time,float length)
        {
            using var scope = new EditorGUILayout.HorizontalScope();
            var rect = EditorGUILayout.GetControlRect(false, kSliderHeight, GUILayout.ExpandWidth(true));
            return GUI.HorizontalSlider(rect, time, 0f, length);
            // return GUI.Slider(
            //     rect, time, kSliderSize, 0f, length,
            //     GUI.skin.horizontalSlider,
            //     GUI.skin.horizontalSliderThumb, true, kSliderId);
        }

        private void DrawPlaybackPosition(float time, float length)
        {
            using var scope = new EditorGUILayout.HorizontalScope();
            EditorGUILayout.LabelField($"{time.TimeFormated()}", TimeLabelStyle);
            GUILayout.FlexibleSpace();
            EditorGUILayout.LabelField($"{length.TimeFormated()}", TimeLabelStyle);
        }
        
        private void DrawSeekBarDash()
        {
            using var scope = new EditorGUILayout.HorizontalScope();
            
            var clip = InternalAudioPlayer.GetCurrentClip();

            var hasClip = clip != null; 
            if (hasClip)
            {
                
            }
            else
            {
                return;
            }
 // "Play Time" ラベル
            GUIStyle cellStyle = new GUIStyle(EditorStyles.label)
            {
                alignment = TextAnchor.MiddleLeft,
                padding = new RectOffset(5, 5, 2, 2),
                wordWrap = true
            };
            float labelColWidth = 100f;
            EditorGUILayout.LabelField("Play Time", cellStyle, GUILayout.Width(labelColWidth));
            
            // 縦の区切り線
            //DrawVerticalLine(1f); // 縦の線の描画ヘルパーメソッドを想定

            // シークバーの有効/無効状態を制御するフラグ
            // bool isSeekBarEnabled = !EditorMusicPlaybackManager.IsLoading && 
            //                         InternalAudioPlayer.SourceInstance != null && 
            //                         InternalAudioPlayer.SourceInstance.clip != null && 
            //                         InternalAudioPlayer.SourceInstance.clip.length > 0;

            // スライダーが無効な場合はグレーアウトするスコープ
            using (new EditorGUI.DisabledScope(!hasClip))
            {
                // 現在の時間表示 (例: "00:00")
                //EditorGUILayout.LabelField($"{FormatTime(currentTime)}", GUILayout.Width(60)); 
                EditorGUILayout.LabelField($"{InternalAudioPlayer.Time:00:00}", GUILayout.Width(60));
                
                // シークスライダー本体
                // 第一引数: 現在のスライダーの値（出力もこの値）
                // 第二引数: スライダーの最小値
                // 第三引数: スライダーの最大値 (音楽の総再生時間)
                //float newTime = EditorGUILayout.Slider(currentTime, 0f, totalLength);
                float newTime = EditorGUILayout.Slider(InternalAudioPlayer.Time, 0f, clip.length);
                
                // スライダーの値が変更された場合の処理
                // Mathf.Abs(newTime - currentTime) > 0.01f は、ユーザーが実際にスライダーを動かしたか、
                // あるいは再生位置が大きく変化したかを判断するための閾値です。
                //if (isSeekBarEnabled && Mathf.Abs(newTime - currentTime) > 0.01f) 
                {
                    // InternalAudioPlayer.SourceInstance は AudioSource のインスタンスを指すと想定
                    // .time プロパティに新しい時間を設定することで、再生位置が変更されます。
                    //InternalAudioPlayer.SourceInstance.time = newTime;
                    InternalAudioPlayer.Time = newTime;
                }

                // 総再生時間表示 (例: "03:45")
                //EditorGUILayout.LabelField($"{FormatTime(totalLength)}", GUILayout.Width(60)); 
                EditorGUILayout.LabelField($"{clip.length:00:00}", GUILayout.Width(60));
            }
            
            
        }
        
        private Texture2D MakeColoredTexture(Color color, int width, int height)
        {
            var pixels = new Color[width * height];
            for (var i = 0; i < pixels.Length; i++)
            {
                pixels[i] = color;
            }

            var tex = new Texture2D(width, height);
            return tex;
        }

        public void Release()
        {
            _playIconTexture = null;
            _pauseIconTexture = null;
            _stopIconTexture = null;
        }
        
        
        public void Dispose()
        {
            Release();
            GC.SuppressFinalize(this);
        }

        ~PlaybackControlPanel()
        {
            Dispose();
        }
    }
}
