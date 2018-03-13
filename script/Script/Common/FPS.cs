/********************************************************************
	created:	2014/11/04
	author :	张呈鹏
    company:    深圳自游网络有限公司
	purpose:	FPS
*********************************************************************/
using System;
using System.IO;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
public class FPS : MonoBehaviour
{
    public float x_ = 50.0F;
    public float y_ = 270.0F;
    float time_elapsed_ = 0.0F;
    float frame_number_ = 0.0F;
    StringBuilder sb = new StringBuilder();
    //
    void Awake()
    {
        //Profiler.enabled = true;
        x_ = 0.0f;// Screen.width / 2;
        y_ = Screen.height / 1.5f;
    }

    // Use this for initialization
    void Start()
    {
    }
    void OnGUI()
    {
        GUIStyle bb = new GUIStyle();
        bb.normal.background = null;                //背景填充
        bb.normal.textColor = new Color(1, 0, 0);   //字体颜色
        bb.fontSize = 20;                           //字体大小
        GUI.Label(new Rect(x_, y_, 2000, 200), sb.ToString(), bb);
    }

    // Update is called once per frame
    void Update()
	{
        ++frame_number_;
        time_elapsed_ += Time.unscaledDeltaTime;
        if (time_elapsed_ > 1.0F)
        {
            float fps = frame_number_ / time_elapsed_;
            sb.Remove(0, sb.Length);
            sb.Append("FPS: ");
            sb.Append((int)fps);
            //
            //sb.Append("\n");
            //sb.Append("GameObjects: ");
            //sb.Append(GameObject.FindObjectsOfType<GameObject>().Length);
            frame_number_ = 0.0F;
            time_elapsed_ = 0.0F;
        }        
	}
}