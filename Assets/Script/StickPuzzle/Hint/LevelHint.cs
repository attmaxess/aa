using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI.Extensions;

public class LevelHint : BaseLevelProperties
{
    public HandHint hand;

    public bool ShowHintAtStart = false;
    /// <summary>
    /// Đây là biến hiện toàn bộ hint của level
    /// Nếu được set = true thì sẽ hiện toàn bộ hint lúc start, không cần phải bấm nút
    /// </summary>
    [ReadOnly] public bool IsShow = false;
    [ReadOnly] public bool DoneShow = false;
    [ReadOnly] public bool IsRepeating = false;
    [ReadOnly] public bool DoneSync = true;

    public Coroutine coShow = null;
    public Coroutine coSync = null;
    public Coroutine coRepeat = null;

    protected virtual void Start()
    {
        level.postWin += HideHint;
        level.postLose += HideHint;

        HoleHint charactorBoard = level.charactor.hole.hint;
        if (charactorBoard != null)
        {
            if (ShowHintAtStart)
            {
                ShowHint();
                charactorBoard.ForceHideBoard();
            }
            else if (charactorBoard.ShowAtStart)
            {
                Vector3 start = charactorBoard.hintText.transform.position + hand.offset3(); ;
                hand.transform.position = start;
                hand.gameObject.SetActive(true);
                hand.DoTapByAnim();
            }
        }

        if (ContainHints())
            UIController.instance.gamePlay.ShowButtonHint();
        else
            UIController.instance.gamePlay.HideButtonHint();
    }
    public virtual void ShowHint()
    {
        if (IsShow) return;
        IsShow = true;
    }
    public virtual void GetAllHint() { }
    public virtual void HideHint() { }
    public virtual void HideHint(bool yes) { if (yes) HideHint(); }
    public virtual void RemoveAllHint() { }
    public virtual bool ContainHints() { return false; }
    public virtual void SyncWithCurrentManager() { }
    protected virtual void Repeat() { }
    public void HideHand() { hand.gameObject.SetActive(false); }
    public virtual void StopCoShow() { StopCo(out coShow, coShow); }
    public virtual void StopCoSync() { StopCo(out coSync, coSync); }
    public virtual void StopCoRepeat() { StopCo(out coRepeat, coRepeat); }
    void StopCo(out Coroutine outCo, Coroutine co)
    {
        outCo = co;
        if (co != null)
        {
            StopCoroutine(co);
            outCo = null;
        }
    }
}
