using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI.Extensions;

public class MovementHelperController : BaseMammalProperties
{
    [Serializable]
    public class SpeedReference
    {
        public float lerpSpeed;
        public float animSpeed;
        public SpeedReference(
            float lerpSpeed,
            float animSpeed)
        {
            this.lerpSpeed = lerpSpeed;
            this.animSpeed = animSpeed;
        }
    }
    public SpeedReference IdleSpeed = new SpeedReference(1f, 1f);
    public SpeedReference StalkingSpeed = new SpeedReference(1f, 1.5f);
    public SpeedReference ChargingSpeed = new SpeedReference(2f, 1.5f);
    public SpeedReference FightingSpeed = new SpeedReference(1f, 1.5f);

    [ContextMenu("SetIdleSpeed")]
    public void SetIdleSpeed()
    {
        mammalAI?.SetMovementReference(IdleSpeed);
    }
    [ContextMenu("SetStalkingSpeed")]
    public void SetStalkingSpeed()
    {
        mammalAI?.SetMovementReference(StalkingSpeed);
    }
    [ContextMenu("SetChargingSpeed")]
    public void SetChargingSpeed()
    {
        mammalAI?.SetMovementReference(ChargingSpeed);
    }
    [ContextMenu("SetFightingSpeed")]
    public void SetFightingSpeed()
    {
        mammalAI?.SetMovementReference(FightingSpeed);
    }
}
