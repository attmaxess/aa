using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TiaSetTrap : Trap
{
    public override float AttackHealth(float charactorHealth)
    ///Tia sét x2 máu
    {
        return charactorHealth * 2;
    }
    protected override void DoLose(BaseMammal killer)
    {
        base.DoLose(killer);
        gameObject.SetActive(false);
    }
}
