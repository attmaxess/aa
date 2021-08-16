using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelectController : MonoBehaviour
{
    public Image image;
    public Sprite spriteDefault;
    public Material matDefault;

    public Sprite spriteHighlight;
    public Material matHighlight;
    public bool IsSelected
    {
        get { return _IsSelected; }
        set { _IsSelected = value; SetSelected(value); }
    }
    [SerializeField] bool _IsSelected = false;
    protected void SetSelected(bool isSelect)
    {
        if (image == null) return;
        image.sprite = isSelect ? spriteHighlight : spriteDefault;
        image.material = isSelect ? matHighlight : matDefault;
    }
    public void SetSelectedFromOutside(bool isSelect)
    {
        IsSelected = isSelect;
    }
}
