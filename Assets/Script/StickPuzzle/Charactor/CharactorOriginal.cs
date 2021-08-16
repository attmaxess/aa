using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharactorOriginal : BaseMammalProperties
{
    public Vector3 scale = Vector3.one;
    public Vector3 skeleton1Scale = new Vector3(.33f, .33f, 1f);

    public Hole originalHole;
    private void Start()
    {
        DebugOriginStatus();
    }
    void DebugOriginStatus()
    {
        Vector3 scale0 = AbsVec3(skeletonController.listSkeleton[0].transform.localScale);
        bool isWellScale = true;
        for (int i = 1; i < skeletonController.listSkeleton.Count; i++)
        {
            Vector3 scaleI = AbsVec3(skeletonController.listSkeleton[i].transform.localScale);
            if (scaleI != scale0)
            {
                isWellScale = false;
                i = skeletonController.listSkeleton.Count;
            }
        }

        if (level.useDebugLog)
            Debug.Log("Well Scale : " + isWellScale);
    }
    Vector3 AbsVec3(Vector3 v)
    {
        return new Vector3(Mathf.Abs(v.x), Mathf.Abs(v.y), Mathf.Abs(v.z));
    }
    [ContextMenu("Originate")]
    public void Originate()
    {
        transform.localScale = scale;
        foreach (var skeleton in skeletonController.listSkeleton)
            skeleton.transform.localScale = skeleton1Scale;
    }

}
