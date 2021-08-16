using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelTapKillDrawHint : BaseLevelProperties
{
#if UNITY_EDITOR
    public bool CanDraw = false;

    private void OnDrawGizmos()
    {
        if (CanDraw)
        {
            List<HoleHint> hints = ((LevelTapKillHint)level.levelHint).hints;
            for (int i = 0; i < hints.Count - 1; i++)
            {
                if (hints[i] != null && hints[i + 1] != null)
                    Gizmos.DrawLine(hints[i].transform.position,
                        hints[i + 1].transform.position);
            }
        }
    }
#endif
}
