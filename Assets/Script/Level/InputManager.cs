using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class InputManager : MonoBehaviour
{
    public float swipeThreshold = 10f;
    public float timeThreshold = 0.3f;

    [Header("Swipe Event")]
    public UnityEvent OnSwipeLeft;
    public UnityEvent OnSwipeRight;
    public UnityEvent OnSwipeUp;
    public UnityEvent OnSwipeDown;

    [Header("Click Event")]
    public UnityEvent OnMouseDown;
    public UnityEvent OnMouseEnter;
    public UnityEvent OnMouseUp;

    private Vector3 fingerDown;
    private DateTime fingerDownTime;
    private Vector2 fingerUp;
    private DateTime fingerUpTime;
    Vector3 initialWorldFingerDown;
    Vector3 runtimeWorldFingerDown;

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 vec1 = Input.mousePosition;
            Vector3 vec2 = new Vector3(vec1.x, vec1.y, 0f);
            this.initialWorldFingerDown = Camera.main.ScreenToWorldPoint(vec2);            

            this.fingerDown = Input.mousePosition;
            this.fingerUp = Input.mousePosition;
            this.fingerDownTime = DateTime.Now;

            this.OnMouseDown.Invoke();
        }
        else if (Input.GetMouseButton(0))
        {
            Vector3 vec1 = Input.mousePosition;
            Vector3 vec2 = new Vector3(vec1.x, vec1.y, 0f);
            this.runtimeWorldFingerDown = Camera.main.ScreenToWorldPoint(vec2);

            this.fingerDown = Input.mousePosition;
            this.fingerUpTime = DateTime.Now;
            this.OnMouseEnter.Invoke();
            this.CheckSwipe();
        }
        else if (Input.GetMouseButtonUp(0))
        {
            this.OnMouseUp.Invoke();
        }
    }

    private void CheckSwipe()
    {
        float duration = (float)this.fingerUpTime.Subtract(this.fingerDownTime).TotalSeconds;
        if (duration > this.timeThreshold) return;

        float deltaX = this.fingerDown.x - this.fingerUp.x;
        if (Mathf.Abs(deltaX) > this.swipeThreshold)
        {
            if (deltaX > 0)
            {
                this.OnSwipeRight.Invoke();
            }
            else if (deltaX < 0)
            {
                this.OnSwipeLeft.Invoke();
            }
        }

        float deltaY = fingerDown.y - fingerUp.y;
        if (Mathf.Abs(deltaY) > this.swipeThreshold)
        {
            if (deltaY > 0)
            {
                this.OnSwipeUp.Invoke();
            }
            else if (deltaY < 0)
            {
                this.OnSwipeDown.Invoke();
            }
        }

        this.fingerUp = this.fingerDown;
    }

    public Vector3 MousePosToWorld ()
    {
        Vector3 vec1 = Input.mousePosition;
        Vector3 vec2 = new Vector3(vec1.x, vec1.y, 0f);
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(vec2);
        mousePos.z = 80;

        return mousePos;
    }

    // Get touch object with screen pos
    public T GetTouchObject<T>(bool runtime = false)
    {
        Vector3 pos = runtime ? runtimeWorldFingerDown : initialWorldFingerDown;
        var hit = Physics2D.LinecastAll(pos, new Vector3(pos.x, pos.y, 100));

        foreach (var item in hit)
        {
            if (item.collider != null)
            {
                T obj = item.collider.gameObject.GetComponent<T>();
                if (obj != null)
                {
                    return obj;
                }
            }
        }

        return default(T);
    }

    // Get touch object with custom Pos
    public T GetTouchObject<T>(Vector3 worldPos)
    {
        var hit = Physics2D.LinecastAll(worldPos, new Vector3(worldPos.x, worldPos.y, 100));

        foreach (var item in hit)
        {
            if (item.collider != null)
            {
                T obj = item.collider.gameObject.GetComponent<T>();
                if (obj != null)
                {
                    return obj;
                }
            }
        }

        return default(T);
    }
}
