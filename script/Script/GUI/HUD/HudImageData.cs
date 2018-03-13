using System;
using UnityEngine;

public class HudImageData : ScriptableObject
{
    public string name = "";
    public int width;
    public int height;
    public HudImageRect[] data;
}

[Serializable]
public class HudImageRect
{
    public string id = "";
    public int x;
    public int y;
    public int z;
    public int w;
}
