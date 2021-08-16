using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MammalAIBaseProperties : BaseMammalProperties
{   
    
    public AIController charactorAIController
    {
        get
        {
            if (_charactorAIController == null)
                _charactorAIController = level.charactor.GetComponentInChildren<AIController>();
            return this._charactorAIController;
        }
    }
    AIController _charactorAIController;
    public AIController aiControll
    {
        get
        {
            if (_aiControl == null)
                _aiControl = GetComponentInChildren<AIController>();
            return this._aiControl;
        }
    }
    AIController _aiControl;
    public Seeker seeker
    {
        get
        {
            if (_seeker == null) _seeker = GetComponent<Seeker>();
            if (_seeker == null) _seeker = this.gameObject.AddComponent<Seeker>();
            return this._seeker;
        }
    }
    Seeker _seeker;
    public IAstarAI astarAI
    {
        get
        {
            if (_astarAI == null) _astarAI = seeker.GetComponent<IAstarAI>();
            return this._astarAI;
        }
    }
    IAstarAI _astarAI;
    public AILerp lerp
    {
        get
        {
            if (_lerp == null) _lerp = GetComponent<AILerp>();
            if (_lerp == null) _lerp = this.gameObject.AddComponent<AILerp>();
            return this._lerp;
        }
    }
    AILerp _lerp;
    public AIDestinationSetter setter
    {
        get
        {
            if (_setter == null) _setter = GetComponent<AIDestinationSetter>();
            if (_setter == null) _setter = this.gameObject.AddComponent<AIDestinationSetter>();
            return this._setter;
        }
    }
    AIDestinationSetter _setter;
    public TargetMover mover
    {
        get
        {
            if (_mover == null) _mover = GetComponentInChildren<TargetMover>();
            return this._mover;
        }
    }
    TargetMover _mover;
    public ColliderDetection detection
    {
        get
        {
            if (_detection == null) _detection = GetComponent<ColliderDetection>();
            if (_detection == null) _detection = this.gameObject.AddComponent<ColliderDetection>();
            return this._detection;
        }
    }
    ColliderDetection _detection;
    public DangerZone dangerZone
    {
        get
        {
            if (_dangerZone == null) _dangerZone = GetComponentInChildren<DangerZone>();
            return this._dangerZone;
        }
    }
    DangerZone _dangerZone;    
    public CharactorAIController charactorAI
    {
        get
        {
            if (_charactorAI == null) _charactorAI = this as CharactorAIController;
            return this._charactorAI;
        }
    }
    CharactorAIController _charactorAI;
    public WallBox3DColliderController barrierCheck
    {
        get
        {
            if (_barrierCheck == null)
                _barrierCheck = GetComponentInChildren<WallBox3DColliderController>();
            return this._barrierCheck;
        }
    }
    WallBox3DColliderController _barrierCheck;
}
