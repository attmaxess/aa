using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class WallManager : LevelController, iLevelController
{
    public bool CanDestroyWall
    {
        get { return _CanDestroyWall; }
        set { _CanDestroyWall = value; SetCanDrawOrMove(value); }
    }
    [SerializeField] bool _CanDestroyWall = true;

    public delegate void HandleSetCanDestroy(bool value);
    public HandleSetCanDestroy handleSetCanDestroy;

    public List<Wall> walls;

    public delegate void AfterFinish(bool scanStar, bool poke);
    public AfterFinish afterFinish_DestroyWall;
    void Start()
    {
        onPostSetLevelStatus += ResetAllTouchBy;
    }

    [ContextMenu("SyncWallsFromScene")]
    public void SyncWallsFromScene()
    {
        walls = GetComponentsInChildren<Wall>().ToList();
    }
    [ContextMenu("SyncDataFromScene")]
    public void SyncDataFromScene()
    {
        foreach (Wall wall in walls)
            wall.collidor.UpdateCollider();
    }
    [ContextMenu("UpdateAppearance")]
    public void UpdateAppearance()
    {
        walls.ForEach((x) => x.UpdateAppearance());
    }
    public void UpdateWallThickness()
    {
        foreach (Wall wall in ((WallManager)levelController).walls)
        {
            wall.UpdateAppearance();
        }
    }
    public bool CanUserPlay()
    {
        return this.levelStatus == eLevelStatus.Idle && CanDestroyWall == true;
    }
    void SetCanDrawOrMove(bool value)
    {
        if (handleSetCanDestroy != null)
            handleSetCanDestroy.Invoke(value);
    }
    public override void ResetAllTouchBy(eLevelStatus levelStatus)
    {
        base.ResetAllTouchBy(levelStatus);
        //foreach (Wall wall in walls)
        //    wall.touchController.col2D.enabled = levelStatus == iLevelStatus.Idle ? true : false;
    }
    [ContextMenu("ResetAllTouch")]
    public void ResetAllTouch()
    {
        throw new System.NotImplementedException();
    }
}
