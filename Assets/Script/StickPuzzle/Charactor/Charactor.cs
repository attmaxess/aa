using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Spine.Unity;
using Spine;
using System;
using System.Collections;

[RequireComponent(typeof(HealthController))]
public class Charactor : Attackable
{
    public WeaponType weaponType;
    public bool isLeftDirection;

    [SerializeField] List<Canvas> listCanvas;
    public Hole targetHole;
    public Image circle;

    [SerializeField] List<GameObject> listSlot;

    [Space(20)]
    public int fightingAnimCount;
    public int fightingCount;

    public delegate void OnPostAIMeetTarget(Charactor charactor, ColliderDetection seekerDetection);
    public OnPostAIMeetTarget postMeetHoleTarget;

    public CharactorAIController AI
    {
        get
        {
            if (_ai == null) _ai = GetComponent<CharactorAIController>();
            return _ai;
        }
    }
    CharactorAIController _ai;

    protected override void Start()
    {
        base.Start();
        List<SkeletonGraphic> _listSkeleton = listSkeleton;
        healthController.UpdateHealth(true, false);
        UpdateCharactorObj();
    }

    public void HitTrapAnim()
    {
        skeletonController.GetActiveSkeletons()
            .ForEach(item => item.AnimationState.SetAnimation(0, "blue_hit", true));
    }

    public override void Fighting(int fightingCount)
    {
        GameController.instance.currentLevel.fighting = true;
        SoundManager.instance.PlayCrownBattle();

        this.fightingCount += fightingCount;

        if (fightingAnimCount == 0)
            fightingAnimCount = 1;
        else
            fightingAnimCount += 1;

        List<SkeletonGraphic> _listSkeleton = listSkeleton;

        if (fightingCount == 4)
            _listSkeleton.ForEach((item) => item.AnimationState.TimeScale = 1.5f);
        else
            _listSkeleton.ForEach((item) => item.AnimationState.TimeScale = 1f);

        for (int i = 0; i < _listSkeleton.Count; i++)
        {
            if (i == 0)
            {
                _listSkeleton[i].AnimationState.Complete -= OnAnimationComplete;
                _listSkeleton[i].AnimationState.Complete += OnAnimationComplete;
            }
            _listSkeleton[i].AnimationState.SetAnimation(0, "blue_attack", false);
            TaskUtil.Delay(this, delegate
            {
                switch (weaponType)
                {
                    case WeaponType.Sword:
                        SoundManager.instance.PlayAudioClipCharactor(SoundManager.instance.swordMc);
                        break;
                    case WeaponType.Archery:
                        SoundManager.instance.PlayAudioClipCharactor(SoundManager.instance.archeryMc);
                        break;
                    case WeaponType.Spear:
                        SoundManager.instance.PlayAudioClipCharactor(SoundManager.instance.spearMc);
                        break;
                    case WeaponType.Mace:
                        SoundManager.instance.PlayAudioClipCharactor(SoundManager.instance.maceMc);
                        break;
                    case WeaponType.Shield:
                        SoundManager.instance.PlayAudioClipCharactor(SoundManager.instance.shieldMc);
                        break;
                    case WeaponType.SwordShield:
                        SoundManager.instance.PlayAudioClipCharactor(SoundManager.instance.swordShieldMc);
                        break;
                    default:
                        break;
                }
            }, 0.25f);
        }
    }

    private void OnAnimationComplete(TrackEntry trackEntry)
    {
        if (trackEntry.Animation.ToString().Equals("blue_attack") && fightingAnimCount < this.fightingCount)
        {
            fightingAnimCount += 1;
            List<SkeletonGraphic> activeSkeletons = skeletonController.GetActiveSkeletons();
            for (int i = 0; i < activeSkeletons.Count; i++)
            {
                activeSkeletons[i].AnimationState.SetAnimation(0, "blue_attack", false);
            }
            TaskUtil.Delay(this, delegate
            {
                switch (weaponType)
                {
                    case WeaponType.Sword:
                        SoundManager.instance.PlayAudioClipCharactor(SoundManager.instance.swordMc);
                        break;
                    case WeaponType.Archery:
                        SoundManager.instance.PlayAudioClipCharactor(SoundManager.instance.archeryMc);
                        break;
                    case WeaponType.Spear:
                        SoundManager.instance.PlayAudioClipCharactor(SoundManager.instance.spearMc);
                        break;
                    case WeaponType.Mace:
                        SoundManager.instance.PlayAudioClipCharactor(SoundManager.instance.maceMc);
                        break;
                    case WeaponType.Shield:
                        SoundManager.instance.PlayAudioClipCharactor(SoundManager.instance.shieldMc);
                        break;
                    case WeaponType.SwordShield:
                        SoundManager.instance.PlayAudioClipCharactor(SoundManager.instance.swordShieldMc);
                        break;
                    default:
                        break;
                }
            }, 0.25f);
        }
    }

    public void SwapWeapon(WeaponType type, SkeletonDataAsset dataAsset)
    {
        weaponType = type;
        List<SkeletonGraphic> skes = skeletonController.listSkeleton;
        for (int i = 0; i < skes.Count; i++)
        {
            skes[i].skeletonDataAsset = dataAsset;
            skes[i].Initialize(true);
        }
    }

    public void ChangeDirection(Transform enemy)
    {
        List<SkeletonGraphic> activeSkeletons = skeletonController.GetActiveSkeletons();
        bool isLeft = enemy.transform.position.x - this.transform.position.x > 0;
        activeSkeletons.ForEach(item =>
        {
            Vector3 v = item.transform.localScale;
            v.x = isLeft ? -Mathf.Abs(v.x) : Mathf.Abs(v.x);
            item.transform.localScale = v;
        });
    }

    public override void SetMoving()
    {
        base.SetMoving();
        if (IsAnimMoving() != false) return;
        DoAnimation("blue_move");
        SoundManager.instance.PlayMoving();
    }

    public override bool? IsAnimMoving()
    {
        List<SkeletonGraphic> _listSkeleton = listSkeleton;
        if (_listSkeleton.Count == 0) return null;
        if (_listSkeleton[0].AnimationState == null) return null;
        if (_listSkeleton[0].AnimationState.GetCurrent(0) == null) return false;
        return _listSkeleton[0].AnimationState.GetCurrent(0).Animation.ToString().Equals("blue_move");
    }

    public override void DoWin(BaseMammal helper = null)
    {
        base.DoWin();
        GameController.instance.currentLevel.fighting = false;
        List<SkeletonGraphic> _listSkeleton = listSkeleton;
        _listSkeleton.ForEach(item => item.AnimationState.SetAnimation(0, "blue_idle", true));
        SoundManager.instance.StopCrownBattle();
        SoundManager.instance.PlayAudioClip(SoundManager.instance.lineWin);
    }

    public override bool CanMoveSnap() { return true; }

    public override void DoIdle()
    {
        GameController.instance.currentLevel.fighting = false;

        List<SkeletonGraphic> _listSkeleton = listSkeleton;
        _listSkeleton.ForEach(item => item.AnimationState.SetAnimation(0, "blue_idle", true));

        SoundManager.instance.StopMoving();
    }

    public void HideHealthText()
    {
        healthController.healthText.gameObject.SetActive(false);
    }

    public void VictoryAnim()
    {
        List<SkeletonGraphic> activeSkeletons = skeletonController.GetActiveSkeletons();
        TaskUtil.Delay(this, delegate
        {
            if (weaponType != WeaponType.Archery)
                activeSkeletons.ForEach(item => item.AnimationState.SetAnimation(0, "blue_victory", true));
            else
                activeSkeletons.ForEach(item => item.AnimationState.SetAnimation(0, "blue_victorry", true));

            GameController.instance.currentLevel.HideHealthText();
        }, 0.05f);
    }

    public void VictoryAnimWhenMerge()
    {
        List<SkeletonGraphic> activeSkeletons = skeletonController.GetActiveSkeletons();
        TaskUtil.Delay(this, delegate
        {
            if (weaponType != WeaponType.Archery)
                activeSkeletons.ForEach(item => item.AnimationState.SetAnimation(0, "blue_victory", true));
            else
                activeSkeletons.ForEach(item => item.AnimationState.SetAnimation(0, "blue_victorry", true));
        }, 0.05f);
    }
    public override void DoLose(Hole hole, BaseMammal killer = null)
    {
        if (level.useDebugLog)
            Debug.Log("Charactor Do Lose");

        SoundManager.instance.PlayAudioClipCharactor(SoundManager.instance.charactorFail);
        GameController.instance.currentLevel.fighting = false;

        healthController.healthText.gameObject.SetActive(false);
        skeletonController.HideAllSkeletons();

        SoundManager.instance.StopCrownBattle();
        GameController.instance.currentLevel.AllEnemyWinAnim();
    }
    public int GetFightingCountLeft()
    {
        return this.fightingCount - this.fightingAnimCount;
    }
    public void AddRemainAnimCount(int remain)
    {
        this.fightingAnimCount -= remain;
    }
    public void AIMeetCharactorTarget(ColliderDetection seekerDetection, Transform target)
    {
        if (this.transform == target)
        {
            if (postMeetHoleTarget != null)
                postMeetHoleTarget.Invoke(this, seekerDetection);
        }
    }
}

public enum WeaponType
{
    Sword,             // Kiem
    Archery,           // Cung
    Spear,             // Giao
    Mace,              // Chuy
    Shield,            // Khien
    SwordShield,       // Kiem khien
    NoneWeapon         // Khong cam vu khi, khi danh thi cong hp cho player
}

public enum AnimStage
{
    Idle,
    Attack,
    Moving
}
