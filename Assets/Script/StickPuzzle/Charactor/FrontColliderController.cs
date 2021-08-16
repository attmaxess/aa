using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrontColliderController : MonoBehaviour
{
    public ColliderDetection detection;
    public SkeletonGraphic skeleton;
    public RectTransform mammalRt;
    public BoxCollider2D box;

    public Vector2 pivotOffset = Vector2.zero;
    public Vector2 sizeOffset = Vector2.zero;

    [ContextMenu("UpdateCollider")]
    public void UpdateCollider()
    {
        if (!skeleton.MatchRectTransformWithBounds())
            Debug.Log("Mesh was not previously generated.");

        box.offset = mammalRt.sizeDelta / 2 + pivotOffset - mammalRt.pivot * mammalRt.sizeDelta.x;
        box.size = mammalRt.sizeDelta + sizeOffset;
    }
}
