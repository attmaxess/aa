using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanGatTrap : Trap
{
    public Animator animtor;
    [ContextMenu("on")] public void SetOn() { animtor.SetBool("IsOn", true); }
    [ContextMenu("off")] public void SetOff() { animtor.SetBool("IsOn", false); }
    public override float AttackHealth(float charactorHealth)
    {
        return charactorHealth;
    }
    protected override void DoLose(BaseMammal killer)
    {
        base.DoLose(killer);
        StartCoroutine(C_DoLose(killer));
    }
    IEnumerator C_DoLose(BaseMammal killer)
    {
        if (!isDefuse)
        {
            SetOff();                       

            for (int i = 0; i < level.listAttackableAI.Count; i++)
            {
                MammalAIController mammalAI = level.listAttackableAI[i];
                if (mammalAI.trap != null)
                    mammalAI.trap.Defuse();                
            }
            isDefuse = true;            
        }
        yield break;
    }
    [ContextMenu("DebugAnimator")]
    public void DebugAnimator()
    {
        Debug.Log("  aa  ");
    }
    public override bool CanMeet(Attackable other)
    {
        if (other.IsEnemy()) return false;
        return true;
    }
}
