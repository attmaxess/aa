using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

public class HoleHint : MonoBehaviour
{
    public Hole hole
    {
        get
        {
            if (this._hole == null) this._hole = GetComponentInParent<Hole>();
            return this._hole;
        }
    }
    Hole _hole;
    Attackable _attackable;
    public TextMeshProUGUI hintText;
    public Animator animtor;
    public CanvasGroup canvasGroup;
    public bool ShowAtStart = false;
    /// <summary>
    /// Đây là biến hiện board hint của hole charactor
    /// Đừng nhầm lẫn với ShowAtStart của level hint manager
    /// </summary>

    [Header("Jump config")]
    [ReadOnly] public float jumpPower = 1f;
    [ReadOnly] public int intJump = 10;
    public bool IsNextHint
    {
        get { return this._IsNextHint; }
        set { this._IsNextHint = value; OnpostSetNextHint(value); }
    }
    [ReadOnly] public bool _IsNextHint = false;
    private void OnpostSetNextHint(bool value)
    {
        if (hole != null && hole.glowImg != null)
        {
            hole.glowImg.gameObject.SetActive(value);
            /*
            if (value)
                //hole.glow.SetGlow();
                
            else
                //hole.glow.SetNoGlow();
            */
        }
    }
    private void Awake()
    {
        if (ShowAtStart)
            animtor.enabled = true;
        else
            Show0();
    }
    private void Start()
    {
        IsNextHint = false;
    }
    public int GetIntHint()
    {
        int.TryParse(hintText.text, out int i);
        return i;
    }
    [ContextMenu("Jump")]
    public void Jump()
    {
        transform.DOJump(transform.position, jumpPower, intJump, .5f, true);
    }
    public void SetHint(string text)
    {
        hintText.text = text;
    }
    public void CharactorShowHint()
    {
        hole.level.ShowHint();
        HideBoard();
    }
    public void ShowBoard()
    {
        animtor.SetBool("Show", true);
        animtor.enabled = true;
    }
    public void HideBoard()
    {
        animtor.SetBool("Show", false);
        animtor.enabled = true;
    }
    public void ForceHideBoard()
    {
        animtor.enabled = false;
        transform.localScale = new Vector3(0, 0, transform.localScale.z);
    }
    public void Show100()
    {
        canvasGroup.alpha = 1;
    }
    public void Show0()
    {
        canvasGroup.alpha = 0;
    }
}
