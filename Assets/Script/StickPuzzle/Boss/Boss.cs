using Spine;
using Spine.Unity;
using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;

public class Boss : Enemy
{
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

    public override void Fighting(int fightingCount)
    {
        GameController.instance.currentLevel.HideEmoji(this);
        if (coShowEmoji != null)
        {
            StopCoroutine(coShowEmoji);
            coShowEmoji = null;
        }

        if (coRotate != null)
        {
            StopCoroutine(coRotate);
            coRotate = null;
        }

        List<SkeletonGraphic> activeSkeletons = skeletonController.GetActiveSkeletons();
        for (int i = 0; i < activeSkeletons.Count; i++)
        {
            activeSkeletons[i].AnimationState.Complete -= OnAnimationComplete;
        }

        this.fightingCount = fightingCount;
        fightingAnimCount = 1;
        string animationName = "attack";

        for (int i = 0; i < activeSkeletons.Count; i++)
        {
            activeSkeletons[i].AnimationState.TimeScale = 1.5f;
        }

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
                SoundManager.instance.PlayRandomAudioClipBoss(SoundManager.instance.bossAttack);
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
                SoundManager.instance.PlayRandomAudioClipBoss(SoundManager.instance.bossAttack);
            }, 0.25f);
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

    public override void DoWin(BaseMammal helper = null)
    {
        DoAnimation("victory");
    }
    public override void DoIdle()
    {
        DoAnimation("idle");
    }
    protected override void DoLose(BaseMammal killer = null)
    {
        if (isLose) return;
        base.DoLose(killer);
        if (level.useDebugLog) 
            Debug.Log(transform.name + " Boss.cs Dolose by" + killer?.transform.name);
        isLose = true;
        gameObject.SetActive(false);
    }
}

[System.Serializable]
public class CollisionEmoji
{
    public Enemy enemy;
    public SkeletonGraphic emojiSkeleton;
}
