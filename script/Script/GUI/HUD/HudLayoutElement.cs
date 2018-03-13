using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HudLayoutElement : MonoBehaviour {

	[SerializeField]
	protected Bounds _bounds = new Bounds();
	public Bounds bounds {
		get {
			return _bounds;
		}
	}

    [SerializeField]
    private bool _single = false;
    public bool single
    {
        get
        {
            return _single;
        }
    }

    [SerializeField]
    private bool _isCenter = false;
    public bool isCenter
    {
        get
        {
            return _isCenter;
        }
    }

	public void UpdateLayoutDirty()
	{
		UpdateChildrenLayoutDirty ();
		UpdateSelfLayoutDirty ();
	}

	public void UpdateChildrenLayoutDirty()
	{
		for (int i = 0; i < transform.childCount; ++i) {
			var hudItem = transform.GetChild (i).GetComponent<HudLayoutElement> ();
			if (hudItem && hudItem.gameObject.activeSelf) {
				hudItem.UpdateLayoutDirty ();
			}
		}
	}

	public virtual void UpdateSelfLayoutDirty()
	{
	}
}
