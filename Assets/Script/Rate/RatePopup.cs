using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using System.Collections;
using System;

public class RatePopup : MonoBehaviour
{
    [SerializeField] GameObject container = null;

    public void OnOpen()
    {
        gameObject.SetActive(true);
        container.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
        UIController.instance.FadeInPanel();
    }

    public void OnClose()
    {
        UIController.instance.FadeOutPanel();
        container.transform.localScale = Vector3.zero;
        gameObject.SetActive(false);
    }
        
    public void OnRateButtonClick()
    {
        Debug.LogError("Rate");

        RateController.instance.HasRate = true;
        Bridge.instance.OpenRate();
        OnClose();
    }

    [ContextMenu("Rate")]
    void Show()
    {
        OnOpen();
    }    
}