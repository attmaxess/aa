using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GridLayerController : GridBaseProperties
{
    public Transform grid0;
    public Transform line;
    public Transform grid1;
    private void Start()
    {
        SetLayer();
    }
    [ContextMenu("SelfSetup")]
    public void SelfSetup()
    {        
        if (grid0 == null) grid0 = bigGrid.currentBigGrid;
        if (line == null) line = bigGrid.GetComponentInChildren<BarrierLineController>().transform;
        if (grid1 == null) grid1 = Instantiate(grid0, bigGrid.transform).transform;
        grid1.localPosition = grid0.localPosition;
        grid1.localScale = grid0.localScale;
        grid0.SetSiblingIndex(line.GetSiblingIndex());
        grid1.SetSiblingIndex(line.GetSiblingIndex() + 1);
        foreach (OneGrid tr in grid1.GetComponentsInChildren<OneGrid>())
        {
            if (Application.isPlaying) Destroy(tr.gameObject);
            else DestroyImmediate(tr.gameObject);
        }
        Create_trOnGrid1();
        SetLayer();
    }
    [ContextMenu("SetLayer")]
    public void SetLayer()
    {
        List<OneGrid> grids = grid0.GetComponentsInChildren<OneGrid>().ToList();

        foreach (OneGrid grid in grids)
        {
            grid.transform.SetParent(grid0);

            grid._imageAvatar.transform.SetParent(grid.trOnGrid1.transform);
            grid._imageAvatar.transform.localPosition = Vector3.zero;

            float originalX = grid.uiText.transform.localPosition.x;
            grid.uiText.transform.SetParent(grid.trOnGrid1.transform);
            grid.uiText.transform.localPosition = new Vector3(originalX, 35f, 0);
        }

        grid0.SetAsFirstSibling();
        hint.hintLine.transform.SetSiblingIndex(grid0.GetSiblingIndex() + 1);
        line.SetSiblingIndex(hint.hintLine.transform.GetSiblingIndex() + 1);
        grid1.SetSiblingIndex(line.GetSiblingIndex() + 1);
    }

    [ContextMenu("Create_trOnGrid1")]
    public void Create_trOnGrid1()
    {
        List<OneGrid> grids = grid0.GetComponentsInChildren<OneGrid>().ToList();

        foreach (OneGrid grid in grids)
        {
            grid.transform.parent = grid0;
            if (grid.trOnGrid1 == null)
            {
                grid.trOnGrid1 = Instantiate(new GameObject() as GameObject, grid1);
                grid.trOnGrid1.name = grid.transform.name;
                grid.trOnGrid1.transform.parent = grid1;
                grid.trOnGrid1.gameObject.AddComponent<RectTransform>();
            }
        }
    }
    [ContextMenu("DeleteAll")]
    public void DeleteAll()
    {
        if (grid0 != null)
            if (Application.isPlaying) Destroy(grid0.gameObject);
            else DestroyImmediate(grid0.gameObject);
        if (grid1 != null)
            if (Application.isPlaying) Destroy(grid1.gameObject);
            else DestroyImmediate(grid1.gameObject);
    }
}
