using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallBox3DColliderController : MonoBehaviour
{
    public RectTransform wallRt;
    public BoxCollider box;

    public Vector2 center = Vector2.zero;
    public Vector2 sizeOffset = Vector2.zero;

    [ContextMenu("UpdateCollider")]
    public void UpdateCollider()
    {
        box.center = center;
        box.size = new Vector3(wallRt.sizeDelta.x + sizeOffset.x,
            wallRt.sizeDelta.y + sizeOffset.y,
            10);
    }
}
