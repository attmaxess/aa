using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PillarTouchController : BaseTouchController,
    IPointerDownHandler,
    IPointerUpHandler,
    IPointerEnterHandler,
    IPointerExitHandler,
    IDragHandler,
    IBeginDragHandler,
    IEndDragHandler
{
    public bool dragOnSurfaces = true;
    //[SerializeField] Sprite dragIcon;

    public Pillar pillar
    {
        get
        {
            if (_pillar == null) _pillar = GetComponentInParent<Pillar>();
            return this._pillar;
        }
    }
    Pillar _pillar;
    GameObject m_DraggingIcon
    {
        get
        {
            return pillar.manager.m_DraggingIcon;
        }
    }
    GameObject _m_BarrierKnob;
    private RectTransform m_DraggingPlane;
    void Start()
    {
        this.enabled = false;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        //Debug.Log(this.gameObject.name + " OnBeginDrag");

        var canvas = FindInParents<Canvas>(gameObject);
        if (canvas == null)
            return;

        if (!m_DraggingIcon.activeSelf)
            m_DraggingIcon.SetActive(true);

        // We have clicked something that can be dragged.
        // What we want to do is create an icon for this.                

        var image = m_DraggingIcon.GetComponent<Image>();
        //image.color = new Color(0, 0, 0, 1 / 255f);
        //image.sprite = dragIcon;
        //image.SetNativeSize();

        if (dragOnSurfaces)
            m_DraggingPlane = transform as RectTransform;
        else
            m_DraggingPlane = canvas.transform as RectTransform;

        PillarManager manager = pillar.manager;
        manager.IsPillarClicked = false;

        SetDraggedPosition(eventData);

        if (pillar.CanCreateBarrier)
            pillar.StartDraw(this.GetComponent<RectTransform>(), m_DraggingIcon.GetComponent<RectTransform>());
        else
        {
            Barrier barrier = pillar.connectedBarriers.Count > 0 ?
                barrier = pillar.connectedBarriers[0]
                : null;

            if (barrier)
            {
                int startID =
                    barrier.lineController.linetransforms[0].gameObject == pillar.gameObject ?
                    1 : 0;

                int moveID = startID == 1 ? 0 : 1;

                pillar.StartMoving(barrier, startID, moveID,
                    m_DraggingIcon.GetComponent<RectTransform>());

                barrier.ForceSkinFine = false;
                barrier.ClearAllDetection();
            }
        }
    }
    public void OnDrag(PointerEventData data)
    {
        //Debug.Log(this.gameObject.name + " OnDrag");

        if (m_DraggingIcon != null)
            SetDraggedPosition(data);

        if (pillar.status == Pillar.ePillarStatus.barrier_moving)
        {
            Barrier barrier = pillar.currentMoveLine.GetComponent<Barrier>();
            barrier.SkinSelfCheck();
        }
    }
    private void SetDraggedPosition(PointerEventData data)
    {
        if (dragOnSurfaces && data.pointerEnter != null && data.pointerEnter.transform as RectTransform != null)
            m_DraggingPlane = data.pointerEnter.transform as RectTransform;

        var rt = m_DraggingIcon.GetComponent<RectTransform>();
        Vector3 globalMousePos;
        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(m_DraggingPlane, data.position, data.pressEventCamera, out globalMousePos))
        {
            rt.position = globalMousePos;
            rt.rotation = m_DraggingPlane.rotation;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        //Debug.Log(this.gameObject.name + " OnEndDrag");

        if (pillar.status == Pillar.ePillarStatus.drawing)
            pillar.FinishDraw();
        else if (pillar.status == Pillar.ePillarStatus.barrier_moving)
            pillar.FinishMove();

        if (m_DraggingIcon.activeSelf)
            m_DraggingIcon.SetActive(false);
    }

    void DestroyCurrentIcon()
    {
        if (m_DraggingIcon.activeSelf)
            m_DraggingIcon.SetActive(false);
    }

    static public T FindInParents<T>(GameObject go) where T : Component
    {
        if (go == null) return null;
        var comp = go.GetComponent<T>();

        if (comp != null)
            return comp;

        Transform t = go.transform.parent;
        while (t != null && comp == null)
        {
            comp = t.gameObject.GetComponent<T>();
            t = t.parent;
        }
        return comp;
    }
    public void OnPointerUp(PointerEventData eventData)
    {

    }
    public void OnPointerDown(PointerEventData eventData)
    {

    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        CheckEnter();
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        CheckExit();
    }
    void OnMouseEnter()
    {
        //CheckEnter();

    }
    void CheckEnter()
    {
        PillarManager manager = pillar.manager;
        if (manager.startPillar?.status == Pillar.ePillarStatus.drawing)
        {
            if (manager.startPillar == pillar) return;
            manager.startPillar.lastFinishWhenDrawing = pillar;
        }

        if (manager.saveMovePillar?.status == Pillar.ePillarStatus.barrier_moving)
        {
            if (pillar != manager.startPillar &&
                pillar != manager.saveMovePillar &&
                pillar.connectedPillars.Count > 0)
            {
                pillar.skin = Pillar.eSkin.existedBarrier;
                return;
            }

            if (pillar == manager.startPillar) return;

            pillar.skin = Pillar.eSkin.readyConnect;

            Barrier barrier = manager.saveMovePillar.currentMoveLine.GetComponent<Barrier>();
            barrier.ForceSkinFine = true;
            barrier.skin = Barrier.eSkin.fine;

            manager.saveMovePillar.lastFinishWhenMoving = pillar;
            manager.IsPillarClicked = true;
            manager.m_BarrierEndKnob.transform.position = transform.position;
        }
    }
    void CheckExit()
    {
        PillarManager manager = pillar.manager;
        if (manager.startPillar?.status == Pillar.ePillarStatus.drawing)
        {
            if (!manager.startPillar.lastFinishWhenDrawing) return;
            if (manager.startPillar.lastFinishWhenDrawing == pillar)
            {
                if (manager.startPillar.status == Pillar.ePillarStatus.drawing)
                    manager.startPillar.ResetToDrawingMode();
            }
        }

        if (manager.saveMovePillar?.status == Pillar.ePillarStatus.barrier_moving)
        {
            pillar.skin = Pillar.eSkin.fine;

            Barrier barrier = manager.saveMovePillar.currentMoveLine.GetComponent<Barrier>();
            barrier.ForceSkinFine = false;
            barrier.ClearAllDetection();
            barrier.SkinSelfCheck();

            if (!manager.saveMovePillar.lastFinishWhenMoving) return;
            if (manager.saveMovePillar.lastFinishWhenMoving == pillar)
            {
                manager.saveMovePillar.ResetToMovingMode();
                manager.IsPillarClicked = false;
            }
        }
    }
    private void OnMouseExit()
    {
        //CheckExit();
    }
}
