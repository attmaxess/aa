using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

public class WallHint : MonoBehaviour
{
    public Wall wall;
    public Image image;
    public SnapRotation hintRotation;
    public TextMeshProUGUI hint;
    public CanvasGroup canvasGroup;
    public bool IsNextHint
    {
        get { return this._IsNextHint; }
        set { this._IsNextHint = value; OnpostSetNextHint(value); }
    }
    [ReadOnly] public bool _IsNextHint = false;
    private void OnpostSetNextHint(bool value)
    {
        float alpha = value ? 1f : 50f / 255f;
        Color color = hint.color;
        color = new Color(color.r, color.g, color.b, alpha);
        hint.color = color;
        wall.SetFocus(value);
    }
    private void Start()
    {
        IsNextHint = false;
    }
    public int GetIntHint()
    {
        int.TryParse(hint.text, out int i);
        return i;
    }
    public void Show0()
    {
        canvasGroup.alpha = 0;
    }
    public void Show100()
    {
        canvasGroup.alpha = 1;
    }
}
