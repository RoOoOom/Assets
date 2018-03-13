using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class HudMeshBase : HudLayoutElement {

	private Mesh _mesh = null;
	public Mesh mesh 
    {
		get 
        {
            if (_mesh == null)
            {
                _mesh = new Mesh();
                _mesh.name = name;
                meshFilter.mesh = _mesh;
            }
			return _mesh;
		}
	}

	private MeshFilter _meshFilter = null;
	public MeshFilter meshFilter 
    {
		get 
        {
            if (_meshFilter == null) _meshFilter = GetComponent<MeshFilter>();
            if (_meshFilter == null) _meshFilter = gameObject.AddComponent<MeshFilter>();
			return _meshFilter;
		}
	}

    [System.NonSerialized]
    private HudLayout _hudLayout;
    public HudLayout HudLayout
    {
        get
        {
            // 因为逻辑是专用的，挖个坑也不所谓了
            if (_hudLayout == null)
                _hudLayout = GetComponentInParent<HudLayout>();
            return _hudLayout;
        }
    }

	protected List<Vector3> _verts = new List<Vector3>();
	protected List<Vector2> _uvs = new List<Vector2>();
	protected List<int> _indices = new List<int>();
	protected List<Color> _colors = new List<Color>();

	[SerializeField]
	private bool _dirty = true;
	public bool dirty {
		get { return _dirty; }
		set { _dirty = value; }
	}

	[SerializeField]
	private Color _color = Color.white;
	public Color color {
		get { return _color; }
		set {
			_color = value;

			dirty = true;
		}
	}

    [SerializeField]
    private bool _showshadow = true;
    public bool showshadow
    {
        get { return _showshadow; }
        set {
            _showshadow = value;

            dirty = true;
        }
    }

    [SerializeField]
    private Color _shadowcolor = new Color(0.3f, 0.3f, 0.3f, 1.0f);
    public Color shadowcolor
    {
        get { return _shadowcolor; }
        set {
            _shadowcolor = value;

            dirty = true;
        }
    }

    [SerializeField]
    private float _shadowdistance = 0.8f;
    public float shadowdistance
    {
        get { return _shadowdistance; }
        set {
            _shadowdistance = value;

            dirty = true;
        }
    }

    void Awake()
	{
		dirty = true;
	}

    void OnEnable()
    {
        dirty = true;
    }

    void OnDisable()
    {
        if (HudLayout)
        {
            HudLayout.dirty = true;
        }
    }

	void LateUpdate()
	{
		if (dirty) {
			UpdateDirty ();
			dirty = false;
		}
	}

	public override void UpdateSelfLayoutDirty()
	{
		if (dirty) {
			UpdateDirty ();
			dirty = false;
		}
	}


	public virtual void UpdateDirty()
	{
		
	}
}
