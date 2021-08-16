using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CapsuleController : BaseMammalProperties
{
    [ContextMenu("UpdateCapsule")]
    public void UpdateCapsule()
    {
        RectTransform rt = skeletonController.listSkeleton[0].GetComponent<RectTransform>();
        Vector2 size = new Vector2(GetSkeletonWidth(), capsule.size.y);
        Vector2 offset = new Vector2(size.x / 2 - rt.pivot.x * GetSkeletonWidth(), capsule.offset.y);

        capsule.size = size;
        capsule.offset = offset;
    }
    public float GetSkeletonWidth()
    {
        RectTransform rt = skeletonController.listSkeleton[0].GetComponent<RectTransform>();
        return rt.sizeDelta.x * Mathf.Abs(rt.localScale.x);
    }
    public Vector3 GetCapsulePivot()
    {
        return new Vector3(capsule.transform.position.x + capsule.offset.x,
            capsule.transform.position.y + capsule.offset.y, 0);
    }
    [ContextMenu("PutHealthAtCapsulePivot")]
    public void PutHealthAtCapsulePivot()
    {
        healthController.healthText.transform.position = GetCapsulePivot();
    }
}
