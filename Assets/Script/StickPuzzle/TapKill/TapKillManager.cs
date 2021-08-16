using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class TapKillManager : LevelController, iLevelController
{
    public bool CanTapKill
    {
        get { return _CanTapKill; }
        set { _CanTapKill = value; SetCanTapKill(value); }
    }
    [SerializeField] bool _CanTapKill = true;

    public delegate void HandleSetCanTapKill(bool value);
    public HandleSetCanTapKill handleSetCanTapKill;

    public List<Hole> holes
    {
        get
        {
            if (_holes.Count == 0) SyncHolesFromScene();
            return _holes;
        }
    }
    public List<Hole> _holes;
    public delegate void AfterFinish(Hole targerHole);
    public AfterFinish afterFinish_TapKill;
    void Start()
    {
        onPostSetLevelStatus += ResetAllTouchBy;
    }

    [ContextMenu("SyncHolesFromScene")]
    public override void SyncHolesFromScene()
    {
        this._holes = new List<Hole>();
        if (level == null)
            Debug.Log(transform.name + " khong tim thay level");
        foreach (var hole in level.listHoles)
        {
            if (hole == null) continue;
            if (hole.hint == null) continue;
            if (hole.hint.GetIntHint() == 0) continue;
            this._holes.Add(hole);
        }
    }
    public bool CanUserPlay()
    {
        return this.levelStatus == eLevelStatus.Idle && CanTapKill == true;
    }
    void SetCanTapKill(bool value)
    {
        if (handleSetCanTapKill != null)
            handleSetCanTapKill.Invoke(value);
    }
    public override void ResetAllTouchBy(eLevelStatus levelStatus)
    {
        base.ResetAllTouchBy(levelStatus);
    }
    [ContextMenu("ResetAllTouch")]
    public void ResetAllTouch()
    {
        throw new System.NotImplementedException();
    }
}
