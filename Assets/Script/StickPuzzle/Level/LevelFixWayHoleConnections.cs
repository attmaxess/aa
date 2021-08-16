using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class LevelFixWayHoleConnections : BaseLevelProperties
{
    public WayHoleSO so;
    //public Sprite wayImage;
    public int chooseCombo = -1;
    WayHoleSO.WayCombo combo;
    public bool drawGrass = false;

    private void Reset()
    {
        if (GetComponents<LevelFixWayHoleConnections>().Length > 1)
        {
            Destroy(this);
        }
    }
    private void InitFromSO()
    {
        if (chooseCombo == -1)
        {
            combo = null;
            List<Transform> childs = level.layerController.mapLayer.GetComponentsInChildren<Transform>().ToList();
            Transform way = childs.Find(
                (x) => x.name.ToLower().Contains("way") &&
                x.GetComponent<Image>() != null &&
                x.GetComponent<Image>().sprite != null);
            if (way == null) return;
            combo = so.FindCombo(way.GetComponent<Image>().sprite);
            chooseCombo = so.combos.IndexOf(combo);
        }
        else
        {
            combo = so.combos[chooseCombo];
        }
    }
    [ContextMenu("CreateWays")]
    public void CreateWays()
    {
        Level1_1 level1_1 = level as Level1_1;
        if (level1_1 == null)
        {
            Debug.Log(level.transform.name + " khong phai 1_1");
            return;
        }

        Debug.Log("Tao duong " + level.transform.name);
        DeleteAllWays();
        InitFromSO();
        SetLayerSibling();
        level.listWay.RemoveAll((x) => x == null);
        if (combo == null)
        {
            Debug.Log("Khong tim thay combo");
            return;
        }

        int firstHoleID = FirstHoleInHierachyID();
        foreach (var hole in level.listHoles)
        {
            foreach (var connect in hole.connections)
            {
                if (IsWayExisted(hole, connect, out Way found))
                {
                    found.wayImage = combo.horizontal;
                    found.DrawWay(hole, connect, combo.horizontal);
                    continue;
                }
                CreateWay(hole,
                    connect,
                    combo.horizontal,
                    combo.vertical,
                    firstHoleID - 1);
            }
        }
    }
    void SetLayerSibling()
    {
        int minID = 9999;
        int maxID = -9999;
        foreach(Hole hole in level.listHoles)
        {
            int sibID = hole.transform.GetSiblingIndex();
            if (sibID > maxID) maxID = sibID;
            if (sibID < minID) minID = sibID;
        }
        wayLayer.transform.SetSiblingIndex(maxID + 1);
        grassLayer.transform.SetSiblingIndex(minID);
    }
    void CreateWay(
        Hole hole1,
        Hole hole2,
        Sprite spriteWay,
        Sprite spriteGrass,
        int sibID = -1)
    {
        if (so.wayPrefab == null) return;
        Way newWay = Instantiate(so.wayPrefab as GameObject, wayLayer).
            GetComponent<Way>();
        if (sibID >= 0) newWay.transform.SetSiblingIndex(sibID);
        newWay.transform.name = "Way_" + hole1.transform.name + "_" +
            hole2.transform.name;
        newWay.DrawWay(hole1, hole2, spriteWay);
        if (!level.listWay.Contains(newWay))
            level.listWay.Add(newWay);

        if (drawGrass)
        {
            Grass grass = Instantiate(so.grassPrefab as GameObject, grassLayer).
                GetComponent<Grass>();
            grass.transform.name = "Grass_" + hole1.transform.name + "_" + hole2.transform.name;
            grass.transform.SetSiblingIndex(newWay.transform.GetSiblingIndex() + 1);
            grass.DrawWay(hole1, hole2, spriteGrass);
        }
    }

    public void DeleteAllWays()
    {
        if (wayLayer != null)
            Debug.Log("Destroy ways di");
        //Destroy(wayLayer);
    }
    [ContextMenu("CreateGrass")]
    public void CreateGrass()
    {

    }
    bool IsWayExisted(Hole hole1, Hole hole2, out Way found)
    {
        found = level.listWay.Find((x) =>
        (x.lineController.linetransforms[0].transform == hole1.transform &&
        x.lineController.linetransforms[1].transform == hole2.transform) ||
        (x.lineController.linetransforms[0].transform == hole2.transform &&
        x.lineController.linetransforms[1].transform == hole1.transform));

        return found != null;
    }
    int FirstHoleInHierachyID()
    {
        if (level.listHoles.Count == 0) return -1;
        int idFirst = level.listHoles[0].transform.GetSiblingIndex();
        for (int i = 1; i < level.listHoles.Count; i++)
        {
            if (level.listHoles[i].transform.GetSiblingIndex() < idFirst)
                idFirst = level.listHoles[i].transform.GetSiblingIndex();
        }
        return idFirst;
    }
    #region layer properties
    GameObject levelLayer
    {
        get
        {
            if (_levelLayer == null) _levelLayer = Resources.Load<GameObject>("LevelUltis/LayerPrefab");
            return this._levelLayer;
        }
    }
    GameObject _levelLayer;
    public RectTransform wayLayer
    {
        get
        {
            if (this._wayLayer == null) this._wayLayer = level.layerController.mapLayer.Find("ways")?.GetComponent<RectTransform>();
            if (this._wayLayer == null) this._wayLayer = CreateLayer("ways", level.layerController.mapLayer, Quaternion.identity, Vector3.one);
            return this._wayLayer;
        }
    }
    RectTransform _wayLayer;
    public RectTransform grassLayer
    {
        get
        {
            if (this._grassLayer == null) this._grassLayer = level.layerController.mapLayer.Find("grass")?.GetComponent<RectTransform>();
            if (this._grassLayer == null) this._grassLayer = CreateLayer("grass", level.layerController.mapLayer, Quaternion.identity, Vector3.one);
            return this._grassLayer;
        }
    }
    RectTransform _grassLayer;
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
    [ContextMenu("RemoveConnectors")]
    public void RemoveConnectors()
    {
        List<Way> ways = level.layerController.mapLayer.GetComponentsInChildren<Way>().ToList();
        foreach (Way way in ways)
        {
            if (way.lineController.lineConnect != null)
            {
                Debug.Log("Destroy connect :" + way.transform.name);
                DestroyImmediate(way.lineController.lineConnect);
            }
        }
    }
    #endregion layer properties
}
