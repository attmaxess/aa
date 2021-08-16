using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class WallTouchController : BaseTouchController,
    IPointerDownHandler
{
    public Wall wall
    {
        get
        {
            if (_wall == null) _wall = GetComponent<Wall>();
            return this._wall;
        }
    }
    Wall _wall;
    public void OnPointerDown(PointerEventData eventData)
    {
        wall.FinishDestroy();
    }
}
