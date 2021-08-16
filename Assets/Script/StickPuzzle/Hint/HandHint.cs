using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI.Extensions;

public class HandHint : MonoBehaviour
{
    public bool IsInit = false;
    public Transform point;
    public Vector2 offset;

    public delegate void OnReachEnd();
    public OnReachEnd onReachEnd;

    public bool CanMove = false;
    public Vector3 end;
    const float speedMove = 3f;

    public bool CanRotate = false;
    const int doubleTap = 2;
    private int countRotate = 0;
    Quaternion qua0 { get { return Quaternion.identity; } }
    Quaternion qua20 { get { return new Quaternion(0, 0, .2f, 1); } }
    const float speedRotate = 5f;

    public CanvasGroup group;
    public TrailRenderer trail;
    public Animator animtor;

    Coroutine coTap;

    public bool CanShow = false;
    public bool CanFade = false;
    const float speedAlpha = 99f;

    public bool CanRotateOnetime = false;
    public bool CanTravel = false;
    const float speedTravel = 5f;
    List<Vector3> travelPositions = new List<Vector3>();
    int indexNextTravel = -1;
    float mustwaitTravel = 2f;
    float startwaitTravel = 0;

    private void Awake()
    {
        //CalculateOffset();
        IsInit = true;
        HideHand(false, false);
    }
    [ContextMenu("CalculateOffset")]
    public void CalculateOffset()
    {
        offset = new Vector2(transform.position.x - point.position.x,
            transform.position.y - point.position.y);
    }
    public void PointTo(Vector2 position)
    {
        transform.position = position + offset;
    }
    public Vector3 offset3()
    {
        return new Vector3(offset.x, offset.y, 0);
    }
    private void Update()
    {
        #region Xử lý level phá dây
        if (CanMove)
        {
            if ((transform.position - end).magnitude > 0.05f)
                transform.position = Vector3.MoveTowards(transform.position, end, Time.deltaTime * speedMove);
            else
            {
                CanMove = false;
                if (onReachEnd != null)
                    onReachEnd.Invoke();
            }
        }
        #endregion Xử lý level phá dây

        #region Xử lý level phá tường
        if (CanRotate)
        {
            if (countRotate < doubleTap)
            {
                if (!RotationEquals(transform.rotation, qua0))
                    transform.rotation = Quaternion.Lerp(transform.rotation, qua0, Time.deltaTime * speedRotate);
                else
                {
                    if (countRotate < doubleTap)
                    {
                        countRotate++;
                        if (countRotate < doubleTap)
                            transform.rotation = qua20;
                        else
                        {
                            CanRotate = false;
                            if (onReachEnd != null)
                                onReachEnd.Invoke();
                        }
                    }
                }
            }
        }
        #endregion Xử lý level phá tường

        #region Xử lý level nối ô
        if (CanRotateOnetime)
        {
            if (!RotationEquals(transform.rotation, qua0))
                transform.rotation = Quaternion.Lerp(transform.rotation, qua0, Time.deltaTime * speedRotate);
            else
            {
                CanRotateOnetime = false;
                CanTravel = true;
            }
        }

        if (CanTravel)
        {
            if (indexNextTravel < travelPositions.Count)
            {
                if ((transform.position - travelPositions[indexNextTravel]).magnitude > 0.05f)
                    transform.position = Vector3.MoveTowards(transform.position, travelPositions[indexNextTravel], Time.deltaTime * speedTravel);
                else
                {
                    indexNextTravel++;
                }
            }
            else
            {
                CanTravel = false;
                if (onReachEnd != null)
                    onReachEnd.Invoke();
            }
        }
        #endregion Xử lý level nối ô

        if (CanShow && !CanFade)
        {
            if (1 - group.alpha > 0.05f)
                group.alpha = Mathf.Lerp(group.alpha, 1, Time.deltaTime * speedAlpha);
            else
            {
                CanShow = false;
                group.alpha = 1;
            }
        }

        if (CanFade && !CanShow)
        {
            if (group.alpha > 0.05f)
                group.alpha = Mathf.Lerp(group.alpha, 0, Time.deltaTime * speedAlpha);
            else
            {
                CanFade = false;
                group.alpha = 0;
            }
        }
    }
    public void DoMove(Vector3 start, Vector3 end)
    {
        transform.position = start;
        transform.rotation = qua0;
        transform.gameObject.SetActive(true);
        this.end = end;

        CanShow = true;
        CanMove = true;
    }
    public void InitBeforeTap(Vector3 start, OnReachEnd action)
    {
        transform.position = start;
        transform.rotation = qua20;
        group.alpha = 0;

        if (onReachEnd == null)
            onReachEnd = action;
    }
    public void DoTap(Vector3 start, float delay = -1)
    {
        StartCoroutine(C_DoTap(start, delay));
    }
    IEnumerator C_DoTap(Vector3 start, float delay = -1)
    {
        if (delay == -1)
        {
            yield return StartCoroutine(C_OnceTap(start));
        }
        else
        {
            while (true)
            {
                yield return StartCoroutine(C_OnceTap(start));
                yield return new WaitForSeconds(delay);
            }
        }

        yield break;

    }
    IEnumerator C_OnceTap(Vector3 start)
    {
        yield return new WaitUntil(() => transform.position == start);
        yield return new WaitUntil(() => RotationEquals(transform.rotation, qua20) == true);

        CanShow = true;

        countRotate = 0;
        CanRotate = true;

        yield break;
    }
    public void DoTravel(List<Vector3> positions)
    {
        if (positions.Count == 0) return;

        travelPositions = positions;
        transform.position = travelPositions[0];
        indexNextTravel = 1;

        transform.gameObject.SetActive(true);

        CanShow = true;
        CanRotateOnetime = true;
        CanTravel = false;
    }
    [ContextMenu("DebugQuaternion")]
    public void DebugQuaternion()
    {
        Debug.Log(transform.rotation);
    }
    public bool RotationEquals(Quaternion r1, Quaternion r2)
    {
        float abs = Mathf.Abs(Quaternion.Dot(r1, r2));
        if (abs >= 0.999f)
            return true;
        return false;
    }
    public void HideHand(bool scanStar, bool poke)
    {
        Hide();
    }
    public void HideHand(Hole targetHole)
    {
        Hide();
    }
    void Hide()
    {
        gameObject.SetActive(false);
        HideTrail();
    }
    public void ShowTrail()
    {
        trail.gameObject.SetActive(true);
        trail.time = .3f;
    }
    public void HideTrail()
    {
        trail.gameObject.SetActive(false);
        trail.time = 0;
    }
    [ContextMenu("DoTapByAnim")]
    public void DoTapByAnim()
    {
        animtor.SetBool("Tap", true);
        animtor.enabled = true;
    }
    public void StopTapByAnim()
    {
        animtor.SetBool("Tap", false);
        animtor.enabled = false;
    }
    private void OnDisable()
    {
        StopTapByAnim();
    }
    public bool IsAtEndTravel()
    {
        return indexNextTravel >= travelPositions.Count && CanTravel == false;
    }
}
