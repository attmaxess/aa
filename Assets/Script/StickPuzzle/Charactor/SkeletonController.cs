using Pathfinding;
using Spine.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI.Extensions;
using Unity;
using DG.Tweening;

public class SkeletonController : SkeletonControllerBaseProperties
{
    public List<SkeletonGraphic> listSkeleton = new List<SkeletonGraphic>();
    public List<SkeletonGraphic> actives
    {
        get { return GetActiveSkeletons(); }
    }
    [Serializable]
    public class SkeletonData
    {
        public Vector3 originalPosition;
        public Vector3 originalScale;
        public SkeletonGraphic graphic;
        public SkeletonData(SkeletonGraphic skeleton,
            Vector3 localPosition,
            Vector3 localScale)
        {
            this.graphic = skeleton;
            this.originalPosition = localPosition;
            this.originalScale = localScale;
        }
    }
    public List<SkeletonData> cached;
    public SkeletonReferencesSO reference;
    private void Awake()
    {
        CalculateCached();
        FixScale();
        SyncGraphic();
    }
    [ContextMenu("CalculateCached")]
    public void CalculateCached()
    {
        cached = new List<SkeletonData>();
        foreach (SkeletonGraphic skeleton in listSkeleton)
            cached.Add(new SkeletonData(
                skeleton,
                skeleton.transform.localPosition,
                skeleton.transform.localScale));
    }
    public void RestoreCached(bool restorePosition,
        bool restoreScale)
    {
        if (restorePosition)
            foreach (SkeletonData skeletonData in cached)
                skeletonData.graphic.transform.DOLocalMove(skeletonData.originalPosition, 0.2f);

        if (restoreScale)
            foreach (SkeletonData skeletonData in cached)
                skeletonData.graphic.transform.DOScale(skeletonData.originalScale, 0.2f);
    }
    [ContextMenu("RandomPositioning")]
    public void RandomPositioning()
    {
        if (listSkeleton.Count == 0) return;
        Vector3 vecRan0 = VecRandom(new Vector3(-.1f, -.1f, -.1f), new Vector3(-10f, -10f, -10f));
        listSkeleton[0].transform.localPosition = vecRan0;
        for (int i = 1; i < listSkeleton.Count; i++)
        {
            Vector3 vecRan = VecRandom(new Vector3(-10f, -10f, -10f), new Vector3(-40f, -40f, -40f));
            listSkeleton[i].transform.localPosition = vecRan;
        }
    }
    Vector3 VecRandom(Vector3 min, Vector3 max)
    {
        return new Vector3(UnityEngine.Random.Range(min.x, max.x),
            UnityEngine.Random.Range(min.y, max.y), UnityEngine.Random.Range(min.z, max.z));
    }
    public void FastFade()
    {
        if (listSkeleton.Count == 0) return;
        Color color = listSkeleton[0].color;
        foreach (var skeleton in listSkeleton) 
            skeleton.color = new Color(color.r, color.g, color.b, 0);
    }
    void SyncGraphic()
    {
        if (listSkeleton.Count == 0) return;
        SkeletonGraphic graphic0 = listSkeleton[0];
        for (int i = 1; i < listSkeleton.Count; i++)
            listSkeleton[i].skeletonDataAsset = graphic0.skeletonDataAsset;
        foreach (SkeletonGraphic graphic in listSkeleton)
            graphic.Initialize(true);
    }
    public Vector3 GetCachedPosition()
    {
        return cached.Count > 0 ? cached[0].originalPosition : Vector3.zero;
    }
    public Vector3 GetOriginalOffset()
    {
        return cached.Count > 0 ? cached[0].originalPosition : Vector3.zero;
    }
    public List<SkeletonGraphic> GetActiveSkeletons()
    {
        List<SkeletonGraphic> actives = new List<SkeletonGraphic>();
        listSkeleton.ForEach(item =>
        {
            if (item.gameObject.activeSelf == true)
                actives.Add(item);
        });
        return actives;
    }
    public void Balance(Vector3 position)
    {
        foreach (SkeletonGraphic skeleton in GetActiveSkeletons())
        {
            BalancingHelper.StaticBalance(skeleton.transform, position);
        }
    }
    public bool IsDoneBalance()
    {
        foreach (SkeletonGraphic skeleton in GetActiveSkeletons())
        {
            if (BalancingHelper.StaticIsDoneBalance(skeleton.transform) == false)
            {
                return false;
            }
        }
        return true;
    }
    public static SkeletonController GetOrAdd(Transform tr)
    {
        SkeletonController skeletonController = tr.GetComponent<SkeletonController>();
        if (skeletonController == null)
        {
            skeletonController = tr.gameObject.AddComponent<SkeletonController>();
            skeletonController.listSkeleton = tr.GetComponentsInChildren<SkeletonGraphic>().ToList();
        }
        return skeletonController;
    }
    public void ToogleAllSkeleton(int toogle, int count = 0)
    ///0 : toggle, -1 : off, 1 : on    
    {
        if (listSkeleton.Count == 0) return;
        if (count == 0) return;
        bool active = false;
        if (toogle == 0) active = !listSkeleton[0].gameObject.activeSelf;
        else if (toogle == -1) active = false;
        else if (toogle == 1) active = true;
        for (int i = 0; i < listSkeleton.Count; i++)
        {
            if (i < count)
                listSkeleton[i].gameObject.SetActive(active);
            else
                listSkeleton[i].gameObject.SetActive(false);
        }
    }
    public void SetScale(Vector3 scale, int index = -1)
    {
        if (index == -1)
            foreach (SkeletonGraphic skeleton in listSkeleton)
                skeleton.transform.localScale = scale;
        else
            listSkeleton[index].transform.localScale = scale;
    }
    public void HeadToward(Quaternion quaternion)
    {
        listSkeleton.ForEach(item =>
        {
            item.transform.rotation = quaternion;
        });
    }
    public Vector3 GetScale()
    {
        if (actives.Count == 0)
            return Vector3.one;

        Vector3 scale = actives[0].transform.localScale;
        return new Vector3(Mathf.Abs(scale.x), Mathf.Abs(scale.y), Mathf.Abs(scale.z));
    }
    [ContextMenu("HideAllSkeletons")]
    public void HideAllSkeletons()
    {
        ToggleAppearance(-1);
    }
    [ContextMenu("ShowAllSkeletons")]
    public void Show100()
    {
        ToggleAppearance(1);
    }
    void ToggleAppearance(int toogleType = 0)
    {
        float alpha = 0;
        switch (toogleType)
        {
            case 0: alpha = listSkeleton[0].color.a > 0 ? 0 : 1; break;
            case 1: alpha = 1; break;
            case -1: alpha = 0; break;
        }
        for (int i = 0; i < listSkeleton.Count; i++)
        {
            Color color = listSkeleton[i].color;
            listSkeleton[i].color = new Color(color.r, color.g, color.b, alpha);
        }
    }
    [ContextMenu("FixScale")]
    public void FixScale()
    {
        foreach (SkeletonGraphic skeleton in listSkeleton)
        {
            Vector3 scale = new Vector3(Mathf.Abs(skeleton.transform.localScale.x),
                Mathf.Abs(skeleton.transform.localScale.y),
                Mathf.Abs(skeleton.transform.localScale.z));
            skeleton.transform.localScale = scale;
        }
    }
    public void DoOffset(Vector3 vec)
    {
        foreach (SkeletonData skeleton in cached)
            skeleton.graphic.transform.localPosition = skeleton.originalPosition + vec;
    }
    [ContextMenu("DebugTimeScale")]
    public void DebugTimeScale()
    {
        if (listSkeleton.Count == 0) return;
        Debug.Log("Time scale " + listSkeleton[0].AnimationState.TimeScale);
    }
    public void SetTimeScale(float scale)
    {
        foreach (var skeleton in listSkeleton)
            if (skeleton != null && skeleton.AnimationState != null)
                skeleton.AnimationState.TimeScale = scale;
    }
    public Vector3 Center()
    {
        Vector3 center = Vector3.zero;
        if (listSkeleton.Count == 0) return -Vector3.one;
        foreach (var skeleton in listSkeleton)
            center += skeleton.transform.position;
        center /= listSkeleton.Count;
        return center;
    }
    public Vector3 ActiveCenter()
    {
        Vector3 center = Vector3.zero;
        if (actives.Count == 0) return -Vector3.one;
        foreach (var skeleton in actives)
            center += skeleton.transform.position;
        center /= actives.Count;
        return center;
    }
    void ShowNumberOfSkeleton(int number)
    {
        for (int i = 0; i < 3; i++)
            listSkeleton[i].gameObject.SetActive(i < number);
    }
    void SetBaseMammalType(WeaponType weaponType)
    {
        if (mammal.enemy != null) mammal.enemy.weaponType = weaponType;
        else if (mammal.charactor != null) mammal.charactor.weaponType = weaponType;
    }
    public void SyncWithHealth(float health)
    ///Hiện tại chỉ mới call ở OnValidate -> Health.cs
    ///Tức là chỉ gọi ở editor. Chưa dám gọi lúc runtime, sợ lỗi.
    {
        if (mammal == null) return;
        if (mammal.attackable == null) return;
        if (!mammal.attackable.IsNormalEnemy()) return;
        if (health > 0 && health <= 10f) ShowNumberOfSkeleton(1);
        else if (health > 10 && health <= 20f) ShowNumberOfSkeleton(2);
        else if (health > 20) ShowNumberOfSkeleton(3);
    }
    #region Services
    [ContextMenu("1")] public void Set1() { ShowNumberOfSkeleton(1); }
    [ContextMenu("2")] public void Set2() { ShowNumberOfSkeleton(2); }
    [ContextMenu("3")] public void Set3() { ShowNumberOfSkeleton(3); }
    [ContextMenu("UpdateAppearance")]
    public void UpdateAppearance()
    {
        if (IsToFind("archer_Ske")) SetArcher();
        else if (IsToFind("mace_Ske")) SetMace();
        else if (IsToFind("shield_Ske")) SetShield();
        else if (IsToFind("spear_Ske")) SetSpear();
        else if (IsToFind("sword_Ske")) SetSword();
        else if (IsToFind("sword_shield_Ske")) SetSwordShield();
        else if (IsToFind("empty_idle_Ske")) SetEmpty();
    }
    bool IsToFind(string toFind)
    {
        if (listSkeleton.Count == 0) return false;
        return listSkeleton[0].skeletonDataAsset.name.Contains(toFind);
    }
    [ContextMenu("SetArcher")]
    public void SetArcher()
    {
        reference.FindAndSet(this, "archer_Ske");
        SetBaseMammalType(WeaponType.Archery);
    }
    bool IsArcher() { return IsToFind("archer_Ske"); }
    [ContextMenu("SetMace")]
    public void SetMace()
    {
        reference.FindAndSet(this, "mace_Ske");
        SetBaseMammalType(WeaponType.Mace);
    }
    bool IsMace() { return IsToFind("mace_Ske"); }
    [ContextMenu("SetShield")]
    public void SetShield()
    {
        reference.FindAndSet(this, "shield_Ske");
        SetBaseMammalType(WeaponType.Shield);
    }
    bool IsShield() { return IsToFind("shield_Ske"); } 
    [ContextMenu("SetSpear")]
    public void SetSpear()
    {
        reference.FindAndSet(this, "spear_Ske");
        SetBaseMammalType(WeaponType.Spear);
    }
    bool IsSpear() { return IsToFind("spear_Ske"); }
    [ContextMenu("SetSword")]
    public void SetSword()
    {
        reference.FindAndSet(this, "sword_Ske");
        SetBaseMammalType(WeaponType.Sword);
    }
    bool IsSword() { return IsToFind("sword_Ske"); }
    [ContextMenu("SetSwordShield")]
    public void SetSwordShield()
    {
        reference.FindAndSet(this, "sword_shield_Ske");
        SetBaseMammalType(WeaponType.SwordShield);
    }
    bool IsSwordShield() { return IsToFind("sword_shield_Ske"); }
    [ContextMenu("SetEmpty")]
    public void SetEmpty()
    {
        reference.FindAndSet(this, "empty_idle_Ske");
        SetBaseMammalType(WeaponType.NoneWeapon);
    }    
    bool IsEmpty() { return IsToFind("empty_idle_Ske"); }
    [ContextMenu("SetSoi2Dau")] public void SetSoi2Dau() { reference.FindAndSet(this, "Soi_2_dau_Ske"); }
    [ContextMenu("SetRong2Dau")] public void SetRong2Dau() { reference.FindAndSet(this, "rong_2_dau_Ske"); }
    [ContextMenu("SetKhonglo")] public void SetKhonglo() { reference.FindAndSet(this, "Nguoi_khong_lo_Ske"); }
    [ContextMenu("SetNguoiDa")] public void SetNguoiDa() { reference.FindAndSet(this, "Nguoi_da_Ske"); }
    #endregion Services
}
