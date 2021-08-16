using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CungTrap : Trap
{
    public override float AttackHealth(float charactorHealth)
    {
        return trapPoint / 2f;
    }
}
