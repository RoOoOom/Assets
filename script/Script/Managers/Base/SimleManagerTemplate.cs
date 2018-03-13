using System;

public class SimleManagerTemplate<T> where T : SimleManagerTemplate<T>, new()
{
    private static T m_instance = null;
    public static T instance
    {
        get
        {
            if (null == m_instance)
            {
                m_instance = new T();
                m_instance.OnInit();
            }
            return m_instance;
        }
    }

    protected virtual void OnInit() { }
}