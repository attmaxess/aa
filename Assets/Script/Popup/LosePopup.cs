using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LosePopup : MonoBehaviour
{
    [SerializeField] CanvasGroup container = null;
    [SerializeField] GameObject levelPlay = null;
    [SerializeField] Transform boardShowLevel = null;
    [SerializeField] List<Transform> listButton = null;

    [Space(10)]
    [SerializeField] GameObject skipButton = null;
    [SerializeField] GameObject replaceButton = null;

    [Space(10)]
    [SerializeField] CrossAdsEvent crossAds = null;
    //
    [Space(10)]
    [SerializeField] List<Image> tIcons = null;
    [SerializeField] Sprite bgSmall = null, bgBig = null;
    [Space(10)]
    [SerializeField] List<Image> icons = null;
    [SerializeField] Slider sliderIcon = null;
    [Space(10)]
    [SerializeField] List<GameObject> ticks = null;
    [SerializeField] List<GameObject> locks = null;

    public void OnOpen()
    {
        this.gameObject.SetActive(true);
        listButton.ForEach(s => s.GetComponent<DOTweenAnimation>().DORestartById("Display"));
        SoundManager.instance.PlayLose();

        Level currentLevel = GameController.instance.currentLevel;
        currentLevel?.charactor?.circle?.gameObject.SetActive(false);

        Vector3 temp = GameController.instance.currentLevel.charactor.transform.position;
        container.DOFade(1, 1f);
        float ratio = 1f;

        levelPlay.transform.DOMove(boardShowLevel.transform.position - temp * ratio, .5f);
        levelPlay.transform.DOScale(ratio, .5f);

        TaskUtil.Delay(RateController.instance, delegate
        {
            RateController.instance.CheckRateStatus();
        }, 0.25f);

        TaskUtil.Delay(this, delegate
        {
            skipButton.transform.DOScale(1, 0.25f).OnComplete(delegate
            {
                TaskUtil.Delay(this, delegate
                {
                    //nextSkeleton.AnimationState.SetAnimation(0, "idle", true);
                }, 0.5f);
            });
            TaskUtil.Delay(this, delegate
            {
                replaceButton.transform.DOScale(1, 0.25f);
            }, .1f);
        }, .5f);
        SetIcon();
        TaskUtil.Delay(this, delegate
        {
            crossAds.ShowCrossAds();
        }, 0.1f);
    }

    private void SetIcon()
    {
        UIController.instance.winPopup.AddIconUpdateForList();
        int mod = GameController.instance.currentLevelID % 4;
        if (mod == 0)
        {
            mod = 4;
        }
        sliderIcon.value = (mod - 1) / 3f;
        int rangeListIcon;
        if (SectionSettings.TotalLevel != GameController.instance.currentLevelID)
        {
            rangeListIcon = (GameController.instance.currentLevelID - 2) / 4;
        }
        else
        {
            rangeListIcon = (GameController.instance.currentLevelID - 1) / 4;
        }
        for (int i = 0; i < icons.Count; i++)
        {
            icons[i].sprite = UIController.instance.winPopup.listIconLevel[rangeListIcon * 4 + i];
        }

        foreach (var item in ticks)
        {
            item.SetActive(false);
            item.gameObject.transform.localScale = new Vector3(0.8f, 0.8f, 1f);
        }
        foreach (var item in locks)
        {
            item.SetActive(false);
        }

        for (int i = 0; i < mod - 1; i++)
        {
            ticks[i].SetActive(true);
        }
        for (int i = mod; i < locks.Count; i++)
        {
            locks[i].SetActive(true);
        }

        for (int i = 0; i < tIcons.Count; i++)
        {
            tIcons[i].sprite = bgSmall;
            icons[i].transform.localScale = new Vector3(0.75f, 0.75f, 1f);
            if (i == mod - 1)
            {
                tIcons[i].sprite = bgBig;
                icons[i].transform.localScale = Vector3.one;
            }
            tIcons[i].SetNativeSize();
            icons[i].SetNativeSize();
        }
        if (SectionSettings.BiggestPlayLevel > (GameController.instance.currentLevelID / 4 + 1) * 4)
        {
            sliderIcon.value = 1f;
            for (int i = 0; i < ticks.Count; i++)
            {
                ticks[i].SetActive(true);
                if (i == mod - 1)
                {
                    ticks[i].transform.localScale = Vector3.one;
                }
            }
            for (int i = 0; i < locks.Count; i++)
            {
                locks[i].SetActive(false);
            }
        }
    }
    public void OnClose()
    {
        container.alpha = 0;
        listButton.ForEach(s => s.localScale = Vector2.zero);
        levelPlay.transform.DOKill();

        levelPlay.transform.localScale = Vector3.one;
        levelPlay.transform.localPosition = Vector3.zero;

        skipButton.transform.localScale = Vector3.zero;
        replaceButton.transform.localScale = Vector3.zero;

        crossAds.HideCrossAds();

        this.gameObject.SetActive(false);
    }
    public void OnClickSkipLevel()
    {
        Bridge.instance.ButtonSkipOffIn(GameController.instance.currentLevelID);
        UIController.instance.popupLoadingAds.OnOpen(LoadingAdsPopup.LoadingVideoType.ButtonSkip);
        Bridge.instance.ShowRewardForSkipLevel(delegate
        {
            if (GameController.instance.CanSkipNewLevel())
            {
                GameController.instance.SkipNewLevel();
                UIController.instance.gamePlay.OnOpen();
                UIController.instance.popupLoadingAds.OnClose();
            }
            else
            {
                UIController.instance.listLevel.OnOpen();
                GameController.instance.TrashAll();
            }
        }, GameController.instance.currentLevelID);

        OnClose();
    }

    public void OnReplaceButtonClick()
    {
        if (!GameController.instance.IsReadyToLoadLevel())
            return;

        StartCoroutine(C_OnReplaceButtonClick());
    }
    IEnumerator C_OnReplaceButtonClick()
    {
        GameController.instance.levelAsync.Trash();
        GameController.instance.LoadLevelByGameID(
            GameController.instance.currentLevelID);
        yield return new WaitUntil(() => GameController.instance.DoneLoadLevel == true);

        UIController.instance.popupLoadingAds.OnClose();
        UIController.instance.gamePlay.OnOpen();
        OnClose();
    }
}
