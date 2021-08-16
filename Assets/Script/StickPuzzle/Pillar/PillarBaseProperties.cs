using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PillarBaseProperties : MonoBehaviour
{
    public PillarManager manager
    {
        get
        {
            if (_manager == null) _manager = FindObjectOfType<PillarManager>();
            return _manager;
        }
    }
    PillarManager _manager;
    
}
