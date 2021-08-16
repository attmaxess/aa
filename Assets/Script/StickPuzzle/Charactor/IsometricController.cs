using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI.Extensions;

public class IsometricController : MonoBehaviour
{
    protected List<Canvas> listCanvas;

    [ReadOnly] public bool CanSortIsometric = false;
    [ReadOnly] protected int lastSortingOrder = -1;

    protected virtual void Start()
    {
        this.listCanvas = GetComponentsInChildren<Canvas>().ToList();
        if (CanSort())
            HandleSortOrder();
    }
    protected virtual void FixedUpdate()
    {
        if (CanSort())
        {
            HandleSortOrder();
            lastSortingOrder = listCanvas[0].sortingOrder;
        }
    }
    protected virtual void HandleSortOrder()
    {
        if (listCanvas == null || listCanvas.Count == 0) return;
        this.listCanvas.ForEach(item =>
        {
            var point = Camera.main.WorldToScreenPoint(item.transform.position);
            float topMaxPosition = 3800; // the top max position for any sprite in the screen
            item.sortingOrder = Mathf.RoundToInt(topMaxPosition - point.y);
        });
    }
    int CurrentSortingOrder()
    {
        if (listCanvas == null || listCanvas.Count == 0) return -1;
        return listCanvas[0].sortingOrder;
    }
    protected bool CanSort()
    {
        return listCanvas != null && listCanvas.Count > 0 && CanSortIsometric;
    }
    public void SetCanvas(List<Canvas> canvas)
    {
        this.listCanvas = canvas;
    }
}