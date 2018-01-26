using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

#if false
public class Testor : IEnumerator {
    public bool done = true;
    public object Current {
        get {
            return null;
        }
    }
    public bool MoveNext()
    {
        return done;
    }

    public void Reset()
    {

    }

    int i = 0;
}

#endif

#if true

public class Testor : MonoBehaviour {

    private void Start()
    {
        StartCoroutine(LoadFileScene("Control-Panel"));
    }

    IEnumerator LoadFileScene(string sceneName)
    {
        WWW download = WWW.LoadFromCacheOrDownload("file://" + Application.streamingAssetsPath + '/' + sceneName + ".ab" , 1);
        yield return download;
        var bundle = download.assetBundle;
        SceneManager.LoadScene(sceneName);
        bundle.Unload(true);
    }

    private void Update()
    {
         
    }
}

#endif
