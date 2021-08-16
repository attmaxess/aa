using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SkeletonReferences", menuName = "Scriptable/SkeletonReferences")]
public class SkeletonReferencesSO : ScriptableObject
{
    public List<SkeletonDataAsset> assets;

    public void FindAndSet(SkeletonController controller, string toFind)
    {
        if (string.IsNullOrEmpty(toFind)) return;
        SkeletonDataAsset archer = assets.Find((x) => x.name.Contains(toFind));
        if (archer != null)
            SetSkeleton(controller, archer);
    }
    void SetSkeleton(SkeletonController controller, SkeletonDataAsset asset)
    {
        foreach (var graphic in controller.listSkeleton)
        {
            graphic.skeletonDataAsset = asset;
            graphic.Initialize(true);
        }
    }
}