using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UI.Extensions;

public class Trap : Attackable, iTrapAttack
{
    public eTrapType type = eTrapType.None;
    [ReadOnly] public bool isDefuse = false;
    [Space(10)]
    [SerializeField] SkeletonGraphic avatarSkeleton = null;
    [SerializeField] string idleAnim = string.Empty;
    [SerializeField] string attackAnim = string.Empty;
    [SerializeField] string dieAnim = string.Empty;

    [Space(10)]
    public SkeletonGraphic waterHitSkeleton;

    public ColliderDetection detection;
    public bool isCollision
    {
        get { return this._isCollision; }
        set
        {
            this._isCollision = value;
            if (onpostSetCollision != null)
                onpostSetCollision.Invoke(value);
        }
    }
    [SerializeField] bool _isCollision = false;

    public int trapPoint = 0;

    public delegate void OnPostSetCollision(bool value);
    public OnPostSetCollision onpostSetCollision;

    public bool requireToFight
    {
        get { return this._requireToFight; }
        set { this._requireToFight = value; }
    }
    [SerializeField] bool _requireToFight = true;

    public delegate void OnAttack(Trap trap);
    public OnAttack onAttack;
    public virtual float AttackHealth(float charactorHealth)
    {
        Debug.Log("Trap AttackHealth");

        float health = 0;
        switch (type)
        {
            case eTrapType.PunjiStick:
                health = Mathf.CeilToInt((charactorHealth * 25f / 100f));
                break;
            case eTrapType.WaterHole:
                SoundManager.instance.PlayAudioClipCharactor(SoundManager.instance.hotWater);
                health = Mathf.CeilToInt(charactorHealth / 2f);
                break;
            case eTrapType.Wolf:
                break;
            case eTrapType.PricklySphere:
                break;
            case eTrapType.Stone:
                health = Mathf.CeilToInt((charactorHealth * 35f / 100f));
                break;
            case eTrapType.Wall:
                break;
            case eTrapType.Teleport:
                break;
            case eTrapType.CocGo:
                health = trapPoint;
                break;
            case eTrapType.Bom:
                health = Mathf.CeilToInt((charactorHealth * .5f));
                break;
            case eTrapType.TiaSet:
                health = Mathf.CeilToInt((charactorHealth * 2f));
                break;
            case eTrapType.NgoiSaoHivong:
                health = Mathf.CeilToInt((charactorHealth * 3f));
                break;
            case eTrapType.CanGat:
                //Vô hiệu hóa tất cả các bẫy
                break;
            case eTrapType.ConTin:
                health = charactorHealth + trapPoint;
                break;
            case eTrapType.ThapCungThu:
                //Gây 10 sát thương mỗi lần nhân vật di chuyển trong phạm vi 1 ô với tháp canh                
                break;
            case eTrapType.Cung:
                //Nhân vật chuyển sang dạng đánh xa trong phạm vi 1 ô.
                //Gây 10 sát thương cho kẻ địch trước, sau đó tính kết quả trận đấu dựa theo lượng HP của nhân vật và kẻ địch.
                //Khi tấn công bằng cung, kẻ địch sẽ di chuyển về phía nhân vật
                break;
            case eTrapType.Khobau:
                //
                break;
        }

        return health;
    }
    public void Attack()
    {
        StartCoroutine(C_Attack());
    }
    IEnumerator C_Attack()
    {
        switch (type)
        {
            case eTrapType.PunjiStick:
                avatarSkeleton.AnimationState.SetAnimation(0, attackAnim, false);
                TaskUtil.Delay(this, delegate
                {
                    GetComponent<Collider2D>().enabled = false;
                    SoundManager.instance.PlayAudioClipForTrap(SoundManager.instance.punjiAndStoneTrap);
                }, 2 * SectionSettings.fightingTime);
                break;
            case eTrapType.Stone:
                avatarSkeleton.AnimationState.SetAnimation(0, attackAnim, false);
                TaskUtil.Delay(this, delegate
                {
                    GetComponent<Collider2D>().enabled = false;
                    SoundManager.instance.PlayAudioClipForTrap(SoundManager.instance.punjiAndStoneTrap);
                }, 2 * SectionSettings.fightingTime);
                break;
            case eTrapType.CocGo:
                TaskUtil.Delay(this, delegate
                {
                    //GetComponent<Collider2D>().enabled = false;
                    SoundManager.instance.PlayAudioClipForTrap(SoundManager.instance.punjiAndStoneTrap);
                }, 2 * SectionSettings.fightingTime);
                break;
            case eTrapType.Bom:
                TaskUtil.Delay(this, delegate
                {
                    GetComponent<Collider2D>().enabled = false;
                    SoundManager.instance.PlayAudioClipForTrap(SoundManager.instance.punjiAndStoneTrap);
                }, 2 * SectionSettings.fightingTime);
                break;
            case eTrapType.TiaSet:
                TaskUtil.Delay(this, delegate
                {
                    GetComponent<Collider2D>().enabled = false;
                    SoundManager.instance.PlayAudioClipForTrap(SoundManager.instance.punjiAndStoneTrap);
                }, 2 * SectionSettings.fightingTime);
                break;
        }

        yield return new WaitForSeconds(2 * SectionSettings.fightingTime);
        isCollision = true;
    }
    protected override void Start()
    {
        base.Start();

        if (detection != null)
            detection.onAddOnce += MeetTarget;

        if (selectController != null)
        {
            onpostSetCollision += selectController.SetSelectedFromOutside;
            selectController.IsSelected = isCollision;
        }

        onpostSetCollision += SelfDestroy;
    }
    void MeetTarget(ColliderDetection detection, Transform tr)
    {
        if (tr.GetComponent<Charactor>() != null)
        {

        }
    }
    public virtual void SelfDestroy(bool hit)
    {
        switch (type)
        {
            case eTrapType.WaterHole:
            case eTrapType.CanGat:
                break;
            default:
                this.gameObject.SetActive(false);
                if (healthController != null)
                    healthController.Health = 0;
                break;
        }
    }
    protected override void DoLose(BaseMammal killer)
    {
        base.DoLose(killer);

        if (level.useDebugLog)
            Debug.Log(transform.name + " Trap.cs Dolose by" + killer?.transform.name);
    }
    public bool IsRequireHole()
    {
        switch (type)
        {
            case eTrapType.Wolf: return true;
            case eTrapType.WaterHole: return true;

            default: return false;
        }
    }
    public override void Show100()
    {
        skeletonController.GetActiveSkeletons().ForEach(item => item.DOFade(1, 0.1f));
    }
    public bool IsDefusable()
    {
        switch (type)
        {
            case eTrapType.PunjiStick:
            case eTrapType.WaterHole:
            case eTrapType.Stone:
            case eTrapType.CocGo:
            case eTrapType.Bom:
            case eTrapType.ThapCungThu:
                return true;
            case eTrapType.CanGat:
            case eTrapType.TiaSet:
            case eTrapType.NgoiSaoHivong:
            case eTrapType.ConTin:
            case eTrapType.Cung:
            case eTrapType.Khobau:
                return false;
            default: return false;
        }
    }
    public void Defuse()
    {
        if (!IsDefusable()) return;
        /*
        isDefuse = true;
        healthController.SetOff();
        */
        SelfDestroy(true);
    }
    public virtual void InitHoleIsPassed(Hole hole)
    {
        switch (type)
        {
            case eTrapType.WaterHole:
            case eTrapType.Stone:
            case eTrapType.CocGo:
            case eTrapType.Bom:
            case eTrapType.ThapCungThu:
                hole.IsPassed = true;
                break;
            case eTrapType.PunjiStick:
            case eTrapType.CanGat:
            case eTrapType.TiaSet:
            case eTrapType.NgoiSaoHivong:
            case eTrapType.ConTin:
            case eTrapType.Cung:
            case eTrapType.Khobau:
                hole.IsPassed = false;
                break;
            default:
                hole.IsPassed = true;
                break;
        }
    }
}

public enum eTrapType
{
    None,
    PunjiStick,           // Cai chong gai
    WaterHole,            // Ho nuoc
    Wolf,                 // Cho soi
    PricklySphere,        // Qua cau gai
    Stone,                // Tang da
    Wall,                 // Buc tuong
    Teleport,             // Dich chuyen
    CocGo,
    Bom,
    TiaSet,
    NgoiSaoHivong,
    CanGat,
    ConTin,
    ThapCungThu,
    Cung,
    Khobau
}