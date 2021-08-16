using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;
using Spine;

public class Wolf : Enemy
{
    [SerializeField] bool canMoving = true;
    [SerializeField] Hole movingHole1 = null;
    [SerializeField] Hole movingHole2 = null;

    protected override void OnStart()
    {
        base.OnStart();

        if (canMoving)
        {
            coMoving = StartCoroutine(IEMoving());
        }
    }

    public void StopMove()
    {
        if (coMoving != null)
        {
            StopCoroutine(coMoving);
            coMoving = null;

            isCollision = true;

            listSkeleton.ForEach(item => item.AnimationState.SetAnimation(0, "idle", true));

            movingHole1.GetComponent<BoxCollider2D>().enabled = false;
            movingHole2.GetComponent<BoxCollider2D>().enabled = false;
        }
    }

    protected override void OnUpdate()
    {
        if (canMoving && coMoving != null && !isCollision)
        {
            for (int i = 0; i <= listDangerRaycastPoints.Count - 1; i++)
            {
                RaycastHit2D hit;
                if (i != listDangerRaycastPoints.Count - 1)
                    hit = Physics2D.Linecast(listDangerRaycastPoints[i].position, listDangerRaycastPoints[i + 1].position, playerLayer);
                else
                    hit = Physics2D.Linecast(listDangerRaycastPoints[i].position, listDangerRaycastPoints[0].position, playerLayer);

                if (hit.collider != null &&
                    hit.collider.gameObject.tag.Equals("Player") &&
                    GameController.instance.currentLevel.charactor.targetHole != null &&
                    GameController.instance.currentLevel.charactor.targetHole.enemyAttackable != this)
                {
                    isCollision = true;
                    GameController.instance.currentLevel.EnemyAttackPlayer(this, GameController.instance.currentLevel.GetHole(this), GameController.instance.currentLevel.charactor.targetHole);

                    StopCoroutine(coMoving);
                    coMoving = null;

                    break;
                }
            }
        }
    }

    public override void SetMoving()
    {
        if (IsAnimMoving() != false) return;
        DoAnimation("move");
    }
    public override bool? IsAnimMoving()
    {
        if (listSkeleton.Count == 0) return null;
        if (listSkeleton[0].AnimationState == null) return null;
        if (listSkeleton[0].AnimationState.GetCurrent(0) == null) return false;
        return listSkeleton[0].AnimationState.GetCurrent(0).Animation.ToString().Equals("move");
    }
    Coroutine coMoving;

    Hole targetMovingHole;

    protected IEnumerator IEMoving()
    {
        if (GameController.instance.currentLevel != null)
            yield return new WaitUntil(() => GameController.instance.DoneLoadLevel == true);

        targetMovingHole = movingHole1;

        SetMoving();
        head?.Toward(targetMovingHole.transform);

        while (GameController.instance.currentState == GameController.State.Play && !GameController.instance.currentLevel.isWin && !GameController.instance.currentLevel.isLose && !isCollision)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetMovingHole.transform.position, 0.5f * Time.deltaTime);

            if (Vector2.Distance(targetMovingHole.transform.position, transform.position) < 0.1f)
            {
                targetMovingHole = targetMovingHole == movingHole1 ? targetMovingHole = movingHole2 : movingHole1;
                head.Toward(targetMovingHole.transform);
            }

            yield return null;
        }
    }

    public override void Fighting(int fightingCount)
    {
        this.fightingCount = fightingCount;
        fightingAnimCount = 1;
        string animationName = "attack";

        //for (int i = 0; i < listenemiesObj.Count; i++)
        //{
        //    listenemiesObj[i].AnimationState.TimeScale = 1.5f;
        //}

        for (int i = 0; i < listSkeleton.Count; i++)
        {
            if (i == 0)
            {
                listSkeleton[i].AnimationState.Complete -= OnAnimationComplete;
                listSkeleton[i].AnimationState.Complete += OnAnimationComplete;
            }
            listSkeleton[i].AnimationState.SetAnimation(0, animationName, false);
            TaskUtil.Delay(this, delegate
            {
                SoundManager.instance.PlayAudioClipBoss(SoundManager.instance.wolfAttack);
            }, 0.25f);
        }
    }

    private void OnAnimationComplete(TrackEntry trackEntry)
    {
        if (GameController.instance.currentLevel.charactor.Health == 0)
        {
            GameController.instance.currentLevel.StopEnemyAttack(this, GameController.instance.currentLevel.charactor.targetHole);
            return;
        }
        string animationName = "attack";
        if (trackEntry.Animation.ToString().Equals(animationName) && fightingAnimCount < fightingCount)
        {
            fightingAnimCount += 1;
            for (int i = 0; i < listSkeleton.Count; i++)
            {
                listSkeleton[i].AnimationState.SetAnimation(0, animationName, false);
                TaskUtil.Delay(this, delegate
                {
                    SoundManager.instance.PlayAudioClipBoss(SoundManager.instance.wolfAttack);
                }, 0.25f);
            }
        }
    }
    public override void DoWin(BaseMammal helper = null)
    {
        //healthController.healthText.transform.localPosition = originalLocalPos;
        DoAnimation("victory");
    }
    protected override void DoLose(BaseMammal killer = null)
    {
        if (isLose) return;
        base.DoLose(killer);
        if (level.useDebugLog)
            Debug.Log(transform.name + " Wolf.cs Dolose by" + killer?.transform.name);
        isLose = true;
        gameObject.SetActive(false);
    }
    public override void DoIdle()
    {
        DoAnimation("idle");
    }
    public override bool IsMoving()
    {
        return canMoving == true && coMoving != null;
    }
}
