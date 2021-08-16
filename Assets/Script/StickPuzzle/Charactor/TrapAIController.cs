using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TrapAIController : MammalAIController
{
    public void SyncHoleToTrap()
    {
        if (trap.hole == null) return;
        trap.hole.transform.position = this.transform.position;
    }
    public void Start()
    {
        detection.onAddOnce += OnMeet;
        lerp.onpostFinalizeMovement += OnLerpFinalizeMovement;
    }
    [ContextMenu("Enemy_MoveByAstar")]
    public override void MoveByAstar()
    {
        Attackable attackable = Attackable.GetAttackable(this.transform);
        if (attackable != null && !attackable.CanBeMoveByAI()) return;
        base.MoveByAstar();
    }
    public void OnLerpFinalizeMovement(AILerp lerp)
    {
        CharactorAIController charactorAI = level.charactor.GetComponent<CharactorAIController>();
        charactorAI.OnMoveCheckNear(this);
        charactorAI.OnMoveAdd(this);
    }
    public override void OnMeet(ColliderDetection detection, Transform tr)
    {
        base.OnMeet(detection, tr);
        if (!handlingMammal.Contains(tr))
            HandleMeet(tr);
    }
    public void HandleMeet(Transform tr)
    {
        if (tr == this.transform) return;
    }
}
