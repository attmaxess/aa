using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PillarColliderController : MonoBehaviour
{
    public RectTransform wallRt;
    public CircleCollider2D circle;
    public float offset;

    [ContextMenu("UpdateCollider")]
    public void UpdateCollider()
    {
        circle.radius = wallRt.sizeDelta.magnitude + offset;
    }
}
