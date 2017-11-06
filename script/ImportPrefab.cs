using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEditor;

public class ImportPrefab : MonoBehaviour {
    string path = "/CubePre.prefab";
	// Use this for initialization
	void Start () {
        path = Application.dataPath + path;
        AssetDatabase.ImportAsset(path);
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
