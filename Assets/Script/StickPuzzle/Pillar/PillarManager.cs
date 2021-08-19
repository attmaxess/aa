using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

public class PillarManager : LevelController, iLevelController
{
    public bool CanDrawOrMove
    {
        get { return _CanDrawOrMove; }
        set { _CanDrawOrMove = value; SetCanDrawOrMove(value); }
    }
    [ReadOnly] public bool _CanDrawOrMove = true;

    public delegate void HandleSetCanDrawOrMove(bool value);
    public HandleSetCanDrawOrMove handleSetCanDrawOrMove;

    public List<Pillar> pillars = new List<Pillar>();
    public List<Barrier> barriers = new List<Barrier>();

    [Space(20)]
    public Pillar startPillar = null;
    public int saveMovePillarID = -1;
    public Pillar saveMovePillar = null;
    public Pillar lastSecondPillar = null;

    public GameObject m_DraggingIcon;
    Image imgDragIcon
    {
        get
        {
            if (this._imgDragIcon == null) _imgDragIcon = m_DraggingIcon.GetComponent<Image>();
            return this._imgDragIcon;
        }
    }
    Image _imgDragIcon;
    public GameObject m_BarrierEndKnob;
    Image imgBarrierKnob
    {
        get
        {
            if (this._imgBarrierKnob == null) _imgBarrierKnob = m_BarrierEndKnob.GetComponent<Image>();
            return this._imgBarrierKnob;
        }
    }
    Image _imgBarrierKnob;

    public bool IsPillarClicked
    {
        get { return this._IsPillarClicked; }
        set { this._IsPillarClicked = value; HandleSetPillarClicked(value); }
    }
    [SerializeField] bool _IsPillarClicked = false;

    private void HandleSetPillarClicked(bool value)
    {
        imgBarrierKnob.enabled = value;
        //imgDragIcon.enabled = !value;
    }
    public void ClearMovementImage(bool scanStar, bool poke)
    {
        imgBarrierKnob.enabled = false;
        //imgDragIcon.enabled = false;
    }

    public delegate void AfterFinish(bool scanStar, bool poke);
    public AfterFinish afterFinish_DrawingOrMoving;

    public LayerMask mask;

    void Start()
    {
        onPostSetLevelStatus += ResetAllTouchBy;
        afterFinish_DrawingOrMoving += ClearMovementImage;
    }
    void Update()
    {
        #region Moving pillars write code here
        if (Input.GetMouseButton(0))
        //Co click
        {
            if (CanUserPlay())
            ///Lúc này chưa có điểm bắt đầu
            {
                if (IsRegularSize())
                    ToggleHulk();

                RaycastHit hit;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                Debug.DrawRay(ray.origin, ray.direction * 100, Color.red);

                if (saveMovePillar == null)
                ///Chưa có điểm bắt đầu
                ///Có nhấn chuột
                ///Có UserCanPlay
                {
                    if (Physics.Raycast(ray, out hit, Mathf.Infinity, mask))
                    {
                        Transform objectHit = hit.transform;
                        Pillar pillarHit = hit.transform.GetComponent<Pillar>();
                        if (pillarHit != null)
                        {
                            Barrier barrier = pillarHit.connectedBarriers.Count > 0 ?
                                barrier = pillarHit.connectedBarriers[0]
                                : null;

                            if (barrier)
                            {
                                int startID =
                                    barrier.lineController.linetransforms[0].gameObject == pillarHit.gameObject ?
                                    1 : 0;

                                int moveID = startID == 1 ? 0 : 1;

                                SetPositionMDragIcon(pillarHit.transform.position);
                                pillarHit.StartMoving(barrier, startID, moveID,
                                    m_DraggingIcon.GetComponent<RectTransform>());

                                barrier.ForceSkinFine = false;
                                barrier.ClearAllDetection();
                            }
                        }
                    }
                }
                else
                ///Đã có điểm bắt đầu
                ///UserCanPlay = true
                ///Có nhấn chuột
                {

                }
            }
            else if (this.levelStatus == eLevelStatus.Idle
                && !CanDrawOrMove)
            ///Lúc này đã có điểm bắt đầu, và chỉ mới có 1 điểm bắt đầu đấy
            ///Chưa có điểm thứ 2
            {
                RaycastHit hit;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                Debug.DrawRay(ray.origin, ray.direction * 100, Color.red);

                if (Physics.Raycast(ray, out hit, Mathf.Infinity, mask) && saveMovePillar != null)
                ///Snap vào Pillar đó
                {
                    Transform objectHit = hit.transform;
                    Pillar pillarHit = hit.transform.GetComponent<Pillar>();
                    if (pillarHit != null)
                    {
                        if (pillarHit != saveMovePillar &&
                            pillarHit != startPillar &&
                            pillarHit.connectedPillars.Count == 0)
                        ///Chạm pillar mới
                        {
                            if (!m_DraggingIcon.activeSelf)
                                m_DraggingIcon.SetActive(true);
                            SetPositionMDragIcon(pillarHit.transform.position);
                            pillarHit.skin = Pillar.eSkin.readyConnect;

                            if (pillarHit != saveMovePillar &&
                                pillarHit != startPillar
                                && lastSecondPillar == null)
                                lastSecondPillar = pillarHit;
                        }
                        else
                        ///Chạm pillar cũ
                        {
                            if (!m_DraggingIcon.activeSelf)
                                m_DraggingIcon.SetActive(true);

                            SetPositionMDragIcon(Camera.main.ScreenToWorldPoint(Input.mousePosition));

                            if (pillarHit != saveMovePillar &&
                                pillarHit != startPillar &&
                                lastSecondPillar == null)
                            {
                                lastSecondPillar = pillarHit;
                                if (lastSecondPillar.connectedPillars.Count != 0)
                                    lastSecondPillar.skin = Pillar.eSkin.existedBarrier;
                            }
                        }
                    }
                }
                else
                ///Di chuyển ở ngoài các pillars
                {
                    if (!m_DraggingIcon.activeSelf)
                        m_DraggingIcon.SetActive(true);

                    SetPositionMDragIcon(Camera.main.ScreenToWorldPoint(Input.mousePosition));

                    if (lastSecondPillar != null)
                    {
                        lastSecondPillar.skin = Pillar.eSkin.fine;
                        lastSecondPillar = null;
                    }
                }
            }
        }
        else
        ///Không có click
        ///Thì xóa hết các biến tạm
        {
            if (saveMovePillar != null)
            {
                if (IsHulk())
                    ToggleRegularSize();

                RaycastHit hit;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                Debug.DrawRay(ray.origin, ray.direction * 100, Color.red);

                if (Physics.Raycast(ray, out hit, Mathf.Infinity, mask) && saveMovePillar != null)
                ///Thả chuột tại 1 Pillar mới
                {
                    Transform objectHit = hit.transform;
                    Pillar pillarHit = hit.transform.GetComponent<Pillar>();
                    if (pillarHit != null)
                    {
                        if (pillarHit != saveMovePillar &&
                                pillarHit != startPillar &&
                                pillarHit.connectedPillars.Count == 0 &&
                                saveMovePillar.currentMoveLine.skin == Barrier.eSkin.fine)
                        ///Chọn pillar mới
                        {
                            saveMovePillar.lastFinishWhenMoving = pillarHit;
                            saveMovePillar.FinishMove();
                            if (level.useDebugLog)
                                Debug.Log("Cham pillar moi");

                            if (m_DraggingIcon.gameObject.activeSelf)
                                m_DraggingIcon.gameObject.SetActive(false);
                        }
                        else
                        ///Thả chuột và
                        ///Chạm 1 trong 2 Pillar đầu, hoặc chạm Pillar đã có dây
                        {
                            saveMovePillar.FinishMove();
                            if (level.useDebugLog)
                                Debug.Log("Cham 1 trong 2 pillar dau hoac pillar co day");

                            if (m_DraggingIcon.gameObject.activeSelf)
                                m_DraggingIcon.gameObject.SetActive(false);
                        }
                    }
                }
                else
                ///Thả chuột bên ngoài các Pillars
                {
                    if (startPillar != null)
                    {
                        if (startPillar.currentMoveLine != null)
                            startPillar.currentMoveLine = null;

                        startPillar = null;
                    }

                    if (saveMovePillar != null)
                    {
                        saveMovePillar.FinishMove();
                        if (level.useDebugLog)
                            Debug.Log("Tha chuot ngoai pillar");

                        saveMovePillar = null;
                    }

                    if (saveMovePillarID != -1) saveMovePillarID = -1;

                    if (m_DraggingIcon.gameObject.activeSelf)
                        m_DraggingIcon.gameObject.SetActive(false);
                }
            }
        }
        #endregion
    }
    void SetPositionMDragIcon(Vector3 position)
    {
        m_DraggingIcon.transform.position = new Vector3(position.x, position.y, 0);
        m_DraggingIcon.transform.localPosition = new Vector3(m_DraggingIcon.transform.localPosition.x,
            m_DraggingIcon.transform.localPosition.y, 0);
    }
    public void ResetToIdleMode()
    {
        startPillar = null;
        saveMovePillarID = -1;
        saveMovePillar = null;
        CanDrawOrMove = true;
        foreach (Pillar pillar in pillars)
            pillar.skin = Pillar.eSkin.fine;
    }
    [ContextMenu("SyncPillarsFromScene")]
    public void SyncPillarsFromScene()
    {
        pillars = GetComponentsInChildren<Pillar>().ToList();
        barriers = GetComponentsInChildren<Barrier>().ToList();
    }

    [ContextMenu("SyncDataFromScene")]
    public void SyncDataFromScene()
    {
        foreach (Pillar pillar in pillars)
            pillar.SyncDataFromBarrierOnScene();
        foreach (Barrier barrier in barriers)
        {
            if (!Application.isPlaying)
            {
                barrier.OnPostCollider.UpdateCollider();
                barrier.OnDragCollider.UpdateCollider();
            }
            else
            {
                barrier.OnPostCollider.UpdateAsyncCollider();
                barrier.OnDragCollider.UpdateAsyncCollider();
            }
        }
    }
    public bool CanUserPlay()
    {
        return this.levelStatus == eLevelStatus.Idle &&
            CanDrawOrMove == true &&
            level.isWin == false &&
            level.isLose == false;
    }
    void SetCanDrawOrMove(bool value)
    {
        if (handleSetCanDrawOrMove != null)
            handleSetCanDrawOrMove.Invoke(value);
    }
    public override void ResetAllTouchBy(eLevelStatus levelStatus)
    {
        base.ResetAllTouchBy(levelStatus);
        //foreach (Pillar pillar in pillars)
        //    pillar.touchController.col2D.enabled = levelStatus == iLevelStatus.Idle ? true : false;
    }
    public void ResetAllTouch()
    {
        throw new System.NotImplementedException();
    }
    [ContextMenu("ToogleOnFocusMode")]
    public void ToogleOnFocusMode()
    {
        foreach (Pillar pillar in pillars)
            if (pillar.connectedBarriers.Count == 0)
                pillar.ToogleOnFocusMode();
    }
    [ContextMenu("ToogleOffFocusMode")]
    public void ToogleOffFocusMode()
    {
        foreach (Pillar pillar in pillars)
            pillar.ToogleOffFocusMode();
    }
    public Barrier GetBarrierContain(Pillar pillar)
    {
        return barriers.Find((x) => x.lineController.IsContainTr(pillar.GetComponent<RectTransform>()) == true);
    }
    void ToggleHulk()
    {
        foreach (var pillar in pillars)
            pillar.ToggleHulk();
    }
    void ToggleRegularSize()
    {
        foreach (var pillar in pillars)
            pillar.ToggleRegularSize();
    }
    bool IsHulk() { return pillars.Count > 0 && pillars[0].IsHulk(); }
    bool IsRegularSize() { return pillars.Count > 0 && pillars[0].IsRegularSize(); }
}
