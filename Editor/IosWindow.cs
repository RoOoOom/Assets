using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using System;

public enum IOSType {
    it_none,
    i4,
    tbt,
    g1,
    ts,
    ky,
    xy,
    iphoneios,
    sq
}

public class IosWindow : EditorWindow
{
	
	[MenuItem("IOS/血饮天下打包")]
	static void AddSCWTLindow()
	{
		//创建窗口
		Rect wr = new Rect(0, 0, 500, 500);
		IosWindow window = (IosWindow)EditorWindow.GetWindowWithRect(typeof(IosWindow), wr, true, "IOS渠道打包编辑器");
		//window.SetAppId("1122", "ug8xxoe8izaqedmwaco2l19rnlciqynu");
		window.SetAppId("{82A28AA7-4616F02E}", "UK76YYRG0D580LWDX4YLVTU32NI5NG6A");
		window.Show();
		//UpdatinAssets
	}
	
	//[MenuItem("IOS/血饮天下打包")]
	//static void AddXYTXWindow()
	//{
	//    //创建窗口
	//    Rect wr = new Rect(0, 0, 500, 500);
	//    IosWindow window = (IosWindow)EditorWindow.GetWindowWithRect(typeof(IosWindow), wr, true, "IOS渠道打包编辑器");
	//    window.SetAppId("1122", "ug8xxoe8izaqedmwaco2l19rnlciqynu");
	//    window.Show();
	//}
	
	//输入文字的内容
	private List<string> text = new List<string>();
	
	private string pathEx = "";
	private IOSType mType = IOSType.i4;
	
	private string mAppId;
	private string mAppKey;
	
	private string mVersionString = "1.3";
	private void SetAppId(string id,string appKey)
	{
		mAppId = id;
		mAppKey = appKey;
	}
	
	
	public void Awake()
	{
		foreach (IOSType hs1 in Enum.GetValues(typeof(IOSType)))
		{
			string keyName = "";
			if (hs1 == IOSType.g1||hs1 == IOSType.tbt)
			{
				keyName = "(特殊解压SDK)";
				if (hs1 == IOSType.g1)
				{
					keyName += "(手动设置identifier字段)";
				}
				else if (hs1 == IOSType.tbt)
				{
					keyName += "";
				}
			}
			else if (hs1 == IOSType.ts)
			{
				keyName = "(info里添加view controller 不然bar会出现)";
			}
			else if (hs1 == IOSType.xy)
			{
				//keyName = "(特殊解压xcodeSDK,替换里面的文件)";
			}
			else if (hs1 == IOSType.ky)
			{
				keyName = "com.ky.xSDK.alipay --identifier";
			}
			else if (hs1 == IOSType.i4)
			{
				keyName = "AlixPay+As747-AsQQPay+bunldid";
			}
			else if (hs1 == IOSType.iphoneios)
			{
				keyName = "正版渠道";
			}
			else if (hs1 == IOSType.sq)
			{
				keyName = "(sq test)";
			}
			text.Add(keyName);
		}
	}
	
	void setToggle(IOSType id)
	{
		for (int i = 0; i < text.Count;i++)
		{
			if (i == (int)id)
			{
				if (id == IOSType.it_none)
				{
					this.ShowNotification(new GUIContent("打包平台选择无效"));
				}
				else
				{
					text[i] = ":已选择";
					//pathEx = IosApk.IosSet(id,mAppId,mAppKey,mVersionString);
					mType = id;
				}
			}
			else
			{
				text[i] = "";
			}
		}
	}
	//绘制窗口时调用
	void OnGUI()
	{
		
		EditorGUILayout.LabelField("选择要打包的渠道1");
		
		mVersionString = EditorGUILayout.TextField("输入版本号:", mVersionString);
		
		
		foreach (IOSType hs1 in Enum.GetValues(typeof(IOSType)))
		{
			if (GUILayout.Button(hs1.ToString() + text[(int)hs1], GUILayout.Width(450)))
			{
				setToggle(hs1);
			}
		}
		
		GUILayout.BeginArea(new Rect(200, 400, 400, 400));
		
		if (GUILayout.Button("开始打包", GUILayout.Width(80)))
		{
			if (pathEx == "")
			{
				this.ShowNotification(new GUIContent("请先选择一个打包平台"));
			}
			else
			{
				//IosApk.PlayAPK(pathEx, mType);
				//关闭窗口
				this.Close();
			}
			
		}
		GUILayout.EndArea();
	}
	
	//更新
	void Update()
	{
		
	}
	
	void OnFocus()
	{
		Debug.Log("当窗口获得焦点时调用一次");
	}
	
	void OnLostFocus()
	{
		Debug.Log("当窗口丢失焦点时调用一次");
	}
	
	void OnHierarchyChange()
	{
		Debug.Log("当Hierarchy视图中的任何对象发生改变时调用一次");
	}
	
	void OnProjectChange()
	{
		Debug.Log("当Project视图中的资源发生改变时调用一次");
	}
	
	void OnInspectorUpdate()
	{
		//Debug.Log("窗口面板的更新");
		//这里开启窗口的重绘，不然窗口信息不会刷新
		this.Repaint();
	}
	
	void OnSelectionChange()
	{
		//当窗口出去开启状态，并且在Hierarchy视图中选择某游戏对象时调用
		foreach (Transform t in Selection.transforms)
		{
			//有可能是多选，这里开启一个循环打印选中游戏对象的名称
			Debug.Log("OnSelectionChange" + t.name);
		}
	}
	
	void OnDestroy()
	{
		Debug.Log("当窗口关闭时调用");
	}
}
