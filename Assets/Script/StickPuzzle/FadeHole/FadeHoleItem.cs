using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Spine;
using Spine.Unity;

[System.Serializable]
public class FadeHoleItem
{
    [Header("Condition")]
    public Hole targetHole;
    public List<Hole> passedHoles = new List<Hole>();
    [Header("Connections")]
    public bool CanFadeAtAwake = true;
    public List<Image> targetImage;
    public List<SkeletonGraphic> targetSkeleton;
    public List<Text> targetText;
    public List<HoleConnection> holeConnections;
    public List<Hole> unfadeHoles;
    [Header("Internal")]
    public bool isShowed = false;

    public void FadeAtAwake()
    {
        foreach (var hole in unfadeHoles)
        {
            if (hole.enemyAttackable != null)
            {
                if (CanFadeAtAwake)
                    hole.enemyAttackable.FastFade();
                hole.IsPassed = true;
            }
        }
    }
    public void Show()
    {
        if (isShowed) return;
        isShowed = true;
        foreach (var unFade in unfadeHoles)
        {
            /*
            fadeData.hole.connections.AddRange(fadeData.connections);
            foreach (var i in fadeData.connections)
            {
                if (!fadeData.hole.connections.Exists(j => j == i))
                    fadeData.hole.connections.Add(i);
            }

            
            targetSkeleton.ForEach(i => i.DOFade(1, 0.1f));
            targetText.ForEach(i => i.DOFade(1, 0.1f));
            */

            targetImage.ForEach(i => i.DOFade(1, 0.1f));

            if (unFade.enemyAttackable != null)
            {
                unFade.enemyAttackable.Show100();
                unFade.IsPassed = false;
            }
        }
    }
    public bool IsAllHolesPassed()
    ///Kiểm tra mấy cái holes trong biến PassedHolesCollects này
    {
        if (passedHoles.Count == 0) return false;
        Hole notpass = passedHoles.Find((x) => x.IsPassed == false);
        return !(notpass != null);
    }
}

[System.Serializable]
public class HoleConnection
{
    public Hole hole;
    public List<Hole> connections;
}
