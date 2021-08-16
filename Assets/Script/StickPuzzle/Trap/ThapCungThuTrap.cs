using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThapCungThuTrap : Trap
{    public override float AttackHealth(float charactorHealth)
    {
        return trapPoint;
    }
}
