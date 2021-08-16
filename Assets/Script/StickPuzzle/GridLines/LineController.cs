using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI.Extensions;

public class LineController : MonoBehaviour
{
    public UILineConnector lineConnect;
    public UILineRenderer lineRender;
    public List<RectTransform> linetransforms
    {
        get { return _linetransforms; }
        set { _linetransforms = value; OnPostSetLinesValue(value); }
    }
    [SerializeField] List<RectTransform> _linetransforms = new List<RectTransform>();
    protected virtual void OnPostSetLinesValue(List<RectTransform> value)
    {
        lineRender.enabled = value.Count > 0;
        if (lineRender != null)
        {
            if (lineConnect != null)
            {
                lineConnect.transforms = value.Count > 0 ?
                    value.ToArray() : new RectTransform[0];
            }
            else
            {
                lineRender.Points = new Vector2[value.Count];
                for (int i = 0; i < value.Count; i++)
                    lineRender.Points[i] = value[i].transform.position;
            }
        }
    }
    [Space(10)]
    [SerializeField] RectTransform TestTransform = null;
    public void AddPositionByTransform(RectTransform tr)
    {
        if (linetransforms.Contains(tr)) return;
        RectTransform[] tempList = new RectTransform[linetransforms.Count + 1];
        for (int i = 0; i < linetransforms.Count; i++)
            tempList[i] = linetransforms[i];
        tempList[linetransforms.Count] = tr;
        linetransforms = tempList.ToList();
    }
    public void RemovePositionByTransform(RectTransform tr)
    {
        if (!linetransforms.Contains(tr)) return;
        List<RectTransform> tempList = new List<RectTransform>();
        for (int i = 0; i < linetransforms.Count; i++)
            tempList.Add(linetransforms[i]);
        tempList.Remove(tr);
        linetransforms = tempList;
    }
    public void ReplacePositionByTransformAt(RectTransform tr, int index)
    {
        if (index > linetransforms.Count - 1) return;
        linetransforms[index] = tr;
        if (lineConnect != null)
            lineConnect.transforms = linetransforms.ToArray();
    }
    [ContextMenu("AddThisTransform")]
    public void AddThisTransform()
    {
        if (TestTransform) AddPositionByTransform(TestTransform);
    }
    [ContextMenu("ResetAllPoints")]
    public void ResetAllPoints()
    {
        linetransforms = new List<RectTransform>();
        lineRender.Points = new Vector2[0];
    }
    [ContextMenu("UpdateLineRenderFromConnector")]
    public void UpdateLineRenderFromConnector()
    {
        if (lineConnect != null)
            this.linetransforms = lineConnect.transforms.ToList();
    }
    public bool IsContainTr(RectTransform tr)
    {
        return linetransforms.Contains(tr);
    }
    public OneGrid LastGrid()
    {
        return linetransforms.Count > 0 ?
            linetransforms[linetransforms.Count - 1].GetComponent<OneGrid>()
            : null;
    }
    public OneGrid SecondLastGrid()
    {
        return linetransforms.Count >= 2 ?
            linetransforms[linetransforms.Count - 2].GetComponent<OneGrid>()
            : null;
    }
}
