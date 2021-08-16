using DG.Tweening;
using Dijkstras;
using Pathfinding;
using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
# if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.UI.Extensions;
using static HealthUltis;

public class Level : BaseLevelProperties
{
    public bool useDebugLog = false;
    public bool DoneStartLevel = true;

    public TypeLevel typeLevel;
    public InputManager inputManager;
    public Charactor charactor;

    [Space(10)]
    public Graph graph;

    [Space(10)]
    public SkeletonGraphic dieSkeleton;
    public SkeletonGraphic focusSkeleton;

    [Space(10)]
    public List<Enemy> listEnemies;
    public List<BaseMammal> listMammals;
    public List<MammalAIController> listAttackableAI;
    public List<Hole> listHoles;
    public List<Hole> listWinHole;
    public List<Way> listWay;
    public List<DrawMapUltis> drawMaps;

    public bool moving;
    public bool fighting;

    public delegate void OnPostWin(bool isWin);
    public OnPostWin postWin;

    public delegate void OnPostLose(bool isLose);
    public OnPostLose postLose;

    public delegate void OnPassingHole(Hole hole);
    public OnPassingHole onPostPassingHole;

    public bool isWin
    {
        get { return _isWin; }
        set
        {
            _isWin = value;
            if (postWin != null)
                postWin.Invoke(value);
        }
    }
    [SerializeField] bool _isWin;
    public bool isLose
    {
        get { return _isLose; }
        set
        {
            _isLose = value;
            if (postLose != null)
                postLose.Invoke(value);
        }
    }
    [SerializeField] bool _isLose;

    public bool isCollisionWithTrap;
    protected Vector3 trapHitPoint;
    protected Trap targetTrap;

    protected bool isMousePressed;
    public bool enemyAttack;

    public bool readyPlay = true;
    public bool swapCharactorWeapon = false;

    protected Coroutine coMoving;
    protected Coroutine coBalancing;
    float minY = 0.1f;
    float minDistance = .5f;

    public Coroutine coFightingAnim;
    public Coroutine coAttackHealth;
    public CUIMapBound mapBound
    {
        get
        {
            if (_mapBound == null) _mapBound = GetComponentInChildren<CUIMapBound>();
            return this._mapBound;
        }
    }
    CUIMapBound _mapBound;

    protected Coroutine coEnemyAttackPlayer;

    [Space(10)]
    [SerializeField] List<CollisionEmoji> collisionEmojiAnims = null;
    public CharactorAIController charactorAI
    {
        get
        {
            if (_charactorAI == null) _charactorAI = charactor.GetComponent<CharactorAIController>();
            return this._charactorAI;
        }
    }
    CharactorAIController _charactorAI;

    [Space(10)]
    [ReadOnly] public List<GameObject> AstarObstacles;
    [ReadOnly] public bool DoneReScanAstar = true;
    [HideInInspector]
    public bool HasNewPath = false;
    [HideInInspector]
    public List<bool> CurrentWalkable = new List<bool>();
    [HideInInspector]
    public List<NodeViewData> nodesToView = new List<NodeViewData>();

    Coroutine coAstarScan;

    public bool DonePokeAllMammals = true;
    [HideInInspector]
    public bool DoneJoinEnemy = true;
    /// <summary>
    /// Class khác gọi vào được
    /// </summary>
    public delegate void OnPostReScanPath();
    public OnPostReScanPath onPostReScanPath;
    /// <summary>
    /// Class khác gọi vào được
    /// </summary>
    List<IsometricController> isometrics = new List<IsometricController>();
    /// <summary>
    /// Class khác gọi vào được
    /// </summary>
    List<BarrierRay> barrierrays = new List<BarrierRay>();
    /// <summary>
    /// Xếp layer khi có chuyển động
    /// </summary>
    List<string> emojiAnim = new List<string>() { "angry", "anim", "crazy", "hand_some", "tired" };
    /// <summary>
    /// Emoji của gì thì chưa biết
    /// </summary>
    /// <param name="enemy"></param>    

    public const float holePositionOffset = .2f;
    const float enemyPositionOffset = 3f;

    public void ShowEmoji(Enemy enemy)
    {
        CollisionEmoji emoji = collisionEmojiAnims.Find(item => item.enemy == enemy);

        if (emoji != null)
        {
            emoji.emojiSkeleton.AnimationState.SetAnimation(0, emojiAnim[Random.Range(0, emojiAnim.Count)], false);
        }
    }
    public void HideEmoji(Enemy enemy)
    {
        CollisionEmoji emoji = collisionEmojiAnims.Find(item => item.enemy == enemy);

        if (emoji != null)
        {
            emoji.emojiSkeleton.AnimationState.SetAnimation(0, "empty", true);
        }
    }
    #region Virtual Method
    public void PlaceFocusAnim(Hole endHole, bool show = true)
    {
        focusSkeleton.gameObject.SetActive(show);
        focusSkeleton.transform.position = endHole.HoleCenter();
    }
    public void HideFocusAnim()
    {
        focusSkeleton.gameObject.SetActive(false);
    }
    public Hole GetHole(Enemy enemy)
    {
        if (enemy == null) return null;
        if (listHoles.Count == 0) return null;
        return listHoles.Find(item => item.enemyAttackable != null && item.enemyAttackable.transform == enemy.transform);
    }
    public virtual void AttackHealth(
        int fightingCount,
        float damage,
        float charactorDamage,
        float initialHealth,
        bool? enemyDead,
        Enemy enemy,
        float time,
        bool charactorAttack = true,
        EnemyAIController.OnDamageEntity onDamageEntity = null
        )
    {
        if (coAttackHealth != null)
        {
            StopCoroutine(coAttackHealth);
            coAttackHealth = null;
        }
        coAttackHealth = StartCoroutine(IEAttackHealth(fightingCount, damage, charactorDamage, initialHealth, enemyDead, enemy, time, charactorAttack, onDamageEntity));
    }
    IEnumerator IEAttackHealth(
        int fightingCount,
        float damage,
        float charactorDamage,
        float initialHealth,
        bool? enemyDead,
        Enemy enemy,
        float time,
        bool charactorAttack = true,
        EnemyAIController.OnDamageEntity onDamageEntity = null
        )
    {
        int currentFightingCount = 0;

        float initialCharactorHealth = charactor.Health;

        while (currentFightingCount < fightingCount)
        {
            yield return new WaitForSeconds(time);

            if (currentFightingCount == fightingCount - 1)
            {
                if (enemyDead == true)
                {
                    if (fightingCount != 4)
                    {
                        charactor.Health += (initialHealth - currentFightingCount * damage);
                        enemy.Health -= (initialHealth - currentFightingCount * damage);
                        if (onDamageEntity != null)
                            onDamageEntity.Invoke(initialHealth - currentFightingCount * damage);
                    }
                    else
                    {
                        enemy.Health -= ((initialHealth - currentFightingCount * damage));
                        if (onDamageEntity != null)
                            onDamageEntity.Invoke(initialHealth - currentFightingCount * damage);
                    }
                }
                else
                {
                    charactor.Health -= (initialHealth - currentFightingCount * damage);
                    if (fightingCount != 4 && charactorAttack)
                    {
                        enemy.Health -= initialCharactorHealth - currentFightingCount * charactorDamage;
                        if (onDamageEntity != null)
                            onDamageEntity.Invoke(initialCharactorHealth - currentFightingCount * charactorDamage);
                    }
                }
            }
            else
            {
                if (enemyDead == true)
                {
                    if (fightingCount != 4)
                    {
                        charactor.Health += damage;
                        enemy.Health -= damage;
                        if (onDamageEntity != null)
                            onDamageEntity.Invoke(damage);
                    }
                    else
                    {
                        enemy.Health -= damage;
                        if (onDamageEntity != null)
                            onDamageEntity.Invoke(damage);
                    }
                }
                else
                {
                    charactor.Health -= damage;
                    if (fightingCount != 4 && charactorAttack)
                    {
                        enemy.Health -= charactorDamage;
                        if (onDamageEntity != null)
                            onDamageEntity.Invoke(charactorDamage);
                    }
                }
            }

            currentFightingCount += 1;
        }
    }
    public void StopFight()
    {
        if (coFightingAnim != null)
        {
            StopCoroutine(coFightingAnim);
            coFightingAnim = null;
        }
        if (coAttackHealth != null)
        {
            StopCoroutine(coAttackHealth);
            coAttackHealth = null;
        }
    }
    protected void StopCoMoving()
    {
        if (coMoving != null)
        {
            StopCoroutine(coMoving);
            coMoving = null;
        }
    }
    public void EnemyAttackPlayer(Enemy enemy, Hole startHole, Hole targetHole)
    {
        enemyAttack = true;
        enemy.DoIdle();

        TaskUtil.Delay(this, delegate
        {
            enemy.head.Toward(charactor.transform);
            charactor.head.Toward(enemy.transform);

            List<Vector3> pathPoints = new List<Vector3>();
            Dijkstras.Path movingPath;

            pathPoints.Add(enemy.transform.position);

            float distance = 0f;
            if (graph == null)
            {
                Vector3 disVec = targetHole.transform.position - startHole.transform.position;
                float angle = Mathf.Atan2(disVec.y, disVec.x) * Mathf.Rad2Deg;
                float percentLerp = Mathf.Abs(angle) > 20 ? 0.8f : 0.5f;

                Vector3 v = TaskUtil.LerpByDistance(startHole.transform.position, targetHole.transform.position, percentLerp);
                pathPoints.Add(v);
            }
            else
            {
                movingPath = graph.GetShortestPath(startHole, targetHole);
                movingPath.m_Nodes.RemoveAt(0);
                movingPath.m_Nodes.ForEach(item => Debug.Log("Path " + item));
                for (int i = 0; i < movingPath.m_Nodes.Count; i++)
                {
                    if (i == movingPath.m_Nodes.Count - 1)
                    {
                        Vector3 disVec = movingPath.m_Nodes[i].transform.position - movingPath.m_Nodes[Mathf.Max(0, i - 1)].transform.position;
                        float angle = Mathf.Atan2(disVec.y, disVec.x) * Mathf.Rad2Deg;
                        float percentLerp = Mathf.Abs(angle) > 20 ? 0.8f : 0.5f;

                        Vector3 v = TaskUtil.LerpByDistance(movingPath.m_Nodes[Mathf.Max(0, i - 1)].transform.position, movingPath.m_Nodes[i].transform.position, percentLerp);

                        pathPoints.Add(v);
                    }
                    else
                    {
                        pathPoints.Add(movingPath.m_Nodes[i].transform.position);
                    }
                }
            }

            for (int i = 0; i < pathPoints.Count; i++)
            {
                distance += Vector2.Distance(pathPoints[i], pathPoints[Mathf.Min(i + 1, pathPoints.Count - 1)]);
            }

            if (coEnemyAttackPlayer != null)
            {
                StopCoroutine(coEnemyAttackPlayer);
                coEnemyAttackPlayer = null;
            }

            coEnemyAttackPlayer = StartCoroutine(IEEnemyAttackPlayer(enemy, pathPoints, distance));
        }, 0.75f);
    }

    public void StopEnemyAttack(Enemy enemy, Hole targetHole)
    {
        if (coEnemyAttackPlayer != null)
        {
            StopCoroutine(coEnemyAttackPlayer);
            coEnemyAttackPlayer = null;

            enemy.DoWin();

            Debug.Log("Stop");
        }
    }

    IEnumerator IEEnemyAttackPlayer(Enemy enemy, List<Vector3> pathPoints, float distance)
    {
        yield return new WaitForSeconds(0.25f);

        enemy.transform.DOPath(pathPoints.ToArray(), distance / 3.5f, PathType.CatmullRom).SetEase(Ease.Linear);

        enemy.SetMoving();

        yield return new WaitForSeconds(distance / 3.5f);

        enemy.head.Toward(charactor.transform);
        charactor.head.Toward(enemy.transform);

        if (GameController.instance.currentLevel.charactor.Health == 0)
        {
            enemy.DoWin();
            yield break;
        }

        enemy.Fighting(10);

        yield return new WaitUntil(() => !fighting && !moving);

        if (charactor.Health == 0) yield break;

        int fightingCount = (enemy.GetType().Equals(typeof(Boss))) ? 4 : 2;
        float factor = fightingCount == 4 ? SectionSettings.fightingTime / 1.5f : SectionSettings.fightingTime;

        charactor.Fighting(fightingCount);
        enemy.Fighting(fightingCount);

        bool bossDead = charactor.Health > enemy.Health;
        float damage = bossDead ? Mathf.CeilToInt(enemy.Health / fightingCount) : charactor.Health / fightingCount;
        float initialHealth = enemy.Health;
        float charactorDamage = charactor.Health / fightingCount;

        bool enemyEqualCharactorHealth = (charactor.Health == enemy.Health);

        AnimFighting(enemy.skeletonController.listSkeleton, fightingCount);

        AttackHealth(fightingCount, damage, charactorDamage, initialHealth, bossDead, enemy, factor);

        yield return new WaitForSeconds(fightingCount * factor);

        if (bossDead)
        {
            Hole hole = listHoles.Find(item => item.enemyAttackable == enemy);
            if (hole != null) hole.IsPassed = true;

            enemy.hole.DoLose(charactor);

            yield return new WaitForSeconds(0.25f);

            charactor.UpdateCharactorObj();
            charactor.healthController.UpdateHealth(true, false);

            if (charactor.UpgradeLevel(initialHealth))
            {
                GameController.instance.PlaceLevelUpAnim(charactor.gameObject);
            }

            if (!listWinHole.Exists(item => !item.IsPassed))
            {
                GameController.instance.DoWin();
                isWin = true;
                AllEnemyLoseAnim();
                charactor.VictoryAnim();
            }
            else
            {
                charactor.DoWin();
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

            dieSkeleton.transform.position = p;
            dieSkeleton.AnimationState.SetAnimation(0, "die_enemy", false);

            TaskUtil.Delay(this, delegate
            {
                GameController.instance.DoLose();
            }, 0.5f);
        }

        enemyAttack = false;
    }

    public void AllEnemyWinAnim()
    {
        TaskUtil.Delay(this, delegate
         {
             listEnemies.ForEach(item =>
             {
                 if (item.gameObject.activeInHierarchy)
                 {
                     item.DoWin();
                     item.StopDangerZone();
                 }
             });
         }, 0.1f);
    }

    public void HideHealthText()
    {
        listEnemies.RemoveAll((x) => x == null);
        listEnemies.ForEach(item => item.HideHealthText());
        charactor.HideHealthText();
    }

    public void AllEnemyLoseAnim()
    {
        listEnemies.RemoveAll((x) => x == null);
        listEnemies.ForEach(item =>
        {
            if (item.gameObject.activeInHierarchy && item.IsEnemy())
            {
                if (!item.GetType().Equals(typeof(Boss)) && !item.GetType().Equals(typeof(Wolf)))
                {
                    if (item.weaponType != WeaponType.Archery)
                        item.listSkeleton.ForEach(i => i.AnimationState.SetAnimation(0, "blue_victory", true));
                    else item.listSkeleton.ForEach(i => i.AnimationState.SetAnimation(0, "blue_victorry", true));
                }
            }
        });
    }

    public void PlaceHealthText(Hole endHole)
    {
        Enemy enemy = (Enemy)endHole.enemyAttackable;

        Vector3 pos = charactor.healthController.healthText.transform.position;
        pos.y = enemy.healthController.healthText.transform.position.y;
        pos.x -= 0.3f;

        charactor.healthController.healthText.transform.DOMove(pos, 0.2f);

        pos = enemy.healthController.healthText.transform.position;
        pos.x += 0.5f;
        enemy.healthController.healthText.transform.position = pos;
    }

    public void PlaceWayLine(Dijkstras.Path movingPath)
    {
        wayPoints.Clear();
        for (int i = 1; i < movingPath.m_Nodes.Count; i++)
        {
            Vector3 vec;

            if (i == 1)
                vec = GameController.instance.currentLevel.charactor.transform.position;
            else
                vec = movingPath.m_Nodes[i - 1].HoleCenter();

            Vector3 vec1 = movingPath.m_Nodes[i].HoleCenter();
            vec.z = 0f;
            vec1.z = 0f;

            Vector3[] p = new Vector3[20];

            if (i != 1) wayPoints.Add(vec);
            for (int k = 1; k < 40; k++)
            {
                Vector3 v = TaskUtil.LerpByDistance(vec, vec1, 0.025f * k);
                wayPoints.Add(v);
            }
            wayPoints.Add(vec1);
        }

        GameController.instance.PlaceWayLine(wayPoints);
    }

    public void PlaceWayLine(Vector3 startPos, Vector3 endPos)
    {
        wayPoints.Clear();

        startPos.z = 0f;
        endPos.z = 0f;

        Vector3[] p = new Vector3[20];

        for (int k = 1; k < 40; k++)
        {
            Vector3 v = TaskUtil.LerpByDistance(startPos, endPos, 0.025f * k);
            wayPoints.Add(v);
        }
        wayPoints.Add(endPos);

        GameController.instance.PlaceWayLine(wayPoints);
    }

    List<Vector3> wayPoints = new List<Vector3>();

    protected Hole GetBossAttackHole(List<Hole> holes)
    {
        Hole resultHole;

        foreach (var item in holes)
        {
            if (!item.IsPassed && item.enemyAttackable != null)
            {
                resultHole = item;
                return resultHole;
            }
        }

        resultHole = null;
        return resultHole;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        wayPoints.ForEach(item => Gizmos.DrawCube(item, 0.1f * Vector3.one));
    }
    // anim fighting
    public void AnimFighting(
        List<SkeletonGraphic> listEnemySkeleton,
        int fightingCount,
        bool charactorAttack = true,
        bool enemyAttack = true)
    {
        if (coFightingAnim != null)
        {
            StopCoroutine(coFightingAnim);
            coFightingAnim = null;
        }

        coFightingAnim = StartCoroutine(IEFightingAnim(
            listEnemySkeleton,
            fightingCount,
            charactorAttack,
            enemyAttack));
    }
    IEnumerator IEFightingAnim(
        List<SkeletonGraphic> listEnemySkeleton,
        int fightingCount,
        bool charactorAttack,
        bool enemyAttack)
    {
        bool attackBoss = fightingCount == 4;
        float factor = attackBoss ? 1.5f : 1f;

        List<GameObject> listObj = new List<GameObject>();

        yield return new WaitForSeconds(0.5f / factor);

        if (enemyAttack)
        {
            listObj.Clear();
            charactor.listSkeleton.ForEach(item => listObj.Add(item.gameObject));
            GameController.instance.PlaceHitAnim(true, listObj);
        }

        if (charactorAttack)
        {
            listObj.Clear();
            listEnemySkeleton.ForEach(item => listObj.Add(item.gameObject));
            GameController.instance.PlaceHitAnim(false, listObj);
        }

        yield return new WaitForSeconds(0.7f / factor);

        if (enemyAttack)
        {
            listObj.Clear();
            charactor.listSkeleton.ForEach(item => listObj.Add(item.gameObject));
            GameController.instance.PlaceHitAnim(true, listObj);
        }

        if (charactorAttack)
        {
            listObj.Clear();
            listEnemySkeleton.ForEach(item => listObj.Add(item.gameObject));
            GameController.instance.PlaceHitAnim(false, listObj);
        }

        if (fightingCount == 4)
        {
            yield return new WaitForSeconds(0.9f / factor);

            listObj.Clear();
            charactor.listSkeleton.ForEach(item => listObj.Add(item.gameObject));
            GameController.instance.PlaceHitAnim(true, listObj);

            listObj.Clear();
            listEnemySkeleton.ForEach(item => listObj.Add(item.gameObject));
            GameController.instance.PlaceHitAnim(false, listObj);

            yield return new WaitForSeconds(0.9f / factor);

            listObj.Clear();
            charactor.listSkeleton.ForEach(item => listObj.Add(item.gameObject));
            GameController.instance.PlaceHitAnim(true, listObj);

            listObj.Clear();
            listEnemySkeleton.ForEach(item => listObj.Add(item.gameObject));
            GameController.instance.PlaceHitAnim(false, listObj);
        }

        coFightingAnim = null;
    }

    public void CheckCollisionWithTrap(Hole endHole)
    {
        var colliders = Physics2D.LinecastAll(charactor.transform.position, endHole.transform.position);
        foreach (var item in colliders)
        {
            Trap trap = item.collider.gameObject.GetComponent<Trap>();
            if (trap != null && !trap.isCollision)
            {
                trapHitPoint = item.point;
                isCollisionWithTrap = true;
                targetTrap = trap;
                break;
            }
        }
    }
    [ContextMenu("GetAllList_Enemies_Holes_WinHoles_Hints")]
    public void GetAll_Enemies_Holes_WinHoles()
    {
        listEnemies = GetComponentsInChildren<Enemy>().ToList();
        listMammals = GetComponentsInChildren<BaseMammal>().ToList();

        listAttackableAI = GetComponentsInChildren<MammalAIController>().ToList();
        listAttackableAI.Remove(charactorAI);

        listHoles = GetComponentsInChildren<Hole>().ToList();
        Hole holdInsideCharactor = charactor.GetComponentInChildren<Hole>();
        if (holdInsideCharactor != null && listHoles.Contains(holdInsideCharactor))
            listHoles.Remove(holdInsideCharactor);
        GetWinHoles();

        listWay = GetComponentsInChildren<Way>().ToList();

        drawMaps = GetComponentsInChildren<DrawMapUltis>().ToList();
    }
    [ContextMenu("CenterAllAttackableToHole")]
    public void CenterAllAttackableToHole()
    {
        foreach (Hole hole in listHoles)
            hole.CenterAttackable();
    }
    void GetWinHoles()
    {
        listWinHole = new List<Hole>();

        if (IsLevelContainFadeHole())
        {
            foreach (Hole hole in listHoles)
            {
                if (hole.enemyAttackable != null &&
                    !hole.ContainTrap() &&
                    !hole.ContainCharactor())
                    listWinHole.Add(hole);
            }
            return;
        }

        if (IsLevelContainPrincess())
        {
            listWinHole.Add(GetPrincessHole());
            return;
        }

        if (IsLevelContainTreasure())
        {
            listWinHole.Add(GetHoleTreasure());
            return;
        }

        if (IsLevelContainBoss())
        {
            listWinHole.Add(GetBossHole());
            return;
        }

        foreach (Hole hole in listHoles)
        {
            if (hole.enemyAttackable != null
                && hole.IsPassed == false
                && !hole.ContainTrap())
                listWinHole.Add(hole);
        }
    }
    Hole GetHoleTreasure()
    {
        Hole treasure = listHoles.Find((x) => x.enemyAttackable != null && x.enemyAttackable.trap != null && (x.enemyAttackable.trap as KhoBauTrap) != null);
        return treasure;
    }
    protected bool IsLevelContainFadeHole()
    {
        FadeHole fade = level.GetComponentInChildren<FadeHole>();
        if (fade != null) return true;
        else return false;
    }
    protected bool IsLevelContainTreasure()
    {
        Hole treasure = listHoles.Find((x) => x.enemyAttackable != null && x.enemyAttackable.trap != null && (x.enemyAttackable.trap as KhoBauTrap) != null);
        return treasure != null;
    }
    protected bool IsLevelContainBoss()
    {
        Hole boss = listHoles.Find((x) => x.enemyAttackable != null && x.enemyAttackable.boss != null);
        if (boss != null) return true;
        else return false;
    }
    protected bool IsLevelContainPrincess()
    {
        Hole princess = listHoles.Find((x) => x.enemyAttackable != null && x.enemyAttackable.princess != null);
        if (princess != null) return true;
        else return false;
    }
    void GetAllSkeletonCached()
    {
        List<SkeletonController> skeletons = level.GetComponentsInChildren<SkeletonController>().ToList();
        foreach (var skeleton in skeletons)
            skeleton.CalculateCached();
    }
    protected Hole GetBossHole()
    {
        return listHoles.Find((x) => x.enemyAttackable != null && x.enemyAttackable.boss != null);
    }
    protected Hole GetPrincessHole()
    {
        return listHoles.Find((x) => x.enemyAttackable != null && x.enemyAttackable.princess != null);
    }
    void GetAllIsometricController()
    {
        this.isometrics = FindObjectsOfType<IsometricController>().ToList();
    }
    void GetAllBarrierRay()
    {
        this.barrierrays = FindObjectsOfType<BarrierRay>().ToList();
    }
    public void StopAllEnemyAI()
    {
        foreach (MammalAIController eAI in listAttackableAI)
            if (eAI.gameObject.activeSelf)
                eAI.StopAstar();
    }
    [ContextMenu("StareAllEnemyAI")]
    public void StareAllEnemyAI()
    {
        if (charactorAI != null)
        {
            MammalAIController nearCharactor = charactorAI.NearestMammal();
            if (nearCharactor != null) charactor.head.Toward(nearCharactor.transform);
        }

        listEnemies.RemoveAll((x) => x == null);
        foreach (Enemy enemy in listEnemies)
            enemy.head?.Toward(charactor.transform);
    }
    [ContextMenu("FixAllEnemyScale")]
    public void FixAllEnemyScale()
    {
        listEnemies.RemoveAll((x) => x == null);
        foreach (Enemy enemy in listEnemies)
            enemy.skeletonController.FixScale();
    }
    [ContextMenu("SetAllMammalLiquid")]
    public void SetAllMammalLiquid()
    {
        charactorAI?.Liquify();
        List<EnemyAIController> enemies = GetComponentsInChildren<EnemyAIController>().ToList();
        foreach (EnemyAIController ai in enemies) ai.Liquify();
    }
    public void SetAllMammalIdle()
    {
        SetAllMammalLiquid();
        charactorAI?.mammal.movementhelperController.SetIdleSpeed();
        List<EnemyAIController> enemies = GetComponentsInChildren<EnemyAIController>().ToList();
        foreach (EnemyAIController ai in enemies)
            ai.mammal.movementhelperController.SetIdleSpeed();
    }
    public void SetAllMammalStalking()
    {
        SetAllMammalLiquid();
        charactorAI?.mammal.movementhelperController.SetStalkingSpeed();
        List<EnemyAIController> enemies = GetComponentsInChildren<EnemyAIController>().ToList();
        foreach (EnemyAIController ai in enemies)
            ai.mammal.movementhelperController.SetStalkingSpeed();
    }

    [ContextMenu("PokeAllEnemiesAI")]
    public void PokeAllEnemiesAI()
    {
        StartCoroutine(C_PokeAllEnemiesAI());
    }
    public IEnumerator C_PokeAllEnemiesAI()
    {
        DonePokeAllMammals = false;

        StopAllEnemyAI();
        yield return new WaitUntil(() => IsAllEnemyAIAstarStop() == true);

        foreach (var attackableAI in listAttackableAI)
        {
            if (!attackableAI.CanPokeToFindCharactor())
                continue;

            if (IsReachable(attackableAI.seeker, charactorAI.mover))
            {
                attackableAI.AddOnce_MammalReachable(charactorAI);
                charactorAI.AddOnce_MammalReachable(attackableAI);
            }

            EnemyAIController tryEnemy = attackableAI as EnemyAIController;
            if (tryEnemy != null)
                tryEnemy.BossAddPrincess();
            ///Boss sẽ cố gắng tìm công chúa trước khi tìm nhân vật            

            MammalAIController nearestReach;
            nearestReach = attackableAI.NearestReachableMammal();

            if (nearestReach != null)
            {
                attackableAI.setter.target = nearestReach.transform;
                attackableAI.currentAiming = nearestReach.transform;
                attackableAI.astarAI.SearchPath();
                attackableAI.MoveToMeet(nearestReach);
            }
        }

        DonePokeAllMammals = true;
        yield break;
    }
    public bool IsAllEnemyAIAstarStop()
    {
        return listAttackableAI.Find((x) => x.gameObject.activeSelf &&
        x.IsAstarCompletelyStop() == false) == null;
    }
    public bool IsReachable(Seeker seeker, TargetMover mover)
    {
        if (AstarPath.active == null) return false;
        GraphNode aiNode = AstarPath.active.GetNearest(seeker.transform.position, NNConstraint.Default).node;
        GraphNode targetNode = AstarPath.active.GetNearest(mover.transform.position, NNConstraint.Default).node;
        if (aiNode != null && targetNode != null)
            return PathUtilities.IsPathPossible(aiNode, targetNode);
        else return false;
    }
    public bool IsReachable(Transform tr1, Transform tr2)
    {
        if (AstarPath.active == null) return false;
        GraphNode node1 = AstarPath.active.GetNearest(tr1.position, NNConstraint.Default).node;
        GraphNode node2 = AstarPath.active.GetNearest(tr2.position, NNConstraint.Default).node;
        if (node1 != null && node2 != null)
            return PathUtilities.IsPathPossible(node1, node2);
        else return false;
    }
    private void OnValidate()
    {
        insideAstar = GetComponentInChildren<AstarPath>()?.transform;
    }
    public virtual void Start()
    {
        StartCoroutine(C_Start());
    }
    IEnumerator C_Start()
    {
        if (GameController.instance.currentLevel != null)
            yield return new WaitUntil(() => GameController.instance.DoneLoadLevel == true);

        ///Don't use gravity for all puzzles
        Physics2D.gravity = Vector2.zero;

        GetAll_Enemies_Holes_WinHoles();
        GetAllAstarObstacles();

        SetAllMammalLiquid();

        if (levelController != null && levelHint != null)
        ///Get all hints
        {
            levelController.SyncHolesFromScene();
            levelHint.GetAllHint();
            if (!levelHint.ShowHintAtStart)
                levelHint.HideHint();
        }
    }
    public bool IsParent0()
    {
        return transform.parent.localPosition == Vector3.zero;
    }
    [ContextMenu("ReScanAstar")]
    public void ReScanAstar()
    {
        if (coAstarScan != null) StopCoroutine(coAstarScan);
        coAstarScan = StartCoroutine(C_ReScanAstar());
    }
    IEnumerator C_ReScanAstar()
    {
        DoneReScanAstar = false;
        if (AstarPath.active == null)
        {
            DoneReScanAstar = true;
            yield break;
        }
        float startScan = Time.time;
        ToogleAstarObstacles(-1);
        AstarPath.active.Scan();
        yield return new WaitUntil(() => IsNewScan(CurrentWalkable, out HasNewPath) == true || IsScanTimeOver(startScan) == true);
        ToogleAstarObstacles(1);
        AstarPath.active.Scan();
        yield return new WaitUntil(() => IsNewScan(CurrentWalkable, out HasNewPath) == true || IsScanTimeOver(startScan) == true);
        ToogleAstarObstacles(-1);

        DoneReScanAstar = true;
        yield break;
    }
    public void ToogleAstarObstacles(int toogleType = 0)
    {
        if (AstarObstacles.Count == 0) return;
        bool toggle = false;
        switch (toogleType)
        {
            case 0: toggle = !AstarObstacles[0].activeSelf; break;
            case 1: toggle = true; break;
            case -1: toggle = false; break;
        }

        for (int i = 1; i < AstarObstacles.Count; i++)
        {
            ToggleAstarCollider(AstarObstacles[i], toggle);
        }

        ToggleAstarCollider(AstarObstacles[0], toggle);

        if (mapBound != null)
            ToggleAstarCollider(mapBound.gameObject, toggle);
    }
    void ToggleAstarCollider(GameObject astarGO, bool toggle)
    {
        Collider2D col2 = astarGO.GetComponent<Collider2D>();
        col2.enabled = toggle;
    }
    bool IsNewScan(List<bool> LastWalkable, out bool HasNewPath)
    {
        if (AstarData.active == null)
        {
            HasNewPath = false;
            return false;
        }

        List<bool> NewWalkable = new List<bool>();
        SyncWalkable(out NewWalkable, AstarData.active.data.gridGraph.nodes);
        bool isEqual = true;
        if (LastWalkable.Count != NewWalkable.Count)
        {
            isEqual = false;
        }
        else
        {
            for (int i = 0; i < LastWalkable.Count; i++)
            {
                if (NewWalkable[i] != LastWalkable[i])
                {
                    isEqual = false;
                    i = LastWalkable.Count;
                }
            }
        }
        HasNewPath = !isEqual;
        if (HasNewPath) CurrentWalkable = NewWalkable;
        return HasNewPath;
    }
    bool IsScanTimeOver(float start)
    {
        return (Time.time - start > 10);
    }
    void SyncWalkable(out List<bool> list, GraphNode[] nodes)
    {
        list = new List<bool>();
        foreach (GraphNode node in nodes)
        {
            list.Add(node.Walkable);
        }
    }
    [ContextMenu("GetAllAstarObstacles")]
    public void GetAllAstarObstacles()
    {
        var goArray = level.GetComponentsInChildren<Transform>();
        AstarObstacles = new List<GameObject>();
        for (int i = 0; i < goArray.Length; i++)
        {
            if (goArray[i].gameObject.layer == 21)
            {
                AstarObstacles.Add(goArray[i].gameObject);
            }
        }
    }
    public void ResetDoneFight()
    {
        charactorAI.SetStatus(eMammalStatus.Idle);
        levelController.levelStatus = eLevelStatus.Idle;
        charactorAI.nearestMammalAI = null;
    }
    [ContextMenu("DebugSearchPath")]
    public void DebugSearchPath()
    {
        List<EnemyAIController> enemies = FindObjectsOfType<EnemyAIController>().ToList();
        foreach (EnemyAIController enemyAI in enemies)
        {
            Debug.Log(enemyAI.gameObject.name + " " + IsReachable(enemyAI.seeker, charactorAI.mover));
        }
    }
    public void Join2Enemy(EnemyAIController chosen, EnemyAIController losing)
    {
        StartCoroutine(C_Join2Enemy(chosen, losing));
    }
    IEnumerator C_Join2Enemy(EnemyAIController chosen, EnemyAIController losing)
    {
        DoneJoinEnemy = false;

        int fightingCount = losing.enemy.GetType() == typeof(Boss) ? 4 : 2;
        float factor = fightingCount == 4 ? SectionSettings.fightingTime / 1.5f : SectionSettings.fightingTime;

        chosen.enemy.Health += losing.enemy.Health;
        chosen.enemy.healthController.UpdateHealth();

        float initialHealth = losing.enemy.Health;
        losing.enemy.hole.DoLose(chosen.mammal, false);
        if (chosen.enemy.UpgradeLevel(initialHealth))
        {
            //GameController.instance.PlaceLevelUpAnim(chosen.enemy.gameObject);
        }

        chosen.enemy.UpdateCharactorObj();

        chosen.enemy.DoIdle();
        chosen.StopAstar();
        chosen.enemy.head.Toward(charactor.transform);

        //yield return new WaitForSeconds(fightingCount * factor);

        chosen.enemy.DoIdle();

        DoneJoinEnemy = true;

        yield break;
    }
    public void ReScanPath(Hole targetHole)
    ///Đây là level 1_1, 1_2
    {
        StartCoroutine(C_ReScanPath(targetHole));
    }
    IEnumerator C_ReScanPath(Hole targetHole)
    {
        if (targetHole == null)
            yield break;

        levelController.levelStatus = eLevelStatus.Busy;
        yield return new WaitUntil(() => moving == false && fighting == false);

        if (onPostReScanPath != null)
            onPostReScanPath.Invoke();

        levelController.levelStatus = eLevelStatus.Idle;
        yield break;
    }
    [ContextMenu("SnapAllToAstarGraph")]
    public void SnapAllToAstarGraph()
    {
        StartCoroutine(C_SnapAllToAstarGraph());
    }
    IEnumerator C_SnapAllToAstarGraph()
    {
        yield return new WaitUntil(
            () => AstarPath.active != null &&
            AstarPath.active.transform == insideAstar);

        charactor.SnapToNearStarNode();
        foreach (var mammalAI in listAttackableAI)
            mammalAI.mammal.SnapToNearStarNode();

        yield break;
    }
    [ContextMenu("ReScanPath")]
    public void ReScanPath()
    ///Đây là level 1_3
    {
        if (AstarPath.active == null)
            return;

        ReScanPath(true, true);
    }
    public void ReScanPath(bool scanStar = true, bool poke = false)
    ///go : Gameobject
    ///Từ level 1_3, 1_4, 1_5 thì dùng Astar
    {
        if (AstarPath.active == null)
            return;

        StartCoroutine(C_ReScanPath(scanStar, poke));
    }
    IEnumerator C_ReScanPath(bool scanStar = true, bool poke = false)
    {
        levelController.levelStatus = eLevelStatus.Busy;

        if (scanStar)
        {
            ReScanAstar();
            yield return new WaitUntil(() => DoneReScanAstar == true);
        }

        charactorAI.StopAstar();
        StopAllEnemyAI();
        StareAllEnemyAI();
        SetAllMammalLiquid();

        if (poke)
        {
            yield return StartCoroutine(C_PokeAllEnemiesAI());
            yield return new WaitUntil(() => DonePokeAllMammals == true);
        }

        bool mustWait = true;
        ///Vì 2 con mammal gặp nhau chắc chắc tụi nó stop
        ///Cần phải chờ vài frame để chắc chắn tụi nó có Idle hẳn hay không.
        while (mustWait)
        {
            yield return new WaitUntil(() => charactorAI.IsIdle() == true &&
            listAttackableAI.Find(
                (x) => x.gameObject.activeSelf == true &&
                x.IsIdle() == false) == null);

            yield return new WaitForEndOfFrame();
            yield return new WaitForSeconds(0.2f);

            mustWait = !(charactorAI.IsIdle() == true &&
            listAttackableAI.Find(
                (x) => x.gameObject.activeSelf == true &&
                x.IsIdle() == false) == null);
        }

        levelController.levelStatus = eLevelStatus.Idle;

        if (onPostReScanPath != null)
            onPostReScanPath.Invoke();

        yield break;
    }
    [ContextMenu("LoadFromGameData")]
    public void LoadFromGameData()
    {
        string gamedata = File.ReadAllText(Application.persistentDataPath + "/gameTextData.txt");

        GameData gameData = JsonUtility.FromJson<GameData>(gamedata);
        foreach (InitHealth health in gameData.initHealths)
        {
            if (health.level != this.gameObject.name) continue;

            Transform ObjectNeedHealth = this.transform.Find(health.gameobject);
            if (ObjectNeedHealth == null) Debug.Log(health.gameobject + " ko tim thay " + health.level);

            if (ObjectNeedHealth != null)
            {
                HealthController healthController = ObjectNeedHealth.GetComponentInChildren<HealthController>();
                if (healthController == null) healthController = ObjectNeedHealth.gameObject.AddComponent<HealthController>();

                int h = -1;
                int.TryParse(health.health, out h);
                if (h != -1) healthController.Health = h;
                else Debug.Log("bug " + health.level);
            }

        }
    }
    public void Balance(Transform target1, Transform target2)
    {
        if (coBalancing != null)
        {
            StopCoroutine(coBalancing);
            coBalancing = null;
        }
        coBalancing = StartCoroutine(C_Balance(target1, target2));
    }

    IEnumerator C_Balance(Transform target1, Transform target2)
    {
        Debug.Log(target1.name + " balancing " + target2.name);

        Vector3 midPosition = (target1.transform.position + target2.transform.position) / 2f;
        Transform leftTr = target2.transform.position.x < target1.transform.position.x ? target2.transform : target1;
        Transform rightTr = leftTr == target2.transform ? target1 : target2;
        Vector3 leftBalance = new Vector3(midPosition.x - minDistance, midPosition.y, midPosition.z);
        Vector3 rightBalance = new Vector3(midPosition.x + minDistance, midPosition.y, midPosition.z);

        while ((leftTr.position - leftBalance).magnitude > minY &&
            (rightTr.position - rightBalance).magnitude > minY)
        {
            yield return new WaitForEndOfFrame();

            leftTr.position = Vector3.Lerp(leftTr.position, leftBalance, Time.deltaTime);
            rightTr.position = Vector3.Lerp(rightTr.position, rightBalance, Time.deltaTime);
        }
        coBalancing = null;
    }
    [ContextMenu("FitCharactorIntoFirstHole")]
    public void FitCharactorIntoFirstHole()
    {
        Hole holdInside = charactor.GetComponentInChildren<Hole>();
        Hole nearHole = NearestHoleToCharactor();
        if (nearHole != null)
        {
            if ((nearHole.transform.position - charactor.transform.position).magnitude < 1f)
            {
                charactor.hole = nearHole;
                nearHole.enemyAttackable = charactor;
                holdInside?.gameObject.SetActive(false);
            }
            else
            {
                Debug.Log(level.transform.name + " khoang cach qua lon " + nearHole.transform.name);
            }
        }

        if (charactor.hole != null)
        {
            charactor.hole.IsPassed = true;
            charactor.transform.position = charactor.hole.transform.position;
        }
    }
    [ContextMenu("FitAttackableToHoleCenter")]
    public void FitAttackableToHoleCenter()
    {
        if (!prepareController.CanFitMammalToHoleCenterAtStart) return;

        foreach (Hole hole in listHoles)
        {
            if (hole.enemyAttackable == null) continue;
            SkeletonController skeletonController = hole.enemyAttackable.GetComponent<SkeletonController>();
            if (skeletonController == null) continue;

            Vector3 touchHole = CalculateEndholePosition(charactor.transform, hole.transform, enemyPositionOffset);
            Vector3 directionOffset = touchHole - hole.transform.position;
            skeletonController.DoOffset(directionOffset);
        }
    }
    [ContextMenu("FitAttackableToNearHole")]
    public void FitAttackableToNearHole()
    {
        List<Attackable> attackables = GetComponentsInChildren<Attackable>().ToList();

        foreach (var hole in listHoles)
            hole.enemyAttackable = null;

        foreach (var attack in attackables)
            attack.FindNearHole();
    }
    public Vector3 CalculateEndholePosition(Transform charactor, Transform hole, float offset)
    {
        float positionX = charactor.position.x < hole.position.x ?
            hole.position.x - holePositionOffset : hole.position.x + offset;
        return new Vector3(positionX, hole.position.y, hole.position.z);
    }
    bool IsContainCharactorHole()
    {
        List<Hole> holes = FindObjectsOfType<Hole>().ToList();
        if (holes.Find((x) => x.enemyAttackable != null && x.enemyAttackable.transform == charactor.transform) != null) return true;
        else return false;
    }
    public class HoldData
    {
        public int id;
        public Hole hole;
        public HoldData(int id,
            Hole hole)
        {
            this.id = id;
            this.hole = hole;
        }
    }
    Hole NearestHoleToCharactor()
    {
        if (this.listHoles.Count == 0) return null;
        List<HoldData> holeDatas = new List<HoldData>();

        for (int i = 0; i < listHoles.Count; i++)
            if (listHoles[i].enemyAttackable == null)
                holeDatas.Add(new HoldData(i, listHoles[i]));

        if (holeDatas.Count > 0)
        {
            holeDatas.Sort((x, y) => (x.hole.holeImage.transform.position - charactor.transform.position).magnitude.
            CompareTo((x.hole.holeImage.transform.position - charactor.transform.position).magnitude));

            return listHoles[holeDatas[0].id];
        }
        else
        {
            return null;
        }
    }
    void ControlIsometric(eLevelStatus eLevel)
    {
        GetAllIsometricController();
        switch (eLevel)
        {
            case eLevelStatus.Idle:
                SetAllIsometric(false);
                break;
            case eLevelStatus.Busy:
                SetAllIsometric(true);
                break;
        }
    }
    void ControlBarrierCheckCollision(eLevelStatus eLevel)
    {
        GetAllBarrierRay();
        switch (eLevel)
        {
            case eLevelStatus.Idle:
                SetAllBarrierCanCheckCollision(false);
                break;
            case eLevelStatus.Busy:
                SetAllBarrierCanCheckCollision(true);
                break;
        }
    }
    void SetAllIsometric(bool CanSort)
    {
        foreach (IsometricController isometric in isometrics)
            isometric.CanSortIsometric = CanSort;
    }
    void SetAllBarrierCanCheckCollision(bool CanSort)
    {
        foreach (BarrierRay barrier in barrierrays)
            barrier.CanCheckCollision = CanSort;
    }
    [ContextMenu("ForceAllIsometric")]
    public void ForceAllIsometric()
    {
        ControlIsometric(eLevelStatus.Busy);
    }
    public void ShowAllSkeleton()
    {
        List<SkeletonController> skeletons = GetComponentsInChildren<SkeletonController>().ToList();
        foreach (SkeletonController skeleton in skeletons)
            skeleton.Show100();
    }
    public void ShowHint()
    {
        levelController.levelHint.ShowHint();
    }
    public Hole GetHole(Transform attackable)
    {
        Hole found = listHoles.Find((x) => x.enemyAttackable != null && x.enemyAttackable.transform == attackable);
        return found;
    }

    #region UNITY EDITOR
#if UNITY_EDITOR
    [ContextMenu("TryPrepareScene")]
    public void TryPrepareScene()
    {
        TapKillManager tapkillManager = GetComponentInChildren<TapKillManager>()?.gameObject?.GetComponent<TapKillManager>();
        if (tapkillManager == null)
        {
            GameObject newManager = PrefabUtility.InstantiatePrefab(Resources.Load<GameObject>("LevelUltis/TapKillManager") as GameObject, this.transform) as GameObject;
            tapkillManager = newManager.GetComponent<TapKillManager>();
            tapkillManager.gameObject.name = "TapKillManager";
        }

        layerController.DoSort();

        GetAll_Enemies_Holes_WinHoles();
        tapkillManager.SyncHolesFromScene();

        GetAllSkeletonCached();
        FitCharactorIntoFirstHole();
        FitAttackableToHoleCenter();

        LevelTapKillHint prepareHint = tapkillManager.GetComponent<LevelTapKillHint>();
        prepareHint.GetAllHint();
        if (prepareController.CanSortHint)
            prepareHint.TrySortHint();

        charactor.healthController.UpdateHealth();
        charactor.GetComponent<CharactorOriginal>().Originate();
        foreach (var enemy in listEnemies)
            enemy.healthController.UpdateHealth();

        charactor.hole?.UpdateAppearance();
        foreach (var hole in listHoles)
        {
            hole.UpdateAppearance(
                prepareController.CanPositionHint,
                prepareController.CanToggleImageHole);
            hole.GetComponent<WallBoxColliderController>().UpdateCollider();
        }
    }
#endif
    #endregion UNITY EDITOR

    [ContextMenu("RemoveAllHint")]
    public void RemoveAllHint()
    {
        levelHint.RemoveAllHint();
    }
    public virtual string GetLevelNameType()
    {
        return string.Empty;
    }
    [ContextMenu("Show100")]
    public void Show100()
    {
        float currentAlpha = canvasGroup.alpha;
        DOTween.To(() => currentAlpha,
            x => canvasGroup.alpha = (float)x,
            1f,
            .3f);
    }
    [ContextMenu("Show0")]
    public void Show0()
    {
        canvasGroup.alpha = 0;
        /*
        float currentAlpha = canvasGroup.alpha;
        DOTween.To(() => currentAlpha,
            x => canvasGroup.alpha = (float)x,
            0f,
            .1f).OnComplete(delegate
            {
                gameObject.SetActive(false);
            });
        */
    }
    public void FastShow0()
    {
        canvasGroup.alpha = 0;
    }
    public void DestroyAstar()
    {
        Destroy(insideAstar?.gameObject);
    }
    public bool IsDestroyAstar()
    {
        return AstarPath.active == null;
    }
    public bool CanGetReady()
    {
        return false;
    }
    public void GetReady()
    {
        listMammals.ForEach((x) => x.Ready());
        Show100();
    }
    public void UnReady()
    {
        Show0();
        DestroyAstar();
        listMammals.ForEach((x) => x.UnReady());
    }
    #endregion Virtual Method
}
public enum TypeLevel
{
    Normal,
    Boss
}
