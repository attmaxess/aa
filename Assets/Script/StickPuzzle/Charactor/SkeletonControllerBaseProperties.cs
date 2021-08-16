using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkeletonControllerBaseProperties : MonoBehaviour
{
    public BaseMammal mammal
    {
        get
        {
            if (_mammal == null) _mammal = GetComponent<BaseMammal>();
            return _mammal;
        }
    }
    BaseMammal _mammal;
}
