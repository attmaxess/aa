using Pathfinding;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class NodeViewData
{
    public int NodeInGridIndex;
    public bool Walkable;
    public void SetValueBy(GridNodeBase nodeBase)
    {
        this.NodeInGridIndex = nodeBase.NodeInGridIndex;
        this.Walkable = nodeBase.Walkable;
    }
}
