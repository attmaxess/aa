using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelCheckHoleConnections : BaseLevelProperties
{
#if UNITY_EDITOR
    public bool CanDraw = true;
    public bool DrawLine = true;
    public bool DrawText = true;
    private void OnDrawGizmos()
    {
        if (CanDraw)
            foreach (Hole hole in level.listHoles)
                if (hole != null) hole.DrawConnections(DrawLine, DrawText);
    }
#endif
}
