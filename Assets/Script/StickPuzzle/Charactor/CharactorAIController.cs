using DG.Tweening;
using Pathfinding;
using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI.Extensions;

public class CharactorAIController : MammalAIController
{
    public AttackableEntity enTiAttackable;

    public delegate void OnPostForceFight();
    public OnPostForceFight onPostForceFight;

    [ReadOnly] public int fightingAnimCount;
    [ReadOnly] public int fightingCount;
    [ReadOnly] public float factor;

    Coroutine coFightingAnim;
    Coroutine coFightEnemy;

    [Space(20)]
    [ReadOnly] public Enemy currentEnemyFighting;

    public MammalAIController nearestMammalAI
    {
        get { return _nearestEnemy; }
        set
        {
            _nearestEnemy = value;
            OnPostSetNearest(value);
        }
    }
    [ReadOnly] public MammalAIController _nearestEnemy;
    void OnPostSetNearest(MammalAIController nearest)
    {

    }
    private void OnEnable()
    {
        detection.onAddOnce += OnMeet;
    }
    protected void LateUpdate()
    ///Vì AILerp dùng update nên script này phải dùng lateupdate để đổi hướng nhân vật
    {
        if (mammalReachables.Count == 0)
            return;

        CheckNull();

        ///Current Aiming
        if (!level.isWin && !level.isLose)
        {
            if (currentAiming == null ||
                (currentAiming != null && !IsReachable(currentAiming)))
            ///Current aim ma null thi tim va di
            {
                if (currentAiming != null)
                    currentAiming = null;

                if (mammalReachables.Count > 0)
                {
                    MammalAIController next = FindNextReach();
                    if (next != null)
                    {
                        currentAiming = next.transform;
                        MoveToMeet(next);
                    }
                }
            }
            else if (IsHeadingLastTarget() && mammalReachables.Count > 1)
            ///Dang di den muc tieu cuoi ma co nhieu AI hon thi tim lai
            {
                MammalAIController next = FindNextReach();
                if (next != null)
                {
                    currentAiming = next.transform;
                    MoveToMeet(next);
                }
            }
            else if (nearestMammalAI != null && nearestMammalAI.gameObject.activeSelf == true &&
                (currentAiming != null && currentAiming.gameObject.activeSelf == true) &&
                nearestMammalAI.transform != currentAiming)
            {
                StopAstar();
                currentAiming = nearestMammalAI.transform;
                MoveToMeet(currentAiming);
            }
            else if (currentAiming != null &&
                IsReachable(currentAiming) &&
                IsIdle() && !Met(currentAiming))
            ///Idle va currentAim khac null 
            {
                MoveToMeet(currentAiming);
            }
        }
    }
    void CheckNull()
    {
        mammalReachables.RemoveAll(
            (x) => x == null ||
            x.gameObject.activeSelf == false);

        if (currentAiming != null && currentAiming.gameObject.activeSelf == false)
            currentAiming = null;

        if (nearestMammalAI != null &&
            nearestMammalAI.gameObject.activeSelf == false)
            nearestMammalAI = null;

        if (nearestMammalAI != null &&
            !nearestMammalAI.CanPokeToFindCharactor())
            nearestMammalAI = null;

        if (currentEnemyFighting != null && currentEnemyFighting.gameObject.activeSelf == false)
            currentEnemyFighting = null;
    }
    bool IsReachable(Transform tr)
    {
        if (!tr.gameObject.activeSelf) return false;
        ///Mammal binh thuong, !activeSelf la ko reach duoc roi

        TrapAIController trapAI = tr.GetComponent<TrapAIController>();
        ///Trap thi khac, can xet isCollision
        if (trapAI != null)
            return trapAI.trap.isCollision;

        return true;
    }
    void ResetIfAimIsDefuseTrap()
    {
        if (currentAiming == null) return;
        TrapAIController trapAI = currentAiming.GetComponent<TrapAIController>();
        if (trapAI == null) return;
        if (trapAI != null && trapAI.trap.isDefuse == true)
            currentAiming = null;
    }
    MammalAIController FindNextReach()
    {
        if (mammalReachables.Count == 0)
            return null;

        MammalAIController next = FindNormalEnemy();
        if (next != null)
            return next;
        next = FindNextNormalTrap();
        if (next != null)
            return next;
        next = FindBoss();
        if (next != null)
            return next;
        next = FindPrincessOrKhobau();
        if (next != null)
            return next;
        return null;
    }
    MammalAIController FindNormalEnemy()
    ///Normal enemy hoac wolf
    {
        List<MammalAIController> founds = new List<MammalAIController>();
        for (int i = 0; i < mammalReachables.Count; i++)
        {
            if (mammalReachables[i].gameObject.activeSelf &&
                mammalReachables[i].mammal.enemy != null &&
                (mammalReachables[i].mammal.enemy.IsNormalEnemy() == true
                || mammalReachables[i].mammal.enemy.IsWolf()) &&
                mammalReachables[i].mammal.enemy.IsEnemy() == true)
                founds.Add(mammalReachables[i]);
        }

        if (founds.Count == 0) return null;

        founds.Sort((x, y) => (x.transform.position - level.charactor.transform.position).magnitude
        .CompareTo((y.transform.position - level.charactor.transform.position).magnitude));

        return founds[0];
    }
    MammalAIController FindNextNormalTrap()
    ///Ngoài Kho bau là bình thường
    {
        List<MammalAIController> founds = new List<MammalAIController>();
        for (int i = 0; i < mammalReachables.Count; i++)
        {
            if (mammalReachables[i].gameObject.activeSelf &&
                mammalReachables[i].attackable != null &&
                mammalReachables[i].attackable.trap != null &&
                mammalReachables[i].attackable.trap.type != eTrapType.Khobau)
                founds.Add(mammalReachables[i]);
        }

        if (founds.Count == 0) return null;

        founds.Sort((x, y) => (x.transform.position - level.charactor.transform.position).magnitude
        .CompareTo((y.transform.position - level.charactor.transform.position).magnitude));

        return founds[0];
    }
    MammalAIController FindBoss()
    {
        MammalAIController boss = mammalReachables.Find(
            (x) => x.gameObject.activeSelf &&
            x.mammal.enemy != null &&
            x.mammal.enemy.boss != null);
        if (boss != null)
            return boss;

        return null;
    }
    MammalAIController FindPrincessOrKhobau()
    {
        MammalAIController princess = mammalReachables.Find((x) => x.gameObject.activeSelf && x.mammal.enemy != null && x.mammal.enemy.princess != null);
        if (princess != null)
            return princess;

        MammalAIController khobau = mammalReachables.Find((x) => KhoBauTrap.IsKhoBauTrap(x.transform));
        if (khobau != null)
            return khobau;

        return null;
    }
    public void OnMoveCheckNear(MammalAIController mammalAI)
    {
        StartCoroutine(C_OnMoveCheckNear(mammalAI));
    }
    IEnumerator C_OnMoveCheckNear(MammalAIController mammalAI)
    {
        if (!level.IsReachable(mammalAI.seeker, mover))
            ///Không tiếp cận được thì không kiểm tra.
            yield break;

        if (IsBusyFighting())
            ///Nhân vật đang fight thì không kiểm tra.
            yield break;

        if (IsBusyJoining())
            ///Nhân vật đang join cũng không kiểm tra.
            yield break;

        if (mammalAI.enemy != null && mammalAI.enemy.princess != null)
        {
            ///Không kiểm tra công chúa có gần nhân vật.
            ///Nhân vật tự tìm công chúa khi công chúa là đích cuối cùng.
            yield break;
        }

        if (KhoBauTrap.IsKhoBauTrap(mammalAI.transform))
        ///Không kiểm tra kho báu có gần nhân vật.
        ///Nhân vật tự tìm kho báu khi kho báu là đích cuối cùng.
        {
            if (mammalReachables.Count > 1)
                yield break;
        }

        if (nearestMammalAI == null)
        ///Trường hợp chưa có enemy nào ở gần thì set up
        {
            if (level.useDebugLog)
                Debug.Log("CharactorAI đang không có Enemy gần nhất. Tìm thấy " + mammalAI.transform.name);

            this.nearestMammalAI = mammalAI;
        }
        else if (nearestMammalAI != null &&
            mammalAI != nearestMammalAI &&
            TransformDistance(mammalAI.transform, this.transform) < TransformDistance(nearestMammalAI.transform, this.transform))
        ///Trường hợp đã có thì so sánh khoảng cách
        ///Khoảng cách ngắn hơn thì đổi bằng enemy gần hơn
        {
            if (level.useDebugLog)
                Debug.Log("CharactorAI có Enemy gần nhất là " + nearestMammalAI.transform.name + " Tìm thấy " + mammalAI.transform.name + " gần hơn.");

            this.nearestMammalAI = mammalAI;
        }

        yield break;
    }
    public void OnMoveAdd(MammalAIController mammal)
    {
        if (!level.IsReachable(mammal.seeker, mover)) return;
        AddOnce_MammalReachable(mammal);
    }
    bool IsHeadingLastTarget()
    ///Last tartget là công chúa hoặc kho báu
    {
        if (currentAiming == null)
            return false;
        Attackable attackable = currentAiming.GetComponent<Attackable>();
        if (attackable == null) return false;
        if (attackable.princess != null) return true;
        if (attackable.trap != null && attackable.trap.type == eTrapType.Khobau) return true;
        return false;
    }
    float TransformDistance(Transform tr1, Transform tr2)
    {
        return (tr1.position - tr2.position).magnitude;
    }
    [ContextMenu("Charactor_MoveByAstar")]
    public override void MoveByAstar()
    {
        base.MoveByAstar();
        charactor.SetMoving();
    }

    public override void OnMeet(ColliderDetection detection, Transform tr)
    //Ở Charactor
    {
        base.OnMeet(detection, tr);
        if (!handlingMammal.Contains(tr))
            HandleMeet(tr);
    }
    public void HandleMeet(Transform tr)
    {
        if (tr == this.transform) return;
        coHandleMeet = StartCoroutine(C_HandleMeet(tr));
    }
    IEnumerator C_HandleMeet(Transform tr)
    {
        SetStatus(eMammalStatus.Meet);

        Attackable attackable = Attackable.GetAttackable(tr);
        if (attackable != null)
        {
            enTiAttackable.AddAttack(attackable);

            if (!IsAstarCompletelyStop())
            {
                StopCurrentMoveToMeet();
                StopAstar();
                yield return new WaitUntil(() => IsAstarCompletelyStop() == true);
            }

            charactor.head.Toward(attackable.transform);
            //Balance(attackable.transform, attackable.trap != null);
            StartCoroutine(C_FightMeet(tr));
            yield return new WaitUntil(() => IsNotStatus(eMammalStatus.FightMeet));

            RemoveMammalStatus(eMammalStatus.Meet);
            coHandleMeet = null;
            yield break;
        }

        if (!IsContainAnyReachable())
        {
            Flag flag = tr.GetComponent<Flag>();
            if (flag != null)
            {
                if (!IsAstarCompletelyStop())
                {
                    StopCurrentMoveToMeet();
                    StopAstar();
                    yield return new WaitUntil(() => IsAstarCompletelyStop() == true);

                    if (level.isWin)
                        mammal.DoWin(charactor);
                    else
                        mammal.DoIdle();
                }

                RemoveMammalStatus(eMammalStatus.Meet);
                coHandleMeet = null;
                yield break;
            }
        }

        RemoveMammalStatus(eMammalStatus.Meet);
        coHandleMeet = null;
        yield break;
    }

    public void StopAllEnemy()
    {
        List<EnemyAIController> enemies = FindObjectsOfType<EnemyAIController>().ToList();
        for (int i = 0; i < enemies.Count; i++)
        {
            if (enemies[i].setter.target != this.transform) continue;
            enemies[i].StopAstar();
        }
    }
    public void ForceFightMeet(Transform trMeet)
    {
        Attackable attackable = Attackable.GetAttackable(trMeet);
        if (coFightEnemy != null)
        {
            StopCoroutine(coFightEnemy);
            coFightEnemy = null;
        }
        coFightEnemy = StartCoroutine(C_FightEnemy(attackable, attackable.enemy.ignoreAttack));
    }
    public IEnumerator C_FightMeet(Transform trMeet)
    {
        if (coFightEnemy != null)
        {
            Debug.Log("coFightEnemy : " + this.transform.name + " - " + (coFightEnemy != null).ToString());
            if (currentEnemyFighting != null &&
                trMeet == currentEnemyFighting.transform)
            {
                Debug.Log("Vi balancing nen gap nhau lan nua");
                yield break;
            }
            else
            {
                yield return new WaitUntil(() => coFightEnemy == null);
            }
        }

        if (coJoin != null)
        {
            Debug.Log("coJoin : " + this.transform.name + " - " + (coJoin != null).ToString());
            yield return new WaitUntil(() => coJoin == null);
        }

        if (coFightTrap != null)
        {
            Debug.Log("coFightTrap : " + this.transform.name + " - " + (coFightTrap != null).ToString());
            yield return new WaitUntil(() => coFightTrap == null);
        }

        if (level.useDebugLog)
            Debug.Log("C_FightMeet " + transform.name + " - " + trMeet.name);

        yield return new WaitForEndOfFrame();
        SetStatus(eMammalStatus.FightMeet);

        Attackable attackable = trMeet.GetComponent<Attackable>();

        if (attackable != null)
        {
            if (attackable.IsEnemy())
            {
                if (attackable.enemy != null && attackable.mammalAI.enemyAI != null)
                {
                    if (coFightEnemy == null && (currentEnemyFighting == null || currentEnemyFighting.gameObject.activeSelf == false))
                    {
                        coFightEnemy = StartCoroutine(C_FightEnemy(attackable, attackable.enemy.ignoreAttack));
                        yield return new WaitUntil(() => coFightEnemy == null);
                    }
                    else
                    {
                        EnemyAIController enemyAI = currentEnemyFighting.mammalAI.enemyAI;
                        if (enemyAI != null)
                        {
                            StopFight();

                            if (currentEnemyFighting.gameObject.activeSelf &&
                                attackable.gameObject.activeSelf &&
                                attackable.gameObject != currentEnemyFighting.gameObject)
                            {
                                SetStatus(eMammalStatus.Join);

                                currentEnemyFighting.mammalAI.enemyAI.Join(attackable.mammalAI.enemyAI);
                                attackable.mammalAI.enemyAI.Join(currentEnemyFighting.mammalAI.enemyAI);

                                yield return new WaitUntil(() => enemyAI.IsBusyJoining() == false &&
                                attackable.mammalAI.enemyAI.IsBusyJoining() == false);
                            }
                            else if (!currentEnemyFighting.gameObject.activeSelf)
                            {
                                coFightEnemy = StartCoroutine(C_FightEnemy(attackable, attackable.enemy.ignoreAttack));
                                yield return new WaitUntil(() => coFightEnemy == null);
                            }
                        }
                    }
                }
            }

            if (attackable.IsTrap())
            {
                FightTrap(charactor, attackable.trap, true);
                yield return new WaitUntil(() => coFightTrap == null);
            }
        }

        RemoveMammalStatus(eMammalStatus.FightMeet);
        yield break;
    }

    IEnumerator C_FightEnemy(Attackable attackable, bool ignoreAttack)
    {
        if (level.isWin || level.isLose)
        {
            coFightEnemy = null;
            yield break;
        }

        EnemyAIController enemyAI = attackable.mammalAI as EnemyAIController;
        Enemy enemy = (Enemy)attackable;

        SetStatus(eMammalStatus.FightEnemy);
        enemyAI.SetStatus(eMammalStatus.FightCharactor);

        PlaceSkeletonWhenContactWithCharactor(charactor.hole, enemy);
        yield return new WaitForSeconds(0.2f);

        charactor.head.BothLookCenter(charactorAI, enemyAI);

        //yield return StartCoroutine(BalanceThenSolidify(enemyAI));

        bool enemyDead = false;
        float initialHealth = 0;
        bool enemyEqualCharactorHealth = false;

        if ((enemy.IsNormalEnemy() == true && enemy.weaponType != WeaponType.NoneWeapon) ||
            enemy.boss != null ||
            enemy.wolf)
        ///Là những con enemy thuần túy
        {
            if (currentEnemyFighting == null || currentEnemyFighting.gameObject.activeSelf == false)
            {
                currentEnemyFighting = enemy.enemy;
            }

            initialHealth = currentEnemyFighting.Health;
            enemyEqualCharactorHealth = (charactor.Health == currentEnemyFighting.Health);

            int fightingCount = enemy.GetType() == typeof(Boss) ? 4 : 2;
            float factor = fightingCount == 4 ? SectionSettings.fightingTime / 1.5f : SectionSettings.fightingTime;

            if (currentEnemyFighting != null && currentEnemyFighting.gameObject.activeSelf)
            {
                yield return new WaitUntil(() => currentEnemyFighting.mammalAI.IsAstarCompletelyStop());
                currentEnemyFighting.Fighting(fightingCount);
            }
            else
            {
                yield return new WaitUntil(() => enemy.mammalAI.IsAstarCompletelyStop());
                enemy.Fighting(fightingCount);
            }

            bool charactorCanAttack = !enemy.hole.blockWeapon || (enemy.hole.blockWeapon && charactor.weaponType == WeaponType.Archery);

            if (charactorCanAttack)
            {
                yield return new WaitUntil(() => charactorAI.IsAstarCompletelyStop());
                charactor.Fighting(fightingCount);
            }
            else
                charactor.DoIdle();

            if (charactorCanAttack)
                enemyDead = charactor.Health > currentEnemyFighting.Health;
            else
                enemyDead = false;

            float damage = enemyDead ? Mathf.CeilToInt(currentEnemyFighting.Health / fightingCount) : charactor.Health / fightingCount;

            float charactorDamage = charactor.Health / fightingCount;

            level.AnimFighting(enemy.skeletonController.listSkeleton, fightingCount, charactorCanAttack);
            level.AttackHealth(fightingCount, damage, charactorDamage, initialHealth, enemyDead, enemy, factor, charactorCanAttack, enemyAI.damageEntity);
            yield return new WaitForSeconds(fightingCount * factor);
            yield return new WaitUntil(() => level.coFightingAnim == null);
        }
        else if (enemy.princess != null || (enemy.IsTrap() && enemy.trap.type == eTrapType.Khobau))
        ///Cứu công chúa hoặc Ăn kho báu
        {
            yield return new WaitUntil(() => !level.enemyAttack);

            StopCurrentMoveToMeet();
            enemyAI.StopCurrentMoveToMeet();
            StopAstar();
            enemyAI.StopAstar();
            RemoveMammalReachable(enemy.mammalAI);
            if (currentAiming != null) currentAiming = null;
            if (nearestMammalAI != null) nearestMammalAI = null;

            enemyDead = true;
        }
        else
        ///Đánh mấy con mà có tăng máu như là : con tin
        {
            GameController.instance.PlaceMergeAnim(charactor.gameObject);
            charactor.VictoryAnimWhenMerge();
            enemyDead = true;
            charactor.Health += enemy.Health;

            enemy.hole.DoLose(charactor, false);

            SoundManager.instance.PlayAudioClip(SoundManager.instance.bonusX2Army);

            yield return new WaitForSeconds(1f);
        }

        level.moving = false;

        charactor.skeletonController.RestoreCached(true, false);
        enemy.skeletonController.RestoreCached(true, false);

        if (enemyDead)
        ///Kill được mammalAI
        {
            enTiAttackable.RemoveAttack(enemy);
            enemy.hole.DoLose(charactor, false);
            RemoveMammalReachable(enemy.mammalAI);

            yield return new WaitForSeconds(0.25f);

            if (charactor.UpgradeLevel(initialHealth))
            {
                GameController.instance.PlaceLevelUpAnim(charactor.gameObject);
            }

            charactor.UpdateCharactorObj();

            if (level.swapCharactorWeapon)
            {
                GameController.instance.SwapCharactorWeapon(enemy.weaponType);
            }

            charactor.healthController.UpdateHealth();

            enemy.hole.DoLose(charactor, false);
            charactor.DoWin();

            yield return new WaitUntil(() => coBalancing == null);
            yield return new WaitUntil(() => !level.enemyAttack);

            if (level.isWin)
            {
                level.isWin = false;
                GameController.instance.DoWin();
                level.isWin = true;

                charactor.VictoryAnim();
                if (enemy.princess != null)
                    enemy.DoWin(charactor);

                if (enemy.GetType() == typeof(Boss))
                {
                    level.AllEnemyLoseAnim();
                }
            }
            else if (!level.listWinHole.Exists(item => !item.IsPassed))
            {
                GameController.instance.DoWin();
                level.isWin = true;

                charactor.VictoryAnim();

                if (enemy.GetType() == typeof(Boss))
                {
                    level.AllEnemyLoseAnim();
                }
            }

            if (!level.isWin && !level.isLose && !IsContainAnyReachable() && NearestReachableFlag() != null)
                MoveToMeet(NearestReachableFlag().transform);
        }
        else
        {
            level.isLose = true;
            charactor.hole.DoLose(enemy, false);
            ForceLoseGame();

            if (enemyEqualCharactorHealth)
            {
                enemy.hole.DoLose(charactor, false);
            }
            else
            {
                enemy.DoWin(charactor);
            }

            Vector3 p = charactor.transform.position;
            if (!enemyEqualCharactorHealth) p.x -= 0.5f;

            level.dieSkeleton.transform.position = p;
            level.dieSkeleton.AnimationState.SetAnimation(0, "die_enemy", false);
        }

        level.isCollisionWithTrap = false;

        Liquify();
        StopAstar();
        enemyAI.Liquify();
        ///Hóa thể rắn        

        RemoveMammalStatus(eMammalStatus.FightEnemy);
        enemyAI.RemoveMammalStatus(eMammalStatus.FightCharactor);

        coFightEnemy = null;
        yield break;
    }
    void PlaceSkeletonWhenContactWithCharactor(Hole hole, BaseMammal other)
    {
        if (other.attackable.princess != null) return;
        if (KhoBauTrap.IsKhoBauTrap(other.transform)) return;
        hole.Place(charactor.skeletonController, -1);
        hole.Place(other.skeletonController, 1);
    }
    IEnumerator BalanceThenSolidify(EnemyAIController enemyAI)
    {
        Balance(enemyAI, mammal.head.Direction());
        yield return new WaitUntil(() => coBalancing == null);

        Solidify();
        enemyAI.Solidify();
        yield break;
    }
    public void StopFight()
    {
        level.StopFight();

        if (coFightEnemy != null)
        {
            StopCoroutine(coFightEnemy);
            coFightEnemy = null;
        }

        RemoveMammalStatus(eMammalStatus.FightEnemy);
    }
    public void ForceFight()
    {
        SetStatus(eMammalStatus.Idle);
        charactorAIController.FightNextEnemy();
    }

    public void AnimFighting(EnemyAIController enemy, bool charactorAttack)
    {
        if (coFightingAnim != null)
        {
            StopCoroutine(coFightingAnim);
            coFightingAnim = null;
        }
        coFightingAnim = StartCoroutine(IEFightingAnim(enemy, enemy.enemy.boss != null, charactorAttack));
    }

    IEnumerator IEFightingAnim(EnemyAIController enemyAI, bool isBoss, bool charactorAttack)
    {
        float factor = isBoss ? 1.5f : 1f;
        yield return new WaitForSeconds(0.5f / factor);

        while (this.fightingAnimCount < this.fightingCount)
        {
            yield return new WaitForEndOfFrame();
            yield return StartCoroutine(C_Strike(charactorAttack, enemyAI));
            this.fightingAnimCount += 1;
        }
    }

    IEnumerator C_Strike(bool charactorAttack, EnemyAIController enemyAI)
    {
        List<GameObject> listObj = new List<GameObject>();

        List<SkeletonGraphic> activeCharactorSkeletons = charactor.skeletonController.GetActiveSkeletons();
        activeCharactorSkeletons.ForEach(item => listObj.Add(item.gameObject));
        GameController.instance.PlaceHitAnim(true, listObj);

        charactor.head.Toward(enemyAI.transform);
        enemyAI.enemy.head.Toward(charactor.transform);

        if (charactorAttack)
        {
            listObj.Clear();
            List<SkeletonGraphic> activeEnemySkeletons = charactor.skeletonController.GetActiveSkeletons();
            activeEnemySkeletons.ForEach(item => listObj.Add(item.gameObject));
            GameController.instance.PlaceHitAnim(false, listObj);
        }

        yield return new WaitForSeconds(0.7f / factor);

        yield break;
    }

    IEnumerator C_SavePrincess(Trap targetTrap, Enemy enemy)
    {
        yield break;
    }
}