using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class BetweenScene : Singleton<BetweenScene>
{
    CanvasGroup canvasGroup
    {
        get
        {
            if (_canvasGroup == null) _canvasGroup = GetComponent<CanvasGroup>();
            return this._canvasGroup;
        }
    }
    [SerializeField] RectTransform circle = null;
    CanvasGroup _canvasGroup;
    public float during = .1f;
    [ContextMenu("Show100")]
    public void Show100()
    {
        canvasGroup.DOFade(1, during);
    }
    [ContextMenu("Show0")]
    public void Show0()
    {
        canvasGroup.DOFade(0, during);
    }
    [ContextMenu("ShowDark")]
    public void ShowDark()
    {
        circle.DOSizeDelta(new Vector2(450f, 450f), 0.5f).OnComplete(delegate
        {
            
        });
    }
    [ContextMenu("HideDark")]
    public void HideDark()
    {
        circle.DOSizeDelta(new Vector2(4500f, 4500f), 0.5f);
    }
}
