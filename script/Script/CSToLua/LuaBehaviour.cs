using UnityEngine;
using LuaInterface;
using System.Collections.Generic;
using UnityEngine.UI;

public class LuaBehaviour : MonoBehaviour
{
    public string name;

    private LuaFunction m_start;
    private LuaFunction m_onEnable;
    private LuaFunction m_onDisable;
    private LuaFunction m_onDestroy;

    private Dictionary<GameObject, LuaFunction> m_clickFunctions = new Dictionary<GameObject, LuaFunction>();
    private Dictionary<GameObject, LuaFunction> m_pressFunctions = new Dictionary<GameObject, LuaFunction>();
    private Dictionary<GameObject, LuaFunction> m_begindragFunctions = new Dictionary<GameObject, LuaFunction>();
    private Dictionary<GameObject, LuaFunction> m_enddragFunctions = new Dictionary<GameObject, LuaFunction>();

    void Start()
    {
        if (null != m_start)
        {
            m_start.Call();
        }
    }

    void OnEnable()
    {
        if (null != m_onEnable)
        { 
            m_onEnable.Call();
        }
    }

    void OnDisable()
    {
        if (null != m_onDisable)
        {
            m_onDisable.Call();
        }
    }

    void OnDestroy()
    {
        ClearClick();
        ClearPress();
        ClearBeginDrag();
        ClearEndDrag();
        if (null != m_onDestroy)
        {
            m_onDestroy.Call();
        }
        m_start.Dispose();
        m_onEnable.Dispose();
        m_onDisable.Dispose();
        m_onDestroy.Dispose();
        m_start = null;
        m_onEnable = null;
        m_onDisable = null;
        m_onDestroy = null;
    }

    public void SetName(string name)
    {
        this.name = name;
    }

    public void InitBehaviour(LuaFunction start, LuaFunction onEnable, LuaFunction onDisable, LuaFunction onDestroy)
    {
        m_start = start;
        m_onEnable = onEnable;
        m_onDisable = onDisable;
        m_onDestroy = onDestroy;
    }

    /// <summary>
    /// 添加单击事件
    /// </summary>
    public void AddClick(GameObject go, LuaFunction luafunc) 
    {
        if (go == null || luafunc == null) return;
        LuaFunction func = null;
        if (m_clickFunctions.TryGetValue(go, out func))
        {
            func.Dispose();
            m_clickFunctions[go] = luafunc;
        }
        else
        {
            m_clickFunctions.Add(go, luafunc);
        }

        Button button = go.GetComponent<Button>();
        if (null != button)
        {
            button.onClick.AddListener(() => { luafunc.Call(go); });
        }
        else
        {
            UIEventListener.Get(go).onClick = delegate(GameObject o) { luafunc.Call(go); };
        }
    }

    /// <summary>
    /// 删除单击事件
    /// </summary>
    /// <param name="go"></param>
    public void RemoveClick(GameObject go) 
    {
        if (go == null) return;
        LuaFunction luafunc = null;
        if (m_clickFunctions.TryGetValue(go, out luafunc)) 
        {
            m_clickFunctions.Remove(go);
            luafunc.Dispose();
            luafunc = null;
        }
    }

    /// <summary>
    /// 清除单击事件
    /// </summary>
    public void ClearClick() 
    {
        foreach (var de in m_clickFunctions) 
        {
            if (de.Value != null) 
            {
                de.Value.Dispose();
            }
        }
        m_clickFunctions.Clear();
    }

    public void AddBeginDrag(GameObject go, LuaFunction luafunc)
    {
         if (go == null || luafunc == null) return;
         m_begindragFunctions.Add(go, luafunc);
         UIEventListener.Get(go).onBeginDrag = delegate(GameObject o) { luafunc.Call(go); };
    }

    public void RemoveBeginDrag(GameObject go)
    {
        if (go == null) return;
        LuaFunction luafunc = null;
        if (m_begindragFunctions.TryGetValue(go, out luafunc))
        {
            m_begindragFunctions.Remove(go);
            luafunc.Dispose();
            luafunc = null;
        }
    }

    public void ClearBeginDrag()
    {
        foreach (var de in m_begindragFunctions)
        {
            if (de.Value != null)
            {
                de.Value.Dispose();
            }
        }
        m_begindragFunctions.Clear();
    }

    public void AddEndDrag(GameObject go, LuaFunction luafunc)
    {
        if (go == null || luafunc == null) return;
        m_enddragFunctions.Add(go, luafunc);
        UIEventListener.Get(go).onEndDrag = delegate(GameObject o) { luafunc.Call(go); };
    }

    public void RemoveEndDrag(GameObject go)
    {
        if (go == null) return;
        LuaFunction luafunc = null;
        if (m_enddragFunctions.TryGetValue(go, out luafunc))
        {
            m_enddragFunctions.Remove(go);
            luafunc.Dispose();
            luafunc = null;
        }
    }

    public void ClearEndDrag()
    {
        foreach (var de in m_enddragFunctions)
        {
            if (de.Value != null)
            {
                de.Value.Dispose();
            }
        }
        m_enddragFunctions.Clear();
    }

    /// <summary>
    /// 添加按压事件
    /// </summary>
    public void AddPress(GameObject go, LuaFunction luafunc)
    {
        if (go == null || luafunc == null) return;
        m_pressFunctions.Add(go, luafunc);
        UIEventListener.Get(go).onPress = delegate(GameObject o, bool state) { luafunc.Call(go, state); };
    }

    /// <summary>
    /// 删除按压事件
    /// </summary>
    /// <param name="go"></param>
    public void RemovePress(GameObject go)
    {
        if (go == null) return;
        LuaFunction luafunc = null;
        if (m_pressFunctions.TryGetValue(go, out luafunc))
        {
            m_pressFunctions.Remove(go);
            luafunc.Dispose();
            luafunc = null;
        }
    }

    /// <summary>
    /// 清除按压事件
    /// </summary>
    public void ClearPress()
    {
        foreach (var de in m_pressFunctions)
        {
            if (de.Value != null)
            {
                de.Value.Dispose();
            }
        }
        m_pressFunctions.Clear();
    }

    public void AddValueChange(GameObject go, System.Type type, LuaFunction luafunc)
    {
        if (go == null || luafunc == null) return;

        if (type == typeof(Toggle))
        {
            Toggle tog = go.GetComponent<Toggle>();
            if (null != tog) tog.onValueChanged.AddListener(v => { if (null != luafunc) luafunc.Call(go, v); });
        }
        else if (type == typeof(Slider))
        {
            Slider slider = go.GetComponent<Slider>();
            if (null != slider) slider.onValueChanged.AddListener(v => { if (null != luafunc) luafunc.Call(go, v); });
        }
        else if (type == typeof(InputField))
        {
            InputField inputField = go.GetComponent<InputField>();
            if (null != inputField) inputField.onValueChange.AddListener(v => { if (null != luafunc) luafunc.Call(go, v); });
        }
    }
}
