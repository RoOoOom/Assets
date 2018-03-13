using UnityEngine;
using System.Collections;
using LuaInterface;
using UnityEngine.UI;

public class UIDepth : MonoBehaviour
{
    public int order = 0;
    public bool isUI = true;
    void Update()
    {
        if (isUI)
        {
            Canvas canvas = GetComponent<Canvas>();
            if (canvas == null)
            {
                canvas = gameObject.AddComponent<Canvas>();
            }
            canvas.overrideSorting = true;
            canvas.sortingOrder = order;
        }
        else
        {
            Renderer[] renders = GetComponentsInChildren<Renderer>();

            foreach (Renderer render in renders)
            {
                render.sortingOrder = order;
            }
        }
    }
}