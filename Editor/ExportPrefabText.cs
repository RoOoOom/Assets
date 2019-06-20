using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine.UI;

[ExecuteInEditMode]
public class ExportPrefabText : Editor {
    [MenuItem("Create/导出Prefab的Text内容")]
    public static void ExportText()
    {
        bool hr = EditorUtility.DisplayDialog("注意", "此功能需要选定prefab文件或是饱含有这类文件的文件夹,导出的文件名默认为PanelTextTbl", "确定");
        if (!hr) return;

        //string tblPath = Application.dataPath + "/Editor/PanelTextTbl.lua";
        //string tblPath = Application.dataPath + "/Lua/UI/PanelTextTbl.lua";
        string defaultpath = Application.dataPath + "/Lua/Common/";
        string tblPath = EditorUtility.SaveFolderPanel("选择导出文件的保存目录", defaultpath, "");

        if (string.IsNullOrEmpty(tblPath)) return;

        tblPath = tblPath + "/PanelTextTbl.lua";

        var selectFile = Selection.activeObject;
        string selectPath = AssetDatabase.GetAssetPath(selectFile);

        if (string.IsNullOrEmpty(selectPath))
        {
            EditorUtility.DisplayDialog("警告", "请选择prefab文件或文件夹","ok");
            return;
        }

        if (selectPath.EndsWith(".prefab"))
        {
            Debug.Log("已选择prefab文件：" + selectPath);

            selectPath = selectPath.Substring(selectPath.IndexOf("Assets"));
            GameObject tmpPrefab = AssetDatabase.LoadAssetAtPath(selectPath, typeof(GameObject)) as GameObject;
            Text[] txtComponents = tmpPrefab.GetComponentsInChildren<Text>(true);

            StreamWriter sr = new StreamWriter(tblPath);
            sr.WriteLine("PanelTextTbl = {");
            int tlen = txtComponents.Length;

            sr.WriteLine("\t" + tmpPrefab.name + " = {");
            for (int n = 0;n<txtComponents.Length ;n++)
            {
                string txtPath = GetGameObjectPath(txtComponents[n].transform, tmpPrefab.transform);
                EditorUtility.DisplayProgressBar("正在处理prefab", txtPath, (float)n / (float)tlen);
                string ss =Regex.Escape(txtComponents[n].text);
                //string str = txtComponents[n].text.Replace("\n", "#n");
                string str = ss.Replace("\n", "#n");
                str = str.Replace("#n", "\\n");
                sr.WriteLine(string.Format("\t\t[\"{0}\"] = \"{1}\",", txtPath, str));
            }

            EditorUtility.ClearProgressBar();
            sr.WriteLine("}");

            sr.WriteLine("function InitTextComponent(panel,panelName)");
            sr.WriteLine("	if panel == nil or panelName == nil then return end");
            sr.WriteLine("	local info = PanelTextTbl[panelName]");
            sr.WriteLine("	if not info then return end");
            sr.WriteLine("	for k,v in pairs(info) do");
            sr.WriteLine("		panel.transform:Find(k):GetComponent(\"Text\").text = v");
            sr.WriteLine("	end");
            sr.WriteLine("end");
            sr.Flush();
            sr.Dispose();
            sr.Close();
        }
        else if (Directory.Exists(selectPath))
        {
            Debug.Log("已选择文件夹：" + selectPath);

            string[] filesPath = Directory.GetFiles(selectPath, "*.prefab",SearchOption.AllDirectories);           
            
            {
                StreamWriter sr = new StreamWriter(tblPath);
                sr.WriteLine("PanelTextTbl = {");
                for (int i = 0; i < filesPath.Length; i++)
                {
                    filesPath[i] = filesPath[i].Substring(filesPath[i].IndexOf("Assets"));
                    GameObject tmpPrefab = AssetDatabase.LoadAssetAtPath(filesPath[i], typeof(GameObject)) as GameObject;
                    Text[] txtComponents = tmpPrefab.GetComponentsInChildren<Text>(true);
                    int tlen = txtComponents.Length;
                    if (tlen == 0)
                    {
                        continue;
                    }

                    sr.WriteLine("\t" + tmpPrefab.name + " = {");
                    for (int n = 0; n < tlen; n++)
                    {
                        if (string.IsNullOrEmpty(txtComponents[n].text)) continue;

                        string txtPath = GetGameObjectPath(txtComponents[n].transform,tmpPrefab.transform);
                        EditorUtility.DisplayProgressBar("正在处理prefab", txtPath, (float)n/(float)tlen);
                        string str = txtComponents[n].text.Replace("\\",@"\\");
                        str = str.Replace("\n", "#n");
                        str = str.Replace("#n", "\\n");
                        str = str.Replace("\r", "#r");
                        str = str.Replace("#r", "\\r");
                        sr.WriteLine(string.Format("\t\t[\"{0}\"] = \"{1}\",",txtPath,str));                        
                    }
                    sr.WriteLine("\t},");
                    sr.Flush();
                }                
                sr.WriteLine("}");

                sr.WriteLine("function InitTextComponent(panel,panelName)");
                sr.WriteLine("	if panel == nil or panelName == nil then return end");
                sr.WriteLine("	local info = PanelTextTbl[panelName]");
                sr.WriteLine("	if not info then return end");
                sr.WriteLine("	for k,v in pairs(info) do");
                sr.WriteLine("		panel.transform:Find(k):GetComponent(\"Text\").text = v");
                sr.WriteLine("	end");
                sr.WriteLine("end");
                sr.Flush();
                sr.Dispose();
                sr.Close();

                EditorUtility.ClearProgressBar();
                AssetDatabase.Refresh();
            }
        }
        else
        {
            EditorUtility.DisplayDialog("警告", "请选择prefab文件或文件夹", "ok");
            return;
        }
    }

    public static string GetGameObjectPath(Transform obj,Transform root)
    {
        Transform prnt = obj.parent;
        StringBuilder path = new StringBuilder();
        path.Append(obj.name);
        while (prnt!=root)
        {
            path.Insert(0, '/');
            path.Insert(0, prnt.name);
            prnt = prnt.parent;
        }
        return path.ToString();
    }
}
