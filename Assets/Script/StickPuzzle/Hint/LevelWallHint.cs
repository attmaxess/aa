using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LevelWallHint : LevelHint
{
    WallManager manager
    {
        get
        {
            if (_manager == null) _manager = GetComponent<WallManager>();
            return _manager;
        }
    }
    WallManager _manager;

    public List<WallHint> hints;

    protected override void Start()
    {
        base.Start();
        StartCoroutine(C_Start());
    }
    IEnumerator C_Start()
    {
        if (GameController.instance.currentLevel != null)
            yield return new WaitUntil(() => GameController.instance.DoneLoadLevel == true);

        hand.HideTrail();
    }
    public override void HideHint()
    {
        foreach (var hint in hints)
            if (hint == null) continue;
            else hint.Show0();
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

        yield return new WaitUntil(() => hints.Find((x) => x.gameObject.activeSelf == false) == null);

        level.onPostReScanPath += SyncWithCurrentManager;
        manager.afterFinish_DestroyWall += hand.HideHand;

        if (!ShowHintAtStart)
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

        WallHint first = FirstActive();
        if (first == null) yield break;

        Wall touchWall = WallTouchFirstHint();

        if (first != null && touchWall != null)
        {
            foreach (var hint in hints)
                hint.IsNextHint = hint == first;

            hand.CalculateOffset();
            Vector3 start = touchWall.transform.position + hand.offset3();
            hand.transform.position = start;
            hand.gameObject.SetActive(true);

            hand.DoTapByAnim();
        }

        yield break;
    }
    WallHint FirstActive()
    {
        WallHint first = null;
        for (int i = 0; i < hints.Count; i++)
        {
            if (!hints[i].wall.gameObject.activeSelf) continue;
            first = hints[i];
            break;
        }
        return first;
    }
    Wall WallTouchFirstHint()
    {
        WallHint first = FirstActive();
        if (first == null) return null;
        Wall wall = manager.walls.Find((x) => x.IsContainHint(first) == true);
        return wall;
    }
    [ContextMenu("GetAllHint")]
    public override void GetAllHint()
    {
        hints = GetComponentsInChildren<WallHint>().ToList();
        hints.Sort((x, y) => x.GetIntHint().CompareTo(y.GetIntHint()));
    }
    public override bool ContainHints()
    {
        return hints.Count != 0;
    }
}
