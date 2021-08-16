using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LevelTapKillHint : LevelHint
{
    TapKillManager manager
    {
        get
        {
            if (_manager == null) _manager = GetComponent<TapKillManager>();
            return _manager;
        }
    }
    TapKillManager _manager;

    public List<HoleHint> hints;

    private void Awake()
    {
        hand.HideTrail();
    }
    protected override void Start()
    {
        base.Start();
        hints.RemoveAll((x) => x == null);
    }
    public override void HideHint()
    {
        foreach (var hint in hints)
            if (hint == null) continue;
            else if (!hint.ShowAtStart)
                hint.Show0();
    }
    public override void ShowHint()
    {
        if (IsShow) return;
        base.ShowHint();
        StartCoroutine(C_ShowHint());
    }
    IEnumerator C_ShowHint()
    {
        DoneShow = false;

        foreach (var hint in hints)
            hint.Show100();

        level.onPostReScanPath += SyncWithCurrentManager;

        if (Teleport.instance != null)
            Teleport.instance.onPostTeleport += SyncWithCurrentManager;

        manager.afterFinish_TapKill += hand.HideHand;

        yield return StartCoroutine(C_SyncWithCurrentManager());

        DoneShow = true;

        yield break;
    }
    [ContextMenu("SyncWithCurrentManager")]
    public override void SyncWithCurrentManager()
    {
        StartCoroutine(C_SyncWithCurrentManager());
    }
    IEnumerator C_SyncWithCurrentManager()
    {
        if (level.isWin == true || level.isLose == true)
            yield break;

        HoleHint first = FirstActive();
        if (first == null) yield break;

        Hole touchHole = HoleTouchFirstHint();

        if (first != null && touchHole != null)
        {
            foreach (var hint in hints)
                hint.IsNextHint = hint == first;

            hand.CalculateOffset();
            Vector3 start = touchHole.HoleCenter() + hand.offset3();
            hand.transform.position = start;

            if (Application.isPlaying)
            {
                hand.transform.SetParent(IsHintMove(first) ? touchHole.transform : transform);
                hand.transform.SetAsLastSibling();
            }

            hand.gameObject.SetActive(true);
            hand.DoTapByAnim();
        }

        yield break;
    }
    bool IsHintMove(HoleHint hint)
    {
        return hint.hole.enemyAttackable != null &&
            hint.hole.enemyAttackable.enemy != null &&
            hint.hole.enemyAttackable.enemy.IsMoving();
    }
    HoleHint FirstActive()
    {
        HoleHint first = null;
        for (int i = 0; i < hints.Count; i++)
        {
            if ((hints[i].hole.enemyAttackable == null && !hints[i].hole.ContainTeleport()) ||
                (hints[i].hole.enemyAttackable != null && hints[i].hole.enemyAttackable.gameObject.activeSelf == false) ||
                hints[i].hole.IsPassed == true)
                continue;

            first = hints[i];
            break;
        }
        return first;
    }
    Hole HoleTouchFirstHint()
    {
        HoleHint first = FirstActive();
        if (first == null) return null;
        Hole hole = manager.holes.Find((x) => x.IsContainHint(first) == true);
        return hole;
    }
    [ContextMenu("GetAllHint")]
    public override void GetAllHint()
    {
        hints = new List<HoleHint>();
        ((TapKillManager)levelController).holes.ForEach((x) => hints.Add(x.hint));
        hints.Sort((x, y) => x.GetIntHint().CompareTo(y.GetIntHint()));
    }
    [ContextMenu("TrySortHint")]
    public void TrySortHint()
    {
        List<HealthController> healths = level.GetComponentsInChildren<HealthController>().ToList();
        healths.Sort((x, y) => x.Health.CompareTo(y.Health));
        int indexHint = 0;
        for (int i = 0; i < healths.Count; i++)
        {
            BaseMammal mammal = BaseMammal.GetBaseMammal(healths[i]);
            if (mammal == null) continue;
            if (mammal.transform == level.charactor.transform) continue;
            Hole hintHole = level.GetHole(mammal.transform);
            if (!hintHole.hint.gameObject.activeSelf) continue;
            if (hintHole != null)
            {
                indexHint++;
                hintHole.hint.SetHint(indexHint.ToString());
            }
        }
    }
    public override void RemoveAllHint()
    {
        foreach (var hint in hints)
            hint.gameObject.SetActive(false);
        hints = new List<HoleHint>();
    }
    public override bool ContainHints()
    {
        return hints.Count != 0;
    }
}
