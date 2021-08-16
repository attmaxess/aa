using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkeletonFadeController : MonoBehaviour
{
    public SkeletonController skeleton
    {
        get
        {
            if (_skeleton == null) _skeleton = GetComponent<SkeletonController>();            
            return _skeleton;
        }
    }
    SkeletonController _skeleton;
}
