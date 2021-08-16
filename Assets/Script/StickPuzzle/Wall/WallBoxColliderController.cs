using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallBoxColliderController : MonoBehaviour
{
    public RectTransform wallRt;
    public BoxCollider2D box;

    public Vector2 pivotOffset = Vector2.zero;
    public Vector2 sizeOffset = Vector2.zero;

    [ContextMenu("UpdateCollider")]
    public void UpdateCollider()
    {
        box.offset = pivotOffset;
        box.size = wallRt.sizeDelta + sizeOffset;
    }
    public Vector3 GetOffset()
    {
        return new Vector3(box.offset.x, box.offset.y, 0);
    }
}
