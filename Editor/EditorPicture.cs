using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

enum PicType { AlphaPVRTC, NotAlphaPVRTC, NotPVRTC, NotPVRTCNotAlpha };

public class EditorPicture : EditorWindow
{
    const int CompresssQuality = 100;
    const float halveRate = 0.5f;

    bool m_isToggle = false;
    PicType m_picType = PicType.AlphaPVRTC;
    TextureImporterFormat m_tif = TextureImporterFormat.RGBA16;
    int m_maxSize = 1024;

    string[] PictureTypeGroup = { "透明方正", "不透明方正", "不方正", "不透明不方正" };
    string[] MaxSizeGroup = { "32x32", "64x64", "128x128", "256x256", "512x512", "1024x1024", "2048x2048" };
    int[] MaxSizes = { 32, 64, 128, 256, 512, 1024, 2047 };

    [MenuItem("MyEditor/EditorPicture")]
    static void InitWindow()
    {
        EditorWindow.GetWindow(typeof(EditorPicture));
    }

    void OnGUI()
    {
        int selectCount = 0;
        Object[] selectFolds;

        //        Object[] selectPictures = Selection.objects;


        EditorGUILayout.BeginVertical();

        m_isToggle = EditorGUILayout.Toggle("是否对文件夹进行批量处理", m_isToggle);

        if (m_isToggle)
        {
            selectFolds = Selection.GetFiltered(typeof(Object), SelectionMode.Assets);
        }
        else
        {
            selectFolds = Selection.objects;
        }

        selectCount = selectFolds.Length;

        EditorGUILayout.LabelField("已选中对象数 : " + selectCount);

        //m_picType = (PicType)EditorGUILayout.Popup("图片类型：",(int)m_picType,PictureTypeGroup);

        /*
        switch(m_picType)
        {
        case PicType.AlphaPVRTC:
            m_tif = TextureImporterFormat.PVRTC_RGBA4;
            break;
        case PicType.NotAlphaPVRTC:
            m_tif = TextureImporterFormat.PVRTC_RGB4;
            break;
        case PicType.NotPVRTC:
            m_tif = TextureImporterFormat.RGBA16;
            break;
        case PicType.NotPVRTCNotAlpha:
            m_tif = TextureImporterFormat.RGB16;
            break;
        default:
            break;
        }
        */

        //m_maxSize = EditorGUILayout.IntPopup ("最大分辨率：", m_maxSize, MaxSizeGroup, MaxSizes);

        if (selectCount == 0)
        {
            if (m_isToggle)
            {
                EditorGUILayout.HelpBox("请选择正确的文件夹", MessageType.Warning);
            }
            else
            {
                EditorGUILayout.HelpBox("请选择正确的图片", MessageType.Warning);
            }
        }

        if (GUILayout.Button("对单个图片进行测试"))
        {
            for (int i = 0; i < selectCount; ++i)
            {
                string picPath = AssetDatabase.GetAssetPath(selectFolds[i].GetInstanceID());
                Texture2D tex2D = AssetDatabase.LoadAssetAtPath<Texture2D>(picPath);
                TextureImporter texImp = AssetImporter.GetAtPath(picPath) as TextureImporter;

                TextureImporterPlatformSettings tipSetting = texImp.GetPlatformTextureSettings("iPhone");
                //TextureImporterPlatformSettings tipSetting = texImp.GetDefaultPlatformTextureSettings ();

                tipSetting.overridden = true;

                if ((tex2D.width == tex2D.height) && is2Flag(tex2D.width) && is2Flag(tex2D.height))
                {
                    m_tif = tipSetting.format;
                    tipSetting.compressionQuality = CompresssQuality;
                }
                else
                {
                    if (tex2D.format == TextureFormat.RGB24)
                    {//无alpha通道
                        Debug.Log("not alpha");
                        m_tif = TextureImporterFormat.RGB16;
                    }
                    else
                    {//有alpha通道
                        Debug.Log("include alpha");
                        m_tif = TextureImporterFormat.RGBA16;
                    }
                }
                tipSetting.format = m_tif;

                texImp.SetPlatformTextureSettings(tipSetting);

            }
        }

        if (GUILayout.Button("统一设置"))
        {
            for (int i = 0; i < selectCount; ++i)
            {
                string DirPath = AssetDatabase.GetAssetPath(selectFolds[i]).Replace("\\", "/");

                if (!Directory.Exists(DirPath))
                {
                    Debug.Log("文件夹不存在 ---> " + DirPath);
                    continue;
                }

                DealAllPicture(DirPath);
            }
        }

        EditorGUILayout.EndVertical();

    }

    void DealAllPicture(string dirPath)
    {
        string[] files = Directory.GetFiles(dirPath, "*.*", SearchOption.AllDirectories);

        for (int i = 0; i < files.Length; ++i)
        {
            string filePath = files[i].Replace("\\", "/");

            if (filePath.EndsWith(".png") || filePath.EndsWith(".jpg"))
            {
                EditorUtility.DisplayProgressBar("处理中>>>", filePath, (float)i / (float)files.Length);

                TextureImporter ti = AssetImporter.GetAtPath(filePath) as TextureImporter;

                Texture2D tex2D = AssetDatabase.LoadAssetAtPath<Texture2D>(filePath);

                m_maxSize = GetMaxSize(Mathf.Max(tex2D.width, tex2D.height));

#if UNITY_IOS

                //TextureImporterPlatformSettings tipSettins = ti.GetDefaultPlatformTextureSettings();
                TextureImporterPlatformSettings tipSettins = ti.GetPlatformTextureSettings("iPhone");

                tipSettins.overridden = true ;

                if (tex2D.width == tex2D.height && is2Flag (tex2D.width) && is2Flag (tex2D.height)) {
                    if(tex2D.format == TextureFormat.RGB24)
                    {
                        m_tif = TextureImporterFormat.PVRTC_RGB4;
                    }
                    else if(tex2D.format == TextureFormat.RGBA32)
                    {
                        m_tif = TextureImporterFormat.PVRTC_RGBA4;
                    }
                    else
                    {
                        m_tif = tipSettins.format;    
                    }

                    tipSettins.compressionQuality = CompresssQuality;
                } else {
                    if (tex2D.format == TextureFormat.RGB24) {//无alpha通道
                        m_tif = TextureImporterFormat.RGB16;
                    }
                    else if(tex2D.format == TextureFormat.RGBA32){//有alpha通道
                        m_tif = TextureImporterFormat.RGBA16;
                    }
                    else
                    {
                        m_tif = tipSettins.format;    
                    }
                }

                tipSettins.format = m_tif ;
                tipSettins.maxTextureSize = m_maxSize;
                //tipSettins.textureCompression = TextureImporterCompression.Compressed;

                //ti.SetPlatformTextureSettings("iPhone" ,m_maxSize, m_tif);

                ti.SetPlatformTextureSettings(tipSettins);

#endif

                //AssetDatabase.ImportAsset(path);
                AssetDatabase.SaveAssets();
                //DoAssetReimport (filePath, ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport);
            }
            AssetDatabase.Refresh();

        }
        EditorUtility.ClearProgressBar();
    }


    int GetMaxSize(int size)
    {

        int result = 1024;

        if (size >= 1024)
        {
            result = 2048;
        }
        else if (size >= 512)
        {
            result = 1024;
        }
        else if (size >= 256)
        {
            result = 512;
        }
        else if (size >= 128)
        {
            result = 256;
        }
        else if (size >= 64)
        {
            result = 128;
        }
        else if (size >= 32)
        {
            result = 64;
        }

        return result;
    }

    public void DoAssetReimport(string path, ImportAssetOptions options)
    {
        try
        {
            AssetDatabase.StartAssetEditing();
            AssetDatabase.ImportAsset(path, options);
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
        }
    }

    public bool is2Flag(int num)
    {
        if (num < 1)
            return false;

        return (num & num - 1) == 0;
    }
}