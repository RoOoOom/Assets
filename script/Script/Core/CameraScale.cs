using UnityEngine;
using System.Collections;

public class CameraScale : MonoBehaviour {

	 void Start()
    { 
        int ManualWidth = 1280;
        int ManualHeight = 720;
        int manualHeight;
        if (System.Convert.ToSingle(Screen.height) / Screen.width > System.Convert.ToSingle(ManualHeight) / ManualWidth)
            manualHeight = Mathf.RoundToInt(System.Convert.ToSingle(ManualWidth) / Screen.width * Screen.height);
        else
            manualHeight = ManualHeight;
        Camera camera = GetComponent<Camera>();
        float scale = System.Convert.ToSingle(manualHeight / (float)ManualHeight);
        camera.fieldOfView *= scale;
    }
}
