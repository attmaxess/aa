using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GridScalingController : GridBaseProperties
{
    public float charScaling;

    public float totalHealth;
    public float maxHealth;
    private void Start()
    {
        StartCoroutine(C_Start());
    }
    IEnumerator C_Start()
    {
        if (GameController.instance.currentLevel != null)
            yield return new WaitUntil(() => GameController.instance.DoneLoadLevel == true);

        SelfSetup();
    }
    [ContextMenu("SelfSetup")]
    public void SelfSetup()
    {
        charScaling = -1;
        totalHealth = 0;

        Enemy boss = level.listEnemies.Find((x) => x.boss != null);
        if (boss == null) return;
        if (level.charactor == null) return;

        maxHealth = boss.Health;

        Vector3 bossOriginal = boss.listSkeleton[0].transform.localScale;
        Vector3 charOriginal = level.charactor.listSkeleton[0].transform.localScale;

        boss.listSkeleton[0].MatchRectTransformWithBounds();
        float bossHeight = boss.listSkeleton[0].transform.GetComponent<RectTransform>().sizeDelta.y * bossOriginal.y;
        level.charactor.listSkeleton[0].MatchRectTransformWithBounds();
        float charHeight = level.charactor.listSkeleton[0].transform.GetComponent<RectTransform>().sizeDelta.y * charOriginal.y;
        float charHeightScaleUnit = charHeight / charOriginal.y;

        int countHP = 0;
        foreach (OneGrid grid in bigGrid.gridsData)
            if (grid.gridType == eGridType.Player || grid.gridType == eGridType.TenHP)
                countHP++;

        charScaling = countHP != 0 ? ((bossHeight + 10) - charHeight) / countHP / charHeightScaleUnit : 0;
    }
    public void AddScaleFromFlyingHealth(FlyingHealth flying, UnityAction action)
    {
        Vector3 scale = level.charactor.listSkeleton[0].transform.localScale;
        scale += new Vector3(CordinateDirection(scale.x),
            CordinateDirection(scale.y),
            CordinateDirection(scale.z)) * charScaling;
        level.charactor.skeletonController.SetScale(scale, 0);
        totalHealth += flying.Health;
        PlaceCharactor();
    }
    public void RemoveFlyingScale()
    {
        Vector3 scale = level.charactor.listSkeleton[0].transform.localScale;
        scale -= new Vector3(CordinateDirection(scale.x),
            CordinateDirection(scale.y),
            CordinateDirection(scale.z)) * charScaling;
        level.charactor.skeletonController.SetScale(scale, 0);
        totalHealth -= 10;
        PlaceCharactor();
    }

    void PlaceCharactor()
    {
        int totalCharactor;
        if (totalHealth <= maxHealth / 3f) totalCharactor = 1;
        else if (totalHealth > maxHealth / 3f && totalHealth <= maxHealth * 2f / 3f) totalCharactor = 2;
        else totalCharactor = 3;
        level.charactor.totalCharactor = totalCharactor;
        level.charactor.UpdateCharactorObj();
    }

    float CordinateDirection(float cord)
    {
        return cord < 0 ? -1 : 1;
    }
}
