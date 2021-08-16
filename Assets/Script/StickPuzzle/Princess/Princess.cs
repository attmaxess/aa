using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Princess : Enemy
{
    public override void DoIdle()
    {
        DoAnimation("idle");
    }
    public override void DoWin(BaseMammal helper = null)
    {
        if (isWin) return;
        isWin = true;
        SetTimeScale(1f);
        if (helper != null && helper.transform == level.charactor.transform)
        {
            level.isWin = true;
            DoAnimation("win");
        }
    }
    public override bool CanMoveSnap() { return false; }
    public override void SetMoving() { }
    protected override void DoLose(BaseMammal killer = null)
    {
        if (level.useDebugLog)
            Debug.Log(transform.name + " Princess.cs Dolose by" + killer.transform.name);

        if (killer != null)
        {
            if (killer.transform == level.charactor.transform)
            {
                level.isWin = true;
                DoAnimation("win");
            }
            else
            {
                level.isLose = true;
                LevelPrefabController.instance.DieAt(transform.position);
                gameObject.SetActive(false);
            }
        }
    }
}
