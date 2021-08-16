using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrossAdsEvent : MonoBehaviour
{
    [SerializeField] RectTransform container = null;

    public void ShowCrossAds()
    {
        Bridge.instance.ShowCrossAds(container, 0.5f);
    }

    public void HideCrossAds()
    {
        Bridge.instance.HideCrossAds();
    }
}
