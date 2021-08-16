using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelDebug : MonoBehaviour
{
    public static Level staticLevel
    {
        get
        {
            if (_staticLevel == null) _staticLevel = FindObjectOfType<Level>();
            return _staticLevel;
        }
    }
    static Level _staticLevel;
    public static void DebugLog(string log)
    {
        if (staticLevel != null)
        {
            if (staticLevel.useDebugLog) Debug.Log(log);
        }
        else
        {
            Debug.Log(log);
        }
    }
}
