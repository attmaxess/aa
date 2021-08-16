using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

public class Barrier : BarrierBaseProperties
{
    [SerializeField] UILineRenderer _imgBarrier = null;

    public BarrierStringBoxColliderController OnPostCollider;
    public BarrierStringBoxColliderController OnDragCollider;
    public ColliderDetection detection;

    [Space(20)]
    public Sprite _spriteDefault;
    public Sprite _spriteCollide;

    public enum eSkin { fine, collide }
    public eSkin skin
    {
        get { return this._skin; }
        set { this._skin = value; HandleSkin(value); }
    }
    eSkin _skin = eSkin.fine;
    public bool ForceSkinFine = false;

    public delegate void skinSelfCheck();
    public skinSelfCheck skinselfCheck;

    private void Start()
    {
        skinselfCheck += SkinSelfCheck;
    }

    public bool IsEqual(LineController other)
    {
        return (this != other) &&
            ((lineController.linetransforms[0] == other.linetransforms[0] &&
            lineController.linetransforms[1] == other.linetransforms[1]) ||
            (lineController.linetransforms[0] == other.linetransforms[1] &&
            lineController.linetransforms[1] == other.linetransforms[0]));
    }
    public bool IsEqual(RectTransform rect0, RectTransform rect1)
    {
        return (lineController.linetransforms[0] == rect0 &&
            lineController.linetransforms[1] == rect1 ||
            (lineController.linetransforms[0] == rect1 &&
            lineController.linetransforms[1] == rect0));
    }
    void HandleSkin(eSkin skin)
    {
        if (ForceSkinFine)
        {
            _imgBarrier.sprite = _spriteDefault;
            return;
        }

        switch (skin)
        {
            case eSkin.fine: 
                _imgBarrier.sprite = _spriteDefault;
                _imgBarrier.color = Color.white;
                break;
            case eSkin.collide: 
                _imgBarrier.sprite = _spriteDefault;
                _imgBarrier.color = Color.red;
                break;
            default: break;
        }
    }
    public bool IsContainMammal()
    {
        foreach (Transform tr in detection.GetTrs())
            if (tr.GetComponent<BaseMammal>() != null) return true;
        return false;
    }
    public int IndexContainPillar(Transform pillar)
    {
        if (lineController.linetransforms[0].transform == pillar) return 0;
        if (lineController.linetransforms[1].transform == pillar) return 1;
        return -1;
    }
    public Transform GetOtherPillar(Transform pillar)
    {
        if (lineController.linetransforms.Count != 2) return null;
        Transform other = lineController.linetransforms[0].transform == pillar ?
            lineController.linetransforms[1] : lineController.linetransforms[0];
        return other;
    }
    /// <summary>
    /// otherLine chỉ có 2 pillar thôi
    /// </summary>
    /// <param name="otherLine"></param>
    /// <returns></returns>
    public Transform GetTheSamePillar(LineController otherLine)
    {
        if (otherLine.linetransforms.Count != 2) return null;
        if (otherLine.linetransforms[0].transform == lineController.linetransforms[0].transform) return otherLine.linetransforms[0].transform;
        if (otherLine.linetransforms[1].transform == lineController.linetransforms[0].transform) return otherLine.linetransforms[1].transform;
        if (otherLine.linetransforms[0].transform == lineController.linetransforms[1].transform) return otherLine.linetransforms[0].transform;
        if (otherLine.linetransforms[1].transform == lineController.linetransforms[1].transform) return otherLine.linetransforms[1].transform;
        return null;
    }
    public void SkinSelfCheck()
    {
        //OnDragCollider.UpdateCollider();
        //skin = IsContainMammal() ? Barrier.eSkin.collide : Barrier.eSkin.fine;
    }
    public void ClearAllDetection()
    {
        detection.Clear();
    }
}
