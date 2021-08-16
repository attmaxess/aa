using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;

public class GameCameraController : Singleton<GameCameraController>
{
    public float duration = .3f;
    #region base properties
    Camera mainCam
    {
        get
        {
            if (_mainCam == null) _mainCam = Camera.main;
            return _mainCam;
        }
    }
    Camera _mainCam;
    Volume volume
    {
        get
        {
            if (_volume == null) _volume = mainCam.transform.GetComponent<Volume>();
            return _volume;
        }
    }
    Volume _volume;
    DepthOfField depth
    {
        get
        {
            if (_depth == null) GetDepth();
            return _depth;
        }
    }
    DepthOfField _depth;
    void GetDepth()
    {
        volume?.profile.TryGet(out _depth);
    }
    #endregion base properties
    [ContextMenu("Blur")]
    public void Blur()
    {
        if (depth == null) return;
        float currentFocal = depth.focalLength.value;
        DOTween.To(() => currentFocal,
            x =>  depth.focalLength.value = (float)x,
            140f,
            duration);
    }
    [ContextMenu("Clear")]
    public void Clear()
    {
        if (depth == null) return;
        float currentFocal = depth.focalLength.value;
        DOTween.To(() => currentFocal,
            x => depth.focalLength.value = (float)x,
            1f,
            duration);
    }
    public bool IsClear()
    {
        return depth.focalLength.value == 1f;
    }
}
