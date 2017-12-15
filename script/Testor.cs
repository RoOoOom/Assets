using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
