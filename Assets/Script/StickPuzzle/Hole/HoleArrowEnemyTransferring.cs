using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HoleArrowEnemyTransferring : HoleBaseProperties
{
    public Transform charactorStanding;
    public Transform enemyStanding;

    [ContextMenu("Transfer")]
    public void Transfer()
    {
        if (charactorStanding == null || enemyStanding == null) return;
        transform.position = charactorStanding.transform.position;
        hole.holeImage.transform.position = enemyStanding.transform.position;
        hole.UpdateHoleCollider();
    }
}
