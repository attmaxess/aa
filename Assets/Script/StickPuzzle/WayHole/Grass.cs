using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grass : WayBaseProperties
{
    public WayHoleSO reference;
    public GameObject grassHolePrefab;
    [Space(20)]
    public GrassHole grass1;
    public GrassHole grass2;

    public Sprite wayImage;
    //public Color wayColor;
    //public Material wayMat;
    [SerializeField] bool isRef = false;
    private void Awake()
    {
        lineController.lineConnect.enabled = false;
    }
    public void DrawWay(Hole hole1, Hole hole2, Sprite sprite)
    {
        InstantiateGrassHole(hole1, out GrassHole grass1);
        InstantiateGrassHole(hole2, out GrassHole grass2);

        lineController.lineConnect.transforms = new RectTransform[2]{
            grass1.GetComponent<RectTransform>(),
            grass2.GetComponent<RectTransform>()};
        lineController.lineRender.sprite = sprite;

        lineController.UpdateLineRenderFromConnector();
    }
    void InstantiateGrassHole(Hole hole, out GrassHole grass)
    {
        grass = Instantiate(grassHolePrefab as GameObject, transform.parent).GetComponent<GrassHole>();
        grass.transform.name = "Grass_" + hole.transform.name;
        grass.transform.SetSiblingIndex(transform.GetSiblingIndex() + 1);
        grass.hole = hole.GetComponent<Hole>();
        grass.transform.position = hole.transform.position;
    }
}
