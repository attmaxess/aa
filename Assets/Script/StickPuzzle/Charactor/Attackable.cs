using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Attackable : BaseMammal
{
    public bool ignoreAttack;
    public Boss boss
    {
        get
        {
            if (_boss == null) _boss = GetComponent<Boss>();
            if (_boss == null) _boss = enemy != null ? enemy as Boss : null;
            return this._boss;
        }
    }
    Boss _boss = null;
    public Wolf wolf
    {
        get
        {
            if (_wolf == null) _wolf = GetComponent<Wolf>();
            if (_wolf == null) _wolf = enemy != null ? enemy as Wolf : null;
            return this._wolf;
        }
    }
    Wolf _wolf = null;
    public Princess princess
    {
        get
        {
            if (_princess == null) _princess = GetComponent<Princess>();
            return this._princess;
        }
    }
    Princess _princess = null;
    public override Trap trap
    {
        get
        {
            if (base._trap == null) base._trap = GetComponent<Trap>();
            if (base._trap == null) base._trap = base.trap;
            if (base._trap == null) base._trap = this as Trap;
            return base._trap;
        }
    }
    public static Attackable GetAttackable(Transform tr)
    {
        Attackable attackable = tr.GetComponent<Attackable>();
        ///Try get Trap
        if (attackable == null)
        {
            Trap trap = tr.GetComponent<Trap>();
            if (trap != null) return trap;
        }
        ///Try get Enemy
        if (attackable == null)
        {
            Enemy enemy = tr.GetComponent<Enemy>();
            if (enemy != null) return enemy;
        }
        ///Try get Boss
        if (attackable == null)
        {
            Boss boss = tr.GetComponent<Boss>();
            if (boss != null) return boss;
        }
        ///Try get Wolf
        if (attackable == null)
        {
            Wolf wolf = tr.GetComponent<Wolf>();
            if (wolf != null) return wolf;
        }
        ///Try get Princess
        if (attackable == null)
        {
            Princess princess = tr.GetComponent<Princess>();
            if (princess != null) return princess;
        }
        return attackable;
    }
    public int GetFightingCount()
    {
        if (boss != null) return 4;
        if (princess != null) return 0;
        if (wolf != null) return 2;
        if (enemy != null) return 2;
        if (trap != null) return 2;
        return 1;
    }
    public float GetFactor()
    {
        return GetFightingCount() == 4 ? SectionSettings.fightingTime / 1.5f : SectionSettings.fightingTime;
    }
    public bool CanBeMoveByAI()
    {
        //if (trap != null) return false;
        return true;
    }
    public bool IsNormalEnemy()
    {
        return this.enemy != null && this.boss == null && this.wolf == null && this.princess == null;
    }
    public override bool IsEnemy()
    ///Dùng để kiểm tra những nơi gọi vào attackable
    {
        return base.IsEnemy();
    }
    public override bool IsTrap()
    ///Dùng để kiểm tra những nơi gọi vào attackable
    {
        return base.IsTrap();
    }
    protected override void Start()
    {
        base.Start();
    }
    public bool IsWolf()
    {
        return wolf != null;
    }
    public virtual void FindNearHole()
    {
        if (level.listHoles.Count == 0) return;
        List<Level.HoldData> holeDatas = new List<Level.HoldData>();

        for (int i = 0; i < level.listHoles.Count; i++)
            if (level.listHoles[i].enemyAttackable == null)
                holeDatas.Add(new Level.HoldData(i,
                    level.listHoles[i]));

        if (holeDatas.Count > 0)
        {
            holeDatas.Sort((x, y) => (x.hole.holeImage.transform.position - transform.position).magnitude.
            CompareTo((y.hole.holeImage.transform.position - transform.position).magnitude));

            Hole nearHole = level.listHoles[holeDatas[0].id];
            Debug.Log("Fit " + transform.name + " to " + nearHole.transform.name);
            transform.position = nearHole.holeImage.transform.position;
            nearHole.enemyAttackable = this;
            hole = nearHole;

            if (attackable.IsEnemy()) hole.IsPassed = false;
            else if (attackable.IsTrap()) hole.IsPassed = true;
        }
        else
        {
            Debug.LogError("Cant 'fit " + transform.name);
        }
    }
    public virtual bool CanMeet(Attackable other)
    /// Vi du : Bom ko gap enemy duoc
    {
        return true;
    }
    #region Fade Hole
    public void FastFade()
    {
        if (skeletonController != null) skeletonController.FastFade();
        if (healthController != null) healthController.Show0();
    }
    #endregion Fade Hole
}
