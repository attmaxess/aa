using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarrierStringColliderController : MonoBehaviour
{
    public BarrierLineController lineController;
    public PolygonCollider2D colider;

    public List<GameObject> testPoints;

    [Space(20)] public Vector2 amp = Vector2.one;

    [ContextMenu("UpdateCollider")]
    public void UpdateCollider()
    {
        Vector2[] linePoints = lineController.lineRender.Points;
        Vector2 sortedX = new Vector2(Mathf.Min(linePoints[0].x, linePoints[1].x), Mathf.Max(linePoints[0].x, linePoints[1].x));
        Vector2 sortedY = new Vector2(Mathf.Min(linePoints[0].y, linePoints[1].y), Mathf.Max(linePoints[0].y, linePoints[1].y));
        if (linePoints.Length == 2)
        {
            Vector2[] newPath = new Vector2[4];
            newPath[0] = new Vector2(sortedX.x - amp.x, sortedY.x - amp.y);
            newPath[1] = new Vector2(sortedX.y + amp.x, sortedY.x - amp.y);
            newPath[2] = new Vector2(sortedX.y + amp.x, sortedY.y + amp.y);
            newPath[3] = new Vector2(sortedX.x - amp.x, sortedY.y + amp.y);
            colider.SetPath(0, newPath);
        }
    }

    [ContextMenu("SyncPointsToCollider")]
    public void SyncPointsToCollider()
    {
        if (testPoints.Count == 0) return;
        Vector2[] path = colider.GetPath(0);
        for (int i = 0; i < 4; i++)
        {
            testPoints[i].gameObject.name = path[i].ToString();
            testPoints[i].transform.position = transform.InverseTransformPoint(path[i]);
        }
    }
}
