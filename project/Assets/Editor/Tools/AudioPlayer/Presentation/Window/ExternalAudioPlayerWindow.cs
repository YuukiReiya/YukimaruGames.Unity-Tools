#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.IO;
using YukimaruGames.Editor.Tools.AudioPlayer.Infrastructure;
using YukimaruGames.Editor.Tools.AudioPlayer.View;

// ReSharper disable InconsistentNaming

namespace YukimaruGames.Editor.Tools
{
    internal sealed class ExternalAudioPlayerWindow : EditorWindow
    {
        private const string kToolName = "External Audio Player";
        private const float kGridWidth = 100f;

        private Vector2 _scrollPosition;
        private VolumeControlPanel _volumePanel;
        private PlaybackControlPanel _playbackPanel;
        private PlaylistPanel _playlistPanel;

        private BuiltInEditorIconRepository _iconRepository;
            

        private GUIStyle _headerStyle;
        private GUIStyle _cellStyle;
        private GUIStyle _centerBoldStyle;
        private GUIStyle _controlButtonStyle;
        
        private GUIStyle HeaderStyle
        {
            get
            {
                if (_headerStyle == null || _headerStyle.Equals(null))
                {
                    _headerStyle = new(EditorStyles.boldLabel)
                    {
                        alignment = TextAnchor.MiddleLeft,
                        padding = new RectOffset(5, 5, 2, 2),
                    };
                }

                return _headerStyle;
            }
        }

        private GUIStyle CellStyle
        {
            get
            {
                if (_cellStyle == null || _cellStyle.Equals(null))
                {
                    _cellStyle = new(EditorStyles.label)
                    {
                        alignment = TextAnchor.MiddleLeft,
                        padding = new RectOffset(5, 5, 2, 2)
                    };
                }
                return _cellStyle;
            }
        }
        
        private GUIStyle CenterBoldStyle
        {
            get
            {
                if (_cellStyle == null || _cellStyle.Equals(null))
                {
                    _centerBoldStyle = new(EditorStyles.boldLabel)
                    {
                        alignment = TextAnchor.MiddleCenter,
                    };
                }
                return _centerBoldStyle;
            }
        }

        private string _filePath;
        private bool _isLoading;
        private float _lastUnmuteVolume;

        [MenuItem("Tools/" + kToolName)]
        internal static void ShowWindow()
        {
            GetWindow<ExternalAudioPlayerWindow>(kToolName);
        }

        private void OnEnable()
        {
            SetUp();
        }

        private void OnDisable()
        {
            TearDown();
        }

        private void OnGUI()
        {
            using var scope = new GUILayout.ScrollViewScope(_scrollPosition);
            _scrollPosition = scope.scrollPosition;
            EditorGUILayout.LabelField("Select External Audio File:", EditorStyles.boldLabel);

            // ファイルパスの入力フィールドとファイル選択ボタン
            using (new EditorGUILayout.HorizontalScope())
            {
                _filePath = EditorGUILayout.TextField("File Path", _filePath);
                DrawBrowseButton();
            }

            DrawLoadButton();
            
            //EditorGUILayout.Space(5);
            DrawResetButton();
            //EditorGUILayout.Space(5);
            
            // Info.
            DrawMusicInfo();
            
            // Menu.
            DrawMenuButton();
            
            using (new EditorGUILayout.HorizontalScope())
            {
                DrawPlayButton();
                DrawStopButton();
            }

            if (_isLoading)
            {
                Repaint();
            }
        }

        private void SetUp()
        {
            InternalAudioPlayer.OnUpdateLoadProgress += UpdateLoadProgress;

            _iconRepository = new BuiltInEditorIconRepository();
            _volumePanel = new VolumeControlPanel();
            _playbackPanel = new PlaybackControlPanel(_iconRepository);
            _playlistPanel = new PlaylistPanel(InternalAudioPlayer.AudioClips);
        }

        private void TearDown()
        {
            InternalAudioPlayer.OnUpdateLoadProgress -= UpdateLoadProgress;

            IDisposable disposable = _iconRepository;
            disposable.Dispose();

            _volumePanel = null;
            _playbackPanel = null;
            _playlistPanel = null;
            _iconRepository = null;
        }
        
        private void DrawResetButton()
        {
            if (!GUILayout.Button("Reset"))
            {
                return;
            }
            
            InternalAudioPlayer.TearDown();
            InternalAudioPlayer.SetUp();
        }
        
        private void DrawBrowseButton()
        {
            if (!GUILayout.Button("Browse", GUILayout.Width(60)))
            {
                return;
            }

            var path = EditorUtility.OpenFilePanel("Select Audio File", "", "mp3,wav,ogg,aiff");
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            _filePath = path;
        }

        private void DrawLoadButton()
        {
            using (new EditorGUILayout.VerticalScope())
            {
                using (new EditorGUI.DisabledScope(_isLoading))
                {
                    var isLoad = GUILayout.Button("Load Audio File");
                    if (!InternalAudioPlayer.HasClip())
                    {
                        EditorGUILayout.HelpBox("Load an audio file to enable playback controls.", MessageType.Info);
                    }

                    if (!isLoad)
                    {
                        return;
                    }
                }
            }

            if (!string.IsNullOrEmpty(_filePath) && File.Exists(_filePath))
            {
                Load();
            }
            else
            {
                EditorUtility.DisplayDialog("Error", "Please select a valid audio file.", "OK");
            }
        }

        private void DrawMusicInfo()
        {
            EditorGUILayout.LabelField("Music Info", EditorStyles.boldLabel);

            if (!InternalAudioPlayer.HasClip())
            {
                EditorGUILayout.HelpBox("Music data is none.", MessageType.Info);
                return;
            }

            var clip = InternalAudioPlayer.GetCurrentClip();

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                // ヘッダー.
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField("項目", HeaderStyle, GUILayout.Width(kGridWidth)); // 固定幅
                    DrawVerticalLine();
                    EditorGUILayout.LabelField("値", HeaderStyle); // 残りの幅
                }

                // ヘッダーと内容の間の区切り線
                DrawHorizontalLine();

                // タイトル.
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField("タイトル", CellStyle, GUILayout.Width(kGridWidth));
                    DrawVerticalLine();
                    EditorGUILayout.LabelField(clip.name, CellStyle);
                }
                DrawHorizontalLine();

                // 再生時間.
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField("再生時間", CellStyle, GUILayout.Width(kGridWidth));
                    DrawVerticalLine();
                    EditorGUILayout.LabelField(FormatTime(clip.length), CellStyle);
                }
                DrawHorizontalLine();
                
                // サンプル周波数.
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField("サンプル周波数", CellStyle, GUILayout.Width(kGridWidth));
                    DrawVerticalLine();
                    EditorGUILayout.LabelField($"{clip.frequency}Hz", CellStyle, GUILayout.Width(kGridWidth));
                }
                DrawHorizontalLine();

                // ループ設定行
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField("チャンネル", CellStyle, GUILayout.Width(kGridWidth));
                    DrawVerticalLine();
                    EditorGUILayout.LabelField(clip.channels.ToString(), CellStyle, GUILayout.Width(kGridWidth));
                }
            }
        }

        private void DrawMenuButton()
        {
            EditorGUILayout.LabelField("Control Menu", EditorStyles.boldLabel);
            
            _volumePanel.Show();
            using (new EditorGUI.DisabledScope(InternalAudioPlayer.GetCurrentClip() == null))
            {
                _playbackPanel.Show();
            }
            _playlistPanel.Show();

            //DrawVolume();
            DrawControlButton();
             
        }

        private void DrawControlButton()
        {
            using (new EditorGUILayout.HorizontalScope())
                {
                    // ボタンの共通スタイル (Optional: ボタンの背景色などを変える場合)
                    // GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
                    // if (EditorMusicPlaybackManager.IsPlaying) {
                    //     // 再生中のボタンを強調する例 (テクスチャが必要になるので今回はコメントアウト)
                    //     // buttonStyle.normal.background = EditorGUIUtility.whiteTexture;
                    //     // buttonStyle.normal.textColor = Color.green;
                    // }

                    // 再生ボタン
                    using (new EditorGUI.DisabledScope(_isLoading || InternalAudioPlayer.IsPlaying()))
                    {
                        // GUILayout.ExpandWidth(true) で横いっぱいに広がるようにする
                        if (GUILayout.Button(new GUIContent("Play", EditorGUIUtility.IconContent("d_PlayButton").image), GUILayout.Height(30), GUILayout.ExpandWidth(true)))
                        {
                        }
                    }

                    // 一時停止/再開ボタン
                    using (new EditorGUI.DisabledScope(_isLoading || (!InternalAudioPlayer.IsPlaying() /*&& !EditorMusicPlaybackManager.IsPaused*/)))
                    {
                        var isPaused = true;
                        string pauseResumeText =isPaused ? "Resume" : "Pause";
                        var pauseResumeIcon = isPaused ? EditorGUIUtility.IconContent("d_PlayButton").image : EditorGUIUtility.IconContent("d_PauseButton").image;

                        if (GUILayout.Button(new GUIContent(pauseResumeText, pauseResumeIcon), GUILayout.Height(30), GUILayout.ExpandWidth(true)))
                        {
                            // if (EditorMusicPlaybackManager.IsPaused)
                            // {
                            //     EditorMusicPlaybackManager.Resume();
                            // }
                            // else
                            // {
                            //     EditorMusicPlaybackManager.Pause();
                            // }
                        }
                    }

                    // 停止ボタン
                    using (new EditorGUI.DisabledScope(_isLoading || (!InternalAudioPlayer.IsPlaying()/* && !EditorMusicPlaybackManager.IsPaused*/)))
                    {
                        if (GUILayout.Button(new GUIContent("Stop", EditorGUIUtility.IconContent("d_StopButton").image), GUILayout.Height(30), GUILayout.ExpandWidth(true)))
                        {
                            //EditorMusicPlaybackManager.Stop();
                        }
                    }
                } // End of HorizontalScope for buttons
        }
        
        private void DrawPlayButton()
        {
            bool isPlay;
            using (new EditorGUILayout.VerticalScope())
            {
                using (new EditorGUI.DisabledScope(!InternalAudioPlayer.HasClip()))
                {
                    isPlay = GUILayout.Button("Play");
                }

                if (!InternalAudioPlayer.HasClip())
                {
                    EditorGUILayout.HelpBox("No valid audio file set.", MessageType.Error);
                }
            }

            if (!isPlay)
            {
                return;
            }

            InternalAudioPlayer.Play();
        }

        private void DrawStopButton()
        {
            bool isStop;
            using (new EditorGUILayout.VerticalScope())
            {
                using (new EditorGUI.DisabledScope(!InternalAudioPlayer.HasClip()))
                {
                    isStop = GUILayout.Button("Stop");
                }

                if (!InternalAudioPlayer.IsPlaying())
                {
                    EditorGUILayout.HelpBox("No audio playing.", MessageType.Warning);
                }
            }

            if (!isStop)
            {
                return;
            }

            InternalAudioPlayer.Stop();
        }

        private static void DrawHorizontalLine(int height = 1, Color? color = null)
        {
            var rect = EditorGUILayout.GetControlRect(false, height);
            EditorGUI.DrawRect(rect, color ?? new Color(0.5f, 0.5f, 0.5f, 1)); // デフォルトは灰色
        }
        
        private static void DrawVerticalLine(float width = 1f, Color? color = null)
        {
            // 現在の描画位置に、指定された幅と、現在の行の高さを持つ矩形を取得
            // GUILayoutUtility.GetLastRect() で直前のコントロールのRectを取得し、
            // その右端に縦線を引くのが一般的ですが、今回はHorizontalScope内で直接配置するため、
            // そのままGetControlRectで幅を指定します。
            var rect = EditorGUILayout.GetControlRect(GUILayout.Width(width));
            EditorGUI.DrawRect(rect, color ?? new Color(0.5f, 0.5f, 0.5f, 1));
        }

        // private void DrawVolume()
        // {
        //     var source = InternalAudioPlayer.Source;
        //     // ヘルプボックスで音量コントロールをグループ化
        //     using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
        //     {
        //         EditorGUILayout.LabelField("Volume Control", EditorStyles.boldLabel);
        //         EditorGUILayout.Space(5);
        //
        //         // 1. ミュート/ミュート解除ボタン
        //         using (new EditorGUILayout.HorizontalScope())
        //         {
        //             EditorGUILayout.LabelField("Mute", GUILayout.Width(80));
        //
        //
        //             // 現在の音量が0に近い場合をミュートと判断
        //             bool isMuted = source.volume < 0.001f;
        //             if (GUILayout.Button(isMuted ? "Unmute" : "Mute"))
        //             {
        //                 if (isMuted)
        //                 {
        //                     // ミュート解除時は、以前の音量に戻す（0.5fは例、ScriptableObjectに保存すると良い）
        //                     InternalAudioPlayer.Volume = _lastUnmuteVolume;
        //                     //source.Volume = EditorMusicPlayerSettings.Instance.LastUnmutedVolume > 0 ? EditorMusicPlayerSettings.Instance.LastUnmutedVolume : 0.5f;
        //                 }
        //                 else
        //                 {
        //                     // ミュートする前に現在の音量を保存
        //                     _lastUnmuteVolume = InternalAudioPlayer.Volume;
        //                     InternalAudioPlayer.Volume = 0f;
        //                 }
        //                 // AudioClipが設定されていれば、AudioSourceの音量も更新
        //                 // if (InternalAudioPlayer.SourceInstance != null && !InternalAudioPlayer.SourceInstance.Equals(null))
        //                 // {
        //                 //     InternalAudioPlayer.SourceInstance.volume = EditorMusicPlaybackManager.Volume;
        //                 // }
        //             }
        //         }
        //
        //         EditorGUILayout.Space(5);
        //
        //         // 2. 音量スライダー
        //         float newVolume = EditorGUILayout.Slider("Volume", source.volume, 0f, 1f);
        //         if (Mathf.Abs(newVolume - source.volume) > 0.001f) // 変化があった場合のみ更新
        //         {
        //             source.volume = newVolume;
        //             // AudioClipが設定されていれば、AudioSourceの音量も更新
        //             // if (InternalAudioPlayer.SourceInstance != null && !InternalAudioPlayer.SourceInstance.Equals(null))
        //             // {
        //             //     InternalAudioPlayer.SourceInstance.volume = EditorMusicPlaybackManager.Volume;
        //             // }
        //         }
        //
        //         EditorGUILayout.Space(5);
        //
        //         // 3. 音量プリセットボタン
        //         EditorGUILayout.LabelField("Presets", EditorStyles.boldLabel);
        //         using (new EditorGUILayout.HorizontalScope())
        //         {
        //             if (GUILayout.Button("25%")) source.volume = 0.25f;
        //             if (GUILayout.Button("50%")) source.volume = 0.50f;
        //             if (GUILayout.Button("75%")) source.volume = 0.75f;
        //             if (GUILayout.Button("100%")) source.volume = 1.00f;
        //         }
        //
        //         EditorGUILayout.Space(5);
        //
        //         // 4. 音量微調整ボタン
        //         EditorGUILayout.LabelField("Fine Tune", EditorStyles.boldLabel);
        //         using (new EditorGUILayout.HorizontalScope())
        //         {
        //             if (GUILayout.Button("-5%"))
        //                 source.volume = Mathf.Max(0f, source.volume - 0.05f);
        //             if (GUILayout.Button("-1%"))
        //                 source.volume = Mathf.Max(0f, source.volume - 0.01f);
        //             GUILayout.FlexibleSpace(); // ボタン間のスペースを柔軟に確保
        //             EditorGUILayout.LabelField($"Current Volume: {source.volume:P0}", CenterBoldStyle);
        //             GUILayout.FlexibleSpace(); // ボタン間のスペースを柔軟に確保
        //             if (GUILayout.Button("+1%"))
        //                 source.volume = Mathf.Min(1f, source.volume + 0.01f);
        //             if (GUILayout.Button("+5%"))
        //                 source.volume = Mathf.Min(1f, source.volume + 0.05f);
        //         }
        //
        //         // 音量が変わったらAudioSourceにも反映
        //         // これは各ボタンやスライダーの処理後にも実行されるため、ここにまとめて記述
        //         // if (InternalAudioPlayer.SourceInstance != null && !InternalAudioPlayer.SourceInstance.Equals(null))
        //         // {
        //         //     InternalAudioPlayer.SourceInstance.volume = EditorMusicPlaybackManager.Volume;
        //         // }
        //     }
        // }

        private void Load()
        {
            try
            {
                _isLoading = true;
                InternalAudioPlayer.Set(_filePath);
                
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            finally
            {
                _isLoading = false;
                EditorUtility.ClearProgressBar();

            }
        }

        private void UpdateLoadProgress(float progress) =>
            EditorUtility.DisplayProgressBar(
                "Loading...",
                $"{progress * 100:F1}/100.0%",
                progress);

        private static string FormatTime(float seconds)
        {
            var timeSpan = TimeSpan.FromSeconds(seconds);
            return 1 <= timeSpan.TotalHours
                ? $"{timeSpan.TotalHours:D2}:{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}"
                : $"{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
        }
    }
}
#endif
