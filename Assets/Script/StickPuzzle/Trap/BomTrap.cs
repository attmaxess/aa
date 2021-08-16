using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BomTrap : Trap
{
    public override float AttackHealth(float charactorHealth)
    {
        return !isDefuse ? charactorHealth / 2f : charactorHealth;
    }
    protected override void DoLose(BaseMammal killer)
    {
        base.DoLose(killer);
        gameObject.SetActive(false);
    }
    public override bool CanMeet(Attackable other)
    {
        if (other.IsEnemy()) return false;
        return true;
    }
}
