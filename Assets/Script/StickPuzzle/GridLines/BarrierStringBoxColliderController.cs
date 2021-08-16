using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI.Extensions;

public class BarrierStringBoxColliderController : MonoBehaviour
{
    public UILineRenderer lineRenderer;
    public BoxCollider2D lineCollider;

    [Space(20)] public Vector3 amp = Vector2.one;

    [HideInInspector] public bool DoneUpdateAsync = true;

    [ContextMenu("UpdateCollider")]
    public void UpdateCollider()
    {
        lineCollider.transform.localPosition = Vector3.zero;
        lineCollider.transform.localRotation = Quaternion.identity;

        // get width of collider from line 
        float lineWidth = lineRenderer.lineThickness;
        // get the length of the line using the Distance method
        Vector2 startPoint = lineRenderer.Points[0];
        Vector2 endPoint = lineRenderer.Points[1];
        float lineLength = Vector3.Distance(startPoint, endPoint);
        // size of collider is set where X is length of line, Y is width of line
        //z will be how far the collider reaches to the sky
        lineCollider.size = new Vector3(lineLength, lineWidth, 1f) + amp;
        // get the midPoint
        Vector3 midPoint = (startPoint + endPoint) / 2;
        // move the created collider to the midPoint
        lineCollider.transform.localPosition = midPoint;

        //heres the beef of the function, Mathf.Atan2 wants the slope, be careful however because it wants it in a weird form
        //it will divide for you so just plug in your (y2-y1),(x2,x1)
        float angle = Mathf.Atan2((endPoint.y - startPoint.y), (endPoint.x - startPoint.x));

        // angle now holds our answer but it's in radians, we want degrees
        // Mathf.Rad2Deg is just a constant equal to 57.2958 that we multiply by to change radians to degrees
        angle *= Mathf.Rad2Deg;

        //were interested in the inverse so multiply by -1
        angle *= -1;
        // now apply the rotation to the collider's transform, carful where you put the angle variable
        // in 3d space you don't wan't to rotate on your y axis
        lineCollider.transform.Rotate(0, 0, -angle);
    }
    
    public void UpdateAsyncCollider()
    {
        StartCoroutine(C_UpdateAsyncCollider());
    }
    IEnumerator C_UpdateAsyncCollider()
    {
        DoneUpdateAsync = false;
        UpdateCollider();
        yield return new WaitForEndOfFrame();
        DoneUpdateAsync = true;
    }
}
