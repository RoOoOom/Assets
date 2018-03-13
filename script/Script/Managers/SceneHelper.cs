using System;
using LuaInterface;
using UnityEngine;
public class SceneHelper : SimleManagerTemplate<SceneHelper>
{
    public SMSceneManager manager { get { return m_manager; } }
    private SMSceneManager m_manager = null;

    public string curLevel = "";

    public SceneHelper()
    {
        m_manager = new SMSceneManager(SMSceneConfigurationLoader.LoadActiveConfiguration("SceneConfig"));
        m_manager.LevelProgress = new SMLevelProgress(m_manager.ConfigurationName);
        //m_manager.SetTransitionPrefab(TransitionType.SMBlindsTransition);
    }

    public void LoadLevel(string level)
    {
        curLevel = level;
        manager.LoadLevel(level);
    }

    public void LoadFadeTransition(Action action)
    {
        manager.LoadTransition(TransitionType.SMFadeTransition, action);
    }

    public void SetTransitionType(TransitionType type)
    {
        m_manager.SetTransitionPrefab(type);
    }

    public void UnLoadScene(string name)
    {
        UnityEngine.SceneManagement.SceneManager.UnloadScene(name);
    }
}
