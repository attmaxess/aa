using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelLayerController : BaseLevelProperties
{
    GameObject levelLayer
    {
        get
        {
            if (_levelLayer == null) _levelLayer = Resources.Load<GameObject>("LevelUltis/LayerPrefab");
            return this._levelLayer;
        }
    }
    GameObject _levelLayer;

    [ContextMenu("DoSort")]
    public void DoSort()
    {
        Transform backgroundTr = TransformDeepChildExtension.FindDeepChild(this.transform, "Background");
        //level.transform.Find("Background");
        if (backgroundTr != null)
        {
            SortIntoLayer(backgroundTr, mapLayer);
            foreach (Transform tr in backgroundTr.GetComponentsInChildren<Transform>())
            {
                if (tr != backgroundTr)
                    SortIntoLayer(tr, mapLayer);
            }
            backgroundTr.SetAsFirstSibling();
            backgroundTr.localScale = new Vector3(3, 3, 3);
        }
        mapLayer.SetAsLastSibling();
        if (AstarPath.active != null)
            SortIntoLayer(AstarPath.active.transform, mapLayer);
        SortIntoLayer(level.mapBound?.transform, mapLayer);

        mammalLayer.SetSiblingIndex(mapLayer.GetSiblingIndex() + 1);
        SortIntoLayer(level.charactor.transform, mammalLayer);
        foreach (Enemy enemy in level.listEnemies)
            SortIntoLayer(enemy.transform, mammalLayer);
        foreach (Trap trap in level.GetComponentsInChildren<Trap>())
            SortIntoLayer(trap.transform, mammalLayer);
        level.charactor.transform.SetAsLastSibling();

        levelControllerLayer.SetSiblingIndex(mammalLayer.GetSiblingIndex() + 1);
        SortIntoLayer(level.levelController.transform, levelControllerLayer);
    }

    void SortIntoLayer(Transform tr, Transform layer)
    {
        if (tr != null && layer != null)
            tr.SetParent(layer);

    }
    RectTransform CreateLayer(
        string goName,
        Transform parent,
        Quaternion localRotation,
        Vector3 localScale)
    {
        GameObject newGO = Instantiate(levelLayer);
        newGO.SetActive(true);
        newGO.name = goName;
        newGO.transform.parent = parent;
        newGO.transform.localRotation = localRotation;
        newGO.transform.localScale = localScale;
        RectTransform rt = newGO.GetComponent<RectTransform>();
        rt.pivot = new Vector2(.5f, .5f);
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        return rt;
    }

    #region layer definitions
    public RectTransform mapLayer
    {
        get
        {
            if (this._mapLayer == null) this._mapLayer = transform.Find("map")?.GetComponent<RectTransform>();
            if (this._mapLayer == null) this._mapLayer = CreateLayer("map", transform, Quaternion.identity, Vector3.one);
            return this._mapLayer;
        }
    }
    RectTransform _mapLayer;
    public RectTransform mammalLayer
    {
        get
        {
            if (this._mammal == null) this._mammal = transform.Find("mammal")?.GetComponent<RectTransform>();
            if (this._mammal == null) this._mammal = CreateLayer("mammal", transform, Quaternion.identity, Vector3.one);
            return this._mammal;
        }
    }
    RectTransform _mammal;
    public RectTransform levelControllerLayer
    {
        get
        {
            if (this._levelControllerLayer == null) this._levelControllerLayer = transform.Find("levelController")?.GetComponent<RectTransform>();
            if (this._levelControllerLayer == null) this._levelControllerLayer = CreateLayer("levelController", transform, Quaternion.identity, Vector3.one);
            return this._levelControllerLayer;
        }
    }
    RectTransform _levelControllerLayer;
    #endregion
}
