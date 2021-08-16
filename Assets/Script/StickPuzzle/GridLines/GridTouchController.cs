using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

[RequireComponent(typeof(Image))]
public class GridTouchController : MonoBehaviour,
    IPointerDownHandler,
    IPointerUpHandler,
    IPointerEnterHandler,
    IDragHandler
{
    public BigGrid bigGrid;
    public LineController mainLine;
    public UILineConnector sideLine;
    public RectTransform currentCursor;

    [Serializable] public enum eGridDrawStatus { Idle, Busy, Lock }
    [Space(20)] public eGridDrawStatus drawStatus = eGridDrawStatus.Idle;

    public delegate void OnUpdateHealth(int point);
    public OnUpdateHealth onUpdateHealth;

    public delegate void OnPostDrawing(bool scanStar, bool poke);
    public OnPostDrawing postDrawing;

    void Start()
    {
        ResetAll();
    }

    [ContextMenu("ResetAll")]
    public void ResetAll()
    {
        drawStatus = eGridDrawStatus.Idle;
        mainLine.ResetAllPoints();
    }

    public bool IsPlayerClicked()
    {
        return mainLine.lineConnect.transforms.Length > 0 &&
            (mainLine.lineConnect.transforms[0].GetComponent<OneGrid>()?.gridType == eGridType.Player);
    }

    public void AddOrRemoveGrid(OneGrid grid)
    {
        if (drawStatus == eGridDrawStatus.Lock)
            return;

        RectTransform gridRect = grid.GetComponent<RectTransform>();
        OneGrid lastGrid = mainLine.LastGrid();

        //Check Add Or Remove
        bool IsNew = mainLine.lineConnect.transforms.Length <= 1
            || !mainLine.IsContainTr(gridRect);

        if (IsNew)
        {
            if (!IsPlayerClicked() && !grid.IsPlayer()) return;
            if (IsPlayerClicked() && !grid.IsConnectAble()) return;
            if (IsPlayerClicked() && !grid.IsVerticleOrHorizontal(lastGrid)) return;
            if (mainLine.IsContainTr(gridRect)) return;

            //Debug.Log("Adding " + grid.transform.name);
            mainLine.AddPositionByTransform(gridRect);
            //point += grid.Point;
            bigGrid.CreateFlyHealth(grid, bigGrid.level.charactor.healthController.healthText.transform);
            grid.IsSelected = true;

            if (drawStatus == eGridDrawStatus.Idle)
            {
                drawStatus = eGridDrawStatus.Busy;
                StartCoroutine(C_CheckPointerUp());
            }
        }
        else
        //Check if removing is possible
        {
            bool CanRemove = false;
            if (grid.touchController.checkRemovePosibility != null)
                grid.touchController.checkRemovePosibility.Invoke(out CanRemove);

            if (!CanRemove) return;

            if (mainLine.SecondLastGrid() == grid)
            {
                //Debug.Log("Removing " + lastGrid.transform.name);
                mainLine.RemovePositionByTransform(lastGrid.GetComponent<RectTransform>());
                //lastGrid.SelfSetup();
                lastGrid.IsSelected = false;
                bigGrid.level.charactor.healthController.Health -= lastGrid.data.point;
                bigGrid.level.charactor.healthController.UpdateHealth(true, true);
                lastGrid.RestorePoint();
                bigGrid.scalingController.RemoveFlyingScale();
            }
        }
    }

    IEnumerator C_CheckPointerUp()
    {
        while (Input.GetMouseButton(0))
        {
            //Debug.Log("Start Checking Up");
            //if (updateHealth != null) updateHealth.Invoke(this.point);
            yield return new WaitForEndOfFrame();
        }

        drawStatus = eGridDrawStatus.Lock;

        if (postDrawing != null)
            postDrawing.Invoke(false, true);

        //Debug.Log("Stop Checking Up");
        yield break;

    }

    public void OnPointerUp(PointerEventData eventData)
    {
        //Debug.Log(gameObject.name + " Done");
    }


    public void OnPointerDown(PointerEventData eventData)
    {
        //currentCursor.position = eventData.position;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        //currentCursor.position = eventData.position;
    }

    public void OnDrag(PointerEventData eventData)
    {
        //currentCursor.position = eventData.position;
    }
    public void HideHint()
    {
        ((LevelGridHint)bigGrid.level.levelHint).ShowOnlyLine();
    }
}
