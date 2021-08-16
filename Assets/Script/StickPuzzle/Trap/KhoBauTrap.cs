using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KhoBauTrap : Trap
{
    public override float AttackHealth(float charactorHealth)
    {
        return trapPoint;
    }
    protected override void DoLose(BaseMammal killer)
    {
        base.DoLose(killer);

        if (level.useDebugLog) Debug.Log(transform.name + " Khobau.cs Dolose by" + killer.transform.name);

        if (killer.transform == level.charactor.transform)
            level.isWin = true;
        else
            level.isLose = true;
    }
    public static bool IsKhoBauTrap(Transform tr)
    {
        return tr.GetComponent<KhoBauTrap>() != null;
    }
}
