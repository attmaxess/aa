using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum eResolution
{
    normal,
    ///1.5 -> 2.2
    longscreen,
    ///> 2.2
    squarescreen
    ///< 1.5
}

public class GameResolutionDependency : MonoBehaviour
{
    public eResolution resolution = eResolution.normal;
}
