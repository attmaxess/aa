using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthControllerBaseProperties : MonoBehaviour
{
    public Level level
    {
        get
        {
            if (_level == null) _level = GetComponentInParent<Level>();
            if (_level == null) _level = FindObjectOfType<Level>();
            return _level;
        }
    }
    Level _level;
    public SkeletonController skeletonController
    {
        get
        {
            if (this._skeletonController == null)
                this._skeletonController = SkeletonController.GetOrAdd(this.transform);
            return this._skeletonController;
        }
    }
    SkeletonController _skeletonController;
}
