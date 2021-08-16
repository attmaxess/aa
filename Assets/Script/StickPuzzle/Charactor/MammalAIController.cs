using DG.Tweening;
using Pathfinding;
using Spine.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI.Extensions;

public class MammalAIController : MammalAIBaseProperties
{
    public List<eMammalStatus> statuses = new List<eMammalStatus>() { eMammalStatus.Idle };

    [Space(20)]
    public Vector2 ZeroVelocity = new Vector2(.01f, .01f);
    public float ZeroVelocityAngular = .01f;

    public Coroutine coHandleMeet;
    public Coroutine coMoveToMeet;
    public Coroutine coBalancing;
    public Coroutine coMoveThru;
    public Coroutine coFightingAnimWithTrap;
    public Coroutine coJoin;
    public Coroutine coFightTrap;

    const float minBalanceY = 0.1f;
    [Tooltip("Khoảng cách khi đánh nhau")]
    const float minBalanceDistance = .4f;
    [ReadOnly] public Vector3 lastBalanceCenter;

    public Transform currentAiming
    {
        get { return _currentAiming; }
        set { this._currentAiming = value; OnpostSetAiming(value); }
    }
    [Space(20)]
    [ReadOnly] [SerializeField] Transform _currentAiming;
    private void OnpostSetAiming(Transform value)
    {
        if (value != null)
            mammal.head.Toward(value);
    }
    [ReadOnly] public Transform currentBalanceTr;
    [ReadOnly] public bool CanBalance = false;
    [ReadOnly] float SelfBalanceSpeed = 3f;
    public enum eSolidState { noneDefine, liquid, solid }
    [ReadOnly] public eSolidState esolidState = eSolidState.noneDefine;

    [Space(20)]
    //[HideInInspector] 
    [ReadOnly] public List<Trap> currentTraps = new List<Trap>();
    [HideInInspector] public MammalAIController currentMammalToMeet = null;
    [ReadOnly] public List<MammalAIController> mammalReachables = new List<MammalAIController>();
    [ReadOnly] public List<Transform> handlingMammal = new List<Transform>();

    public delegate void OnPostSyncSpeed(float speed);
    public OnPostSyncSpeed onpostSyncAnimSpeed;

    public delegate void OnPostDisable(Transform mammalAI = null);
    public OnPostDisable onpostDisable;

    private void OnDisable()
    {
        if (onpostDisable != null)
            onpostDisable.Invoke(transform);
    }
    [ContextMenu("MoveByAstar")]
    public virtual void MoveByAstar()
    {
        if (level.useDebugLog && mammal.HasAnimMove)
        {
            if (mammal.IsAnimMoving() == false)
                Debug.Log("MoveAstar MammalAI " + this.transform.name);
        }

        lerp.enabled = true;
        astarAI.canMove = true;
        astarAI.isStopped = false;

        mammal.movementhelperController.SetChargingSpeed();
    }
    [ContextMenu("StopAstar")]
    public void StopAstar()
    {
        if (IsAstarCompletelyStop()) return;

        if (level.useDebugLog)
        {
            Debug.Log("StopAstar MammalAI " + this.transform.name);
            PrintStatus();
        }

        try { StartCoroutine(C_StopAstar()); }
        catch { }
    }
    IEnumerator C_StopAstar()
    {
        astarAI.SetPath(null);
        lerp.enabled = false;
        astarAI.canMove = false;
        astarAI.isStopped = true;
        rigidbody2.velocity = Vector2.zero;
        rigidbody2.angularVelocity = 0;

        yield return new WaitUntil(() => IsAstarCompletelyStop() == true);

        transform.rotation = Quaternion.identity;
        mammal.movementhelperController.SetIdleSpeed();

        yield break;
    }
    public bool IsAstarCompletelyStop()
    {
        return rigidbody2.velocity.magnitude < ZeroVelocity.magnitude &&
            rigidbody2.angularVelocity < ZeroVelocityAngular &&
            lerp.enabled == false &&
            astarAI.canMove == false &&
            astarAI.isStopped == true;
    }
    public void Balance(MammalAIController target, int lastDirection)
    {
        if (coBalancing != null)
        {
            StopCoroutine(coBalancing);
            coBalancing = null;
        }
        coBalancing = StartCoroutine(C_Balance(target, lastDirection));
    }
    IEnumerator C_Balance(MammalAIController target, int lastDirection)
    {
        if (level.useDebugLog)
        {
            Debug.Log(this.transform.name + " balancing " + target.name);
        }

        Vector3 midPosition = (this.transform.position + target.transform.position) / 2f;
        lastBalanceCenter = midPosition;

        Transform leftTr = null;
        Transform rightTr = null;
        MammalAIController leftMammalAI = leftTr.GetComponent<MammalAIController>();
        MammalAIController rightmammalAI = rightTr.GetComponent<MammalAIController>();

        if (this.transform.position.x != target.transform.position.x)
        {
            leftTr = this.transform.position.x < target.transform.position.x ? this.transform : target.transform;
        }
        else
        {
            if (lastDirection == -1)
                leftTr = target.transform;
            else if (lastDirection == 1)
                leftTr = this.transform;
        }
        rightTr = leftTr == this.transform ? target.transform : transform;

        var constraint = NNConstraint.None;
        constraint.constrainWalkability = true;
        constraint.walkable = true;

        float leftOffset = minBalanceDistance;
        float rightOffset = minBalanceDistance;

        if (leftMammalAI != null && rightmammalAI != null)
        {
            leftOffset *= Mathf.Abs(leftMammalAI.skeletonController.listSkeleton[0].transform.localScale.x);
            rightOffset *= Mathf.Abs(rightmammalAI.skeletonController.listSkeleton[0].transform.localScale.x);
        }

        Vector3 leftBalance = Mathf.Abs(midPosition.x - leftTr.position.x) < leftOffset ?
            new Vector3(midPosition.x - leftOffset, midPosition.y, midPosition.z)
            : leftTr.position;
        GraphNode leftNode = AstarPath.active.GetNearest(leftBalance, constraint).node;

        Vector3 rightBalance = Mathf.Abs(rightTr.position.x - midPosition.x) < rightOffset ?
            new Vector3(midPosition.x + rightOffset, midPosition.y, midPosition.z)
            : rightTr.position;
        GraphNode rightNode = AstarPath.active.GetNearest(rightBalance, constraint).node;
        
        if (leftNode == null || rightNode == null)
        {
            coBalancing = null;
            yield break;
        }

        leftBalance = (Vector3)leftNode.position;
        rightBalance = (Vector3)rightNode.position;

        if (level.useDebugLog)
        {
            Debug.Log("Left : " + leftTr.name + " Right : " + rightTr.name);
        }

        if (leftTr.position.x > leftBalance.x)
            BalancingHelper.StaticBalance(leftTr, leftBalance);
        if (rightTr.position.x < rightBalance.x)
            BalancingHelper.StaticBalance(rightTr, rightBalance);

        yield return new WaitUntil(() => BalancingHelper.StaticIsDoneBalance(leftTr) == true &&
        BalancingHelper.StaticIsDoneBalance(rightTr) == true);

        coBalancing = null;
        yield break;
    }    
    public Flag NearestReachableFlag()
    {
        List<Flag> flags = level.GetComponentsInChildren<Flag>().ToList();
        List<Flag> reachableFlags = new List<Flag>();
        foreach (var flag in flags)
            if (level.IsReachable(transform, flag.transform))
                reachableFlags.Add(flag);

        if (reachableFlags.Count == 0) return null;
        reachableFlags.Sort((x, y) => (x.transform.position - this.transform.position).magnitude.
        CompareTo((y.transform.position - this.transform.position).magnitude));
        return reachableFlags[0];
    }
    public void SelfBalanceAfterFight()
    {
        if (coBalancing != null)
        {
            StopCoroutine(coBalancing);
            coBalancing = null;
        }
        try { coBalancing = StartCoroutine(C_SelfBalanceAfterFight()); }
        catch { }
    }
    IEnumerator C_SelfBalanceAfterFight()
    {
        while ((transform.position - lastBalanceCenter).magnitude > minBalanceY)
        {
            yield return new WaitForEndOfFrame();
            transform.position = Vector3.Lerp(transform.position, lastBalanceCenter, Time.deltaTime * SelfBalanceSpeed);
        }
        coBalancing = null;
    }
    public void MoveThru(ColliderDetection detection, Vector3 direction)
    {
        if (coMoveThru != null)
        {
            StopCoroutine(coMoveThru);
            coMoveThru = null;
        }
        try { coMoveThru = StartCoroutine(C_MoveThru(detection, direction)); }
        catch { }
    }
    IEnumerator C_MoveThru(ColliderDetection detection, Vector3 direction)
    ///moveTarget là move Enemy
    {
        Debug.Log("Moving thru " + this.transform.name + " - " + detection.transform.name);
        while (detection.IsContain(this.transform))
        {
            yield return new WaitForEndOfFrame();
            this.transform.position += direction.normalized;
        }
        coMoveThru = null;
    }    
    public virtual void OnMeet(ColliderDetection detection, Transform tr)
    ///Ở base thì xử lý nếu gặp đúng aiming cứ stop
    ///Gặp trap cũng stop
    {
        if (currentAiming != null && currentAiming == tr)
        {
            StopAstar();
        }

        MammalAIController trMammal = tr.GetComponent<MammalAIController>();
        if (trMammal != null) 
        {
            if (trMammal == currentMammalToMeet)
            {
                StopCurrentMoveToMeet();
                trMammal.StopCurrentMoveToMeet();

                RemoveMammalStatus(eMammalStatus.MoveToMeet);
                trMammal.RemoveMammalStatus(eMammalStatus.MoveToMeet);
            }            
        }
    }
    public virtual void AddOnce_MammalAI(Transform mammalAI = null)
    {
        if (mammalAI == null) return;
        if (!handlingMammal.Contains(mammalAI))
        {
            handlingMammal.Add(mammalAI);
            mammalAI.GetComponent<MammalAIController>().onpostDisable += RemoveHandling_MammalAI;
        }
    }
    public virtual void RemoveHandling_MammalAI(Transform mammalAI = null)
    {
        if (mammalAI == null) return;
        if (handlingMammal.Contains(mammalAI))
            handlingMammal.Remove(mammalAI);
    }
    public void AddOnce_Trap(Trap trap = null)
    {
        if (trap == null) return;
        if (!currentTraps.Contains(trap))
            currentTraps.Add(trap);
    }
    protected void UpdateCurrentTrapList(ColliderDetection detection)
    {
        currentTraps = new List<Trap>();
        for (int i = 0; i < detection.TrCount(); i++)
        {
            Transform tr = detection.GetTr(i);
            if (tr == null) continue;

            Trap trap = tr.GetComponent<Trap>();
            if (trap != null) AddOnce_Trap(trap);
        }
    }
    void AddOnceMammalStatus(eMammalStatus eMammalStatus)
    {
        if (!statuses.Contains(eMammalStatus))
            statuses.Add(eMammalStatus);
    }
    public void RemoveMammalStatus(eMammalStatus eMammalStatus)
    {
        if (statuses.Contains(eMammalStatus))
            statuses.Remove(eMammalStatus);

        if (statuses.Count == 0)
            SetStatus(eMammalStatus.Idle);
    }
    public void SetStatus(eMammalStatus eMammalStatus)
    {
        switch (eMammalStatus)
        {
            case eMammalStatus.Idle:
                statuses = new List<eMammalStatus>() { eMammalStatus.Idle };
                break;
            default:
                AddOnceMammalStatus(eMammalStatus);
                RemoveMammalStatus(eMammalStatus.Idle);
                break;
        }
    }
    public bool IsIdle()
    {
        return (statuses.Count == 1 && this.statuses.Contains(eMammalStatus.Idle));
    }
    public bool IsMoving()
    {
        return (this.statuses.Contains(eMammalStatus.MoveToMeet));
    }
    public bool IsStatus(eMammalStatus status)
    {
        return (statuses.Count == 1 && this.statuses.Contains(status));
    }
    public bool IsNotStatus(eMammalStatus eMammalStatus)
    {
        return !this.statuses.Contains(eMammalStatus);
    }
    public bool IsBusyFighting()
    {
        return statuses.Contains(eMammalStatus.FightCharactor) ||
            statuses.Contains(eMammalStatus.FightEnemy) ||
            statuses.Contains(eMammalStatus.FightMeet) ||
            statuses.Contains(eMammalStatus.FightTrap);
    }
    public bool IsBusyJoining()
    {
        return statuses.Contains(eMammalStatus.Join);
    }
    public bool IsFightingCharactor()
    {
        return this.statuses.Contains(eMammalStatus.FightCharactor);
    }
    public bool IsLeft(Transform tr)
    {
        return transform.position.x < tr.position.x;
    }
    public bool MetAI(MammalAIController mammal)
    {
        return detection.IsContain(mammal.transform);
    }
    public bool Met(Transform tr)
    {
        return detection.IsContain(tr);
    }
    public void MoveToMeet(MammalAIController other)
    {
        if (!this.gameObject.activeSelf) return;
        if (!other.gameObject.activeSelf) return;
        if (!mammal.CanMoveSnap()) return;

        if (level.useDebugLog && mammal.IsAnimMoving() != true)
            Debug.Log("MoveToMeet Mammal " + this.transform.name + " - " + other.transform.name);

        StopCurrentMoveToMeet();

        try { coMoveToMeet = StartCoroutine(C_MoveToMeet(other)); }
        catch { }
    }

    IEnumerator C_MoveToMeet(MammalAIController other)
    {
        if (statuses.Contains(eMammalStatus.MoveToMeet))
            yield break;

        if (level.isWin == true || level.isLose == true)
            yield break;

        this.currentMammalToMeet = other;

        SetStatus(eMammalStatus.MoveToMeet);
        if (MetAI(other))
        {
            RemoveMammalStatus(eMammalStatus.MoveToMeet);
            yield break;
        }
        else
        {
            while (!MetAI(other))
            {
                mammal.head.Toward(other.transform);

                if (!other.IsBusyFighting())
                    other.mammal.head.Toward(this.transform);

                yield return new WaitForEndOfFrame();
                setter.target = other.transform;
                astarAI.SearchPath();
                yield return new WaitForEndOfFrame();
                float randomTime = UnityEngine.Random.Range(0.3f, 0.5f);
                yield return new WaitForSeconds(randomTime);
                MoveByAstar();

                if (!other.gameObject.activeSelf)
                {
                    mammal.DoIdle();
                    RemoveMammalStatus(eMammalStatus.MoveToMeet);
                    yield break;
                }
                if (MetAI(other))
                {
                    RemoveMammalStatus(eMammalStatus.MoveToMeet);
                    yield break;
                }
                if (level.isWin == true || level.isLose == true)
                {
                    RemoveMammalStatus(eMammalStatus.MoveToMeet);
                    yield break;
                }

            }
        }

        RemoveMammalStatus(eMammalStatus.MoveToMeet);
        yield break;
    }
    public void MoveToMeet(Transform other)
    {
        if (!this.gameObject.activeSelf) return;
        if (!other.gameObject.activeSelf) return;

        if (level.useDebugLog && mammal.IsAnimMoving() != true)
            Debug.Log("MoveToMeet Mammal " + this.transform.name + " - " + other.transform.name);

        StopCurrentMoveToMeet();

        try { coMoveToMeet = StartCoroutine(C_MoveToMeet(other)); }
        catch { }
    }
    IEnumerator C_MoveToMeet(Transform other)
    {
        if (mammal.listSkeleton.Count == 0)
        {
            if (level.useDebugLog)
                Debug.Log("Dau con ai dau");
            yield break;
        }

        if (statuses.Contains(eMammalStatus.MoveToMeet))
            yield break;

        this.currentMammalToMeet = other.GetComponent<MammalAIController>();

        SetStatus(eMammalStatus.MoveToMeet);
        if (Met(other))
        {
            RemoveMammalStatus(eMammalStatus.MoveToMeet);
            yield break;
        }
        else
        {
            while (!Met(other))
            {
                mammal.head.Toward(other.transform);
                yield return new WaitForEndOfFrame();
                setter.target = other.transform;
                astarAI.SearchPath();
                yield return new WaitForEndOfFrame();
                float randomTime = UnityEngine.Random.Range(0.3f, 0.5f);
                yield return new WaitForSeconds(randomTime);

                if (Met(other))
                {
                    RemoveMammalStatus(eMammalStatus.MoveToMeet);
                    yield break;
                }
                else
                {
                    MoveByAstar();
                }
            }
        }

        RemoveMammalStatus(eMammalStatus.MoveToMeet);
        yield break;
    }
    public void StopCurrentMoveToMeet()
    {
        if (coMoveToMeet != null)
        {
            StopCoroutine(coMoveToMeet);
            coMoveToMeet = null;
        }
        RemoveMammalStatus(eMammalStatus.MoveToMeet);
    }
    public void AddReachableBy(List<MammalAIController> mammals)
    {
        this.mammalReachables = new List<MammalAIController>();
        foreach (MammalAIController mammal in mammals)
        {
            if (mammal.transform == this.transform) continue;
            if (level.IsReachable(this.seeker, mammal.mover))
                AddOnce_MammalReachable(mammal);
        }
    }
    public void AddOnce_MammalReachable(MammalAIController mammal)
    {
        if (mammal.transform == this.transform) return;
        if (!this.mammalReachables.Contains(mammal))
            mammalReachables.Add(mammal);
    }
    public void RemoveMammalReachable(MammalAIController mammal)
    {
        if (this.mammalReachables.Contains(mammal))
            mammalReachables.Remove(mammal);
    }
    public MammalAIController NearestReachableMammal()
    {
        if (mammalReachables.Count == 0) return null;

        this.mammalReachables.Sort((x, y) => (x.transform.position - this.transform.position).magnitude.
        CompareTo((y.transform.position - this.transform.position).magnitude));

        return mammalReachables[0];
    }
    public MammalAIController NearestMammal()
    {
        List<MammalAIController> mammals = level.GetComponentsInChildren<MammalAIController>().ToList();
        MammalAIController thisMammal = mammals.Find((x) => x == this);
        if (thisMammal != null) mammals.Remove(thisMammal);

        mammals.Sort((x, y) => (x.transform.position - this.transform.position).magnitude.
        CompareTo((y.transform.position - this.transform.position).magnitude));

        return mammals[0];
    }

    public void SortListReachMammal()
    {
        this.mammalReachables.Sort((x, y) => (x.transform.position - this.transform.position).magnitude.
        CompareTo((y.transform.position - this.transform.position).magnitude));
    }

    public void FightTrap(BaseMammal mammal, Trap trap, bool effectHealth = true)
    {
        if (coFightTrap != null)
        {
            StopCoroutine(coFightTrap);
            coFightTrap = null;
        }
        try { coFightTrap = StartCoroutine(C_FightTrap(mammal, trap, effectHealth)); }
        catch { }
    }
    public IEnumerator C_FightTrap(BaseMammal mammal, Trap trap, bool effectHealth = true)
    {
        SetStatus(eMammalStatus.FightTrap);
        MammalAIController trapAI = trap.GetComponent<MammalAIController>();

        if (trap.isCollision)
        {
            float damage = 0;

            if (trap.requireToFight == true)
            {
                iTrapAttack trapAttack = trap.GetComponent<iTrapAttack>();
                if (trapAttack != null)
                    damage = trapAttack.AttackHealth(mammal.Health);

                mammal.Fighting(2);
                trap.Attack();

                TaskUtil.Delay(this, delegate
                {
                    mammal.AnimTextHitTrap();
                }, 0.5f);

                int fightingCount = 2;
                float factor = fightingCount == 4 ? SectionSettings.fightingTime / 1.5f : SectionSettings.fightingTime;
                int currentFightingCount = 0;

                AnimFightingTrap();

                while (currentFightingCount < fightingCount)
                {
                    yield return new WaitForSeconds(factor);
                    if (effectHealth) mammal.Health -= damage;
                    currentFightingCount += 1;
                }

                yield return new WaitUntil(() => coFightingAnimWithTrap == null);

                trap.hole.DoLose(mammal, false);
                trap.isCollision = false;
            }
            else
            {
                mammal.DoWin();
                trap.hole.DoLose(mammal, false);
                trap.isCollision = false;


                damage = trap.AttackHealth(mammal.Health);
                bool isHealthIncrease = false;
                bool isHealthReduce = false;

                if (effectHealth)
                {
                    isHealthIncrease = damage > mammal.Health;
                    isHealthReduce = damage < mammal.Health;
                    mammal.Health = damage;
                }

                if (isHealthIncrease)
                {
                    GameController.instance.PlaceMergeAnim(charactor.gameObject);
                    charactor.VictoryAnimWhenMerge();
                    yield return new WaitForSeconds(0.5f);
                }

                if (isHealthReduce)
                {
                    List<GameObject> listObj = new List<GameObject>();
                    charactor.skeletonController.actives.ForEach(item => listObj.Add(item.gameObject));
                    GameController.instance.PlaceHitAnim(true, listObj);
                    charactor.AnimTextHitTrap();
                    yield return new WaitForSeconds(0.5f);
                }
            }

            if (trapAI != null)
                RemoveMammalReachable(trapAI);

            if (charactorAI != null)
            {
                if (level.isWin)
                ///Cần xét mammal này là charactor hay enemy
                ///Cứu được công chúa hoặc lấy được kho báu
                {
                    ForceWinGame();

                    level.AllEnemyLoseAnim();
                    level.charactor.VictoryAnim();
                }
                else if (mammal.Health == 0)
                {
                    level.isLose = true;

                    Vector3 p = mammal.transform.position;
                    p.x -= 0.5f;

                    level.dieSkeleton.transform.position = p;
                    level.dieSkeleton.AnimationState.SetAnimation(0, "die_enemy", false);

                    mammal.transform.DOKill();
                    mammal.hole.DoLose(mammal, false);

                    TaskUtil.Delay(this, delegate
                    {
                        GameController.instance.DoLose();
                    }, 0.5f);
                }
                else
                {
                    mammal.DoIdle();
                }
            }
            else if (enemyAI != null)
            {
                if (mammal.Health > 0)
                {
                    if (level.IsReachable(enemyAI.seeker, level.charactorAI.mover))
                        enemyAI.MoveToMeet(level.charactorAI);
                }
            }
        }

        RemoveMammalReachable(trap.mammalAI);

        yield return new WaitUntil(() => coFightingAnimWithTrap == null);
        RemoveMammalStatus(eMammalStatus.FightTrap);
        coFightTrap = null;

        yield break;
    }
    public void ForceLoseGame()
    {
        try { StartCoroutine(C_ForceLoseGame()); }
        catch { }
    }
    IEnumerator C_ForceLoseGame()
    {
        level.isLose = true;
        yield return new WaitForSeconds(0.5f);
        GameController.instance.DoLose();
    }
    public void ForceWinGame()
    {
        try { StartCoroutine(C_ForceWinGame()); } catch { }
    }
    IEnumerator C_ForceWinGame()
    {
        level.isWin = false;
        yield return new WaitForSeconds(0.5f);
        GameController.instance.DoWin();
        level.isWin = true;
    }
    public void Refresh()
    {
        SetStatus(eMammalStatus.Idle);
        detection.Clear();
    }
    // anim fighting with trap
    public void AnimFightingTrap()
    {
        if (coFightingAnimWithTrap != null)
        {
            StopCoroutine(coFightingAnimWithTrap);
            coFightingAnimWithTrap = null;
        }
        try { coFightingAnimWithTrap = StartCoroutine(IEFightingAnimTrap()); }
        catch { }

        TaskUtil.Delay(this, delegate
        {
            SoundManager.instance.PlayAudioClipEnemy(SoundManager.instance.trapHit);
            TaskUtil.Delay(this, delegate
            {
                SoundManager.instance.PlayAudioClipEnemy(SoundManager.instance.trapHit);
            }, 0.75f);
        }, 0.5f);
    }
    IEnumerator IEFightingAnimTrap()
    {
        List<GameObject> listObj = new List<GameObject>();

        yield return new WaitForSeconds(0.6f);

        mammal.listSkeleton.ForEach(item => listObj.Add(item.gameObject));
        GameController.instance.PlaceHitAnim(true, listObj);

        yield return new WaitForSeconds(0.6f);

        GameController.instance.PlaceHitAnim(true, listObj);

        yield return new WaitForEndOfFrame();
        coFightingAnimWithTrap = null;
    }
    public virtual void StopJoin()
    {
        if (coJoin != null)
        {
            StopCoroutine(coJoin);
            coJoin = null;
        }
    }
    public virtual void StopMoveToMeet()
    {
        if (coMoveToMeet != null)
        {
            StopCoroutine(coMoveToMeet);
            coMoveToMeet = null;
        }
    }
    public virtual void StopBalancing()
    {
        if (coBalancing != null)
        {
            StopCoroutine(coBalancing);
            coBalancing = null;
        }
    }
    public virtual void StopMoveThru()
    {
        if (coMoveThru != null)
        {
            StopCoroutine(coMoveThru);
            coMoveThru = null;
        }
    }
    public virtual void StopFightingAnimWithTrap()
    {
        if (coFightingAnimWithTrap != null)
        {
            StopCoroutine(coFightingAnimWithTrap);
            coFightingAnimWithTrap = null;
        }
    }
    public float GetMovementSpeed()
    {
        return lerp.speed;
    }
    /// <summary>
    /// Set speed for lerp then set speed for skeleton animation (time scale)
    /// </summary>
    /// <param name="speedRef"></param>
    public void SetMovementReference(MovementHelperController.SpeedReference speedRef)
    {
        lerp.speed = speedRef.lerpSpeed;

        if (onpostSyncAnimSpeed != null)
            onpostSyncAnimSpeed.Invoke(speedRef.animSpeed);
    }
    [ContextMenu("Liquify")]
    public void Liquify()
    ///Đi xuyên được
    {
        //mammal.capsule.isTrigger = true;
        //esolidState = eSolidState.liquid;
    }
    [ContextMenu("Solidify")]
    public void Solidify()
    ///Không thể đi xuyên
    {
        //mammal.capsule.isTrigger = false;
        //esolidState = eSolidState.solid;
    }
    IEnumerator C_SnapToNearStarNode(Int3 position)
    {
        while ((transform.position - (Vector3)position).magnitude > minBalanceY)
        {
            yield return new WaitForEndOfFrame();
            transform.position = Vector3.Lerp(transform.position, (Vector3)position, Time.deltaTime * SelfBalanceSpeed);
        }

        yield break;
    }
    public bool IsContainAnyReachable()
    {
        return this.mammalReachables.Find((x) => x.gameObject.activeSelf == true) != null;
    }
    void PrintStatus()
    {
        if (!level.useDebugLog) return;

        string print = string.Empty;
        for (int i = 0; i < statuses.Count; i++)
        {
            print += statuses[i].ToString() + " ";
        }
        Debug.Log(transform.name + " current statuses " + print);
    }
    public bool IsLastTarget()
    ///Last tartget là công chúa hoặc kho báu
    {
        if (mammal.attackable == null) return false;
        if (mammal.attackable == null) return false;
        if (mammal.attackable.princess != null) return true;
        if (mammal.attackable.trap != null && attackable.trap.type == eTrapType.Khobau) return true;
        return false;
    }
    public bool IsTrap()
    {
        return trap != null;
    }
    public bool IsEnemy()
    {
        return enemy != null;
    }
    public bool IsKhoBau()
    {
        return trap != null && GetComponent<KhoBauTrap>() != null;
    }
    public bool IsCanGat()
    {
        return trap != null && GetComponent<CanGatTrap>() != null;
    }
    public bool CanPokeToFindCharactor()
    {
        if (enemyAI != null) return true;
        if (trapAI != null && !trapAI.trap.isDefuse) return true;
        return false;
    }
}
