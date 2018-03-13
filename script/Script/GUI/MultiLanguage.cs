using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class MultiLanguage : MonoBehaviour
{
    public delegate void RefreshHandler();
    public static event RefreshHandler refresh;

    private static Dictionary<Language, Dictionary<string, string>> m_ots = new Dictionary<Language, Dictionary<string, string>>();
    private static bool m_otIniting = false;

    public Text text { get { if (null == m_text) m_text = gameObject.GetComponent<Text>(); return m_text; } }
    private Text m_text;

    public RectTransform rect { get { if (null == m_rect) m_rect = gameObject.GetComponent<RectTransform>(); return m_rect; } }
    private RectTransform m_rect;

    public string key;
    public LanguageConfig[] configs;

    private Language m_curLanguage = Language.ZH_CN;
    void Start()
    {
        m_curLanguage = GameConfig.language;
        refresh += Refresh;
        InitOT();
    }

    void OnDestroy()
    {
        refresh -= Refresh;
    }

    private void M_Refresh()
    {
        if (m_curLanguage != GameConfig.language)
        {
            m_curLanguage = GameConfig.language;
            Refresh();
        }
    }

    public void Refresh()
    {
        LanguageConfig config = ArrayUtils.Find<LanguageConfig>(ref configs, it => it.language == m_curLanguage);
        if (null != config)
        {
            text.fontSize = config.fontSize;
            text.alignment = config.alignment;
            text.lineSpacing = config.lineSpacing;
            rect.sizeDelta = config.widthAndHeight;
            text.text = M_GetOT(m_curLanguage, key);
        }
    }

    private string M_GetOT(Language language, string key)
    {
        if (m_ots.ContainsKey(language))
        {
            Dictionary<string, string> content = m_ots[language];
            if (content.ContainsKey(key))
            {
                return content[key];
            }
        }

        JZLog.Log(" 没有该多语言配置：" + language.ToString() + " ==> " + key);
        return "";
    }

    public static void RefreshAll()
    {
        if (null != refresh) refresh();
    }

    public static void InitOT()
    {
        if (!m_otIniting)
        {
            m_otIniting = true;
        }
    }

    private static void AddOT(Language language, string text)
    {
        if (!m_ots.ContainsKey(language))
        {
            m_ots.Add(language, new Dictionary<string, string>());
        }

        Dictionary<string, string> _text = m_ots[language];


        string[] noComment = Regex.Split(text, @"\s*//.*\s*");
        for (int it = 0; it < noComment.Length; it++)
        {
            string item = noComment[it];
            if (string.IsNullOrEmpty(item)) continue;
            MatchCollection matches = Regex.Matches(item, @"\w+=");
            string[] values = Regex.Split(item, @"\w+=");
            for (int i = 0; i < matches.Count; i++)
            {
                string key = matches[i].Value;
                key = key.Substring(0, key.Length - 1);
                if (_text.ContainsKey(key))
                {
#if  UNITY_EDITOR
                    JZLog.LogError("AddOT - " + "Error: Duplicate " + key + " in OT files");
                    continue;
#endif
                    _text.Remove(key);
                }
                string value = values[i + 1];
                value = Regex.Replace(value, @"[\t\r\n]+", "");//去掉TAB,回车符
                value = Regex.Replace(value, @"\\n", "\n");//将\n替换成换行符
                _text.Add(key, value);
            }
        }
    }
}

[System.Serializable]
public class LanguageConfig
{
    public Language language;
    public int fontSize;
    public TextAnchor alignment;
    public int lineSpacing;
    public Vector2 widthAndHeight;
}

public enum Language
{
    ZH_CN,
    EN_US,
}