using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyAIController : MammalAIController
{
    public delegate void OnDamageEntity(float damage);
    public OnDamageEntity damageEntity;

    public void SyncHoleToEnemy()
    {
        if (enemy.hole == null) return;
        enemy.hole.transform.position = this.transform.position;
    }
    public void Start()
    {
        detection.onAddOnce += OnMeet;
        damageEntity += level.charactor.GetComponent<CharactorAIController>().enTiAttackable.Damage;
        lerp.onpostFinalizeMovement += OnLerpFinalizeMovement;

        if (dangerZone != null && dangerZone.gameObject.activeSelf == true)
        {
            dangerZone.target = level.charactor.transform;
            dangerZone.IsRotate = true;
        }
    }

    [ContextMenu("Enemy_MoveByAstar")]
    public override void MoveByAstar()
    {
        Attackable attackable = Attackable.GetAttackable(this.transform);
        if (attackable != null && !attackable.CanBeMoveByAI()) return;

        base.MoveByAstar();

        if (mammal.IsEnemy()) { enemy.SetMoving(); }
        else if (mammal.IsTrap()) { }
    }
    public void OnLerpFinalizeMovement(AILerp lerp)
    {
        if (level.isWin || level.isLose) return;
        CharactorAIController charactorAI = level.charactor.GetComponent<CharactorAIController>();
        charactorAI.OnMoveCheckNear(this);
        charactorAI.OnMoveAdd(this);
    }
    public override void OnMeet(ColliderDetection detection, Transform tr)
    {
        base.OnMeet(detection, tr);
        if (!handlingMammal.Contains(tr))
            HandleMeet(tr);
    }
    void HandleMeet(Transform tr)
    {
        Attackable attackableTr = tr.GetComponent<Attackable>();
        if (attackableTr == null) return;
        if (!attackableTr.CanMeet(this.attackable)) return;
        StartCoroutine(C_HandleMeet(tr));
    }
    IEnumerator C_HandleMeet(Transform tr)
    {
        SetStatus(eMammalStatus.Meet);

        if (coJoin != null)
        {
            Debug.Log(this.transform.name + "coJoin : " + (coJoin != null).ToString());
            yield return new WaitUntil(() => coJoin == null);
        }

        if (coFightTrap != null)
        {
            Debug.Log(this.transform.name + "coFightTrap : " + (coFightTrap != null).ToString());
            yield return new WaitUntil(() => coFightTrap == null);
        }

        if (level.useDebugLog)
            Debug.Log("C_HandleMeet " + transform.name + " - " + tr.name);

        Charactor charactor = tr.GetComponent<Charactor>();
        Enemy enemy = tr.GetComponent<Enemy>();
        Trap trap = tr.GetComponent<Trap>();

        if (charactor != null || enemy != null || trap != null)
        {
            StopAstar();
            yield return new WaitUntil(() => IsAstarCompletelyStop() == true);
        }

        if (charactor != null)
        //Fight with Charactor
        {
            CharactorAIController charactorAI = charactor.GetComponent<CharactorAIController>();
            if (charactorAI != null)
            {
                this.enemy?.head.Toward(charactor.transform);
                //Balance(charactor.transform, false);
            }
        }
        else if (enemy != null && enemy.princess != null)
        ///Boss kill Princess
        {
            EnemyAIController enemyAI = enemy.GetComponent<EnemyAIController>();
            StopCurrentMoveToMeet();
            yield return StartCoroutine(BossKillPrincess(enemyAI));
            enemy.hole.DoLose(this.mammal, false);
            this.enemy.DoWin();
            level.isLose = true;
            GameController.instance.DoLose();
        }
        else if (enemy != null && trap == null && this.enemy != null && enemy.IsNormalEnemy() == true && enemy.weaponType == this.enemy.weaponType)
        //Merge other enemy with same weapon type
        {
            EnemyAIController enemyAI = enemy.GetComponent<EnemyAIController>();
            StopCurrentMoveToMeet();
            Join(enemyAI);
            yield return new WaitUntil(() => coJoin == null);
        }
        else if (trap != null)
        ///Hit trap
        {
            StopCurrentMoveToMeet();
            FightTrap(this.enemy, trap, false);
            yield return new WaitUntil(() => coFightTrap == null);
        }

        RemoveMammalStatus(eMammalStatus.Meet);

        yield break;
    }

    IEnumerator BossKillPrincess(EnemyAIController otherEnemy)
    {
        int fightingCount = 4;
        enemy.Fighting(fightingCount);
        level.AnimFighting(enemy.skeletonController.listSkeleton, fightingCount, true);
        yield return new WaitUntil(() => level.coFightingAnim == null);
        yield break;
    }
    public void Join(EnemyAIController otherEnemy)
    {
        base.StopJoin();
        if (gameObject.activeSelf)
            coJoin = StartCoroutine(C_Join(otherEnemy));
    }
    IEnumerator C_Join(EnemyAIController otherEnemy)
    {
        if (transform == otherEnemy.transform)
        {
            if (level.useDebugLog)
                Debug.Log("Join same transform???");
            yield break;
        }

        if (level.useDebugLog)
            Debug.Log(this.transform.name + " join " + otherEnemy.transform.name);

        SetStatus(eMammalStatus.Join);

        SoundManager.instance.PlayAudioClip(SoundManager.instance.duplicate);

        ChoseEnemyWhenJoin(this, otherEnemy,
            out EnemyAIController chosenEnemy,
            out EnemyAIController losingEnemy,
            out bool continueFight);

        if (chosenEnemy == this)
        {
            level.Join2Enemy(chosenEnemy, losingEnemy);
            yield return new WaitUntil(() => level.DoneJoinEnemy == true);

            chosenEnemy.Refresh();
            chosenEnemy.enemy.DoIdle();

            RemoveMammalStatus(eMammalStatus.Join);
            level.charactorAI.RemoveMammalStatus(eMammalStatus.Join);

            if (continueFight)
            {
                level.charactorAI.ForceFightMeet(chosenEnemy.transform);
            }
            else
            {
                chosenEnemy.MoveToMeet(level.charactorAI);
            }
        }

        coJoin = null;

        yield break;
    }

    void ChoseEnemyWhenJoin(EnemyAIController self, EnemyAIController otherEnemy,
        out EnemyAIController chosenOne,
        out EnemyAIController losingOne,
        out bool continueFight)
    {
        if (self.enemy.boss != null)
        {
            chosenOne = self; losingOne = otherEnemy;
        }
        else if (otherEnemy.enemy.boss != null)
        {
            chosenOne = otherEnemy; losingOne = self;
        }
        else if (self.enemy.wolf != null)
        {
            chosenOne = otherEnemy; losingOne = self;
        }
        else if (otherEnemy.enemy.wolf != null)
        {
            chosenOne = self; losingOne = otherEnemy;
        }
        else if (self.IsFightingCharactor())
        {
            chosenOne = self; losingOne = otherEnemy;
        }
        else if (otherEnemy.IsFightingCharactor())
        {
            chosenOne = otherEnemy; losingOne = self;
        }
        else if (self.enemy.Health > otherEnemy.enemy.Health)
        {
            chosenOne = self; losingOne = otherEnemy;
        }
        else if (self.enemy.Health < otherEnemy.enemy.Health)
        {
            chosenOne = otherEnemy; losingOne = self;
        }
        else
        {
            if (self.transform.GetSiblingIndex() < otherEnemy.transform.GetSiblingIndex())
            { chosenOne = self; losingOne = otherEnemy; }
            else
            {
                chosenOne = otherEnemy; losingOne = self;
            }
        }

        continueFight = chosenOne.detection.IsContain(level.charactor.transform);
    }
    public void AddReachableBySameType(List<EnemyAIController> enemies)
    {
        this.mammalReachables = new List<MammalAIController>();
        foreach (EnemyAIController otherEnemy in enemies)
        {
            if (otherEnemy.transform == this.transform) continue;
            if (!level.IsReachable(this.seeker, otherEnemy.mover)) continue;
            if (otherEnemy.enemy.weaponType != this.enemy.weaponType) continue;
            AddOnce_MammalReachable(otherEnemy);
        }
    }
    public void BossAddPrincess()
    {
        if (this.enemy == null) return;
        if (this.enemy.boss == null) return;
        foreach (MammalAIController otherEnemy in level.listAttackableAI)
        {
            if (otherEnemy.transform == this.transform) continue;
            if (otherEnemy.enemy == null) continue;
            if (otherEnemy.enemy.princess == null) continue;
            if (!level.IsReachable(this.seeker, otherEnemy.mover)) continue;
            AddOnce_MammalReachable(otherEnemy);
        }
    }
}
