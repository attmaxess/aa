using DG.Tweening;
using Dijkstras;
using Pathfinding;
using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Events;

public class Level1_4 : Level
{
    [ContextMenu("GetNodesToView")]
    public void GetNodesToView()
    {
        //nodesToView = AstarData.active.data.gridGraph.nodes.ToList();
        nodesToView = new List<NodeViewData>();
        for (int i = 0; i < AstarData.active.data.gridGraph.nodes.Length; i++)
        {
            NodeViewData newViewData = new NodeViewData();
            newViewData.SetValueBy(AstarData.active.data.gridGraph.nodes[i]);
            nodesToView.Add(newViewData);
        }
    }
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

        ((PillarManager)levelController).SyncDataFromScene();        
        ((PillarManager)levelController).afterFinish_DrawingOrMoving += ReScanPath;

        charactorAI.onPostForceFight += ResetDoneFight;
        CurrentWalkable = new List<bool>();

        levelController.levelStatus = eLevelStatus.Idle;

        ReScanPath(true);
        DoneStartLevel = true;
        yield break;
    }
    public override string GetLevelNameType()
    {
        return "_1_4";
    }
}
