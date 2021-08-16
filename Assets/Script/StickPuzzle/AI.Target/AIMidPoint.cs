using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIMidPoint : MonoBehaviour
{
    public Seeker seeker
    {
        get
        {
            if (_seeker == null) _seeker = GetComponentInParent<Seeker>();
            return this._seeker;
        }
    }
    Seeker _seeker;
}
