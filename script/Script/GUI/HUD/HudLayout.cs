using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class HudLayout : HudLayoutElement {

	public enum LayoutType
	{
		Horizontal = 0,
		Vertical = 1,
	}

	[SerializeField]
	private bool _dirty = true;
	public bool dirty {
		get { return _dirty; }
		set { _dirty = value; }
	}

	[SerializeField]
	private float _space = 0;
	public float space {
		get { return _space; }
		set { _space = value;

			dirty = true;
		}
	}

	[SerializeField]
	private LayoutType _layoutType = 0;
	public LayoutType layoutType {
		get { return _layoutType; }
		set { _layoutType = value;

			dirty = true;
		}
	}

	void OnEnable()
	{
		dirty = true;
	}

	void LateUpdate()
	{
		if (dirty) {
			UpdateLayoutDirty ();
			dirty = false;
		}
	}

	public override void UpdateSelfLayoutDirty()
	{
		switch(layoutType)
		{
		case LayoutType.Horizontal:
			DoHorizontalLayout ();
			break;
		case LayoutType.Vertical:
			DoVirticalLayout ();
			break;
		}
	}

	void DoHorizontalLayout()
	{
		float width = 0f;
		float height = 0f;

        int centerIndex = -1;

		for (int i = 0; i < transform.childCount; ++i) 
        {
            HudLayoutElement hudItem = transform.GetChild(i).GetComponent<HudLayoutElement>();

			if (hudItem && hudItem.gameObject.activeSelf && !hudItem.single) 
            {
				width += hudItem.bounds.size.x + space;
				height = Mathf.Max (height, hudItem.bounds.size.y);
                if (hudItem.isCenter)
                {
                    centerIndex = i;
                }
			}
		}

		width -= space;
		_bounds.size = new Vector3 (width, height, 0);

        if (centerIndex >= 0)
        {
            var hudItem = transform.GetChild(centerIndex).GetComponent<HudLayoutElement>();
            hudItem.transform.localPosition = new Vector3(0, 0, 0);

            float centerX = hudItem.bounds.extents.x;
            float offX = centerX;
            for (int i = centerIndex - 1; i >= 0; i--)
            {
                hudItem = transform.GetChild(i).GetComponent<HudLayoutElement>();
                if (hudItem && hudItem.gameObject.activeSelf && !hudItem.single)
                {
                    offX += hudItem.bounds.extents.x;
                    hudItem.transform.localPosition = new Vector3(-offX, 0, 0);
                    offX += hudItem.bounds.extents.x;
                }
            }

            offX = centerX;
            for (int i = centerIndex + 1; i < transform.childCount; i++)
            {
                hudItem = transform.GetChild(i).GetComponent<HudLayoutElement>();
                if (hudItem && hudItem.gameObject.activeSelf && !hudItem.single)
                {
                    offX += hudItem.bounds.extents.x;
                    hudItem.transform.localPosition = new Vector3(offX, 0, 0);
                    offX += hudItem.bounds.extents.x;
                }
            }
        }
        else
        {
            float offX = -width / 2f;
            for (int i = 0; i < transform.childCount; ++i)
            {
                var hudItem = transform.GetChild(i).GetComponent<HudLayoutElement>();
                if (hudItem && hudItem.gameObject.activeSelf && !hudItem.single)
                {
                    hudItem.transform.localPosition = new Vector3(offX + hudItem.bounds.extents.x, 0, 0);
                    offX += hudItem.bounds.size.x + space;
                }
            }
        }
	}

	void DoVirticalLayout()
	{
		float width = 0f;
		float height = 0f;

		for (int i = 0; i < transform.childCount; ++i) {
			var hudItem = transform.GetChild (i).GetComponent<HudLayoutElement> ();

            if (hudItem && hudItem.gameObject.activeSelf && !hudItem.single)
            {
				width = Mathf.Max (width, hudItem.bounds.size.x);
				height += hudItem.bounds.size.y + space;
			}
		}

		height -= space;
		_bounds.size = new Vector3 (width, height, 0);

		float offY = 0f;
		for (int i = 0; i < transform.childCount; ++i) {
			var hudItem = transform.GetChild (i).GetComponent<HudLayoutElement> ();
            if (hudItem && hudItem.gameObject.activeSelf && !hudItem.single)
            {
				hudItem.transform.localPosition = new Vector3 (0, hudItem.bounds.extents.y+offY, 0);
				offY += hudItem.bounds.size.y + space;
			}
		}
	}
}
