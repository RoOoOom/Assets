using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[AddComponentMenu("UI/Effects/Gradient")]
public class Gradient : BaseMeshEffect
{
    [SerializeField]
    private Color32 m_topColor = Color.white;

    [SerializeField]
    private Color32 m_bottomColor = Color.black;

    public void SetColor(Color32 topColor, Color32 bottomColor)
    {
        m_topColor = topColor;
        m_bottomColor = bottomColor;
        if (graphic != null)
            graphic.SetVerticesDirty();
    }

    public override void ModifyMesh(VertexHelper vh)
    {
        if (!IsActive())
        {
            return;
        }

        var vertexList = new List<UIVertex>();
        vh.GetUIVertexStream(vertexList);
        int count = vertexList.Count;

        ApplyGradient(vertexList, 0, count);
        vh.Clear();
        vh.AddUIVertexTriangleStream(vertexList);
    }

    private void ApplyGradient(List<UIVertex> vertexList, int start, int end)
    {
        if (vertexList.Count <= 0) return;

        UIVertex uiVertex;
        float buttomY;
        float uiElementHeight;
        for (int i = start; i < end; )
        {
            buttomY = vertexList[i + 2].position.y;
            uiElementHeight = vertexList[i].position.y - buttomY;
            for (int j = i; j < i + 6; j++)
            {
                uiVertex = vertexList[j];
                uiVertex.color = Color32.Lerp(m_bottomColor, m_topColor, (uiVertex.position.y - buttomY) / uiElementHeight);
                vertexList[j] = uiVertex;
            }
            i += 6;
        }
    }
}