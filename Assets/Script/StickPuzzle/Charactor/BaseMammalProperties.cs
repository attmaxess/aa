using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI.Extensions;

public class BaseMammalProperties : MonoBehaviour
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
    public BaseMammal mammal
    {
        get
        {
            if (_mammal == null) _mammal = GetComponent<BaseMammal>();
            return _mammal;
        }
    }
    BaseMammal _mammal;
    public virtual Attackable attackable
    {
        get
        {
            if (_attackable == null) _attackable = GetComponent<Attackable>();
            return this._attackable;
        }
    }
    Attackable _attackable;
    public SkeletonController skeletonController
    {
        get
        {
            if (this._skeletonController == null)
                this._skeletonController = GetComponent<SkeletonController>();
            return this._skeletonController;
        }
    }
    SkeletonController _skeletonController;
    public HeadTowardController head
    {
        get
        {
            if (this._headToward == null)
                this._headToward = GetComponent<HeadTowardController>();
            return this._headToward;
        }
    }
    HeadTowardController _headToward;
    public MovementHelperController movementhelperController
    {
        get
        {
            if (this._movementhelperController == null)
                this._movementhelperController = GetComponent<MovementHelperController>();
            return this._movementhelperController;
        }
    }
    MovementHelperController _movementhelperController;
    public List<SkeletonGraphic> listSkeleton
    {
        get { return this.skeletonController.GetActiveSkeletons(); }
    }
    public HealthController healthController
    {
        get
        {
            if (this._healthController == null)
                this._healthController = GetComponent<HealthController>();
            return this._healthController;
        }
    }
    HealthController _healthController;
    public Rigidbody2D rigidbody2
    {
        get
        {
            if (this._rigidbody2 == null) this._rigidbody2 = GetComponent<Rigidbody2D>();
            if (_rigidbody2 == null) _rigidbody2 = this.gameObject.AddComponent<Rigidbody2D>();
            return this._rigidbody2;
        }
    }
    Rigidbody2D _rigidbody2;
    public CapsuleCollider2D capsule
    {
        get
        {
            if (this._capsule == null)
                this._capsule = GetComponent<CapsuleCollider2D>();
            return this._capsule;
        }
    }
    CapsuleCollider2D _capsule;
    public CapsuleController capsuleController
    {
        get
        {
            if (this._capsuleController == null)
                this._capsuleController = GetComponent<CapsuleController>();
            return this._capsuleController;
        }
    }
    CapsuleController _capsuleController;

    public float Health
    {
        get
        {
            return healthController.Health;
        }
        set
        {
            healthController.Health = value;
        }
    }
    public SelectController selectController
    {
        get
        {
            if (this._selectController == null)
                this._selectController = GetComponent<SelectController>();
            return this._selectController;
        }
    }
    SelectController _selectController;
    public virtual MammalAIController mammalAI
    {
        get
        {
            if (this._AI == null)
                this._AI = GetComponent<MammalAIController>();
            return this._AI;
        }
    }
    MammalAIController _AI;
    public Enemy enemy
    {
        get
        {
            if (_enemy == null) _enemy = GetComponent<Enemy>();
            return this._enemy;
        }
        set
        {
            _enemy = value;
        }
    }
    Enemy _enemy = null;
    public Charactor charactor
    {
        get
        {
            if (_charactor == null) _charactor = GetComponent<Charactor>();
            return this._charactor;
        }
    }
    //[SerializeField]
    Charactor _charactor = null;
    public virtual Trap trap
    {
        get
        {
            if (_trap == null) _trap = GetComponent<Trap>();
            return this._trap;
        }
    }    
    protected Trap _trap = null;
    public CharactorOriginal original
    {
        get
        {
            if (_original == null) _original = GetComponent<CharactorOriginal>();
            return this._original;
        }
    }
    //[SerializeField]
    CharactorOriginal _original = null;
    public virtual TrapAIController trapAI
    {
        get
        {
            if (_trapAI == null) _trapAI = GetComponent<TrapAIController>();
            return this._trapAI;
        }
    }
    protected TrapAIController _trapAI = null;
    public virtual EnemyAIController enemyAI
    {
        get
        {
            if (_enemyAI == null) _enemyAI = GetComponent<EnemyAIController>();
            return this._enemyAI;
        }
    }
    protected EnemyAIController _enemyAI = null;
}
