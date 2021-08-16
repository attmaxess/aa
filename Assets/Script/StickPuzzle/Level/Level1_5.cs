using DG.Tweening;
using Dijkstras;
using Pathfinding;
using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Events;

public class Level1_5 : Level
{
    [ContextMenu("Start")]
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

        ((WallManager)levelController).SyncDataFromScene();
        ((WallManager)levelController).UpdateWallThickness();
        ((WallManager)levelController).afterFinish_DestroyWall += ReScanPath;

        charactorAI.onPostForceFight += ResetDoneFight;

        levelController.levelStatus = eLevelStatus.Idle;

        ReScanPath(true);
        DoneStartLevel = true;
        yield break;
    }
    public override string GetLevelNameType()
    {
        return "_1_5";
    }
}
