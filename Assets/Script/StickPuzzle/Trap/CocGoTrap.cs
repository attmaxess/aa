using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CocGoTrap : Trap
{
    public override float AttackHealth(float charactorHealth)
    ///Chia 2 vì đánh 2 lần
    {
        return !isDefuse ? trapPoint / 2f : 0f;
    }
}
