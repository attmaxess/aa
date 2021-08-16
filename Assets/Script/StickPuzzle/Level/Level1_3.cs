using DG.Tweening;
using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level1_3 : Level
{
    public BigGrid grid
    {
        get
        {
            if (this._grid == null) this._grid = GetComponentInChildren<BigGrid>();
            return this._grid;
        }
    }
    BigGrid _grid;

    public override void Start()
    {
        DoneStartLevel = false;
        base.Start();                
        StartCoroutine(C_Start());
    }
    IEnumerator C_Start()
    {
        if (GameController.instance.currentLevel != null)
            yield return new WaitUntil(() => GameController.instance.DoneLoadLevel == true);

        levelController.levelStatus = eLevelStatus.Busy;

        yield return new WaitUntil(() => IsParent0() == true);
        //SnapAllToAstarGraph();

        grid.gridTouch.postDrawing += ReScanPath;
        grid.gridTouch.mainLine.gameObject.SetActive(true);

        onPostReScanPath += LockLevelController;

        levelController.levelStatus = eLevelStatus.Idle;
        DoneStartLevel = true;
        yield break;
    }
    void LockLevelController()
    {
        levelController.levelStatus = eLevelStatus.Lock;
    }
    public override string GetLevelNameType()
    {
        return "_1_3";
    }
    
}
