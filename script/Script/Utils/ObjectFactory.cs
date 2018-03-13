using UnityEngine;
using System.Collections;

public class ObjectFactory  {

    public static Rect createRect(float left, float top, float width, float height)
    {
        return new Rect(left, top, width, height);
    }

}
