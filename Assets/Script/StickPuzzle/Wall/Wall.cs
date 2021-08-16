using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

public class Wall : MonoBehaviour
{
    public WallManager manager
    {
        get
        {
            if (_manager == null) _manager = GetComponentInParent<WallManager>();
            return _manager;
        }
    }
    WallManager _manager;
    public WallTouchController touchController
    {
        get
        {
            if (_touchController == null) _touchController = GetComponentInChildren<WallTouchController>();
            return _touchController;
        }
    }
    WallTouchController _touchController;
    public WallBoxColliderController wallbox
    {
        get
        {
            if (_wallbox == null) _wallbox = GetComponent<WallBoxColliderController>();
            return _wallbox;
        }
    }
    WallBoxColliderController _wallbox;

    [ReadOnly] public bool ChangeNativeSize = true;

    public WallHint hint;
    public Transform focus;

    public enum eWallType { wood, stone }
    public eWallType ewallType { get { return this._ewallType; } }
    public eWallType _ewallType = Wall.eWallType.wood;
    public bool IsDestroyable()
    {
        switch (ewallType)
        {
            case eWallType.wood: return true;
            case eWallType.stone: return false;
            default: return false;
        }
    }
    public WallBoxColliderController collidor;
    public Image imageWall;
    [ReadOnly] bool _isDestroying = false;

    Coroutine coFinishDestroy = null;

    public void FinishDestroy()
    {
        if (!manager.CanUserPlay()) return;
        if (coFinishDestroy == null)
            coFinishDestroy = StartCoroutine(C_FinishDestroy());
    }

    IEnumerator C_FinishDestroy()
    {
        manager.CanDestroyWall = false;

        if (!IsDestroyable())
            manager.CanDestroyWall = true;
        else
        {
            SelfDestroyAction();
            manager.afterFinish_DestroyWall.Invoke(true, true);

            manager.CanDestroyWall = true;
            coFinishDestroy = null;
        }

        yield break;
    }

    void SelfDestroyAction()
    {
        collidor.box.enabled = false;
        imageWall.enabled = false;
        this._isDestroying = false;
        gameObject.SetActive(false);
    }

    public bool IsReadyToDestroy()
    {
        return collidor.box.enabled == false;
    }
    [ContextMenu("UpdateAppearance")]
    public void UpdateAppearance()
    {
        float charactorSize = manager.level.charactor.capsule.size.x
            * Mathf.Abs(manager.level.charactor.transform.localScale.x);

        RectTransform rt = GetComponent<RectTransform>();
        if (ewallType == eWallType.wood)
        {
            bool useWidth = rt.sizeDelta.x < rt.sizeDelta.y;
            float width = 30f;

            if (!Application.isPlaying)
                hint.gameObject.SetActive(hint.GetIntHint() > 0);

            if (useWidth)
            {
                rt.sizeDelta = new Vector2(width, rt.sizeDelta.y);
                hint.hintRotation.Set90();
                wallbox.sizeOffset = new Vector2(charactorSize, 60);
            }
            else
            {
                hint.hintRotation.Set0();
                rt.sizeDelta = new Vector2(rt.sizeDelta.x, width);
                wallbox.sizeOffset = new Vector2(60, 60);
            }
        }
        else if (ewallType == eWallType.stone)
        {
            Image image = rt.GetComponent<Image>();
            if (ChangeNativeSize)
                image.SetNativeSize();

            wallbox.sizeOffset = new Vector2(30, 30);

            if (!Application.isPlaying)
                hint.gameObject.SetActive(false);
        }

        wallbox.UpdateCollider();
    }
    [ContextMenu("UpdateHintAppearance")]
    public void UpdateHintAppearance()
    {
        if (ewallType == eWallType.stone)
            hint.gameObject.SetActive(false);
        focus.transform.position = transform.position;
    }
    public bool IsContainHint(WallHint hint)
    {
        return this.hint != null && this.hint.transform == hint.transform;
    }
    public void SetFocus(bool value)
    {
        focus.gameObject.SetActive(value);
    }
    [ContextMenu("1")] public void SetHint1() { hint.hint.text = "1"; hint.gameObject.SetActive(true); }
    [ContextMenu("2")] public void SetHint2() { hint.hint.text = "2"; hint.gameObject.SetActive(true); }
    [ContextMenu("3")] public void SetHint3() { hint.hint.text = "3"; hint.gameObject.SetActive(true); }
    [ContextMenu("4")] public void SetHint4() { hint.hint.text = "4"; hint.gameObject.SetActive(true); }
    [ContextMenu("5")] public void SetHint5() { hint.hint.text = "5"; hint.gameObject.SetActive(true); }
    [ContextMenu("6")] public void SetHint6() { hint.hint.text = "6"; hint.gameObject.SetActive(true); }
    [ContextMenu("7")] public void SetHint7() { hint.hint.text = "7"; hint.gameObject.SetActive(true); }
    [ContextMenu("0")] public void SetHint0() { hint.hint.text = "0"; hint.gameObject.SetActive(false); }
}
