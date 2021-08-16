using DG.Tweening;
using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportItem : MonoBehaviour
{
    [Header("Condition")]
    public Hole holeToShow;
    public List<Hole> passedHoles = new List<Hole>();
    [Header("Connections")]
    public Hole hole1;
    public Hole hole2;
    [Header("Internal")]
    [SerializeField] SkeletonGraphic hole1Skeleton = null;
    [SerializeField] SkeletonGraphic hole2Skeleton = null;
    [Space(20)]
    public bool teleported = false;
    public bool isShowed = false;

    public void Show()
    {
        if (isShowed) return;
        isShowed = true;
        hole1Skeleton.transform.position = hole1.transform.position;
        hole1Skeleton.DOFade(1, 0.25f);
        hole2Skeleton.transform.position = hole2.transform.position;
        hole2Skeleton.DOFade(1, 0.25f);

        hole1.IsPassed = false;
    }
    public void Hide(bool permanent = false)
    {
        if (!permanent) isShowed = false;

        hole1Skeleton.DOFade(0, 0.25f);
        hole2Skeleton.DOFade(0, 0.25f);
    }
    public bool IsAllHolesPassed()
    ///Kiểm tra mấy cái holes trong biến PassedHolesCollects này
    {
        Hole notpass = passedHoles.Find((x) => x.IsPassed == false);
        return !(notpass != null);
    }
}
