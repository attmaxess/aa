using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarrierIsometricController : IsometricController
{
    Barrier barrier;
    Canvas canvas;
    protected override void Start()
    {
        barrier = GetComponent<Barrier>();
        canvas = GetComponent<Canvas>();        
        HandleSortOrder();
    }
    protected override void FixedUpdate()
    {
        if (CanSort())
        {
            HandleSortOrder();
            lastSortingOrder = listCanvas[0].sortingOrder;
        }
    }
    protected override void HandleSortOrder()
    {
        if (listCanvas == null || listCanvas.Count != 2) return;
        canvas.sortingOrder = listCanvas[0].sortingOrder > listCanvas[1].sortingOrder ?
            listCanvas[0].sortingOrder : listCanvas[1].sortingOrder;
    }
}
