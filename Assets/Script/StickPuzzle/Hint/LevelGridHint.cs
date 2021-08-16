using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LevelGridHint : LevelHint
{
    BigGrid grid
    {
        get
        {
            if (_grid == null) _grid = GetComponent<BigGrid>();
            return _grid;
        }
    }
    BigGrid _grid;

    public LineController hintLine;
    public Vector2 AddOrRemove = Vector2.zero;

    List<OneGrid> grids = new List<OneGrid>();
    List<bool> bools;
    List<Vector2> path;
    PathData[,] pathDatas;

    protected override void Start()
    {
        base.Start();
        StartCoroutine(C_Start());
    }
    IEnumerator C_Start()
    {
        if (GameController.instance.currentLevel != null)
            yield return new WaitUntil(() => GameController.instance.DoneLoadLevel == true);

        hand.ShowTrail();
    }
    public override void HideHint()
    {
        hintLine.gameObject.SetActive(false);
    }
    public override void ShowHint()
    {
        if (IsShow) return;
        base.ShowHint();
        coShow = StartCoroutine(C_ShowHint());
    }
    IEnumerator C_ShowHint()
    {
        DoneShow = false;
        hintLine.gameObject.SetActive(true);

        coSync = StartCoroutine(C_SyncWithCurrentManager());
        yield return new WaitUntil(() => DoneSync == true);

        DoneShow = true;
        yield return new WaitForSeconds(2f);

        Repeat();
        yield break;
    }
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
            coSync = StartCoroutine(C_SyncWithCurrentManager());
            yield return new WaitUntil(() => DoneSync == true);
            yield return new WaitForSeconds(2f);
        }
        yield break;
    }
    public void ShowOnlyLine()
    {
        hand.gameObject.SetActive(false);
        StopCoShow();
        StopCoSync();
        StopCoRepeat();
    }
    [ContextMenu("SolveHint")]
    public void SolveHint()
    {
        grids = new List<OneGrid>();
        foreach (var item in grid.gridsData)
            if (item.gridType == eGridType.Player || item.gridType == eGridType.TenHP)
                grids.Add(item);

        bools = new List<bool>();
        foreach (var item in grids) bools.Add(false);

        path = new List<Vector2>();
        OneGrid player = grids.Find((x) => x.gridType == eGridType.Player);
        path.Add(player.cordinate);

        Vector2 currentCursor = path[0];
        bool added = true;
        while (added || CursorIndex(currentCursor) < path.Count - 1)
        {
            if (currentCursor.x > 0)
            {
                Vector2 leftGrid = FindGrid(new Vector2(currentCursor.x - 1, currentCursor.y)).cordinate;
                if (leftGrid != null)
                {
                    path.Add(leftGrid);
                    added = true;
                }
            }
        }
    }
    [ContextMenu("SyncWithCurrentManager")]
    public override void SyncWithCurrentManager()
    {
        StartCoroutine(C_SyncWithCurrentManager());
    }
    IEnumerator C_SyncWithCurrentManager()
    {
        DoneSync = false;

        if (level.isWin == true || level.isLose == true)
        {
            coSync = null;
            DoneSync = true;
            yield break;
        }

        if (levelController.levelStatus != eLevelStatus.Idle)
        {
            coSync = null;
            DoneSync = true;
            yield break;
        }

        if (hintLine.linetransforms.Count == 0)
        {
            coSync = null;
            DoneSync = true;
            yield break;
        }

        hand.CalculateOffset();
        Vector3 start = hintLine.linetransforms[0].transform.position + hand.offset3();
        hand.transform.position = start;
        hand.gameObject.SetActive(true);

        yield return new WaitUntil(() => hand.gameObject.activeSelf == true);

        List<Vector3> positions = new List<Vector3>();
        for (int i = 0; i < hintLine.linetransforms.Count; i++)
            positions.Add(hintLine.linetransforms[i].transform.position + hand.offset3());

        hand.ShowTrail();
        hand.DoTravel(positions);
        yield return new WaitUntil(() => hand.IsAtEndTravel() == true);
        hand.HideTrail();

        coSync = null;
        DoneSync = true;
        yield break;
    }
    int CursorIndex(Vector2 cursor)
    {
        return grids.IndexOf(FindGrid(cursor));
    }
    OneGrid FindGrid(Vector2 cordinate)
    {
        return grids.Find((x) => x.cordinate == cordinate);
    }
    public class PathData
    {
        public List<Vector2> longest;
    }
    public void AddToHintLine()
    {
        OneGrid one = grid.gridsData.Find((x) => x.cordinate == AddOrRemove);
        if (one != null)
            hintLine.AddPositionByTransform(one.GetComponent<RectTransform>());
    }
    public void RemoveFromHintline()
    {
        OneGrid one = grid.gridsData.Find((x) => x.cordinate == AddOrRemove);
        if (one != null)
            hintLine.RemovePositionByTransform(one.GetComponent<RectTransform>());
    }
    public Vector2 GetGridSize()
    {
        return new Vector2(grid.rows, grid.columns);
    }
    public override void GetAllHint()
    {

    }
    public override bool ContainHints()
    {
        return hintLine.linetransforms.Count != 0;
    }
}
