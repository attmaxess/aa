using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameResolution : Singleton<GameResolution>
{
    public Camera mainCam;
    public float ratio
    {
        get
        {
            if (_ratio == 0) DebugCameraPixelRect();
            return _ratio;
        }
    }
    [SerializeField] float _ratio;
    public eResolution eRes
    {
        get
        {
            if (_ratio == 0) DebugCameraPixelRect();
            return _eRes;
        }
    }
    [SerializeField] eResolution _eRes = eResolution.normal;
    //y/x

    private void Awake()
    {
        DebugCameraPixelRect();
    }
    [ContextMenu("DebugCameraPixelRect")]
    public void DebugCameraPixelRect()
    {
        //Debug.Log(mainCam.pixelRect);
        _ratio = mainCam.pixelRect.height / mainCam.pixelRect.width;
        if (_ratio < 1.5) _eRes = eResolution.squarescreen;
        else if (_ratio >= 1.5 && _ratio <= 2.2) _eRes = eResolution.normal;
        else if (_ratio > 2.2) _eRes = eResolution.longscreen;
    }
    /*
    [ContextMenu("GetListDependency")]
    public void GetListDependency()
    {
        dependencies = FindObjectsOfType<GameResolutionDependency>().ToList();
    }
    */
    /*
    [ContextMenu("GetResolution")]
    public void GetResolution()
    {
        Resolution currentRes = Screen.currentResolution;
        float width = Mathf.Min(currentRes.width, currentRes.height);
        float height = Mathf.Max(currentRes.width, currentRes.height);
        resolution = new Vector2(width, height);
        eRes = IsSquare() ?
            eResolution.squarescreen :
            eResolution.longscreen;
    }
    */

    /*
    public float GetRatio()
    {
        if (resolution != Vector2.zero)
            return resolution.x / resolution.y;
        else return 0;
    }
    public bool IsSquare()
    {
        return GetRatio() >= 0.65f;
        ///0.65 là lấy trung bình cộng của 3/4 (ipad, tablet) và 9/16 (iphone, android phone)
    }
    public void RefreshDependency(eResolution resolution)
    {
        eRes = resolution;
        foreach (var d in dependencies)
            HandleDependency(d.gameObject, d.resolution);
    }
    public void HideAllDependency()
    {
        //foreach (var d in dependencies)
        //d.gameObject.SetActive(false);
    }
    void HandleDependency(GameObject dependency, eResolution resolution)
    {
        //dependency.SetActive(eRes != resolution);        
    }
    */
}
