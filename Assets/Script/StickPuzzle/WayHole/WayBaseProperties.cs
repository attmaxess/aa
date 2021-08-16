using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WayBaseProperties : MonoBehaviour
{
    public Level level
    {
        get
        {
            if (_level == null) _level = FindObjectOfType<Level>();
            if (_level == null) _level = GetComponentInParent<Level>();
            return _level;
        }
    }
    Level _level;
    public LineController lineController
    {
        get
        {
            if (_lineController == null) this._lineController = GetComponent<LineController>();
            return this._lineController;
        }
    }
    LineController _lineController;
}
