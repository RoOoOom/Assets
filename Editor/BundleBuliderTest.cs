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
}
