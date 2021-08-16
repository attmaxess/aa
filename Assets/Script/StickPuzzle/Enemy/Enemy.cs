using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Spine.Unity;
using Spine;
using DG.Tweening;
using System.Linq;
using UnityEngine.UI.Extensions;

public class Enemy : Attackable
{
    public WeaponType weaponType;
    [SerializeField] List<Canvas> listCanvas;

    [Space(10)]
    public bool hasDangerZone;
    public bool isCollision;
    public List<Transform> listDangerRaycastPoints;
    public LayerMask playerLayer;

    protected Vector3 originalLocalPos;
    protected Vector3 originalLocalScale;

    protected bool isWin;
    protected bool isLose;

    protected int fightingAnimCount;

    protected int fightingCount;

    [Header("Enemy Moving")]
    [SerializeField] List<Transform> movingPoints = new List<Transform>();
    [ReadOnly] public Transform currentMovingPoint;

    public delegate void OnPostAIMeetTarget(Enemy enemy, ColliderDetection seekerDetection);
    public OnPostAIMeetTarget postMeetHoleTarget;

    protected Coroutine coShowEmoji;
    protected Coroutine coRotate;
    protected Coroutine coUpdateDangerZone;
    protected Coroutine coUpdateMoving;

    public bool fighting;

    protected override void Start()
    {
        movingPoints.RemoveAll((x) => x == null);
        base.Start();
        OnStart();
    }
    private void Update()
    {
        OnUpdate();
    }
    protected virtual void OnUpdate()
    {
        if (hasDangerZone && coUpdateMoving != null)
        {
            for (int i = 0; i <= listDangerRaycastPoints.Count - 1; i++)
            {
                RaycastHit2D hit;
                if (i != listDangerRaycastPoints.Count - 1)
                    hit = Physics2D.Linecast(listDangerRaycastPoints[i].position,
                        listDangerRaycastPoints[i + 1].position,
                        playerLayer);
                else
                    hit = Physics2D.Linecast(listDangerRaycastPoints[i].position,
                        listDangerRaycastPoints[0].position,
                        playerLayer);

                if (hit.collider != null
                    && hit.collider.gameObject.tag.Equals("Player")
                    && GameController.instance.currentLevel.charactor.targetHole != null
                    && GameController.instance.currentLevel.charactor.targetHole.enemyAttackable != this)
                {
                    isCollision = true;
                    GameController.instance.currentLevel.EnemyAttackPlayer(
                        this,
                        hole,
                        level.charactor.targetHole);

                    StopCoroutine(coUpdateMoving);
                    coUpdateMoving = null;

                    transform.DOKill();

                    i = listDangerRaycastPoints.Count;
                }
            }
        }
    }
    protected virtual void OnStart()
    {
        if (movingPoints.Count > 0)
        {
            coUpdateMoving = StartCoroutine(IEUpdateMoving());
        }

        coShowEmoji = StartCoroutine(IEShowHideEmoji());

        if (GetType().Equals(typeof(Boss)))
        {
            Boss boss = (Boss)this;
            if (movingPoints.Count == 0)
            {
                coRotate = StartCoroutine(IERotate());
            }
        }
    }
    public void StopRotate()
    {
        if (coRotate != null)
        {
            StopCoroutine(coRotate);
            coRotate = null;
        }
    }
    public virtual bool IsMoving()
    ///Enemy có danger zone, ie : boss, enemy to
    {
        return (hasDangerZone && coUpdateMoving != null);
    }
    IEnumerator IERotate()
    {
        yield return new WaitUntil(() => level.DoneStartLevel == true);
        while (!fighting)
        {
            yield return new WaitForSeconds(8f);
            try { head.ChangeDirection(); }
            catch { }
            yield return new WaitForEndOfFrame();
        }
    }
    IEnumerator IEShowHideEmoji()
    {
        yield return new WaitUntil(() => level.DoneStartLevel == true);
        yield return new WaitForSeconds(Random.Range(4f, 6f));
        while (!fighting)
        {
            try { GameController.instance.currentLevel.ShowEmoji(this); }
            catch { }
            yield return new WaitForSeconds(7.5f);
        }
    }
    protected IEnumerator IEUpdateMoving()
    {
        yield return new WaitUntil(() => level.DoneStartLevel == true);
        int index = 0;

        while (index < movingPoints.Count && !isWin && !isLose)
        {
            Transform trans = movingPoints[index];

            float dis = trans.position.x - transform.position.x;

            float distance = Vector2.Distance(transform.position, trans.position);

            SetMoving();

            List<Vector3> pathPoints = new List<Vector3>();

            pathPoints.Add(transform.position);

            pathPoints.Add(trans.position);

            transform.DOMove(pathPoints[pathPoints.Count - 1], distance / 0.25f)
                .SetEase(Ease.Linear);

            head.Toward(trans);
            //Debug.Log(transform.name + "-" + head.Direction());

            index += 1;
            if (index == movingPoints.Count)
                index = 0;

            yield return new WaitForSeconds(distance / 0.25f);
        }
    }
    public void StopDangerZone()
    {
        if (hasDangerZone && !isCollision && coUpdateMoving != null)
        {
            StopCoroutine(coUpdateMoving);
            coUpdateMoving = null;

            transform.DOKill();
        }
    }
    public void ChangeDirection(bool isLeft)
    {
        skeletonController.GetActiveSkeletons().ForEach(item =>
        {
            Vector3 v = item.transform.localScale;
            if (GetType().Equals(typeof(Enemy)))
            {
                v.x = isLeft ? -Mathf.Abs(v.x) : Mathf.Abs(v.x);
            }
            else
            {
                v.x = isLeft ? Mathf.Abs(v.x) : -Mathf.Abs(v.x);
            }
            item.transform.localScale = v;
        });
    }
    public void ChangeDirection(Charactor charactor)
    {
        bool isLeft = charactor.skeletonController.listSkeleton[0].transform.localScale.x > 0;
        ChangeDirection(isLeft);
    }
    public void HideHealthText()
    {
        healthController.healthText.gameObject.SetActive(false);
    }
    public override void Fighting(int fightingCount)
    {
        this.fightingCount = fightingCount;
        fightingAnimCount = 1;
        fighting = true;

        if (coRotate != null)
        {
            StopCoroutine(coRotate);
            coRotate = null;
        }

        GameController.instance.currentLevel.HideEmoji(this);
        if (coShowEmoji != null)
        {
            StopCoroutine(coShowEmoji);
            coShowEmoji = null;
        }

        string animationName = weaponType == WeaponType.Shield ? "red_gethit" : "red_attack";

        List<SkeletonGraphic> activeSkeletons = skeletonController.GetActiveSkeletons();
        //if (fightingCount == 4)
        //    activeSkeletons.ForEach((item) => item.AnimationState.TimeScale = 1.5f);
        //else
        //    activeSkeletons.ForEach((item) => item.AnimationState.TimeScale = 1f);

        for (int i = 0; i < activeSkeletons.Count; i++)
        {
            if (i == 0)
            {
                activeSkeletons[i].AnimationState.Complete -= OnAnimationComplete;
                activeSkeletons[i].AnimationState.Complete += OnAnimationComplete;
            }

            activeSkeletons[i].AnimationState.SetAnimation(0, animationName, false);
            TaskUtil.Delay(this, delegate
            {
                switch (weaponType)
                {
                    case WeaponType.Sword:
                        SoundManager.instance.PlayAudioClipEnemy(SoundManager.instance.swordEn);
                        break;
                    case WeaponType.Archery:
                        SoundManager.instance.PlayAudioClipEnemy(SoundManager.instance.archeryEn);
                        break;
                    case WeaponType.Spear:
                        SoundManager.instance.PlayAudioClipEnemy(SoundManager.instance.spearEn);
                        break;
                    case WeaponType.Mace:
                        SoundManager.instance.PlayAudioClipEnemy(SoundManager.instance.maceEn);
                        break;
                    case WeaponType.Shield:
                        SoundManager.instance.PlayAudioClipEnemy(SoundManager.instance.shieldEn);
                        break;
                    case WeaponType.SwordShield:
                        SoundManager.instance.PlayAudioClipEnemy(SoundManager.instance.swordShieldEn);
                        break;
                    default:
                        break;
                }
            }, 0.35f);
        }

        //if (hasDangerZone)
        //{
        //    dangerZone.SetActive(false);
        //}
    }
    private void OnAnimationComplete(TrackEntry trackEntry)
    {
        if (GameController.instance.currentLevel.charactor.Health == 0)
        {
            GameController.instance.currentLevel.StopEnemyAttack(this, GameController.instance.currentLevel.charactor.targetHole);
            return;
        }
        string animationName = weaponType == WeaponType.Shield ? "red_gethit" : "red_attack";
        if (trackEntry.Animation.ToString().Equals(animationName) && fightingAnimCount < this.fightingCount)
        {
            fightingAnimCount += 1;
            List<SkeletonGraphic> activeSkeletons = skeletonController.GetActiveSkeletons();
            for (int i = 0; i < activeSkeletons.Count; i++)
            {
                activeSkeletons[i].AnimationState.SetAnimation(0, animationName, false);
            }
            TaskUtil.Delay(this, delegate
            {
                if (GameController.instance.currentLevel.charactor.Health == 0)
                {
                    return;
                }

                switch (weaponType)
                {
                    case WeaponType.Sword:
                        SoundManager.instance.PlayAudioClipEnemy(SoundManager.instance.swordEn);
                        break;
                    case WeaponType.Archery:
                        SoundManager.instance.PlayAudioClipEnemy(SoundManager.instance.archeryEn);
                        break;
                    case WeaponType.Spear:
                        SoundManager.instance.PlayAudioClipEnemy(SoundManager.instance.spearEn);
                        break;
                    case WeaponType.Mace:
                        SoundManager.instance.PlayAudioClipEnemy(SoundManager.instance.maceEn);
                        break;
                    case WeaponType.Shield:
                        SoundManager.instance.PlayAudioClipEnemy(SoundManager.instance.shieldEn);
                        break;
                    case WeaponType.SwordShield:
                        SoundManager.instance.PlayAudioClipEnemy(SoundManager.instance.swordShieldEn);
                        break;
                    default:
                        break;
                }
            }, 0.35f);
        }
    }
    public override void SetMoving()
    {
        base.SetMoving();
        if (IsAnimMoving() != false) return;
        if (HasAnimMove)
        {
            if (IsNormalEnemy())
                DoAnimation("red_move");
        }
    }
    public override bool CanMoveSnap() { return true; }
    public override bool? IsAnimMoving()
    {
        if (listSkeleton.Count == 0) return null;
        if (listSkeleton[0].AnimationState == null) return null;
        if (listSkeleton[0].AnimationState.GetCurrent(0) == null) return false;
        return listSkeleton[0].AnimationState.GetCurrent(0).Animation.ToString().Equals("red_move");
    }
    protected override void DoLose(BaseMammal killer = null)
    {
        if (isLose) return;
        base.DoLose(killer);
        if (level.useDebugLog) Debug.Log(transform.name + " Enemy.cs Dolose by" + killer?.transform.name);
        isLose = true;

        if (killer.transform == this.transform)
        {
            gameObject.SetActive(false);
        }
        else if (IsEnemy())
        ///Enemy thuần túy kill bởi charactor
        {
            gameObject.SetActive(false);
        }
        else if (enemy.princess != null)
        ///Công chúa kill bởi Charactor thì ko biến mất
        ///Kill bởi boss thì biến mất
        {
            gameObject.SetActive(killer.IsCharactor());
        }

        if (boss != null)
            SoundManager.instance.PlayAudioClipBoss(SoundManager.instance.bossDie);
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        for (int i = 0; i <= listDangerRaycastPoints.Count - 1; i++)
        {
            if (i != listDangerRaycastPoints.Count - 1)
                Gizmos.DrawLine(listDangerRaycastPoints[i].position, listDangerRaycastPoints[i + 1].position);
            else
                Gizmos.DrawLine(listDangerRaycastPoints[i].position, listDangerRaycastPoints[0].position);
        }
    }
    public override void DoIdle()
    {
        DoAnimation("red_idle");
    }
    public override void DoWin(BaseMammal helper = null)
    {
        base.DoWin();
        if (isWin) return;
        isWin = true;
        if (weaponType == WeaponType.NoneWeapon)
            return;
        DoAnimation(weaponType != WeaponType.Archery ? "red_victory" : "red_victorry");
    }
    public void AIMeetEnemyTarget(ColliderDetection seekerDetection, Transform target)
    {
        if (this.transform == target)
        {
            if (postMeetHoleTarget != null)
                postMeetHoleTarget.Invoke(this, seekerDetection);
        }
    }
    public bool IsMovingEnemy()
    {
        return movingPoints.Count > 0;
    }
}
