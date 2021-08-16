using DG.Tweening;
using Dijkstras;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;

public class Level1_1 : Level
{
    [Space(10)]
    [SerializeField] SkeletonGraphic waterHitSkeleton = null;

    Hole targetHole;
    bool compleleMoving = true;    

    public void MouseDown()
    {
        if (!readyPlay || !compleleMoving || fighting || isWin || isLose || enemyAttack) return;

        isMousePressed = ((targetHole = inputManager.GetTouchObject<Hole>()) != null) ? true : false;
        if (isMousePressed && !targetHole.IsPassed)
        {
            Hole startHole = charactor.hole;
            charactor.targetHole = targetHole;

            Path movingPath = graph.GetShortestPath(startHole, targetHole);

            if (movingPath.m_Nodes.Count <= 1)
            {
                if (Teleport.instance != null && !Teleport.instance.CanTeleport(targetHole))
                {
                    if (!targetHole.connections.Exists(item => item == startHole))
                    {
                        isMousePressed = false;
                        return;
                    }
                }
                else
                {
                    isMousePressed = false;
                    return;
                }
            }

            if (targetHole.enemyAttackable != null && targetHole.enemyAttackable.GetType().Equals(typeof(Wolf)))
            {
                Wolf wolf = (Wolf)targetHole.enemyAttackable;
                wolf.StopMove();
                wolf.head.Toward(charactor.transform);
            }

            moving = true;
            compleleMoving = false;

            PlaceWayLine(movingPath);

            movingPath.m_Nodes.RemoveAt(0);
            StopCoMoving();
            //Nếu trước đó có CoMoving thì stop để chuẩn bị bắt đầu cái mới

            levelHint.HideHand();

            charactor.head.Toward(targetHole.transform);
            coMoving = StartCoroutine(IEMoving(movingPath));
            PlaceFocusAnim(targetHole);

            if (targetHole.enemyAttackable != null && !targetHole.enemyAttackable.GetType().Equals(typeof(Trap)))
            {
                System.Type type = targetHole.enemyAttackable.GetType();

                switch (type.ToString())
                {
                    case "Boss":
                        Boss b = (Boss)targetHole.enemyAttackable;
                        if (b.hasDangerZone)
                        {
                            b.StopDangerZone();
                            b.fighting = true;
                            b.DoIdle();
                        }
                        break;
                    case "Wolf":
                        Wolf w = (Wolf)targetHole.enemyAttackable;
                        if (w.hasDangerZone)
                            w.StopDangerZone();
                        break;
                    case "Enemy":
                        Enemy e = (Enemy)targetHole.enemyAttackable;
                        e.fighting = true;
                        break;
                }
            }

            if (targetHole.enemyAttackable != null && targetHole.enemyAttackable.GetType().Equals(typeof(Boss)))
            {
                Boss b = (Boss)targetHole.enemyAttackable;
                if (b.hasDangerZone)
                {
                    b.StopDangerZone();
                    b.DoIdle();
                }
            }

            EventDispacher.Dispatch(EventName.OnTap);
        };
    }

    IEnumerator IEMoving(Path path)
    {
        int count = 0;

        while (count < path.m_Nodes.Count && !isWin && !isLose)
        {
            Hole endHole = path.m_Nodes[count];

            List<Hole> holes = new List<Hole>();
            for (int i = count; i < path.m_Nodes.Count; i++)
            {
                holes.Add(path.m_Nodes[i]);
            }

            charactor.targetHole = GetBossAttackHole(holes);
            if (charactor.targetHole != null)
                charactor.head.Toward(charactor.targetHole.transform);

            float distance = Vector2.Distance(charactor.transform.position, endHole.transform.position);
            charactor.SetMoving();

            // Check collision with trap
            CheckCollisionWithTrap(endHole);
            Vector3 touchHole = CalculateEndholePosition(charactor.transform, endHole.transform, holePositionOffset);

            List<Vector3> pathPoints = new List<Vector3>();
            pathPoints.Add(charactor.transform.position);
            if (isCollisionWithTrap)
            {
                pathPoints.Add(trapHitPoint);
            }
            else pathPoints.Add(endHole.transform.position);

            charactor.transform.DOMove(pathPoints[pathPoints.Count - 1], distance / 3.5f)
                .SetEase(Ease.Linear);

            if (endHole.enemyAttackable != null &&
                !endHole.IsPassed &&
                endHole.enemyAttackable.IsEnemy())
            {
                Enemy enemy = (Enemy)endHole.enemyAttackable;
                enemy.head.Toward(charactor.transform);

                TaskUtil.Delay(this, delegate
                {
                    charactor.head.Toward(enemy.head);
                    endHole.Place(charactor.skeletonController, enemy.skeletonController);
                }, 0.7f * (distance / 3.5f));
            }

            yield return new WaitForSeconds(distance / 3.5f);

            EventDispacher<Hole>.Dispatch(EventName.OnMoveToHole, endHole);

            if (isCollisionWithTrap &&
                targetTrap.type == eTrapType.WaterHole &&
                !targetTrap.isCollision)
            ///Đi ngang Trap hồ nước
            {
                charactor.head.Toward(targetTrap.transform);

                targetTrap.isCollision = true;

                float attackHealth = targetTrap.AttackHealth(charactor.Health);
                charactor.Health = attackHealth;

                //waterHitSkeleton.gameObject.SetActive(true);
                //waterHitSkeleton.AnimationState.SetAnimation(0, "hit", false);
                //waterHitSkeleton.transform.position = trapHitPoint;
                LevelPrefabController.instance.WaterAt(trapHitPoint);

                charactor.AnimTextHitTrap();
                charactor.HitTrapAnim();

                pathPoints = new List<Vector3>();
                pathPoints.Add(charactor.transform.position);
                pathPoints.Add(endHole.transform.position);

                distance = Vector2.Distance(pathPoints[0], pathPoints[1]);

                charactor.transform.DOMove(pathPoints[pathPoints.Count - 1], distance / 3.5f).SetEase(Ease.Linear).timeScale = 0.25f;

                Trap t = targetTrap;

                TaskUtil.Delay(this, delegate
                {
                    t.isCollision = false;
                }, 2f);

                yield return new WaitForSeconds(0.5f);
            }
            else if (endHole.enemyAttackable != null &&
                endHole.enemyAttackable.gameObject.activeSelf == true &&
                endHole.enemyAttackable.IsTrap())
            ///Đi ngang các Trap khác (PunjiStick, Stone)
            ///Cần viết thêm khi có các Trap mới 
            {
                Trap trap = (Trap)endHole.enemyAttackable;

                if (!trap.isCollision)
                {
                    float attackHealth = trap.AttackHealth(charactor.Health);
                    charactor.head.Toward(endHole.enemyAttackable.transform);

                    if (trap.requireToFight)
                    {
                        charactor.Fighting(2);
                        trap.Attack();

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
                    ///Lúc này đánh vào các trap tăng/giảm máu trực tiếp
                    {
                        trap.Attack();

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
                        charactor.transform.DOMove(endHole.transform.position, 0.25f);

                        yield return new WaitForSeconds(0.25f);

                        charactor.DoWin();
                        endHole.DoLose(charactor);

                        yield return new WaitForSeconds(0.25f);

                        charactor.hole = targetHole;
                    }
                }
            }
            else if (endHole.enemyAttackable != null &&
                endHole.enemyAttackable.gameObject.activeSelf == true &&
                endHole.enemyAttackable.IsEnemy() &&
                !endHole.IsPassed &&
                !endHole.enemyAttackable.ignoreAttack)
            ///Đi vào hole có enemy
            {
                Enemy enemy = (Enemy)endHole.enemyAttackable;

                bool? enemyDead = null;
                ///true: enemy chết
                ///false: charactor chết
                ///null: không ai tấn công, ko ai chết
                float initialHealth = enemy.Health;
                bool enemyEqualCharactorHealth = (charactor.Health == enemy.Health);

                if (enemy.weaponType != WeaponType.NoneWeapon)
                ///Enemy có vũ khí
                {
                    PlaceHealthText(endHole);

                    yield return new WaitForSeconds(0.25f);

                    int fightingCount = enemy.GetType() == typeof(Boss) ? 4 : 2;
                    float factor = fightingCount == 4 ? SectionSettings.fightingTime / 1.5f : SectionSettings.fightingTime;

                    charactor.head.Toward(endHole.enemyAttackable.head);

                    bool charactorCanAttack = !endHole.blockWeapon ||
                        (endHole.blockWeapon && charactor.weaponType == WeaponType.Archery);

                    bool enemyCanAttack = !endHole.blockWeapon ||
                        (endHole.blockWeapon && enemy.weaponType == WeaponType.Archery);

                    if (charactorCanAttack) charactor.Fighting(fightingCount);
                    else charactor.DoIdle();

                    if (enemyCanAttack) enemy.Fighting(fightingCount);
                    else enemy.DoIdle();

                    if (charactorCanAttack && enemyCanAttack)
                        enemyDead = charactor.Health > enemy.Health;
                    else if (charactorCanAttack && !enemyCanAttack)
                        enemyDead = true;
                    else if (!charactorCanAttack && enemyCanAttack)
                        enemyDead = false;
                    else
                        enemyDead = null;

                    float damage = enemyDead == true ? Mathf.CeilToInt(enemy.Health / fightingCount) : charactor.Health / fightingCount;

                    float charactorDamage = charactor.Health / fightingCount;

                    AnimFighting(enemy.skeletonController.listSkeleton, fightingCount, charactorCanAttack, enemyCanAttack);
                    AttackHealth(fightingCount, damage, charactorDamage, initialHealth, enemyDead, enemy, factor, charactorCanAttack);

                    yield return new WaitForSeconds(fightingCount * factor);
                }
                else
                ///Enemy không có vũ khí
                {
                    GameController.instance.PlaceMergeAnim(charactor.gameObject);
                    charactor.VictoryAnimWhenMerge();
                    enemyDead = true;
                    charactor.Health += enemy.Health;

                    enemy.hole.DoLose(charactor);

                    SoundManager.instance.PlayAudioClip(SoundManager.instance.bonusX2Army);

                    yield return new WaitForSeconds(1f);
                }

                charactor.healthController.UpdateHealth(true, false);
                enemy.healthController.UpdateHealth(true, false);

                moving = false;

                if (enemyDead == true)
                {
                    yield return new WaitForSeconds(0.25f);

                    if (charactor.UpgradeLevel(initialHealth))
                    {
                        GameController.instance.PlaceLevelUpAnim(charactor.gameObject);
                    }

                    if (swapCharactorWeapon)
                    {
                        GameController.instance.SwapCharactorWeapon(enemy.weaponType);
                    }

                    charactor.UpdateCharactorObj();
                    charactor.UpdateHealthTextPos();

                    endHole.DoLose(charactor);
                    charactor.DoWin();

                    yield return new WaitUntil(() => !enemyAttack);

                    if (!listWinHole.Exists(item => !item.IsPassed))
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

                    if (enemy.GetType() == typeof(Wolf) && endHole == targetHole && listWinHole.Count == 1)
                    // Move to princess when attack wolf
                    {
                        List<Hole> neighbor = endHole.connections;
                        Hole princessHole = neighbor.Find(item => !item.IsPassed &&
                        item.enemyAttackable != null &&
                        item.enemyAttackable.GetType().Equals(typeof(Princess)));

                        if (princessHole != null)
                        {
                            yield return new WaitForSeconds(0.25f);

                            distance = Vector3.Distance(charactor.transform.position, listWinHole[0].transform.position);

                            charactor.head.Toward(enemy.transform);
                            charactor.SetMoving();
                            enemy.head.HeadTowardCharactor();

                            pathPoints = new List<Vector3>();
                            pathPoints.Add(charactor.transform.position);
                            pathPoints.Add(listWinHole[0].transform.position);

                            charactor.transform.DOMove(pathPoints[pathPoints.Count - 1], distance / 3.5f).SetEase(Ease.Linear);

                            if (listWinHole[0].enemyAttackable != null && !listWinHole[0].IsPassed)
                            {
                                TaskUtil.Delay(this, delegate
                                {
                                    GameController.instance.DoWin();

                                    Enemy princess = (Enemy)listWinHole[0].enemyAttackable;
                                    listWinHole[0].Place(charactor.skeletonController, princess.skeletonController);
                                    princess.DoWin(charactor);
                                }, 0.7f * (distance / 3.5f));
                            }

                            yield return new WaitForSeconds(distance / 3.5f);
                            
                            StopCoMoving();

                            isWin = true;
                            charactor.VictoryAnim();
                        }
                    }

                    endHole.IsPassed = true;
                }
                else if (enemyDead == false)
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
                    StopCoMoving();
                }
                else
                ///enemyDead = null
                {

                }

                yield return new WaitForSeconds(0.5f);

                isCollisionWithTrap = false;
                charactor.hole = endHole;                
            }
            else if (endHole.enemyAttackable != null && !endHole.IsPassed && endHole.enemyAttackable.ignoreAttack && endHole.enemyAttackable.GetType().Equals(typeof(Princess)))
            ///Cứu công chúa
            {
                charactor.head.Toward(endHole.enemyAttackable.transform);

                moving = false;
                fighting = false;

                yield return new WaitUntil(() => !enemyAttack);

                Princess princess = (Princess)endHole.enemyAttackable;
                charactor.VictoryAnim();
                GameController.instance.DoWin();

                isWin = true;
                princess.DoWin(charactor);

                StopCoMoving();
            }
            else
            ///Hole trống
            {
                charactor.head.Toward(endHole.transform);
                charactor.hole = endHole;
            }

            isCollisionWithTrap = false;
            count += 1;
        }

        GameController.instance.HideArrowObj();

        moving = false;
        compleleMoving = true;
        StopCoMoving();

        ((TapKillManager)levelController).afterFinish_TapKill.Invoke(charactor.targetHole);

        yield break;
    }
    [ContextMenu("TryStart")]
    public override void Start()
    {
        DoneStartLevel = false;
        base.Start();

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
        return "_1_1";
    }
}
