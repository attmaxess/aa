using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class OneGridTouchController : MonoBehaviour,
    IPointerDownHandler,
    IPointerUpHandler,
    IPointerEnterHandler,
    IPointerExitHandler
{
    public GridTouchController gridTouch
    {
        get
        {
            if (!_gridTouch) _gridTouch = FindObjectOfType<GridTouchController>();
            return _gridTouch;
        }
    }
    GridTouchController _gridTouch;

    public OneGrid oneGrid
    {
        get
        {
            if (!_oneGrid) _oneGrid = GetComponent<OneGrid>();
            return _oneGrid;
        }
    }
    OneGrid _oneGrid;

    public delegate void CheckRemovePosibility(out bool CanRemove);
    public CheckRemovePosibility checkRemovePosibility;

    public void OnPointerEnter(PointerEventData eventData)
    {
        //Debug.Log(gameObject.name + " OnPointerEnter");
        if (Input.GetMouseButton(0))
            gridTouch.AddOrRemoveGrid(oneGrid);
    }

    public void OnPointerExit(PointerEventData eventData)
    {

    }

    public void OnPointerDown(PointerEventData eventData)
    {
        //Debug.Log(gameObject.name + " OnPointerDown");
        gridTouch.AddOrRemoveGrid(oneGrid);
        gridTouch.HideHint();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        //Debug.Log(gameObject.name + " Done");
    }
}
