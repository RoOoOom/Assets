using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToolTestor : MonoBehaviour {

    Testor _test = new Testor();
    public GameObject _testPrefab;
    private void Awake()
    {
        /*
        Debug.Log(Application.dataPath);
        Debug.LogWarning(Application.persistentDataPath);
        Debug.LogError(Application.streamingAssetsPath);
        */
    }

    private void OnEnable()
    {
        //  StartCoroutine(LoadTestAssetBundle("testcanvas", "TestPrefab"));
        StartCoroutine(TestCircle());
    }

    // Use this for initialization
    void Start () {
        /*
        AssetBundle bundle = AssetBundle.LoadFromFile(Application.streamingAssetsPath + "/testcanvas");
        Instantiate(bundle.LoadAsset("TestPrefab"));
        */
        
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(KeyCode.U))
        {
            CreateInstance();
        }
	}

    IEnumerator LoadTestAssetBundle( string  bundleName , string resName)
    {
        WWW www = new WWW(Application.streamingAssetsPath + "/" + bundleName);//不能存在中文名
        yield return www;

        AssetBundle bundle = www.assetBundle;
        Instantiate(bundle.LoadAsset("TestPrefab"));
    }

    IEnumerator TestCircle()
    {
        Debug.Log("开始执行");
        yield return StartCoroutine(_test);
        Debug.Log("执行到这里");
    }

    void CreateInstance()
    {
        if (_testPrefab != null)
        {
            GameObject temp = Instantiate(_testPrefab);
        }
    }
}
