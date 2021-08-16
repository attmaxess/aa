using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NgoiSaoHiVongTrap : Trap
{
    public override float AttackHealth(float charactorHealth)
    {
        return charactorHealth * 3;
    }
    protected override void DoLose(BaseMammal killer)
    {
        base.DoLose(killer);
        gameObject.SetActive(false);
    }
}
