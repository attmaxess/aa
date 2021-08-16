using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using TMPro;

public class TextPopup : Singleton<TextPopup>
{
    [SerializeField] CanvasGroup container = null;
    [SerializeField] TextMeshProUGUI text = null;

    public void Open()
    {
        Open();
    }
    public void Open(string text = "")
    {
        SetText(text);

        container.transform.localPosition = Vector3.zero;
        container.alpha = 1;
        container.interactable = true;
        container.blocksRaycasts = true;
    }
    void SetText(string text)
    {
        this.text.text = text;
    }
    public void Close()
    {
        container.alpha = 0;
        container.interactable = false;
        container.blocksRaycasts = false;
    }
}
