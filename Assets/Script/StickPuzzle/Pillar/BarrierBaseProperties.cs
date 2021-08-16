using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarrierBaseProperties : MonoBehaviour
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
    public Barrier barrier
    {
        get
        {
            if (_barrier == null) this._barrier = GetComponent<Barrier>();
            return this._barrier;
        }
    }
    Barrier _barrier;
    public BarrierRay ray
    {
        get
        {
            if (_ray == null) this._ray = GetComponent<BarrierRay>();
            return this._ray;
        }
    }
    BarrierRay _ray;
    public BarrierLineController lineController
    {
        get
        {
            if (_lineController == null) this._lineController = GetComponent<BarrierLineController>();
            return this._lineController;
        }
    }
    BarrierLineController _lineController;
    public BarrierIsometricController isometric
    {
        get
        {
            if (_isometric == null) this._isometric = GetComponent<BarrierIsometricController>();
            return this._isometric;
        }
    }
    BarrierIsometricController _isometric;
}
