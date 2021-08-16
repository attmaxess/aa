using DG.Tweening;
using Pathfinding;
using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

public class BaseMammal : BaseMammalProperties
{
    public int totalCharactor;
    public int addedCharactorCount = 0;
    public Vector3 startScale;
    public bool isSnapAstar = true;

    public Coroutine coBalancing;
    const float minBalanceDistance = .05f;

    public bool HasAnimMove = true;
    //Canmove này dùng để hạn chế trap thực hiện anim di chuyển
    ///Để thực sự hạn chế sự di chuyển thì set speed = 0 ở AILerp.cs
    ///để vẫn call được code search path -> di chuyển NVC        
    public Hole hole
    {
        get
        {
            if (_hole == null) _hole = GetComponentInChildren<Hole>();
            if (_hole == null) _hole = FindObjectsOfType<Hole>().ToList()
                    .Find((x) => x.enemyAttackable != null && x.enemyAttackable.transform == this.transform);
            if (_hole == null) Debug.Log(this.transform.name + " không có hole ");
            return this._hole;
        }
        set { _hole = value; }
    }
    public Hole _hole = null;
   
    #region static calls
    public static BaseMammal GetBaseMammal(HealthController health)
    {
        return health.GetComponentInParent<BaseMammal>();
    }
    #endregion static calls

    #region unity life-circle
    public virtual void Awake()
    {
        if (mammalAI != null)
            mammalAI.onpostSyncAnimSpeed += SyncAnimSpeedWithMovement;
    }
    protected virtual void Start()
    {
        StartCoroutine(C_Start());
    }
    IEnumerator C_Start()
    {
        if (GameController.instance.currentLevel != null)
            yield return new WaitUntil(() => GameController.instance.DoneLoadLevel == true);

        startScale = skeletonController.GetScale();
    }
    #endregion unity life-circle

    #region services
    public void AnimTextHitTrap()
    {
        healthController.AnimTextHitTrap();
    }
    public bool UpgradeLevel(float addedHealth)
    {
        int oldCharactorCount = totalCharactor;
        switch (addedCharactorCount)
        {
            case 0:
                if (addedHealth >= 20)
                {
                    totalCharactor = Mathf.Min(3, totalCharactor + 1);
                    addedCharactorCount += 1;
                    SoundManager.instance.PlayAudioClip(SoundManager.instance.duplicate);
                }
                break;
            case 1:
                if (addedHealth >= 30)
                {
                    totalCharactor = Mathf.Min(3, totalCharactor + 1);
                    addedCharactorCount += 1;
                    SoundManager.instance.PlayAudioClip(SoundManager.instance.duplicate);
                }
                break;
            case 2:
                if (addedHealth >= 60)
                {
                    totalCharactor = Mathf.Min(3, totalCharactor + 1);
                    addedCharactorCount += 1;
                    if (addedCharactorCount < 3)
                    {
                        SoundManager.instance.PlayAudioClip(SoundManager.instance.duplicate);
                    }
                }
                break;
        }
        return oldCharactorCount != totalCharactor;
    }
    public void UpdateCharactorObj()
    {
        skeletonController.ToogleAllSkeleton(1, totalCharactor);
    }
    public void UpdateHealthTextPos()
    {
        TaskUtil.Delay(this, delegate
        {
            Vector3 pos;

            List<SkeletonGraphic> actives = skeletonController.actives;
            if (actives.Count > 0)
            {
                Transform first = actives[0].transform;
                Transform last = actives[actives.Count - 1].transform;

                pos.x = (first.position.x + last.position.x) / 2f;

                if (totalCharactor < 3)
                    pos.y = first.position.y + .8f;
                else
                    pos.y = first.position.y + 1f;

                pos.z = healthController.healthText.transform.position.z;

                healthController.healthText.transform.DOMove(pos, 0.05f);
            }
            //healthController.UpdateHealth(true, false);
        }, 0.1f);
    }
    public virtual void Fighting(int fightingCount)
    {
        if (level.useDebugLog)
            Debug.Log(transform.name + " Base Fighting");
    }
    public virtual void DoLose(Hole hole, BaseMammal killer = null)
    {
        if (hole != this.hole)
        {
            if (level.useDebugLog)
                Debug.Log("Wrong hole??? ");
            return;
        }
        DoLose(killer);
    }
    protected virtual void DoLose(BaseMammal killer = null)
    ///Hàm này chỉ được gọi bằng Hole.Dolose()
    ///KHÔNG được gọi trực tiếp
    {
        if (level.useDebugLog)
            Debug.Log(transform.name + " Base DoLose ");
    }
    public virtual void DoWin(BaseMammal helper = null)
    {
        if (level.useDebugLog)
            Debug.Log(transform.name + " Base DoWin ");

        SetTimeScale(1f);
    }
    public virtual void DoIdle()
    {
        if (level.useDebugLog)
            Debug.Log(transform.name + " Base DoIdle ");

        SetTimeScale(1f);
    }
    public bool IsCharactor()
    {
        return GetComponent<Charactor>() != null;
    }
    public virtual bool? IsAnimMoving()
    {
        if (listSkeleton.Count == 0) return null;
        if (listSkeleton[0].AnimationState == null) return null;
        if (listSkeleton[0].AnimationState.GetCurrent(0) == null) return false;
        return null;
    }
    public void DoAnimation(string anim, AudioClip audio = null)
    {
        skeletonController.GetActiveSkeletons().
            ForEach(item => item.AnimationState.SetAnimation(0, anim, true));

        if (audio != null)
            SoundManager.instance.PlayAudioClipCharactor(audio);
    }
    public void SetTimeScale(float scale)
    {
        skeletonController.SetTimeScale(scale);
    }
    private void SyncAnimSpeedWithMovement(float speed)
    ///Private because I want it is called only by AI
    {
        if (speed < 1) speed = 1;
        listSkeleton.ForEach(item => item.AnimationState.TimeScale = speed);
    }
    [ContextMenu("SnapToNearStarNode")]
    public void SnapToNearStarNode()
    {
        isSnapAstar = false;
        if (AstarPath.active == null)
        {
            isSnapAstar = true;
            return;
        }
        var constraint = NNConstraint.None;
        constraint.constrainWalkability = true;
        constraint.walkable = true;
        GraphNode node = AstarPath.active.GetNearest(transform.position, constraint).node;
        if (node == null)
        {
            isSnapAstar = true;
            return;
        }
        if (CanMoveSnap() && HasAnimMove)
        {
            head.Toward((Vector3)node.position);
            SetMoving();
            transform.DOMove((Vector3)node.position, .5f)
                    .SetEase(Ease.Linear)
                    .OnComplete(delegate
                    {
                        DoIdle();
                        isSnapAstar = true;
                        if (IsEnemy())
                            head.HeadTowardCharactor();
                        else if (IsCharactor())
                            head.HeadTowardNearest();
                    });
        }
    }
    public void Balance(BaseMammal target, bool moveTarget)
    {
        if (coBalancing != null)
        {
            StopCoroutine(coBalancing);
            coBalancing = null;
        }
        coBalancing = StartCoroutine(C_Balance(target, moveTarget));
    }
    IEnumerator C_Balance(BaseMammal target, bool moveTarget)
    {
        Debug.Log(this.transform.name + " balancing " + target.name);

        Vector3 midPosition = (this.transform.position + target.transform.position) / 2f;
        Transform leftTr = this.transform.position.x < target.transform.position.x ? this.transform : target.transform;
        Transform rightTr = leftTr == this.transform ? target.transform : transform;
        Vector3 leftBalance = new Vector3(midPosition.x - minBalanceDistance, midPosition.y, midPosition.z);
        Vector3 rightBalance = new Vector3(midPosition.x + minBalanceDistance, midPosition.y, midPosition.z);

        BalancingHelper.StaticBalance(leftTr, leftBalance);
        BalancingHelper.StaticBalance(rightTr, rightBalance);

        yield return new WaitUntil(() => BalancingHelper.StaticIsDoneBalance(leftTr) == true &&
        BalancingHelper.StaticIsDoneBalance(rightTr) == true);

        coBalancing = null;
        yield break;
    }
    public void DebugTimeScale()
    {
        Debug.Log(skeletonController.listSkeleton[0].timeScale);
    }
    [ContextMenu("HideHoleImage")]
    public void HideHoleImage()
    {
        if (hole != null)
            hole.holeImage.gameObject.SetActive(false);
    }
    [ContextMenu("ShowHoleImage")]
    public void ShowHoleImage()
    {
        if (hole != null)
            hole.holeImage.gameObject.SetActive(true);
    }
    public virtual bool IsEnemy()
    ///Dùng để kiểm tra tại EnemyAIController.cs hoặc những nơi gọi vào basemammal
    {
        return enemy != null && trap == null;
    }
    public virtual bool IsTrap()
    ///Dùng để kiểm tra tại EnemyAIController.cs hoặc những nơi gọi vào basemammal
    {
        return trap != null;
    }
    [ContextMenu("Show100")]
    public virtual void Show100()
    {
        if (Application.isPlaying)
        {
            skeletonController.GetActiveSkeletons().ForEach(item => item.DOFade(1, 0f));
            healthController.healthText.DOFade(1, 0f);
        }
        else
        {
            skeletonController.Show100();
            healthController.Show100();
        }
    }
    public void Ready()
    {
        if (capsule != null)
            capsule.enabled = true;
    }
    public void UnReady()
    {
        if (capsule != null)
            capsule.enabled = false;
    }
    public virtual void SetMoving() { }
    public virtual bool CanMoveSnap() { return false; }
    #endregion services
}
