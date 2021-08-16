using Pathfinding;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AIController : MonoBehaviour
{
    public enum eObstacleType { None, Barrier, Wall }
    public eObstacleType eobstacleType = AIController.eObstacleType.None;

    public GameObject AIprefab;
    public GameObject AITrackerprefab;
    public GameObject Targetprefab;

    //[Space(20)]
    //[HideInInspector]
    public List<Seeker> ais = new List<Seeker>();
    //[HideInInspector]
    public List<IAstarAI> iais = new List<IAstarAI>();
    //[HideInInspector]
    public List<AIDestinationSetter> setters = new List<AIDestinationSetter>();
    //[HideInInspector]
    public List<TargetMover> targets = new List<TargetMover>();
    //[HideInInspector]
    public List<ColliderDetection> aicolliders = new List<ColliderDetection>();
    //[HideInInspector]
    public int countReached = 0;

    Transform charactor;
    List<Enemy> enemies;

    [Space(20)]
    public List<Enemy> foundEnemies = new List<Enemy>();

    [Space(20)]
    public bool DoneCreateAI = true;
    public bool DoneRetach = true;
    public bool DoneSearchPath = true;
    public bool DoneCheckOpenPath = true;

    [Space(20)]
    public Seeker inSideSeeker;
    public Seeker outSideSeeker;

    public delegate void onFoundTarget(Enemy enemy);
    public onFoundTarget onfoundTarget;

    public delegate void OnPostRemoveEnemy();
    public OnPostRemoveEnemy onPostRemoveEnemy;

    [ContextMenu("CreateAllAI")]
    public void CreateAllAI(Transform charactor, List<Enemy> enemies)
    {
        DoneCreateAI = false;

        this.charactor = charactor;
        this.enemies = enemies;

        ais = new List<Seeker>();
        iais = new List<IAstarAI>();
        targets = new List<TargetMover>();

        foreach (Enemy enemy in this.enemies)
        {
            if (enemy.transform == charactor) continue;

            if (enemy != null)
            {
                Seeker newAI = Instantiate(AIprefab as GameObject, this.transform).GetComponent<Seeker>();
                newAI.gameObject.name = "Seeker_" + charactor.name + " -> " + enemy.gameObject.name;
                newAI.GetComponent<RectTransform>().position = this.charactor.position;
                
                IAstarAI newAstarAI = newAI.GetComponent<IAstarAI>();                

                AILerp newLerp = newAI.GetComponent<AILerp>();
                newLerp.handleOnTargetReached += OnTargetReached_AILerp;

                TargetMover newTarget = enemy.GetComponent<TargetMover>();
                newAstarAI.destination = newTarget.transform.position;

                AIDestinationSetter newSetter = newAI.GetComponent<AIDestinationSetter>();
                newSetter.target = newTarget.transform;

                ColliderDetection newColliderAI = newAI.GetComponent<ColliderDetection>();
                newColliderAI.onAddOnce += enemy.AIMeetEnemyTarget;
                enemy.postMeetHoleTarget += AddOnce_FoundEnemy;
                enemy.postMeetHoleTarget += SeekerMeetTarget;

                StopAI(newAI);

                ais.Add(newAI);
                iais.Add(newAstarAI);
                setters.Add(newSetter);
                targets.Add(newTarget);
                aicolliders.Add(newColliderAI);
            }
        }

        DoneCreateAI = true;
    }

    void OnTargetReached_AILerp(Seeker seeker)
    {
        AIMidPoint midPoint = seeker.GetComponentInChildren<AIMidPoint>();
        if (midPoint != null)
        {
            List<GraphNode> path = seeker.GetCurrentPath().path;
            if (path == null || path.Count == 0) return;
            int midIndex = path.Count / 2;
            midPoint.transform.position = (Vector3)path[midIndex].position;
        }
    }

    void StopAI(Seeker ai)
    {
        ai.GetComponent<AILerp>().enabled = false;
        ai.GetComponent<IAstarAI>().canMove = false;
        ai.GetComponent<IAstarAI>().isStopped = true;
    }

    private void Start()
    {

    }

    void AddOnce_FoundEnemy(Enemy enemy, ColliderDetection seekerDetection)
    {
        if (!foundEnemies.Contains(enemy) && CanEnemyFight(enemy, seekerDetection))
        {
            //Debug.Log("AddOnce_FoundEnemy " + enemy.gameObject.name);
            foundEnemies.Add(enemy);
        }
    }

    void SeekerMeetTarget(Enemy enemy, ColliderDetection seekerDetection)
    {
        countReached += 1;
    }

    [ContextMenu("DebugMovingStatus")]
    public void DebugMovingStatus()
    {
        foreach (IAstarAI astarAI in iais)
            Debug.Log(astarAI.isStopped);
    }

    public void FightEnemy(Enemy enemy)
    {
        if (enemy == null) return;
        if (CanEnemyFight(enemy))
        {
            onfoundTarget.Invoke(enemy);
        }
    }
    bool CanEnemyFight(Enemy enemy, ColliderDetection seekerDetection)
    ///Check Enemy and Seeker at the moment of touching
    ///if they can be added to queue
    ///Type T là type của obstacles sẽ làm Charactor không di chuyển được
    {
        bool CanFight = true;
        bool IsSeekerContainsEnemy = seekerDetection.IsContain(enemy.transform);
        CanFight = IsSeekerContainsEnemy && !CheckSeekerContainObstacle(seekerDetection);
        return CanFight;
    }
    bool CanEnemyFight(Enemy enemy)
    ///Check if enemy contains NONE-OBSTACLEs SEEKER
    ///At the moment the Enemy is already in queue
    {
        ColliderDetection detection = enemy.GetComponent<ColliderDetection>();
        bool CanFight = true;
        for (int i = 0; i < detection.TrCount(); i++)
        {
            Seeker seeker = detection.GetTr(i)?.GetComponent<Seeker>();
            if (seeker == null ||
                seeker.GetComponent<AIDestinationSetter>().target != enemy.transform) continue;

            bool ContainBarrier = CheckSeekerContainObstacle(seeker.GetComponent<ColliderDetection>());
            if (ContainBarrier)
            {
                CanFight = false;
                i = detection.TrCount();
            }
        }
        return CanFight;
    }

    bool CheckSeekerContainObstacle(ColliderDetection seekerdetection)
    {
        bool Contains = false;
        for (int i = 0; i < seekerdetection.TrCount(); i++)
        {
            Transform tr = seekerdetection.GetTr(i);
            if (tr == null) continue;
            if (tr.transform.parent == null || tr.transform.parent.GetComponent(eobstacleType.ToString()) as MonoBehaviour == null) continue;
            Contains = true;
            break;
        }
        return Contains;
    }

    [ContextMenu("FightNextHole")]
    public void FightNextEnemy()
    {
        if (IsStillContainsFoundEnemy())
        {
            Enemy enemyToFight = foundEnemies[0];
            //foundEnemies.Find((x) => x.GetComponent<EnemyAIController>().IsAstarCompletelyStop() == true);
            if (enemyToFight != null)
            {
                FightEnemy(enemyToFight);
                PokeAllRemainFoundEnemies();
            }
        }
    }

    float Distance(Transform tr1, Transform tr2)
    {
        return (tr1.position - tr2.position).magnitude;
    }

    int DistanceComparor(float distance1, float distance2)
    {
        return distance1.CompareTo(distance2);
    }

    public void PokeAllRemainFoundEnemies()
    {
        StartCoroutine(C_PokeAllFoundEnemy());
    }
    IEnumerator C_PokeAllFoundEnemy()
    {
        yield return new WaitUntil(() => DoneSearchPath == true);

        CharactorAIController charactorAI = charactor.GetComponent<CharactorAIController>();
        if (charactorAI == null) yield break;

        foreach (Enemy enemy in foundEnemies)
        {
            if (enemy.transform == charactorAI.currentAiming) continue;
            EnemyAIController enemyAIController = enemy.GetComponent<EnemyAIController>();
            if (enemyAIController != null && enemyAIController.IsAstarCompletelyStop() == true)
            {
                enemyAIController.setter.target = charactor.transform;
                enemyAIController.currentAiming = charactor.transform;
                enemyAIController.astarAI.SearchPath();
                enemyAIController.MoveByAstar();                
            }
        }
        yield break;
    }

    void RemoveNullEnemy()
    {
        foundEnemies.RemoveAll((x) => x == null);
        foundEnemies.RemoveAll((x) => x.gameObject.activeSelf == false);
    }

    public bool IsStillContainsFoundEnemy()
    {
        //RemoveNullEnemy();
        return foundEnemies.Count > 0;
    }

    [ContextMenu("PokeAllAI")]
    public void PokeAllAI()
    {
        StartCoroutine(C_PokeAllAI());
    }
    IEnumerator C_PokeAllAI()
    {
        DoneSearchPath = false;
        for (int i = 0; i < ais.Count; i++)
        {
            if (targets[i] != null)
            {
                AILerp lerp = ais[i].GetComponent<AILerp>();
                lerp.enabled = true;

                iais[i].canMove = true;
                iais[i].isStopped = false;
                ais[i].GetComponent<AILerp>().canMove = true;

                //aiTrackers[i].CanTrack = true;
            }
        }
        yield return new WaitUntil(() => iais.Find((x) => x.hasPath == false) == null);
        yield return new WaitUntil(() => countReached == ais.Count);

        DoneSearchPath = true;
        yield break;
    }

    public void ReAttachAndPokeWhileFighting()
    {
        StartCoroutine(C_ReAttachAndPokeWhileFighting());
    }

    IEnumerator C_ReAttachAndPokeWhileFighting()
    {
        ReAttach();
        yield return new WaitUntil(() => DoneRetach == true);

        PokeAllAI();
        yield return new WaitUntil(() => DoneSearchPath == true);
    }

    [ContextMenu("ReAttach")]
    public void ReAttach()
    {
        StartCoroutine(C_ReAttach());
    }
    IEnumerator C_ReAttach()
    {
        DoneRetach = false;
        countReached = 0;

        for (int i = 0; i < this.ais.Count; i++)
        {
            if (targets[i] != null)
            {
                AILerp lerp = ais[i].GetComponent<AILerp>();
                lerp.enabled = false;

                iais[i].canMove = false;
                iais[i].isStopped = true;
                //aiTrackers[i].CanTrack = false;

                aicolliders[i].Clear();
                ais[i].transform.position = charactor.transform.position;
                //targets[i].transform.position = holes[i].transform.position;

                //aiTrackers[i].ResetForce();
                //aiTrackers[i].transform.position = charactor.transform.position;
            }
        }
        yield return new WaitUntil(() => iais.Find((x) => x.canMove == true) == null);
        DoneRetach = true;
        yield break;
    }

    [ContextMenu("DebugReachable")]
    public void DebugReachable()
    {
        for (int i = 0; i < ais.Count; i++)
        {
            Debug.Log(IsReachable(ais[i], targets[i]));
        }
    }

    public void SetMovementStatusAllAI(bool CanMove)
    {
        for (int i = 0; i < ais.Count; i++)
        {
            iais[i].canMove = CanMove;
            //aiTrackers[i].CanTrack = CanMove;
        }
    }

    bool IsReachable(Seeker seeker, TargetMover mover)
    {
        GraphNode aiNode = AstarPath.active.GetNearest(seeker.transform.position, NNConstraint.Default).node;
        GraphNode targetNode = AstarPath.active.GetNearest(mover.transform.position, NNConstraint.Default).node;
        if (aiNode != null && targetNode != null)
        {
            return PathUtilities.IsPathPossible(aiNode, targetNode);
        }
        else return false;
    }    

    float distance(Seeker seeker, TargetMover mover)
    {
        GraphNode aiNode = AstarPath.active.GetNearest(seeker.transform.position, NNConstraint.Default).node;
        GraphNode targetNode = AstarPath.active.GetNearest(mover.transform.position, NNConstraint.Default).node;
        if (aiNode != null && targetNode != null && PathUtilities.IsPathPossible(aiNode, targetNode) == true)
        {
            return -1;
        }
        else return -1;
    }

    public void RemoveFoundEnemy(Enemy enemy)
    {
        if (foundEnemies.Contains(enemy))
        {
            int index = setters.FindIndex((x) => x.target.transform == enemy.transform);
            if (index != -1)
            {
                ais.RemoveAt(index);
                iais.RemoveAt(index);
                setters.RemoveAt(index);
                targets.RemoveAt(index);
                aicolliders.RemoveAt(index);
            }
            foundEnemies.Remove(enemy);
            Charactor charactor = this.charactor.GetComponent<Charactor>();
            if (charactor != null) charactor.targetHole = null;

            if (onPostRemoveEnemy != null)
                onPostRemoveEnemy.Invoke();
        }
    }

    public Seeker GetSeekerByEnemy(Enemy enemy)
    {
        int index = setters.FindIndex((x) => x.target.transform == enemy.transform);
        return index != -1 ? ais[index] : null;
    }
}
