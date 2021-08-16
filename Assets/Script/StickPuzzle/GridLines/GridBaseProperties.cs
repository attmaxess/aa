using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridBaseProperties : MonoBehaviour
{
    public Level level
    {
        get
        {
            if (this._level == null) this._level = GetComponentInParent<Level>();
            return this._level;
        }
    }
    Level _level;
    public LevelController levelController
    {
        get
        {
            if (this._levelController == null) this._levelController = GetComponent<LevelController>();
            return this._levelController;
        }
    }
    LevelController _levelController;
    public BigGrid bigGrid
    {
        get
        {
            if (this._bigGrid == null) _bigGrid = GetComponent<BigGrid>();
            return this._bigGrid;
        }
    }
    BigGrid _bigGrid;
    public GridTouchController gridTouch
    {
        get
        {
            if (this._gridTouch == null) _gridTouch = GetComponent<GridTouchController>();
            return this._gridTouch;
        }
    }
    GridTouchController _gridTouch;
    public GridLayerController layerController
    {
        get
        {
            if (this._layerController == null) this._layerController = GetComponent<GridLayerController>();
            return this._layerController;
        }
    }
    GridLayerController _layerController;
    public GridScalingController scalingController
    {
        get
        {
            if (this._scalingController == null) this._scalingController = GetComponent<GridScalingController>();
            return this._scalingController;
        }
    }
    GridScalingController _scalingController;
    public LevelGridHint hint
    {
        get
        {
            if (this._hint == null) this._hint = GetComponent<LevelGridHint>();
            return this._hint;
        }
    }
    LevelGridHint _hint;
}
