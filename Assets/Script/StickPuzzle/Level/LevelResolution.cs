using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelResolution : MonoBehaviour
{
    public eResolution preferResolution = eResolution.longscreen;
    void OnEnable()
    {
        //GameResolution.instance.HideAllDependency();
    }
    private void Awake()
    {        
        //GameResolution.instance.RefreshDependency(this.preferResolution);
    }

    private void OnDisable()
    {
        try
        {
            //GameResolution.instance.HideAllDependency();
        }
        catch { }
    }
}
