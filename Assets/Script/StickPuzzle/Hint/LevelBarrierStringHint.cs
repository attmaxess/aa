using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LevelBarrierStringHint : LevelHint
{
    PillarManager manager
    {
        get
        {
            if (_manager == null) _manager = GetComponent<PillarManager>();
            return _manager;
        }
    }
    PillarManager _manager;

    public List<BarrierHint> hints;
    public List<RectTransform> cachedBarrier;
    bool DoneCached = false;
    private void Awake()
    {
        hand.HideTrail();
    }
    protected override void Start()
    {
        base.Start();
        StartCoroutine(C_Start());
    }
    IEnumerator C_Start()
    {
        if (GameController.instance.currentLevel != null)
            yield return new WaitUntil(() => GameController.instance.DoneLoadLevel == true);

        hints.Sort((x, y) => x.GetIntHint().CompareTo(y.GetIntHint()));
        GetCached();
    }
    void GetCached()
    {
        DoneCached = false;
        cachedBarrier = new List<RectTransform>();
        for (int i = 0; i < manager.barriers.Count; i++)
        {
            if (manager.barriers[i].lineController.linetransforms.Count != 2) continue;
            cachedBarrier.Add(manager.barriers[i].lineController.linetransforms[0]);
            cachedBarrier.Add(manager.barriers[i].lineController.linetransforms[1]);
        }
        DoneCached = true;
    }
    bool IsEqualCached()
    {
        for (int i = 0; i < manager.barriers.Count; i++)
        {
            if (!manager.barriers[i].IsEqual(cachedBarrier[i * 2], cachedBarrier[i * 2 + 1]))
            {
                return false;
            }
        }
        return true;
    }
    public override void HideHint()
    {
        foreach (var barrier in hints)
            barrier.Show0();
    }
    public override void ShowHint()
    {
        if (IsShow) return;
        base.ShowHint();
        StartCoroutine(C_ShowHint());
    }
    IEnumerator C_ShowHint()
    {
        yield return new WaitUntil(() => level.DoneStartLevel == true);
        yield return new WaitUntil(() => DoneCached == true);

        if (!IsEqualCached())
        {
            DoneShow = true;
            yield return new WaitUntil(
                () => GameController.instance.IsReadyToLoadLevel());

            GameController.instance.LoadLevelByGameID(
               GameController.instance.currentLevelID,
               forceShowHint: true);
            yield break;
        }

        DoneShow = false;

        hand.ShowTrail();
        foreach (var hint in hints)
            hint.Show0();

        level.onPostReScanPath += SyncWithCurrentManager;
        manager.afterFinish_DrawingOrMoving += hand.HideHand;

        if (!ShowHintAtStart)
            SyncWithCurrentManager();

        DoneShow = true;
        Repeat();
        yield break;
    }
    /*
    bool IsFollowingHint()
    {
        bool isFollow = true;

        BarrierHint first = FirstActive();
        if (first == null) return true;
        int indexDidFollow = first.GetIntHint() - 1;
        if (indexDidFollow <= 0) return true;

        for (int i = 0; i < indexDidFollow; i++)
        {
            Barrier followTracking = manager.barriers.Find((x) => x.Equals(hints[i].lineController));
            if (followTracking == null)
                isFollow = false;
        }
        return isFollow;
    }*/
    [ContextMenu("Repeat")]
    protected override void Repeat()
    {
        if (coRepeat == null && !IsRepeating)
            coRepeat = StartCoroutine(C_Repeat());
    }
    IEnumerator C_Repeat()
    {
        IsRepeating = true;
        while (IsRepeating)
        {
            yield return new WaitForSeconds(3f);
            SyncWithCurrentManager();
        }
        yield break;
    }
    [ContextMenu("SyncWithCurrentManager")]
    public override void SyncWithCurrentManager()
    {
        if (coSync == null)
            coSync = StartCoroutine(C_SyncWithCurrentManager());
    }
    IEnumerator C_SyncWithCurrentManager()
    {
        if (level.isWin == true || level.isLose == true)
        {
            coSync = null;
            yield break;
        }

        if (manager.levelStatus != eLevelStatus.Idle)
        {
            coSync = null;
            yield break;
        }

        BarrierHint first = FirstActive();
        if (first == null)
        {
            coSync = null;
            yield break;
        }

        Barrier touchBarrier = BarrierTouchFirstHint();

        while (touchBarrier != null && first != null && touchBarrier.IsEqual(first.lineController))
        {
            first.gameObject.SetActive(false);
            first = FirstActive();
            touchBarrier = BarrierTouchFirstHint();
        }

        if (first != null && touchBarrier != null)
        {
            foreach (BarrierHint hint in hints)
                hint.IsNextHint = hint == first;

            Transform samePillar = touchBarrier.GetTheSamePillar(first.lineController);
            if (samePillar != null)
            {
                Transform otherPillar1 = touchBarrier.GetOtherPillar(samePillar);
                Pillar _otherpillar1 = otherPillar1.GetComponent<Pillar>();

                Transform otherPillar2 = first.GetOtherPillar(samePillar);
                Pillar _otherpillar2 = otherPillar2.GetComponent<Pillar>();

                if (_otherpillar1 != null && _otherpillar2 != null)
                {
                    _otherpillar1.ToogleOnFocusMode();
                    _otherpillar2.ToogleOnFocusMode();

                    hand.CalculateOffset();

                    Vector3 start = otherPillar1.position + hand.offset3();
                    hand.transform.position = start;
                    hand.gameObject.SetActive(true);

                    hand.DoMove(otherPillar1.position + hand.offset3(),
                        otherPillar2.position + hand.offset3());
                }
            }
        }

        coSync = null;
        yield break;
    }
    BarrierHint FirstActive()
    {
        BarrierHint first = null;
        for (int i = 0; i < hints.Count; i++)
        {
            if (!hints[i].gameObject.activeSelf) continue;
            first = hints[i];
            break;
        }
        return first;
    }
    Barrier BarrierTouchFirstHint()
    {
        BarrierHint first = FirstActive();
        if (first == null) return null;
        Barrier barrier = manager.barriers.Find((x) => x.IndexContainPillar(first.lineController.linetransforms[0].transform) != -1 ||
        x.IndexContainPillar(first.lineController.linetransforms[1].transform) != -1);
        return barrier;
    }
    [ContextMenu("GetAllHint")]
    public override void GetAllHint()
    {
        hints = GetComponentsInChildren<BarrierHint>().ToList();
    }
    public override bool ContainHints()
    {
        return hints.Count != 0;
    }
    [ContextMenu("Show100")]
    public void Show100()
    {
        hints.ForEach((x) => x.Show100());
    }
    [ContextMenu("Show0")]
    public void Show0()
    {
        hints.ForEach((x) => x.Show0());
    }
}
