using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public enum eLevelStatus { Idle, Busy, Lock }
public class LevelController : BaseLevelProperties
{
    public delegate void OnPostSetLevelStatus(eLevelStatus levelStatus);
    public OnPostSetLevelStatus onPostSetLevelStatus;

    public eLevelStatus levelStatus
    {
        get { return this._levelStatus; }
        set
        {
            this._levelStatus = value;
            if (onPostSetLevelStatus != null)
                onPostSetLevelStatus.Invoke(value);
        }
    }

    [SerializeField] eLevelStatus _levelStatus = eLevelStatus.Idle;

    public virtual void ResetAllTouchBy(eLevelStatus levelStatus) { }
    public virtual void SyncHolesFromScene() { }    
}
