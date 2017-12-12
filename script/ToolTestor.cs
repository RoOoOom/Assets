using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToolTestor : MonoBehaviour {

    private void Awake()
    {
        Debug.Log(Application.dataPath);
        Debug.LogWarning(Application.persistentDataPath);
        Debug.LogError(Application.streamingAssetsPath);
    }

    private void OnEnable()
    {
        
    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
