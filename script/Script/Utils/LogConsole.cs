using System.Collections.Generic;
using UnityEngine;
using System;

namespace Consolation
{
    /// <summary>  
    /// A console to display Unity's debug logs in-game.  
    /// </summary>  
    public class LogConsole : MonoBehaviour
    {
        public static LogConsole instance { get { return m_instance; } }
        private static LogConsole m_instance = null;

        public static void Show()
        {
            if (null == m_instance)
            {
                GameObject gameObject = new GameObject("LogConsole");
                m_instance = gameObject.AddComponent<LogConsole>();
            }
            m_instance.visible = true;
        }
        
        struct Log
        {
            public string message;
            public string stackTrace;
            public LogType type;
        }

        #region Inspector Settings

        /// <summary>  
        /// Whether to open the window by shaking the device (mobile-only).  
        /// </summary>  
        public bool shakeToOpen = true;

        /// <summary>  
        /// The (squared) acceleration above which the window should open.  
        /// </summary>  
        public float shakeAcceleration = 8f;

        /// <summary>  
        /// Whether to only keep a certain number of logs.  
        ///  
        /// Setting this can be helpful if memory usage is a concern.  
        /// </summary>  
        public bool restrictLogCount = true;

        /// <summary>  
        /// Number of logs to keep before removing old ones.  
        /// </summary>  
        public int maxLogs = 1000;

        public Action<string> onGmCmd = null;

        #endregion

        readonly List<Log> logs = new List<Log>();
        Vector2 scrollPosition;
        bool visible;
        bool collapse;
        bool showWindow = false;
        bool showGmWindow = false;

        bool showLog = true;
        bool showError = true;
        bool showWarning = true;
        bool showException = true;
        bool showAssert = true;

        private string m_gm = "";

        // Visual elements:  

        static readonly Dictionary<LogType, Color> logTypeColors = new Dictionary<LogType, Color>  
        {  
            { LogType.Assert, Color.white },  
            { LogType.Error, Color.red },  
            { LogType.Exception, Color.red },  
            { LogType.Log, Color.white },  
            { LogType.Warning, Color.yellow },  
        };

        const string windowTitle = "Window";
        const int margin = 20;
        static readonly GUIContent clearLabel = new GUIContent("Clear", "Clear the contents of the console.");
        static readonly GUIContent closeLabel = new GUIContent("Close", "Close the console window");
        static readonly GUIContent btnHide = new GUIContent("Hide", "Hide the console window");
        static readonly GUIContent btnShow = new GUIContent("Show", "Show the console window");
        static readonly GUIContent btnGM = new GUIContent("GM", "Show the GM window");
        static readonly GUIContent btnProfile = new GUIContent("Profile", "Show game profile");
        static readonly GUIContent collapseLabel = new GUIContent("Collapse", "Hide repeated messages.");


        readonly Rect fullDragRect = new Rect(0, 0, 10000, 10000);
        readonly Rect titleBarRect = new Rect(0, 0, 10000, 20);

        Rect windowRect = new Rect(margin, margin, Screen.width - (margin * 2), Screen.height - (margin * 2));
        Rect btnWindowRect = new Rect((Screen.width - 45) / 2, (Screen.height - 45) / 2, 45, 45);
        Rect gmWindowRect = new Rect(margin, margin, Screen.width - (margin * 2), Screen.height - (margin * 2));

        void Awake()
        {
            DontDestroyOnLoad(this);
            visible = true;
        }

        void OnEnable()
        {
#if UNITY_5  
            Application.logMessageReceived += HandleLog;  
#else
            Application.RegisterLogCallback(HandleLog);
#endif
        }

        void OnDisable()
        {
#if UNITY_5  
            Application.logMessageReceived -= HandleLog;  
#else
            Application.RegisterLogCallback(null);
#endif
        }

        void Update()
        {
            if (shakeToOpen && Input.acceleration.sqrMagnitude > shakeAcceleration)
            {
                visible = true;
            }
        }

        void OnGUI()
        {
            if (!visible)
            {
                return;
            }


            if (showWindow)
            {
                windowRect = GUILayout.Window(200000, windowRect, DrawConsoleWindow, windowTitle);
            }
            else
            {
                btnWindowRect = GUILayout.Window(200001, btnWindowRect, DrawButtonWindow, windowTitle);
            }

            if (showGmWindow)
            {
                gmWindowRect = GUILayout.Window(200002, gmWindowRect, DrawGMWindow, "GM");
            }
        }

        void DrawButtonWindow(int windowID)
        {
            if (GUILayout.Button(btnShow, GUILayout.Width(45), GUILayout.Height(45)))
            {
                showWindow = true;
            }
            GUI.DragWindow(fullDragRect);
        }

        /// <summary>  
        /// Displays a window that lists the recorded logs.  
        /// </summary>  
        /// <param name="windowID">Window ID.</param>  
        void DrawConsoleWindow(int windowID)
        {
            DrawLogsList();
            DrawToolbar();

            // Allow the window to be dragged by its title bar.  
            GUI.DragWindow(titleBarRect);
        }

        private void DrawGMWindow(int windowID)
        {
            GUI.color = Color.green;
            GUILayout.Space(40f);
            GUILayout.BeginHorizontal();
            GUILayout.Label("指令：", GUILayout.Width(100f), GUILayout.Height(150f));
            m_gm = GUILayout.TextField(this.m_gm, GUILayout.Height(150f));
            GUILayout.EndHorizontal();
            GUILayout.Space(20f);
            if (GUILayout.Button("OK", GUILayout.Height(150f)))
            {
                showGmWindow = false;
                if (m_gm != null && m_gm.Length > 0)
                {
                    if (onGmCmd != null)
                    {
                        onGmCmd(m_gm);
                    }
                    m_gm = "";
                }
            }
            GUI.color = Color.white;
            GUI.DragWindow(fullDragRect);
        }

        bool IsShow(Log log)
        {
            if (log.type == LogType.Log) return showLog;
            if (log.type == LogType.Error) return showError;
            if (log.type == LogType.Exception) return showException;
            if (log.type == LogType.Assert) return showAssert;
            if (log.type == LogType.Warning) return showWarning;
            return true;
        }

        /// <summary>  
        /// Displays a scrollable list of logs.  
        /// </summary>  
        void DrawLogsList()
        {
            scrollPosition = GUILayout.BeginScrollView(scrollPosition);

            GUIStyle style = new GUIStyle();
            style.fontSize = 20;
            // Iterate through the recorded logs.  
            for (var i = 0; i < logs.Count; i++)
            {
                var log = logs[i];

                if (!IsShow(log)) continue;

                // Combine identical messages if collapse option is chosen.  
                if (collapse && i > 0)
                {
                    var previousMessage = logs[i - 1].message;

                    if (log.message == previousMessage)
                    {
                        continue;
                    }
                }

                // GUI.contentColor = logTypeColors[log.type];  
                style.normal.textColor = logTypeColors[log.type];
                GUILayout.Label(log.message, style);
            }

            GUILayout.EndScrollView();

            // Ensure GUI colour is reset before drawing other components.  
            GUI.contentColor = Color.white;
        }

        /// <summary>  
        /// Displays options for filtering and changing the logs list.  
        /// </summary>  
        void DrawToolbar()
        {
            GUILayout.BeginHorizontal();

            if (GUILayout.Button(clearLabel, GUILayout.Height(40)))
            {
                logs.Clear();
            }

            if (GUILayout.Button(closeLabel, GUILayout.Height(40)))
            {
                visible = false;
            }

            if (GUILayout.Button(btnHide, GUILayout.Height(40)))
            {
                showWindow = false;
            }

            if (GUILayout.Button(btnGM, GUILayout.Height(40)))
            {
                showGmWindow = !showGmWindow;
            }

            if (GUILayout.Button(btnProfile, GUILayout.Height(40)))
            {
                LogProfiler lProfiler = gameObject.GetComponent<LogProfiler>();
                if (null == lProfiler)
                {
                    gameObject.AddComponent<LogProfiler>();
                }
                else
                {
                    GameObject.Destroy(lProfiler);
                }
            }

            collapse = GUILayout.Toggle(collapse, collapseLabel, GUILayout.ExpandWidth(false));

            GUILayout.EndHorizontal();
        }

        /// <summary>  
        /// Records a log from the log callback.  
        /// </summary>  
        /// <param name="message">Message.</param>  
        /// <param name="stackTrace">Trace of where the message came from.</param>  
        /// <param name="type">Type of message (error, exception, warning, assert).</param>  
        void HandleLog(string message, string stackTrace, LogType type)
        {
            logs.Add(new Log
            {
                message = message,
                stackTrace = stackTrace,
                type = type,
            });

            TrimExcessLogs();
        }

        /// <summary>  
        /// Removes old logs that exceed the maximum number allowed.  
        /// </summary>  
        void TrimExcessLogs()
        {
            if (!restrictLogCount)
            {
                return;
            }

            var amountToRemove = Mathf.Max(logs.Count - maxLogs, 0);

            if (amountToRemove == 0)
            {
                return;
            }

            logs.RemoveRange(0, amountToRemove);
        }
    }
}