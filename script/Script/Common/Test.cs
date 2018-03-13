using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

public class Test : MonoBehaviour
{
    public Texture tex = null;
    Graphic gra = null;

    void Start()
    {
        if (gra == null)
        {
            gra = gameObject.GetComponent<Graphic>();
        }

        print(gra.material.name);
    }

    void Update()
    {
        tex = gra.material.GetTexture("_TestTex");
    }
}
