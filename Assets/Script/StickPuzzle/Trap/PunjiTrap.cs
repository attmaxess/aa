using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PunjiTrap : Trap
{
    public override float AttackHealth(float charactorHealth)
    {
        return trapPoint;
    }
}
