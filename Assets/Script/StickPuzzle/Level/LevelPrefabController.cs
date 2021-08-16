using System.Collections;
using System.Collections.Generic;
using Spine.Unity;
using UnityEngine;

public class LevelPrefabController : Singleton<LevelPrefabController>
{
    public GameObject die;
    public GameObject waterHit;

    public void DieAt(Vector3 position)
    {
        if (die == null) return;
        Level level = GameController.instance.currentLevel;
        if (level == null) return;
        GameObject newDie = Instantiate(die, level.layerController.mapLayer);
        newDie.transform.position = position;
        SkeletonGraphic dieSkeleton = newDie.GetComponent<SkeletonGraphic>();
        dieSkeleton.AnimationState.SetAnimation(0, "die_enemy", false);
    }
    public void WaterAt(Vector3 position)
    {
        if (waterHit == null) return;
        Level level = GameController.instance.currentLevel;
        if (level == null) return;
        GameObject newWaterHit = Instantiate(waterHit, level.layerController.mapLayer);
        newWaterHit.transform.position = position;
        newWaterHit.gameObject.SetActive(true);
        SkeletonGraphic waterhitSkeleton = newWaterHit.GetComponent<SkeletonGraphic>();
        waterhitSkeleton.AnimationState.SetAnimation(0, "hit", false);
    }
}
