using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Text;
using UnityEngine.Events;
using UnityEngine.EventSystems;

//[ExecuteInEditMode]
public class GraphicText : Text, IPointerClickHandler
{
    /// <summary>
    /// 图片池
    /// </summary>
    private readonly List<Image> m_ImagesPool = new List<Image>();

    /// <summary>
    /// 图片的最后一个顶点的索引
    /// </summary>
    private readonly List<int> m_ImagesVertexIndex = new List<int>();

    /// <summary>
    /// 图片y轴偏移
    /// </summary>
    private readonly List<float> m_ImagePivotY = new List<float>();

    /// <summary>
    /// 正则取出所需要的属性
    /// </summary>
    private static readonly Regex s_ImageRegex =
          new Regex(@"<quad b=(.+?) n=(.+?) size=(\d*\.?\d+%?) width=(\d*\.?\d+%?) (pivot=((-)?\d*\.?\d+%?))?/>", RegexOptions.Singleline);
    
    /// <summary>
    /// 超链接信息列表
    /// </summary>
    private readonly List<HrefInfo> m_HrefInfos = new List<HrefInfo>();

    /// <summary>
    /// 文本构造器
    /// </summary>
    private static readonly StringBuilder s_TextBuilder = new StringBuilder();

    /// <summary>
    /// 超链接正则
    /// </summary>
    private static readonly Regex s_HrefRegex =
        new Regex(@"<material=href=([^>\n\s]+)>(.*?)(</material>)", RegexOptions.Singleline);

    public System.Action<string> clickURL = null;

    /// <summary>
    /// 解析完最终的文本
    /// </summary>
    private string m_OutputText;
    public override void SetVerticesDirty()
    {
        //log(transform);
        base.SetVerticesDirty();
        UpdateQuadImage();
    }

    void log(Transform t)
    {
        print(t.name);
        if (null != t.parent)
        {
            log(t.parent);
        }
    }

    /// <summary>
    /// 获取超链接解析后的最后输出文本
    /// </summary>
    /// <returns></returns>
    protected string GetOutputText()
    {
        s_TextBuilder.Length = 0;
        m_HrefInfos.Clear();
        var indexText = 0;
        foreach (Match match in s_HrefRegex.Matches(text))
        {
            s_TextBuilder.Append(text.Substring(indexText, match.Index - indexText));

            var group = match.Groups[1];
            var hrefInfo = new HrefInfo
            {
                startIndex = s_TextBuilder.Length * 4, // 超链接里的文本起始顶点索引
                endIndex = (s_TextBuilder.Length + match.Groups[2].Length - 1) * 4 + 3,
                name = group.Value
            };
            m_HrefInfos.Add(hrefInfo);

            s_TextBuilder.Append(match.Groups[2].Value);
            indexText = match.Index + match.Length;
        }
        s_TextBuilder.Append(text.Substring(indexText, text.Length - indexText));
        return s_TextBuilder.ToString();
    }

    protected void UpdateQuadImage()
    {
        m_OutputText = GetOutputText();
        m_ImagesVertexIndex.Clear();
        m_ImagePivotY.Clear();
        foreach (Match match in s_ImageRegex.Matches(m_OutputText))
        {
            var picIndex = match.Index;
            var endIndex = picIndex * 4 + 3;
            m_ImagesVertexIndex.Add(endIndex);

            m_ImagesPool.RemoveAll(image => image == null);
            if (m_ImagesPool.Count == 0)
            {
                GetComponentsInChildren<Image>(m_ImagesPool);
            }
            if (m_ImagesVertexIndex.Count > m_ImagesPool.Count)
            {
                var resources = new DefaultControls.Resources();
                var go = DefaultControls.CreateImage(resources);
                go.layer = gameObject.layer;
                var rt = go.transform as RectTransform;
                if (rt)
                {
                    rt.SetParent(rectTransform);
                    rt.gameObject.SetActive(false);
                    rt.localPosition = Vector3.zero;
                    rt.localRotation = Quaternion.identity;
                    rt.localScale = Vector3.one;
                }
                m_ImagesPool.Add(go.GetComponent<Image>());
            }

            var bundleName = match.Groups[1].Value;
            var spriteName = match.Groups[2].Value;
            var size = float.Parse(match.Groups[3].Value);
            var width = float.Parse(match.Groups[4].Value);
            if (match.Groups[6].Value != "")
            {
                var pivotY = float.Parse(match.Groups[6].Value);
                m_ImagePivotY.Add(pivotY);
            }
            else
            {
                m_ImagePivotY.Add(-fontSize * 0.15f);
            }
            var img = m_ImagesPool[m_ImagesVertexIndex.Count - 1];
            if (img.sprite == null || img.sprite.name != spriteName)
            {
                //Transform t = transform;
                //string u = t.name;
                //while (t.parent)
                //{
                //    t = t.parent;
                //    u = t.name + "/" + u;
                //}

                //Debug.Log(u);
                AssetBundleManager.instance.GetResourceAsync<Sprite>(bundleName, spriteName, (Sprite go, bool result) =>
                {
                    if (result && null != go && img != null && "null" != img.ToString())
                    {
                        img.sprite = go;
                        img.gameObject.SetActive(true);
                    }
                });
                //img.sprite = Resources.Load<Sprite>(spriteName);
            }
            img.rectTransform.sizeDelta = new Vector2(size * width, size);
            img.rectTransform.pivot = Vector2.one * 0.5f;
            img.rectTransform.anchorMin = rectTransform.pivot;
            img.rectTransform.anchorMax = rectTransform.pivot;
            img.enabled = true;
        }

        for (var i = m_ImagesVertexIndex.Count; i < m_ImagesPool.Count; i++)
        {
            if (m_ImagesPool[i])
            {
                m_ImagesPool[i].enabled = false;
            }
        }
    }

    protected override void OnPopulateMesh(VertexHelper toFill)
    {
        var orignText = m_Text;
        m_Text = m_OutputText;
        base.OnPopulateMesh(toFill);
        m_Text = orignText;

        UIVertex vert = new UIVertex();
        for (var i = 0; i < m_ImagesVertexIndex.Count; i++)
        {
            var endIndex = m_ImagesVertexIndex[i];
            var rt = m_ImagesPool[i].rectTransform;
            var size = rt.sizeDelta;
            if (endIndex < toFill.currentVertCount)
            {
                toFill.PopulateUIVertex(ref vert, endIndex);
                rt.anchoredPosition = new Vector2(vert.position.x + size.x / 2, vert.position.y + size.y / 2 + m_ImagePivotY[i]);

                // 抹掉左下角的乱码
                toFill.PopulateUIVertex(ref vert, endIndex - 3);
                var pos = vert.position;
                //print((endIndex - 3) + " : " + vert.position);
                for (int j = endIndex, m = endIndex - 3; j > m; j--)
                {
                    toFill.PopulateUIVertex(ref vert, endIndex);
                    //print(j + " : " + vert.position);
                    vert.position = pos;
                    toFill.SetUIVertex(vert, j);
                }
            }
        }

        if (m_HrefInfos.Count > 0)
        {
            // 处理超链接包围框
            foreach (var hrefInfo in m_HrefInfos)
            {
                hrefInfo.boxes.Clear();
                if (hrefInfo.startIndex >= toFill.currentVertCount)
                {
                    continue;
                }

                // 将超链接里面的文本顶点索引坐标加入到包围框
                toFill.PopulateUIVertex(ref vert, hrefInfo.startIndex);
                var pos = vert.position;
                var bounds = new Bounds(pos, Vector3.zero);
                var lineBounds = new Bounds(pos, Vector3.zero);
                Color startColor = Color.white;
                int tempIndex = hrefInfo.startIndex;
                for (int i = hrefInfo.startIndex, m = hrefInfo.endIndex; i <= m; i++)
                {
                    if (i >= toFill.currentVertCount)
                    {
                        break;
                    }

                    toFill.PopulateUIVertex(ref vert, i);
                    pos = vert.position;
                    if (pos.x < bounds.min.x) // 换行重新添加包围框
                    {
                        hrefInfo.boxes.Add(new Rect(bounds.min, bounds.size));
                        startColor = vert.color;
                        bounds = new Bounds(pos, Vector3.zero);
                    }
                    else
                    {
                        bounds.Encapsulate(pos); // 扩展包围框
                    }

                    if (pos.x <= lineBounds.min.x && (i - tempIndex) > 3)
                    {

                        AddUnderlineQuad(toFill, lineBounds, startColor);
                        tempIndex = i;
                        lineBounds = new Bounds(pos, Vector3.zero);
                    }
                    else
                    {
                        lineBounds.Encapsulate(pos);
                    }
                }
                hrefInfo.boxes.Add(new Rect(bounds.min, bounds.size));
                AddUnderlineQuad(toFill, lineBounds, startColor);
            }
        }
    }



    UIVertex CreateUIVertex(Color color)
    {
        UIVertex v = UIVertex.simpleVert;
        v.uv0 = new Vector2(0.1f, 0.1f);
        v.tangent = new Vector4(1f, 0f, 0f, -1f);
        v.color = color;
        return v;
    }

    private UIVertex[] m_TempVerts = new UIVertex[4];
    void AddUnderlineQuad(VertexHelper toFill, Bounds bounds, Color color)
    {
        //UIVertex v = CreateUIVertex(color);
        //v.position = new Vector3(bounds.min.x, bounds.min.y, 0f);
        //m_TempVerts[0] = v;

        //v = CreateUIVertex(color);
        //v.position = new Vector3(bounds.min.x + bounds.size.x, bounds.min.y, 0f);
        //m_TempVerts[1] = v;

        //v = CreateUIVertex(color);
        //v.position = new Vector3(bounds.min.x + bounds.size.x, bounds.min.y - fontSize * 0.1f, 0f);
        //m_TempVerts[2] = v;

        //v = CreateUIVertex(color);
        //v.position = new Vector3(bounds.min.x, bounds.min.y - fontSize * 0.1f, 0f);
        //m_TempVerts[3] = v;

        //toFill.AddUIVertexQuad(m_TempVerts);
    }  

    /// <summary>
    /// 点击事件检测是否点击到超链接文本
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerClick(PointerEventData eventData)
    {
        Vector2 lp;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform, eventData.position, eventData.pressEventCamera, out lp);

        foreach (var hrefInfo in m_HrefInfos)
        {
            var boxes = hrefInfo.boxes;
            for (var i = 0; i < boxes.Count; ++i)
            {
                if (boxes[i].Contains(lp))
                {
                    //Debug.Log(hrefInfo.name);
                    if (null != clickURL)
                    {
                        clickURL(hrefInfo.name);
                    }
                    return;
                }
            }
        }
    }

    /// <summary>
    /// 超链接信息类
    /// </summary>
    private class HrefInfo
    {
        public int startIndex;

        public int endIndex;

        public string name;

        public readonly List<Rect> boxes = new List<Rect>();
    }
}

