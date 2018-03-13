using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class HudTextMesh : HudMeshBase {
    private class TextInfo
    {
        public int start;
        public int end;
        public Color color;
        public bool marked;
    }


	[SerializeField]
	private string _text = "";
	public string text
	{
		get { return _text.Length > 0 ? _text : " "; }
		set { 
			_text = value;
			dirty = true;
		}
	}

	[SerializeField]
	private Font _font;
	public Font font {
		get { return _font; }
		set {
			_font = value;

			dirty = true;
		}
	}

	[SerializeField]
	private int _fontSize = 28;
	public int fontSize {
		get { return _fontSize; }
		set {
			_fontSize = value;

			dirty = true;
		}
	}

    [SerializeField]
    private int _characterSpace = 0;
    public int characterSpace
    {
        get { return _characterSpace; }
        set {
            _characterSpace = value;

            dirty = true;
        }
    }

    [SerializeField]
    private bool _richText = false;

    private Color _richColor = Color.white;

	protected int GetRealFontSize()
	{
		return font.dynamic ? fontSize : font.fontSize;
	}
	// when the font is dynamic ,the scale is 1;else the scale is fontsize/font.fontsize
	protected float GetFontScale()
	{
		return font.dynamic ? 1f : (float)fontSize / font.fontSize;
	}

	void OnFontTextureRebuilt(Font changedFont)
	{
        //if (changedFont != font)
        //    return;

		UpdateDirty ();
	}


	void Start()
	{
		Font.textureRebuilt += OnFontTextureRebuilt;
	}

	public override void UpdateDirty()
	{
		if (font) {
			if (font.dynamic) {
				font.RequestCharactersInTexture (text, GetRealFontSize());
			}

			DoGenerateMesh ();
		}
	}

    private List<TextInfo> textInfos = new List<TextInfo>();

    Color GetColor(string str)
    {
        int length = str.Length;
        string r = null, g = null, b = null, a = null;
        if (length > 2)
        {
            r = str.Substring(1, 2);
            if (length > 4)
            {
                g = str.Substring(3, 2);
                if (length > 6)
                {
                    b = str.Substring(5, 2);
                    if (length > 8)
                    {
                        a = str.Substring(7, 2);
                    }
                }
            }
        }

        byte ir = 255, ig = 255, ib = 255, ia = 255;
        if (!string.IsNullOrEmpty(r))
        {
            ir = byte.Parse(r, System.Globalization.NumberStyles.HexNumber);
        }

        if (!string.IsNullOrEmpty(g))
        {
            ig = byte.Parse(g, System.Globalization.NumberStyles.HexNumber);
        }

        if (!string.IsNullOrEmpty(b))
        {
            ib = byte.Parse(b, System.Globalization.NumberStyles.HexNumber);
        }

        if (!string.IsNullOrEmpty(a))
        {
            ia = byte.Parse(a, System.Globalization.NumberStyles.HexNumber);
        }

        return new Color32(ir, ig, ib, ia);
    }

    // 暂时只有简单的颜色变换
    string ParseText()
    {
        _richColor = color;

        if (!_richText) return text;

        if (text.StartsWith("<color=#") && text.EndsWith("</color>"))
        {
            int closingIndex = text.IndexOf(">", 0);
            int equalsIndex = closingIndex > -1 ? text.IndexOf("=", 0, closingIndex - 0) : -1;
            int endIndex = (equalsIndex > -1 && closingIndex > -1) ? Mathf.Min(equalsIndex, closingIndex) : closingIndex;
            if (closingIndex != -1)
            {
                string myString = equalsIndex > -1 ? text.Substring(equalsIndex + 1, closingIndex - equalsIndex - 1) : "";
                _richColor = GetColor(myString);
                return text.Substring(closingIndex + 1, text.IndexOf("</color>") - closingIndex - 1);
            }
        }

        return text;
    }

	void DoGenerateMesh()
	{
		_verts.Clear();
		_uvs.Clear ();
		_indices.Clear ();
		_colors.Clear ();

		_bounds.size = Vector3.zero;
		_bounds.center = Vector3.zero;

		float fontScale = GetFontScale ();
		float xOff = 0;
		CharacterInfo ch;
		Vector3 vector = new Vector3();
        string parseText = ParseText();
        for (int i = 0; i < parseText.Length; ++i)
        {
            bool ret = font.GetCharacterInfo(parseText[i], out ch, GetRealFontSize());

            vector.Set(xOff + ch.minX * fontScale, ch.maxY * fontScale, 0);
			_verts.Add(vector);
			_bounds.Encapsulate (vector);

            vector.Set(xOff + ch.maxX * fontScale, ch.maxY * fontScale, 0);
			_verts.Add(vector);
			_bounds.Encapsulate (vector);

            vector.Set(xOff + ch.maxX * fontScale, ch.minY * fontScale, 0);
			_verts.Add(vector);
			_bounds.Encapsulate (vector);

            vector.Set(xOff + ch.minX * fontScale, ch.minY * fontScale, 0);
			_verts.Add(vector);
			_bounds.Encapsulate (vector);

			_uvs.Add( ch.uvTopLeft);
			_uvs.Add( ch.uvTopRight);
			_uvs.Add( ch.uvBottomRight);
			_uvs.Add( ch.uvBottomLeft);

			for (int k = 0; k < 4; ++k) {
                _colors.Add(_richColor);
			}

			_indices.Add( i * 4);
			_indices.Add( i * 4 + 1);
			_indices.Add( i * 4 + 2);
			_indices.Add( i * 4);
			_indices.Add( i * 4 + 2);
			_indices.Add(i * 4 + 3);

			xOff += ch.advance*fontScale + characterSpace;
		}

		for (int i = 0; i < _verts.Count; ++i) {
			_verts [i] = _verts [i] - bounds.center;
		}

        Vector3 v = _bounds.size;
        v.x *= transform.localScale.x;
        v.y *= transform.localScale.y;
        v.x += 4;
        v.y += 4;
        _bounds.size = v;

		mesh.Clear ();
		mesh.SetVertices (_verts);
		mesh.SetUVs (0, _uvs);
		mesh.SetColors (_colors);
		mesh.SetTriangles (_indices, 0);
	}

	void OnDestroy()
	{
		Font.textureRebuilt -= OnFontTextureRebuilt;
	}
}
