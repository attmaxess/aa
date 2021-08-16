using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelSyncDebug : BaseLevelProperties
{
    [ContextMenu("SetOff")]
    public void SetOff()
    {
        level.useDebugLog = false;
    }
}
