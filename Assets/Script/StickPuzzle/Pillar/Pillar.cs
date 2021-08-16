using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Pillar : PillarBaseProperties
{
    [SerializeField] Image _imgPillar = null;
    [SerializeField] Sprite _spriteDefault = null;
    [SerializeField] Sprite _spriteHighlight = null;
    [SerializeField] RectTransform focusRT = null;

    public bool CanCreateBarrier = false;
    public bool IsSelected
    {
        get { return _IsSelected; }
        set { _IsSelected = value; SetSelected(value); }
    }
    [SerializeField] bool _IsSelected = false;

    void SetSelected(bool isSelect)
    {
        _imgPillar.sprite = isSelect ? _spriteHighlight : _spriteDefault;
    }

    [SerializeField] GameObject barrierPrefab = null;
    public enum ePillarStatus { idle, drawing, barrier_moving }
    public ePillarStatus status = ePillarStatus.idle;

    [SerializeField] Sprite _spriteExistedBarrier = null;
    [SerializeField] Sprite _spriteReadyConnect = null;

    public enum eSkin { fine, existedBarrier, readyConnect }
    public eSkin skin
    {
        get { return this._eskin; }
        set { this._eskin = value; HandleSkin(value); }
    }
    [SerializeField] eSkin _eskin = eSkin.fine;
    
    public PillarTouchController touchController
    {
        get
        {
            if (_touchController == null) _touchController = GetComponentInChildren<PillarTouchController>();
            return _touchController;
        }
    }
    PillarTouchController _touchController;

    [Space(20)] public BarrierLineController currentCreateLine;
    public Barrier currentMoveLine;
    public Pillar lastFinishWhenDrawing
    {
        get { return this._finishDraw; }
        set { _finishDraw = value; SetFinishDrawingPillar(value); }
    }
    Pillar _finishDraw;
    public Pillar lastFinishWhenMoving
    {
        get { return this._finishMove; }
        set { _finishMove = value; SetFinishMovingPillar(value); }
    }
    Pillar _finishMove;

    [Space(20)]
    public List<Pillar> connectedPillars;
    public List<Barrier> connectedBarriers;

    public Animator animtor = null;

    public void AddOncePillar(Pillar pillar)
    {
        if (!connectedPillars.Contains(pillar))
            connectedPillars.Add(pillar);
    }
    public void RemovePillar(Pillar pillar)
    {
        if (!connectedPillars.Contains(pillar))
            connectedPillars.Remove(pillar);
    }
    public bool ContainPillar(Pillar pillar)
    {
        return connectedPillars.Contains(pillar);
    }

    public void AddOnceBarrier(Barrier barrier)
    {
        if (!connectedBarriers.Contains(barrier))
            connectedBarriers.Add(barrier);
    }
    public bool IsContainEqualBarrier(Barrier barrier, RectTransform rect1, RectTransform rect2)
    ///thay vì dùng barrier thì phải dùng rect1, rect2 vì barrier chưa hình thành
    ///rect1, rect2 là 2 điểm mà barrier sẽ nối đến
    {
        foreach (Barrier other in connectedBarriers)
        {
            if (other == barrier) continue;
            if (other.IsEqual(rect1, rect2)) return true;
        }
        return false;
    }
    public void StartDraw(RectTransform start, RectTransform drag)
    {
        if (!manager.CanUserPlay()) return;

        if (!CanCreateBarrier) return;

        if (status == ePillarStatus.idle)
        {
            manager.CanDrawOrMove = false;
            status = ePillarStatus.drawing;

            currentCreateLine = Instantiate(barrierPrefab as GameObject, this.transform.parent).GetComponent<BarrierLineController>();
            currentCreateLine.transform.localPosition = Vector3.zero;
            currentCreateLine.AddPositionByTransform(start);
            currentCreateLine.AddPositionByTransform(drag);

            manager.startPillar = this;
            lastFinishWhenDrawing = null;
        }
    }
    public void StartMoving(Barrier barrier, int startID, int moveID, RectTransform drag)
    {
        if (!manager.CanUserPlay()) return;
        if (startID == -1) return;
        if (moveID == -1) return;
        if (barrier.lineController == null) return;

        if (status == ePillarStatus.idle)
        {
            manager.ToogleOnFocusMode();
            manager.level.SetAllMammalStalking();

            currentMoveLine = barrier;
            manager.startPillar = barrier.lineController.linetransforms[startID].GetComponent<Pillar>();
            if (manager.startPillar == null) return;

            manager.saveMovePillarID = moveID;
            manager.saveMovePillar = currentMoveLine.lineController.linetransforms[moveID].GetComponent<Pillar>();
            if (manager.saveMovePillar == null) return;

            if (!manager.startPillar.ContainPillar(manager.saveMovePillar)) return;
            manager.startPillar.RemovePillar(manager.saveMovePillar);

            if (!manager.saveMovePillar.ContainPillar(manager.startPillar)) return;
            manager.saveMovePillar.RemovePillar(manager.startPillar);

            manager.CanDrawOrMove = false;
            status = ePillarStatus.barrier_moving;

            currentMoveLine.lineController.ReplacePositionByTransformAt(drag, moveID);
            barrier.ray.CanCheckCollision = true;
            lastFinishWhenDrawing = null;
        }
    }
    public void ResetToDrawingMode()
    {
        if (currentCreateLine != null && currentCreateLine.linetransforms.Count == 2)
        {
            currentCreateLine.ReplacePositionByTransformAt(manager.m_DraggingIcon.GetComponent<RectTransform>(), 1);
        }
    }
    public void ResetToMovingMode()
    {
        currentMoveLine.lineController.ReplacePositionByTransformAt(manager.m_DraggingIcon.GetComponent<RectTransform>(), manager.saveMovePillarID);
    }
    public void FinishDraw()
    {
        if (!CanCreateBarrier) return;

        if (lastFinishWhenDrawing == null ||
            currentCreateLine.linetransforms[1].gameObject == manager.m_DraggingIcon)
        {
            if (currentCreateLine != null)
                Destroy(currentCreateLine.gameObject);
        }
        else
        {
            manager.SyncDataFromScene();
            Barrier barrier = currentCreateLine.GetComponent<Barrier>();
            barrier.OnPostCollider.UpdateCollider();
            barrier.ClearAllDetection();
            barrier.SkinSelfCheck();
        }

        currentCreateLine = null;
        manager.ResetToIdleMode();
        status = ePillarStatus.idle;

        manager.SyncDataFromScene();

        if (manager.afterFinish_DrawingOrMoving != null)
            manager.afterFinish_DrawingOrMoving.Invoke(true, true);
    }

    public void FinishMove()
    {
        StartCoroutine(C_FinishMove());
    }
    IEnumerator C_FinishMove()
    {
        manager.ToogleOffFocusMode();

        if (lastFinishWhenMoving == null ||
            currentMoveLine.lineController.linetransforms[0].gameObject == manager.m_DraggingIcon ||
            currentMoveLine.lineController.linetransforms[1].gameObject == manager.m_DraggingIcon)
        {
            currentMoveLine.lineController.ReplacePositionByTransformAt(manager.saveMovePillar.GetComponent<RectTransform>(), manager.saveMovePillarID);
        }

        Barrier barrier = currentMoveLine.GetComponent<Barrier>();
        barrier.ray.CanCheckCollision = false;
        barrier.OnPostCollider.UpdateCollider();
        barrier.ClearAllDetection();
        barrier.SkinSelfCheck();

        currentMoveLine.skin = Barrier.eSkin.fine;

        yield return new WaitForEndOfFrame();

        currentMoveLine = null;
        manager.ResetToIdleMode();
        manager.level.SetAllMammalIdle();
        status = ePillarStatus.idle;

        manager.SyncDataFromScene();

        if (manager.afterFinish_DrawingOrMoving != null)
            manager.afterFinish_DrawingOrMoving.Invoke(true, true);

        yield break;
    }
    void SetFinishDrawingPillar(Pillar pillar)
    {
        if (!pillar) return;
        if (currentCreateLine != null && currentCreateLine.linetransforms.Count == 2)
        {
            RectTransform rtFinish = lastFinishWhenDrawing.GetComponent<RectTransform>();
            currentCreateLine.ReplacePositionByTransformAt(rtFinish, 1);
        }
    }
    void SetFinishMovingPillar(Pillar pillar)
    {
        if (!pillar) return;
        if (currentMoveLine != null && currentMoveLine.lineController.linetransforms.Count == 2)
        {
            RectTransform rtFinish = lastFinishWhenMoving.GetComponent<RectTransform>();
            currentMoveLine.lineController.ReplacePositionByTransformAt(rtFinish, manager.saveMovePillarID);
        }
    }
    [ContextMenu("SyncDataFromBarrierOnScene")]
    public void SyncDataFromBarrierOnScene()
    {
        connectedPillars = new List<Pillar>();
        connectedBarriers = new List<Barrier>();
        Barrier[] barriers = FindObjectsOfType<Barrier>();
        foreach (Barrier barrier in barriers)
        {
            if (barrier.lineController.linetransforms.Count != 2 ||
                barrier.lineController.linetransforms[0] == null ||
                barrier.lineController.linetransforms[1] == null)
            {
                Debug.Log(barrier.name + " loi~ ");
                return;
            }

            barrier.lineController.UpdateLineRenderFromConnector();

            if (barrier.lineController.linetransforms[0].gameObject == this.gameObject)
            {
                AddOncePillar(barrier.lineController.linetransforms[1].GetComponent<Pillar>());
                AddOnceBarrier(barrier);
            }

            if (barrier.lineController.linetransforms[1].gameObject == this.gameObject)
            {
                AddOncePillar(barrier.lineController.linetransforms[0].GetComponent<Pillar>());
                AddOnceBarrier(barrier);
            }
        }
        UpdateHighlightStatus();
    }
    [ContextMenu("UpdateHighlightStatus")]
    public void UpdateHighlightStatus()
    {
        IsSelected = connectedBarriers.Count > 0;
    }
    void HandleSkin(eSkin skin)
    {
        switch (skin)
        {
            case eSkin.fine:
                _imgPillar.sprite = _spriteDefault;
                _imgPillar.color = Color.white;
                break;
            case eSkin.existedBarrier:
                _imgPillar.sprite = _spriteExistedBarrier;
                _imgPillar.color = Color.red;
                break;
            case eSkin.readyConnect:
                _imgPillar.sprite = _spriteReadyConnect;
                _imgPillar.color = Color.green;
                break;
            default:
                _imgPillar.sprite = _spriteDefault;
                _imgPillar.color = Color.white;
                break;
        }
    }
    public void FlashCurrentBarrier(Barrier.eSkin eSkin)
    {
        if (currentMoveLine == null) return;
        Barrier barrier = currentMoveLine.GetComponent<Barrier>();
        barrier.skin = eSkin;
    }
    [ContextMenu("ToogleOnFocusMode")]
    public void ToogleOnFocusMode()
    {
        focusRT.gameObject.SetActive(true);
    }
    [ContextMenu("ToogleOffFocusMode")]
    public void ToogleOffFocusMode()
    {
        focusRT.gameObject.SetActive(false);
    }
    public void ToggleHulk()
    {
        if (animtor != null)
            animtor.SetBool("Hulk", true);
    }
    public bool IsHulk() { return animtor.GetBool("Hulk") == true; }
    public void ToggleRegularSize()
    {
        if (animtor != null)
            animtor.SetBool("Hulk", false);
    }
    public bool IsRegularSize() { return animtor.GetBool("Hulk") == false; }
}
