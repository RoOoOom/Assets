using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

public class LogHelper : MonoBehaviour {
    public const int MAX_ROWS = 100;

    private static LogHelper _instance;
    public static LogHelper Instance {
        get { return _instance; }
    }

    public bool m_onUGUI = true;

    [SerializeField]
    private UnityEngine.UI.Button m_openButton;
    [SerializeField]
    private UnityEngine.UI.Text m_text;
    [SerializeField]
    private GameObject m_panel;
    [SerializeField]
    private GUISkin m_guiSkin;

    private Dictionary<LogType, string> m_msgColor;
    private StringBuilder m_sb;
    private List<string> m_strList;
    private Vector2 m_posVec;
    private Rect m_rect = new Rect(0, 0, Screen.width, Screen.height);

    /// <summary>
    /// 静态方法，用于加载log窗口
    /// </summary>
    public static void Show()
    {
        GameObject gameObj = GameObject.Find("LogWindow");
        if (gameObj == null)
        {
            GameObject temp = Resources.Load("LogWindow") as GameObject;
            gameObj = Instantiate(temp);
        }

        _instance = gameObj.GetComponent<LogHelper>();
    }

    private void Awake()
    {
        DontDestroyOnLoad(this);

        m_msgColor = new Dictionary<LogType, string>
        {
            { LogType.Log , "<color=black>{0}</color>" },
            { LogType.Warning,"<color=yellow>{0}</color>"},
            { LogType.Error,"<color=red>{0}</color>"},
            { LogType.Assert , "<color=green>{0}</color>" },
            { LogType.Exception , "<color=red>{0}</color>" },
        };

        m_strList = new List<string>();
    }

    private void OnEnable()
    {
        if (m_onUGUI)
        {
            Application.logMessageReceived += HandleLog1;
        }
        else
        {
            Application.logMessageReceived += HandleLog2;
        }
    }

    private void OnDisable()
    {
        if (m_onUGUI)
        {
            Application.logMessageReceived -= HandleLog1;
        }
        else {
            Application.logMessageReceived -= HandleLog2;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Y))
        {
            Debug.Log(Time.time);
        }
        if (Input.GetKeyDown(KeyCode.H))
        {
            Debug.Log(m_strList.ToString());
            
        }


    }

    private void OnGUI()
    {
        if (m_onUGUI) return;

        GUI.skin = m_guiSkin;

        m_rect = GUILayout.Window(10000 , m_rect,DrawScroll ,"Log");
    }

    public void DrawScroll(int windowID)
    {
        m_posVec = GUILayout.BeginScrollView(m_posVec);

        for (int i = 0; i < m_strList.Count; i++)
        {
            GUILayout.Label(m_strList[i]);
        }
        GUILayout.EndScrollView();

        GUI.DragWindow();
    }

    /// <summary>
    /// 接收debug消息的方法1
    /// </summary>
    /// <param name="message">要打印的消息</param>
    /// <param name="stackTrace"></param>
    /// <param name="logType">消息类型</param>
    public void HandleLog1( string message , string stackTrace , LogType logType )
    {
        string msg = string.Format(m_msgColor[logType] , message);
        m_strList.Add(msg);
        m_text.GetComponent<TextAutoLong>().FixTextHeight();

        RefreshText();
    }

    /// <summary>
    /// 刷新文本界面
    /// </summary>
    public void RefreshText()
    {
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < m_strList.Count; ++i)
        {
            sb.AppendLine(m_strList[i]);
        }

        m_text.text = sb.ToString();
    }

    /// <summary>
    /// 接收debug消息的方法2
    /// </summary>
    /// <param name="message">要打印的消息</param>
    /// <param name="stackTrace"></param>
    /// <param name="logType">消息类型</param>
    public void HandleLog2(string message, string stackTrace, LogType logType)
    {
        m_strList.Add(message);
    }

    /// <summary>
    /// 缩小log窗口
    /// </summary>
    public void CloseLogWindow()
    {
        m_panel.SetActive(false);
        m_openButton.gameObject.SetActive(true);
    }

    /// <summary>
    /// 重新打开log窗口
    /// </summary>
    public void OpenLogWindow()
    {
        m_panel.SetActive(true);
        m_openButton.gameObject.SetActive(false);
    }
    
}
