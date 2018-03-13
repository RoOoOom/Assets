using UnityEngine;
using System.Collections.Generic;

public enum ImageExpendType
{
    Width,
    Height,
    WidthAndHeight,
    NativeSize,
}

[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class HudImageMesh : HudMeshBase
{
	[SerializeField]
	private HudImageData _imageData = null;
    [System.NonSerialized]
    private Dictionary<string, HudImageRect> _rectDic = null;

    Dictionary<string, HudImageRect> rectDic
    {
        get
        {
            if (_imageData == null)
            {
                return null;
            }

            if (_rectDic == null)
            {
                _rectDic = new Dictionary<string, HudImageRect>();
                for (int i = 0; i < _imageData.data.Length; i++ )
                {
                    _rectDic.Add(_imageData.data[i].id, _imageData.data[i]);
                }
            }

            return _rectDic;
        }
    }

    [SerializeField]
    private string _imageId = "";
    public string imageId
    {
        get { return _imageId; }
        set
        {
            if (_imageId == value)
                return;

            _imageId = value;
            dirty = true;
        }
    }

    [SerializeField]
	private int _height = 1;
	public int height
	{
		get { return _height; }
		set {
			_height = value;
			dirty = true;
		}
	}

    [SerializeField]
    private int _width = 1;
    public int width
    {
        get { return _width; }
        set
        {
            _width = value;
            dirty = true;
        }
    }

    [SerializeField]
    private ImageExpendType _imageExpendType = ImageExpendType.Height;
    public ImageExpendType imageExpendType
    {
        get
        {
            return _imageExpendType;
        }

        set
        {
            if (_imageExpendType != value)
            {
                _imageExpendType = value;
                dirty = true;
            }
        }
    }

	[SerializeField, Range(0, 1)]
	private float _percent = 1f;
	public float percent
	{
		get { return Mathf.Clamp01(_percent); }
		set {
			_percent = value;
			dirty = true;
		}
	}

    void Awake()
    {
        dirty = true;
    }

    public override void UpdateDirty()
	{
        if (rectDic != null && !string.IsNullOrEmpty(_imageId))
        {
			DoGenerateMesh ();
		}

        if (HudLayout)
        {
            HudLayout.dirty = true;
        }
	}

	void DoGenerateMesh()
	{
        HudImageRect rect;
        if (_rectDic.TryGetValue(_imageId, out rect))
        {
            _verts.Clear();
            _uvs.Clear();
            _indices.Clear();
            _colors.Clear();

            _bounds.center = Vector3.zero;

            float texWidth = _imageData.width;
            float texHeight = _imageData.height;

            switch(imageExpendType)
            {
                case ImageExpendType.NativeSize:
                    _bounds.size = new Vector3(rect.z, rect.w, 0);
                    break;
                case ImageExpendType.Width:
                    _bounds.size = new Vector3(width, width * rect.w / (float)rect.z, 0);
                    break;
                case ImageExpendType.WidthAndHeight:
                    _bounds.size = new Vector3(width, height, 0);
                    break;
                default:
                    _bounds.size = new Vector3(rect.z * height / (float)rect.w, height, 0);
                    break;
            }
            
            if (imageExpendType != ImageExpendType.WidthAndHeight)
            {
                var extents = _bounds.extents;

                _verts.Add(new Vector3(-extents.x, extents.y, extents.z));
                _verts.Add(new Vector3(-extents.x + _bounds.size.x * percent, extents.y, extents.z));
                _verts.Add(new Vector3(-extents.x + _bounds.size.x * percent, -extents.y, extents.z));
                _verts.Add(new Vector3(-extents.x, -extents.y, extents.z));
            }
            else
            {
                var halfX = rect.z / 2f;
                var halfY = rect.w / 2f;

                _verts.Add(new Vector3(-halfX, halfY, 0));
                _verts.Add(new Vector3(-halfX + rect.z * percent, halfY, 0));
                _verts.Add(new Vector3(-halfX + rect.z * percent, -halfY, 0));
                _verts.Add(new Vector3(-halfX, -halfY, 0));
            }

            _uvs.Add(new Vector2(rect.x / texWidth, (rect.y + rect.w) / texHeight));
            _uvs.Add(new Vector2((rect.x + rect.z * percent) / texWidth, (rect.y + rect.w) / texHeight));
            _uvs.Add(new Vector2((rect.x + rect.z * percent) / texWidth, rect.y / texHeight));
            _uvs.Add(new Vector2(rect.x / texWidth, rect.y / texHeight));

            //_bounds.size = new Vector3(image.rect.width * height / image.rect.height, height, 0);
            //var extents = _bounds.extents;

            //_verts.Add(new Vector3(-extents.x, extents.y, extents.z));
            //_verts.Add(new Vector3(-extents.x + _bounds.size.x * percent, extents.y, extents.z));
            //_verts.Add(new Vector3(-extents.x + _bounds.size.x * percent, -extents.y, extents.z));
            //_verts.Add(new Vector3(-extents.x, -extents.y, extents.z));

            //_uvs.Add(new Vector2(image.rect.x / image.texture.width, (image.rect.y + image.rect.height) / image.texture.height));
            //_uvs.Add(new Vector2((image.rect.x + image.rect.width * percent) / image.texture.width, (image.rect.y + image.rect.height) / image.texture.height));
            //_uvs.Add(new Vector2((image.rect.x + image.rect.width * percent) / image.texture.width, (image.rect.y) / image.texture.height));
            //_uvs.Add(new Vector2(image.rect.x / image.texture.width, (image.rect.y) / image.texture.height));

            for (int k = 0; k < 4; ++k)
            {
                _colors.Add(color);
            }

            _indices.Add(0);
            _indices.Add(1);
            _indices.Add(2);
            _indices.Add(0);
            _indices.Add(2);
            _indices.Add(3);

            mesh.Clear();
            mesh.SetVertices(_verts);
            mesh.SetUVs(0, _uvs);
            mesh.SetColors(_colors);
            mesh.SetTriangles(_indices, 0);
        }
        else
        {
            mesh.Clear();
        }
	}
}
