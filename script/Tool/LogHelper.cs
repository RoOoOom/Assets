using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

public class LogHelper : MonoBehaviour {
    public const int MAX_ROWS = 200;

    private static LogHelper _instance;
    public static LogHelper Instance {
        get { return _instance; }
    }

    [SerializeField]
    private GUISkin m_guiSkin;

    private bool openLog = false;
    private bool _showAll = true;
    private bool _regLowUp = false; //区分大小写
    private string _searchText = "";    //搜索关键字
    private LogType _limitType = LogType.Log;   //只想展示的消息类型
    private GUIStyle _custStyle;                     //
    private Dictionary<LogType, string> m_msgColor; //不同类型消息对应不同颜色
    private StringBuilder m_sb; 
    private List<strMessage> m_strList;
    private Vector2 m_posVec;
    private Rect m_rect = new Rect(20,0, Screen.width-40, Screen.height);
    private Rect m_littleRect = new Rect(Screen.width / 2, Screen.height / 2, 100, 100);
    private Color[] _colorList = { Color.red, Color.red, Color.yellow, Color.black, Color.red }; //LogType里 error = 0 , assert = 1 , warinig =2 ,log = 3 ,exception = 4 

    struct strMessage
    {
        public string _message;
        public LogType _logType;
    }

    /// <summary>
    /// 静态方法，用于加载log窗口
    /// </summary>
    public static void Show()
    {
        GameObject gameObj = GameObject.Find("LogWindow");
        if (gameObj == null)
        {
            GameObject temp = Resources.Load("LogWindow") as GameObject;
            GameObject parentObj = GameObject.Find("Canvas");

            gameObj = Instantiate(temp, parentObj.transform);
        }

        _instance = gameObj.GetComponent<LogHelper>();
    }

    private void Awake()
    {
        DontDestroyOnLoad(this);

        if (m_guiSkin == null)
        {
            m_guiSkin = Resources.Load("My GUI Skin") as GUISkin ;
        }

        m_msgColor = new Dictionary<LogType, string>
        {
            { LogType.Log , "<color=black>{0}</color>" },
            { LogType.Warning,"<color=yellow>{0}</color>"},
            { LogType.Error,"<color=red>{0}</color>"},
            { LogType.Assert , "<color=green>{0}</color>" },
            { LogType.Exception , "<color=red>{0}</color>" },
        };

        _custStyle = new GUIStyle();
        _custStyle.fontSize = 15;
        _custStyle.normal.textColor = Color.white;

        m_strList = new List<strMessage>();

    }

    private void OnEnable()
    {
        Application.logMessageReceived += HandleLog2;
    }

    private void OnDisable()
    {
       Application.logMessageReceived -= HandleLog2;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Y))
        {
            Debug.LogError(Time.time);
        }
        if (Input.GetKeyDown(KeyCode.H))
        {
            Debug.Log(m_strList.ToString());
        }

        if (Input.GetKeyDown(KeyCode.End))
        {
            GUI.UnfocusWindow();//这个没什么用，要失去焦点可以直接点GUI以外的地方
        }
    }

    private void OnGUI()
    {
        GUI.skin = m_guiSkin;

        if (openLog)
        {
            m_rect = GUILayout.Window(10000, m_rect, DrawScroll, "Log");
        }
        else
        {
            m_littleRect = GUILayout.Window(10001, m_littleRect, DrawLittleWindow, "");
        }
    }

    /// <summary>
    /// 绘制用于打开的小窗口
    /// </summary>
    /// <param name="windowID"></param>
    public void DrawLittleWindow( int windowID )
    {
        if (GUILayout.Button("+"))
        {
            OpenLogWindow();
        }

        GUI.DragWindow();
    }

    /// <summary>
    /// 绘制log窗口
    /// </summary>
    /// <param name="windowID"></param>
    public void DrawScroll(int windowID)
    {
        if (GUI.Button(new Rect(m_rect.width - 40, 0, 40, 40), "-"))
        {
            CloseLogWindow();
        }

        m_posVec = GUILayout.BeginScrollView(m_posVec);

        for (int i = 0; i < m_strList.Count; i++)
        {
            if (!_showAll && m_strList[i]._logType != _limitType) continue;

            if (!string.IsNullOrEmpty(_searchText))
            {
                if (_regLowUp && !m_strList[i]._message.Contains(_searchText)) continue;
                else if (!m_strList[i]._message.ToLower().Contains(_searchText.ToLower())) continue;
            }

            m_guiSkin.label.normal.textColor = _colorList[(int)m_strList[i]._logType];
            GUILayout.Label(m_strList[i]._message);
        }

        DrawButtonPart();

        GUI.DragWindow();
    }

    /// <summary>
    /// 绘制底部的GUI
    /// </summary>
    public void DrawButtonPart()
    {
        GUILayout.EndScrollView();

        _searchText = GUILayout.TextField(_searchText);

        GUILayout.BeginHorizontal();

        if (GUILayout.Button("All", GUILayout.Height(40)))
        {
            _showAll = true;
        }

        if (GUILayout.Button("Normal", GUILayout.Height(40)))
        {
            _showAll = false;
            _limitType = LogType.Log;
        }

        if (GUILayout.Button("Warning", GUILayout.Height(40)))
        {
            _showAll = false;
            _limitType = LogType.Warning;
        }

        if (GUILayout.Button("Error", GUILayout.Height(40)))
        {
            _showAll = false;
            _limitType = LogType.Error;
        }

        if (GUILayout.Button("Clear", GUILayout.Height(40)))
        {
            ClearContent();
        }

        _regLowUp = GUILayout.Toggle(_regLowUp, "区分大小写");
        GUILayout.EndHorizontal();
    }

    /// <summary>
    /// 接收debug消息的方法2
    /// </summary>
    /// <param name="message">要打印的消息</param>
    /// <param name="stackTrace"></param>
    /// <param name="logType">消息类型</param>
    public void HandleLog2(string message, string stackTrace, LogType logType)
    {
        strMessage sm;
        sm._message = message;
        sm._logType = logType;
        m_strList.Add(sm);

        if (m_strList.Count > MAX_ROWS)
        {
            m_strList.RemoveAt(0);
        }
    }

    /// <summary>
    /// 清理log窗口的文本内容
    /// </summary>
    public void ClearContent()
    {
        m_strList.Clear();
    }

    /// <summary>
    /// 缩小log窗口
    /// </summary>
    public void CloseLogWindow()
    {
        openLog = false;
    }

    /// <summary>
    /// 重新打开log窗口
    /// </summary>
    public void OpenLogWindow()
    {
        openLog = true;
    }
    
}
