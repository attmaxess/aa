using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI.Extensions;

public class HeadTowardController : BaseMammalProperties
{
    [Space(20)]
    public bool CanHeadToward = true;
    public bool OriginalLookRight = false;

    Quaternion QuaReverse = new Quaternion(0, 1, 0, 0);
    Quaternion Qua0 = new Quaternion(0, 0, 0, 1);

    [ReadOnly] public int CurrentDirection = 0;

    private void Start()
    {
        CurrentDirection = Direction();
    }

    [ContextMenu("HeadTowardCharactor")]
    public void HeadTowardCharactor()
    {
        if (!CanHeadToward) return;
        Toward(level.charactor.transform);
    }
    [ContextMenu("HeadTowardNearest")]
    public void HeadTowardNearest()
    {
        if (!CanHeadToward) return;
        List<Transform> arrounds = new List<Transform>();
        level.listEnemies.ForEach((x) => arrounds.Add(x.transform));
        arrounds.Sort((x, y) => Distance(x, transform).CompareTo(Distance(y, transform)));
        if (arrounds.Count > 0)
            Toward(arrounds[0]);
    }
    float Distance(Transform tr1, Transform tr2)
    {
        return (tr1.position - tr2.position).magnitude;
    }
    public void Toward(Transform target, Transform pivot = null)
    {
        if (!CanHeadToward) return;
        if (pivot == null)
            pivot = transform;

        int look = 0;

        if ((pivot.position - target.transform.position).magnitude < 0.01f)
        {
            if (level.useDebugLog)
                Debug.Log("X bang nhau. HeadToward " + pivot.name + " - " + target.name);

            SkeletonController targetSkeleton = target.GetComponent<SkeletonController>();
            if (skeletonController != null && targetSkeleton != null)
                look = skeletonController.GetCachedPosition().x < targetSkeleton.GetCachedPosition().x ? 1 : -1;
        }
        else
        {
            look = pivot.position.x < target.position.x ? 1 : -1;
            if (look == 1)
                LookRight();
            else if (look == -1)
                LookLeft();
        }

        if (level.useDebugLog)
            Debug.Log("HeadToward " + pivot.name + " - " + target.name);

        //look = OriginalLookRight ? look : -look;
        //skeletonController.HeadToward(look == -1 ? QuaReverse : Qua0);

        if (capsule != null)
        {
            float newX = look * (Mathf.Abs(capsule.offset.x));
            capsule.offset = new Vector2(newX, capsule.offset.y);
        }

        CurrentDirection = Direction();
    }
    public void Toward(Vector3 target, Transform pivot = null)
    {
        if (!CanHeadToward) return;
        if (pivot == null)
            pivot = transform;

        int look = 0;

        if ((pivot.position - target).magnitude < 0.01f)
        {
            return;
        }
        else
        {
            look = pivot.position.x < target.x ? 1 : -1;
            if (look == 1)
                LookRight();
            else if (look == -1)
                LookLeft();
        }

        if (capsule != null)
        {
            float newX = look * (Mathf.Abs(capsule.offset.x));
            capsule.offset = new Vector2(newX, capsule.offset.y);
        }

        CurrentDirection = Direction();
    }
    public void Toward(HeadTowardController other)
    {
        Vector3 center = (this.transform.position + other.transform.position) / 2f;
        int thisToLook = this.skeletonController.ActiveCenter().x < center.x ? 1 : -1;
        if (thisToLook == -1)
        {
            LookLeft();
            other.LookRight();
        }
        else
        {
            LookRight();
            other.LookLeft();
        }
    }
    [ContextMenu("DebugDirection")]
    public void DebugDirection()
    {
        if (!CanHeadToward) return;

        Quaternion q = skeletonController.listSkeleton[0].transform.rotation;
        if (OriginalLookRight)
        {
            if (RotationEquals(q, Qua0)) Debug.Log("Right");
            else if (RotationEquals(q, QuaReverse)) Debug.Log("Left");
        }
        else
        {
            if (RotationEquals(q, Qua0)) Debug.Log("Left");
            else if (RotationEquals(q, QuaReverse)) Debug.Log("Right");
        }

        CurrentDirection = Direction();
    }
    public int Direction()
    {
        int direction = 0;
        Quaternion q = skeletonController.listSkeleton[0].transform.rotation;
        if (OriginalLookRight)
        {
            if (RotationEquals(q, Qua0)) direction = 1;
            else if (RotationEquals(q, QuaReverse)) direction = -1;
        }
        else
        {
            if (RotationEquals(q, Qua0)) direction = -1;
            else if (RotationEquals(q, QuaReverse)) direction = 1;
        }
        return direction;
    }
    public int ReverseDirection()
    {
        int d = Direction();
        if (d == 1) return -1;
        if (d == -1) return 1;
        else return 0;
    }
    public bool IsHeadToward(Transform other)
    {
        if (other.position.x == transform.position.x) return true;
        bool otherSide = other.position.x > transform.position.x;
        if (otherSide) return Direction() == 1;
        else return Direction() == -1;
    }
    [ContextMenu("DebugQuaternion")]
    public void DebugQuaternion()
    {
        Debug.Log(listSkeleton[0].transform.rotation);
    }
    public void ToggleDirection()
    {
        Quaternion reverse = Quaternion.identity;
        if (Direction() == 1)
            reverse = OriginalLookRight ? QuaReverse : Qua0;
        else if (Direction() == -1)
            reverse = OriginalLookRight ? Qua0 : QuaReverse;

        skeletonController.HeadToward(reverse);
    }
    bool RotationEquals(Quaternion r1, Quaternion r2)
    {
        float abs = Mathf.Abs(Quaternion.Dot(r1, r2));
        if (abs >= 0.99f)
            return true;
        return false;
    }
    public void UnifyDirection()
    {
        if (listSkeleton.Count == 0) return;
        Quaternion q = listSkeleton[0].transform.rotation;
        skeletonController.HeadToward(q);
    }
    [ContextMenu("ChangeDirection")]
    public void ChangeDirection()
    {
        if (!CanHeadToward) return;
        Quaternion q = listSkeleton[0].transform.rotation;
        if (RotationEquals(q, Qua0))
            skeletonController.HeadToward(QuaReverse);
        else if (RotationEquals(q, QuaReverse))
            skeletonController.HeadToward(Qua0);
        else
            Debug.Log("Wrong");
    }
    [ContextMenu("LookLeft")]
    public void LookLeft()
    {
        if (!CanHeadToward) return;
        skeletonController.HeadToward(OriginalLookRight ? QuaReverse : Qua0);
    }
    [ContextMenu("LookRight")]
    public void LookRight()
    {
        if (!CanHeadToward) return;
        skeletonController.HeadToward(OriginalLookRight ? Qua0 : QuaReverse);
    }
    public void BothLookCenter(MammalAIController mammal1, MammalAIController mammal2)
    {
        if (mammal1.listSkeleton.Count == 0) return;
        if (mammal2.listSkeleton.Count == 0) return;

        Vector3 center = (mammal1.listSkeleton[0].transform.position +
            mammal2.listSkeleton[0].transform.position) / 2f;
        MammalAIController left = mammal1.skeletonController.listSkeleton[0].transform.position.x < center.x ?
            mammal1 : mammal2;
        MammalAIController right = left == mammal1 ? mammal2 : mammal1;

        left.head.LookRight();
        right.head.LookLeft();
    }
}
