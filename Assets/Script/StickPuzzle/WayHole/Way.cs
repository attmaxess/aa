using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI.Extensions;

public class Way : WayBaseProperties
{
    public WayHoleSO reference;

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
        lineController.lineConnect.transforms = new RectTransform[2]{
            hole1.GetComponent<RectTransform>(),
            hole2.GetComponent<RectTransform>()};
        lineController.lineRender.sprite = sprite;
        lineController.UpdateLineRenderFromConnector();
    }
}
