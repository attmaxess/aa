using DG.Tweening;
using Spine.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level1_2 : Level
{
    [Space(10)]
    [SerializeField] SkeletonGraphic waterHitSkeleton = null;

    Hole targetHole;

    public void MouseDown()
    {
        if (!readyPlay || moving || fighting || isWin || isLose || enemyAttack) return;

        isMousePressed = ((targetHole = inputManager.GetTouchObject<Hole>()) != null) ? true : false;

        if (isMousePressed && !targetHole.IsPassed)
        {
            moving = true;
            Hole startHole = charactor.hole;
            charactor.targetHole = targetHole;

            // Check collision with trap
            CheckCollisionWithTrap(targetHole);

            PlaceWayLine(charactor.skeletonController.listSkeleton[0].transform.position, targetHole.transform.position);

            StopCoMoving();

            levelHint.HideHand();

            coMoving = StartCoroutine(IEMoving());
            charactor.head.Toward(targetHole.transform);

            PlaceFocusAnim(targetHole);

            if (targetHole.enemyAttackable != null && !targetHole.enemyAttackable.GetType().Equals(typeof(Trap)))
            {
                System.Type type = targetHole.enemyAttackable.GetType();

                switch (type.ToString())
                {
                    case "Boss":
                        Boss b = (Boss)targetHole.enemyAttackable;
                        if (b.hasDangerZone)
                            b.StopDangerZone();

                        b.fighting = true;
                        b.DoIdle();
                        break;
                    case "Wolf":
                        Wolf w = (Wolf)targetHole.enemyAttackable;
                        if (w.hasDangerZone)
                            w.StopDangerZone();
                        break;
                    case "Enemy":
                        Enemy e = (Enemy)targetHole.enemyAttackable;

                        e.fighting = true;

                        if (e.hasDangerZone)
                        {
                            e.StopDangerZone();
                            e.DoIdle();
                        }
                        break;
                }
            }

            EventDispacher.Dispatch(EventName.OnTap);
        };
    }

    IEnumerator IEMoving()
    {
        float dis = targetHole.transform.position.x - charactor.transform.position.x;

        if (dis > 0)
            targetHole.fightingDirection = FightingDirection.Right;
        else
            targetHole.fightingDirection = FightingDirection.Left;

        charactor.SetMoving();

        List<Vector3> pathPoints = new List<Vector3>();
        pathPoints.Add(charactor.transform.position);

        if (isCollisionWithTrap) pathPoints.Add(trapHitPoint);
        else pathPoints.Add(targetHole.transform.position);

        float distance = Vector2.Distance(pathPoints[0], pathPoints[1]);

        if (charactor.targetHole != null)
            charactor.head.Toward(charactor.targetHole.transform);

        charactor.transform.DOPath(pathPoints.ToArray(), distance / 3.5f, PathType.CatmullRom).SetEase(Ease.Linear).OnUpdate(delegate
        {

        });

        float timeDelayPlace = distance > 4f ? 0.9f * (distance / 3.5f) : 0.7f * (distance / 3.5f);

        if (targetHole.enemyAttackable != null && !targetHole.IsPassed && !isCollisionWithTrap)
        {
            TaskUtil.Delay(this, delegate
            {
                Enemy enemy = (Enemy)targetHole.enemyAttackable;
                targetHole.Place(charactor.skeletonController, enemy.skeletonController);
                charactor.head.LookRight();
                enemy.head.LookLeft();
            }, timeDelayPlace);
        }

        yield return new WaitForSeconds(distance / 3.5f);

        EventDispacher<Hole>.Dispatch(EventName.OnMoveToHole, targetHole);

        if (isCollisionWithTrap)
        {
            if (!targetTrap.isCollision)
            {
                float attackHealth = targetTrap.AttackHealth(charactor.Health);
                charactor.head.Toward(targetTrap.transform);

                if (targetTrap.type == eTrapType.WaterHole)
                {
                    charactor.Health -= attackHealth;
                    waterHitSkeleton.gameObject.SetActive(true);
                    waterHitSkeleton.AnimationState.SetAnimation(0, "hit", false);

                    waterHitSkeleton.transform.position = trapHitPoint;

                    charactor.HitTrapAnim();
                    charactor.AnimTextHitTrap();

                    if (charactor.Health > 0)
                    {
                        Debug.Log("Water");

                        pathPoints = new List<Vector3>();
                        pathPoints.Add(charactor.transform.position);
                        pathPoints.Add(targetHole.transform.position);

                        distance = Vector2.Distance(pathPoints[0], pathPoints[1]);

                        charactor.transform.DOMove(pathPoints[pathPoints.Count - 1], distance / 3.5f).SetEase(Ease.Linear).timeScale = 0.25f;

                        Trap t = targetTrap;

                        TaskUtil.Delay(this, delegate
                        {
                            t.isCollision = false;
                        }, 2f);

                        yield return new WaitForSeconds(0.5f);
                    }
                }
                else if (targetTrap.type == eTrapType.Stone ||
                    targetTrap.type == eTrapType.PunjiStick)
                {
                    charactor.Fighting(2);
                    targetTrap.Attack();
                    charactor.head.Toward(targetTrap.transform);

                    if (targetHole.enemyAttackable == targetTrap)
                        GameController.instance.HideArrowObj();

                    TaskUtil.Delay(this, delegate
                    {
                        charactor.AnimTextHitTrap();
                    }, 0.5f);

                    int fightingCount = 2;
                    float factor = fightingCount == 4 ? SectionSettings.fightingTime / 1.5f : SectionSettings.fightingTime;

                    int currentFightingCount = 0;
                    float damage = Mathf.CeilToInt(attackHealth / 2);
                    float initialHealth = attackHealth;

                    charactorAI?.AnimFightingTrap();

                    while (currentFightingCount < fightingCount)
                    {
                        yield return new WaitForSeconds(factor);

                        if (currentFightingCount == fightingCount - 1)
                        {
                            charactor.Health -= (initialHealth - currentFightingCount * damage);
                        }
                        else
                        {
                            charactor.Health -= damage;
                        }

                        currentFightingCount += 1;
                    }
                }
                else
                {
                    targetTrap.Attack();

                    TaskUtil.Delay(this, delegate
                    {
                        charactor.AnimTextHitTrap();
                    }, 0.5f);

                    charactor.Health = attackHealth;
                }

                isCollisionWithTrap = false;

                if (charactor.Health == 0)
                {
                    isLose = true;

                    Vector3 p = charactor.transform.position;
                    p.x -= 0.5f;

                    dieSkeleton.transform.position = p;
                    dieSkeleton.AnimationState.SetAnimation(0, "die_enemy", false);

                    charactor.transform.DOKill();
                    charactor.DoLose(charactor.hole);

                    TaskUtil.Delay(this, delegate
                    {
                        GameController.instance.DoLose();
                    }, 0.5f);

                    StopCoMoving();
                }
                else
                {                    
                    if (targetHole.enemyAttackable != targetTrap)
                    {
                        charactor.SetMoving();
                        charactor.transform.DOKill();

                        pathPoints = new List<Vector3>();
                        pathPoints.Add(charactor.transform.position);
                        pathPoints.Add(targetHole.transform.position);

                        distance = Vector2.Distance(pathPoints[0], pathPoints[1]);

                        charactor.transform.DOMove(pathPoints[pathPoints.Count - 1], distance / 3.5f).SetEase(Ease.Linear).timeScale = 1f;

                        float time = distance > 4f ? 0.9f * (distance / 3.5f) : 0.7f * (distance / 3.5f);

                        if (targetHole.enemyAttackable != null && !targetHole.IsPassed)
                        {
                            TaskUtil.Delay(this, delegate
                            {
                                Enemy enemy = (Enemy)targetHole.enemyAttackable;
                                targetHole.Place(charactor.skeletonController, enemy.skeletonController);
                                charactor.head.LookRight();
                                enemy.head.LookLeft();
                            }, time);
                        }
                        yield return new WaitForSeconds(distance / 3.5f);

                        StartCoroutine(IEMoveToTargetHole());
                    }
                    else
                    {
                        charactor.healthController.UpdateHealth(true, false);
                        charactor.DoWin();
                        targetHole.DoLose();

                        isCollisionWithTrap = false;
                        moving = false;
                        charactor.hole = targetHole;
                    }
                }
            }
        }
        else
        {
            yield return StartCoroutine(IEMoveToTargetHole());
        }

        StopCoMoving();
        ((TapKillManager)levelController).afterFinish_TapKill.Invoke(charactor.hole);

        yield break;
    }

    IEnumerator IEMoveToTargetHole()
    {
        Enemy enemy = (Enemy)targetHole.enemyAttackable;
        if (enemy == null) yield break;

        if (enemy.weaponType != WeaponType.NoneWeapon)
        {
            PlaceHealthText(targetHole);

            yield return new WaitForSeconds(0.25f);

            int fightingCount = targetHole.enemyAttackable.GetType() == typeof(Boss) ? 4 : 2;
            float factor = fightingCount == 4 ? SectionSettings.fightingTime / 1.5f : SectionSettings.fightingTime;

            enemy.Fighting(fightingCount);
            charactor.Fighting(fightingCount);

            bool enemyDead = enemy.weaponType == WeaponType.NoneWeapon ? true : charactor.Health > enemy.Health;
            float damage = enemyDead ? Mathf.CeilToInt(enemy.Health / fightingCount) : charactor.Health / fightingCount;
            float initialHealth = enemy.Health;
            float charactorDamage = charactor.Health / fightingCount;

            bool enemyEqualCharactorHealth = (charactor.Health == enemy.Health);

            AnimFighting(enemy.skeletonController.listSkeleton, fightingCount);

            AttackHealth(fightingCount, damage, charactorDamage, initialHealth, enemyDead, enemy, factor);

            yield return new WaitForSeconds(fightingCount * factor);

            if (enemyDead)
            {
                enemy.hole.DoLose(charactor);

                yield return new WaitForSeconds(0.25f);

                if (charactor.UpgradeLevel(initialHealth))
                {
                    GameController.instance.PlaceLevelUpAnim(charactor.gameObject);
                }

                charactor.UpdateCharactorObj();
                charactor.UpdateHealthTextPos();

                targetHole.DoLose();

                bool isWinLevel = !listWinHole.Exists(item => !item.IsPassed);

                if (!isWinLevel)
                {
                    charactor.DoWin();
                }

                if (isWinLevel)
                {
                    GameController.instance.DoWin();
                    StopCoMoving();

                    isWin = true;

                    charactor.VictoryAnim();

                    if (enemy.GetType() == typeof(Boss))
                    {
                        AllEnemyLoseAnim();
                    }
                }                
            }
            else
            {
                isLose = true;

                charactor.DoLose(charactor.hole);
                if (enemyEqualCharactorHealth)
                {
                    enemy.hole.DoLose(charactor);
                }
                else
                {
                    enemy.DoWin();
                }

                Vector3 p = charactor.transform.position;
                if (!enemyEqualCharactorHealth) p.x -= 0.5f;

                dieSkeleton.transform.position = p;
                dieSkeleton.AnimationState.SetAnimation(0, "die_enemy", false);

                yield return new WaitForSeconds(0.5f);

                GameController.instance.DoLose();
                try
                {
                    StopCoMoving();
                }
                catch (Exception e) { }
            }

            isCollisionWithTrap = false;
            moving = false;
            charactor.hole = targetHole;
        }
        else
        {
            GameController.instance.PlaceMergeAnim(charactor.gameObject);
            charactor.VictoryAnimWhenMerge();
            charactor.Health += enemy.Health;

            enemy.hole.DoLose(charactor);

            SoundManager.instance.PlayAudioClip(SoundManager.instance.bonusX2Army);

            moving = false;

            yield return new WaitForSeconds(1f);

            if (charactor.UpgradeLevel(enemy.Health))
            {
                GameController.instance.PlaceLevelUpAnim(charactor.gameObject);
            }

            charactor.UpdateCharactorObj();
            charactor.UpdateHealthTextPos();

            charactor.DoWin();
            targetHole.DoLose();
        }
    }
    [ContextMenu("TryStart")]
    public override void Start()
    {
        DoneStartLevel = false;
        base.Start();
        //FitCharactorIntoFirstHole();

        StartCoroutine(C_Start());
    }
    IEnumerator C_Start()
    {
        levelController.levelStatus = eLevelStatus.Busy;
        ((TapKillManager)levelController).afterFinish_TapKill += ReScanPath;
        levelController.levelStatus = eLevelStatus.Idle;
        ReScanPath(charactor.targetHole);
        DoneStartLevel = true;
        yield break;
    }
    public override string GetLevelNameType()
    {
        return "_1_2";
    }
}
