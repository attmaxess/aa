using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoleBaseProperties : MonoBehaviour
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
    public Hole hole
    {
        get
        {
            if (_hole == null) _hole = GetComponent<Hole>();
            return _hole;
        }
    }
    Hole _hole;
    public FXGlow glow
    {
        get
        {
            if (_glow == null) _glow = GetComponent<FXGlow>();
            return _glow;
        }
    }
    FXGlow _glow;
    public WallBoxColliderController colliderController
    {
        get
        {
            if (_colliderController == null) _colliderController = GetComponent<WallBoxColliderController>();
            return _colliderController;
        }
    }
    WallBoxColliderController _colliderController;
    public Teleport teleport
    {
        get
        {
            if (_teleport == null) _teleport = level.GetComponentInChildren<Teleport>();
            if (_teleport == null) _teleport = FindObjectOfType<Teleport>();
            return _teleport;
        }
    }
    Teleport _teleport;
    public Appearance appearance
    {
        get
        {            
            if (_appearance == null) _appearance = GetComponent<Appearance>();
            return _appearance;
        }
    }
    Appearance _appearance;
}
