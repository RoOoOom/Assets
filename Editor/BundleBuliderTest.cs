using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class BundleBuliderTest{

    [MenuItem("MyEditor/BulidAssetsBundle")]
    public static void TestBulidAsssetBundle()
    {
        string path = Application.dataPath + "/StreamingAssets";
        BuildAssetBundleOptions option = BuildAssetBundleOptions.DeterministicAssetBundle | BuildAssetBundleOptions.ChunkBasedCompression;
        AssetBundleManifest cacheMainifest = BuildPipeline.BuildAssetBundles(path, option, EditorUserBuildSettings.activeBuildTarget);

        AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
    }

    [MenuItem("MyEditor/BuildSceneAsset")]
    public static void BuildSceneAsset()
    {
        Object[] objs = Selection.objects;
        if (objs == null || objs.Length <= 0) return;

        foreach (Object obj in objs)
        {
            string savePath = Application.streamingAssetsPath + '/' + obj.name + ".ab";
            string objPath = AssetDatabase.GetAssetPath(obj);
            objPath = objPath.Replace(Application.dataPath, "Assets");
            Debug.Log(objPath);
            string[] levels = new string[] {objPath};
            BuildPipeline.BuildPlayer( levels,savePath , BuildTarget.StandaloneWindows64 , BuildOptions.BuildAdditionalStreamedScenes );
        }
    }
}
