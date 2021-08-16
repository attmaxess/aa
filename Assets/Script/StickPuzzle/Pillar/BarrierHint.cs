using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

public class BarrierHint : BarrierBaseProperties
{
    public Transform hintTr;
    public TextMeshProUGUI hintText;
    public CanvasGroup canvasGroup;
    public bool IsNextHint
    {
        get { return this._IsNextHint; }
        set { this._IsNextHint = value; OnpostSetNextHint(value); }
    }
    [ReadOnly] public bool _IsNextHint = false;
    private void OnpostSetNextHint(bool value)
    {
        float alpha = value ? 1f : 0f / 255f;
        Color color = lineController.lineRender.color;
        color = new Color(color.r, color.g, color.b, alpha);
        lineController.lineRender.color = color;
    }
    private void Awake()
    {
        canvasGroup.alpha = 0;
    }
    private void Start()
    {
        IsNextHint = false;
    }
    [ContextMenu("PositionHint")]
    public void PositionHint()
    {
        hintTr.transform.position = (lineController.linetransforms[0].position +
            lineController.linetransforms[1].position) / 2 + new Vector3(-50, 50f, 0);
    }
    [ContextMenu("Show0")]
    public void Show0()
    {
        canvasGroup.alpha = 0;
    }
    [ContextMenu("Show100")]
    public void Show100()
    {
        canvasGroup.alpha = 1;
    }
    public bool IsEqual(Barrier other)
    {
        return (this != other) &&
            ((lineController.linetransforms[0] == other.lineController.linetransforms[0] &&
            lineController.linetransforms[1] == other.lineController.linetransforms[1]) ||
            (lineController.linetransforms[0] == other.lineController.linetransforms[1] &&
            lineController.linetransforms[1] == other.lineController.linetransforms[0]));
    }
    public Transform GetOtherPillar(Transform pillar)
    {
        if (lineController.linetransforms.Count != 2) return null;
        Transform other = lineController.linetransforms[0].transform == pillar ?
            lineController.linetransforms[1] : lineController.linetransforms[0];
        return other;
    }
    [ContextMenu("RefreshHint")]
    public void RefreshHint()
    {
        lineController.UpdateLineRenderFromConnector();
        PositionHint();
        transform.name = "Hint" + hintText.text;
    }
    public int GetIntHint()
    {        
        int.TryParse(hintText.text, out int hintInt);
        return hintInt;
    }
    [ContextMenu("1")] public void SetHint1() { hintText.text = "1"; hintText.gameObject.SetActive(true); RefreshHint(); } 
    [ContextMenu("2")] public void SetHint2() { hintText.text = "2"; hintText.gameObject.SetActive(true); RefreshHint(); }
    [ContextMenu("3")] public void SetHint3() { hintText.text = "3"; hintText.gameObject.SetActive(true); RefreshHint(); }
    [ContextMenu("4")] public void SetHint4() { hintText.text = "4"; hintText.gameObject.SetActive(true); RefreshHint(); }
    [ContextMenu("5")] public void SetHint5() { hintText.text = "5"; hintText.gameObject.SetActive(true); RefreshHint(); }
    [ContextMenu("6")] public void SetHint6() { hintText.text = "6"; hintText.gameObject.SetActive(true); RefreshHint(); }
    [ContextMenu("7")] public void SetHint7() { hintText.text = "7"; hintText.gameObject.SetActive(true); RefreshHint(); }
    [ContextMenu("8")] public void SetHint8() { hintText.text = "8"; hintText.gameObject.SetActive(true); RefreshHint(); }
    [ContextMenu("0")] public void SetHint0() { hintText.text = "0"; hintText.gameObject.SetActive(false); RefreshHint(); } 
}
