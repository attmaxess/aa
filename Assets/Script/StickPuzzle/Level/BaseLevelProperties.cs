using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseLevelProperties : MonoBehaviour
{
    public Level level
    {
        get
        {
            if (_level == null) _level = GetComponent<Level>();
            if (_level == null) _level = GetComponentInParent(typeof(Level)) as Level;
            if (_level == null) _level = FindObjectOfType<Level>();
            return _level;
        }
    }
    Level _level;
    public LevelController levelController
    {
        get
        {
            if (_levelController == null) _levelController = GetComponentInChildren<LevelController>();
            if (_levelController == null) _levelController = FindObjectOfType<LevelController>();
            return _levelController;
        }
    }
    LevelController _levelController;
    public LevelLayerController layerController
    {
        get
        {
            if (_layerController == null) _layerController = GetComponent<LevelLayerController>();
            if (_layerController == null) _layerController = FindObjectOfType<LevelLayerController>();
            if (_layerController == null) _layerController = gameObject.AddComponent<LevelLayerController>();
            return _layerController;
        }
    }
    LevelLayerController _layerController;
    public LevelPrepareController prepareController
    {
        get
        {
            if (_prepareController == null) _prepareController = GetComponent<LevelPrepareController>();
            if (_prepareController == null) _prepareController = FindObjectOfType<LevelPrepareController>();
            if (_prepareController == null) _prepareController = gameObject.AddComponent<LevelPrepareController>();
            return _prepareController;
        }
    }
    LevelPrepareController _prepareController;
    public LevelHint levelHint
    {
        get
        {
            if (_levelHint == null) _levelHint = GetComponentInChildren<LevelHint>();
            return _levelHint;
        }
    }
    LevelHint _levelHint;
    public CanvasGroup canvasGroup
    {
        get
        {
            if (_canvasGroup == null) _canvasGroup = GetComponent<CanvasGroup>();
            return _canvasGroup;
        }
    }
    CanvasGroup _canvasGroup;
    public Transform insideAstar
    {
        get
        {
            if (_insideAstar == null)
                _insideAstar = GetComponentInChildren<AstarPath>()?.transform;
            return _insideAstar;
        }
        set
        {
            _insideAstar = value;
        }
    }
    Transform _insideAstar;
}
