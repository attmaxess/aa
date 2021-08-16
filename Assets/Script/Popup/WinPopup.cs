using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

public class WinPopup : PopupBase
{
    [SerializeField] CanvasGroup container = null;
    [SerializeField] GameObject levelPlay = null;
    [SerializeField] Transform boardShowLevel = null;
    [SerializeField] Text tittleText = null;
    [SerializeField] List<Transform> listButton = null;
    [SerializeField] List<ParticleSystem> fxWin = null;
    [SerializeField] List<Transform> iqFX = null;

    [Space(10)]
    [SerializeField] Text iqText = null;
    //
    [Space(10)]
    [SerializeField] List<Image> tIcons = null;
    [SerializeField] Sprite bgSmall = null, bgBig = null;
    [Space(10)]
    [SerializeField] List<Image> icons = null;
    [SerializeField] Slider sliderIcon = null;
    [SerializeField] Transform highlightIcon = null;
    [Space(10)]
    [SerializeField] List<GameObject> ticks = null;
    [SerializeField] List<GameObject> locks = null;
    [Space(10)]
    [SerializeField] public List<Sprite> listIconLevel = null;
    [SerializeField] Sprite iconUpdate = null;

    bool isAddIconUpdate = false;
    //[SerializeField] SkeletonGraphic nextSkeleton;
    //[Space(10)]
    //[SerializeField] GameObject homeButton;
    //[SerializeField] GameObject replaceButton;
    //[SerializeField] GameObject shareButton;
    //[SerializeField] GameObject nextButton;

    [Space(10)]
    [SerializeField] CrossAdsEvent crossAds = null;

    string[] tittleRandoms = new string[5]
    {
        "So good",
        "Genius",
        "So magical",
        "So easy",
        "It's great"
    };

    private void Awake()
    {
        /*
        GameController.instance.imageCapturing
            .onpostNativeShare += ReOpenAfterCaptureShare;
        */
    }
    void ReOpenAfterCaptureShare()
    {
        Show100();
        GameController.instance.
            currentLevel.gameObject.SetActive(true);
        GameController.instance.gameCapture.
            currentLevel.gameObject.SetActive(false);
        GameController.instance.gameCapture.
            levelParent.gameObject.SetActive(false);
    }
    [ContextMenu("Show100")]
    public override void Show100()
    {
        base.Show100();
        fxWin.ForEach((x) => x.gameObject.SetActive(true));
        iqFX.ForEach((x) => x.gameObject.SetActive(true));
    }
    [ContextMenu("Show0")]
    public override void Show0()
    {
        base.Show0();
        fxWin.ForEach((x) => x.gameObject.SetActive(false));
        iqFX.ForEach((x) => x.gameObject.SetActive(false));
    }
    public void OnOpen(bool upperIQ)
    {
        this.gameObject.SetActive(true);

        if (!GameController.instance.playBeforeLastLevel &&
            GameController.instance.currentLevelID ==
            GameController.instance.GetTotalRealLevel())
        {
            SectionSettings.IsWinAll = true;
        }

        Level currentLevel = GameController.instance.currentLevel;
        currentLevel?.charactor?.circle?.gameObject.SetActive(false);

        //tittleText.text = tittleRandoms[Random.Range(0, 5)];
        listButton.ForEach(s => s.GetComponent<DOTweenAnimation>().DORestartById("Display"));
        SoundManager.instance.PlayWin();
        Vector3 temp = GameController.instance.currentLevel.charactor.transform.position;
        container.DOFade(1, 1f);
        float ratio = 1f;
        Level level = FindObjectOfType<Level>();
        float lastScale = level.charactor.startScale.magnitude
            / level.charactor.skeletonController.GetScale().magnitude;
        levelPlay.transform.DOMove(boardShowLevel.transform.position - temp * ratio, .5f);
        //ScaleLevel(lastScale);
        levelPlay.transform.DOScale(lastScale, 0.5f).OnComplete(() =>
        {
            foreach (var item in fxWin)
            {
                item.gameObject.SetActive(true);
            }
        });

        //nextSkeleton.AnimationState.SetAnimation(0, "off", false);

        RemoveAdsButtonState();

        TaskUtil.Delay(RateController.instance, delegate
        {
            RateController.instance.CheckRateStatus();
        }, 0.25f);

        //TaskUtil.Delay(this, delegate
        //{
        //    homeButton.transform.DOScale(1, 0.25f).OnComplete(delegate
        //    {
        //        TaskUtil.Delay(this, delegate
        //        {
        //            //nextSkeleton.AnimationState.SetAnimation(0, "idle", true);
        //        }, 0.5f);
        //    });
        //    TaskUtil.Delay(this, delegate
        //    {
        //        shareButton.transform.DOScale(1, 0.25f);
        //        TaskUtil.Delay(this, delegate
        //        {
        //            removeAdsButton.transform.DOScale(1, 0.25f);
        //        }, .1f);
        //    }, .1f);
        //}, .5f);

        SetIcon();

        if (upperIQ)
            TaskUtil.Delay(this, delegate { SetIQ(); }, 0.25f);

        TaskUtil.Delay(this, delegate
        {
            crossAds.ShowCrossAds();
        }, 0.1f);
    }
    void ScaleLevel(Vector3 lastScale)
    {
        StartCoroutine(C_Scale(lastScale));
    }
    IEnumerator C_Scale(Vector3 lastScale)
    {
        while ((levelPlay.transform.localScale - lastScale).magnitude > 0.1f)
        {
            levelPlay.transform.localScale = Vector3.Lerp(levelPlay.transform.localScale, lastScale, Time.deltaTime);
        }
        yield break;
    }

    void SetIQ()
    {
        float rand = TaskUtil.Round(Random.Range(0.3f, 0.7f), 2);
        float iq = SectionSettings.IQ;

        int divMob = (GameController.instance.currentLevelID - 1) % 4;

        SectionSettings.IQ = TaskUtil.Round(Mathf.Min(160f, (iq + rand)), 2);

        iqText.text = TaskUtil.Round(Mathf.Min(160f, iq), 2).ToString();
        GameController.instance.UpdateText(iq, SectionSettings.IQ, iqText, 0.5f, null);
    }
    public void AddIconUpdateForList()
    {
        if (isAddIconUpdate ||
            GameController.instance.GetTotalRealLevel() % 4 == 0)
            return;
        //SectionSettings.TotalLevel % 4 == 0) return;
        isAddIconUpdate = true;
        int div = GameController.instance.GetTotalRealLevel() / 4;
        int listCount = (div + 1) * 4;
        Debug.Log(listCount + " " + GameController.instance.GetTotalRealLevel());
        for (int i = 0; i < listCount - GameController.instance.GetTotalRealLevel(); i++)
        {
            listIconLevel.Add(iconUpdate);
        }
    }
    private void SetIcon()
    {
        AddIconUpdateForList();
        int mod = (
            GameController.instance.currentLevelID ==
            GameController.instance.GetTotalRealLevel() &&
            !GameController.instance.playBeforeLastLevel) ?
            GameController.instance.currentLevelID % 4 :
            (GameController.instance.currentLevelID - 1) % 4;

        if (mod == 0)
        {
            mod = 4;
        }
        int rangeListIcon;
        if (GameController.instance.GetTotalRealLevel() !=
            GameController.instance.currentLevelID)
        {
            rangeListIcon = (GameController.instance.currentLevelID - 2) / 4;
        }
        else
        {
            rangeListIcon = (GameController.instance.currentLevelID - 1) / 4;
        }
        for (int i = 0; i < icons.Count; i++)
        {
            icons[i].sprite = listIconLevel[rangeListIcon * 4 + i];
        }

        highlightIcon.gameObject.SetActive(false);
        foreach (var item in ticks)
        {
            item.SetActive(false);
            item.gameObject.transform.localScale = new Vector3(0.8f, 0.8f, 1f);
        }
        foreach (var item in locks)
        {
            item.SetActive(false);
        }
        for (int i = 0; i <= mod - 1; i++)
        {
            ticks[i].SetActive(true);
            if (i == mod - 1)
            {
                ticks[i].gameObject.transform.localScale = Vector3.one;
            }
        }
        for (int i = mod + 1; i < locks.Count; i++)
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
                highlightIcon.localPosition = tIcons[i].gameObject.transform.localPosition;
            }
            tIcons[i].SetNativeSize();
            icons[i].SetNativeSize();
        }

        if (mod == 1)
        {
            if (GameController.instance.currentLevelID < SectionSettings.BiggestPlayLevel)
            {
                for (int i = 0; i < ticks.Count; i++)
                {
                    ticks[i].SetActive(true);
                }
                for (int i = 0; i < locks.Count; i++)
                {
                    locks[i].SetActive(false);
                }
            }
            else
            {
                sliderIcon.value = 0f;
            }
            highlightIcon.gameObject.SetActive(true);
        }
        else
        {
            if (GameController.instance.currentLevelID < SectionSettings.BiggestPlayLevel)
            {
                highlightIcon.gameObject.SetActive(true);
                sliderIcon.value = 1f;
                for (int i = 0; i < ticks.Count; i++)
                {
                    ticks[i].SetActive(true);
                }
                for (int i = 0; i < locks.Count; i++)
                {
                    locks[i].SetActive(false);
                }
            }
            else
            {
                sliderIcon.value = (mod - 2) / 3f;
                TaskUtil.Delay(this, delegate
                {
                    sliderIcon.DOValue((mod - 1) / 3f, 1.5f).OnComplete(() =>
                    {
                        highlightIcon.gameObject.SetActive(true);
                    });
                }, 1f);
            }
        }
    }

    public void OnClose()
    {
        foreach (var item in fxWin)
        {
            item.gameObject.SetActive(false);
        }
        container.alpha = 0;
        listButton.ForEach(s => s.localScale = Vector2.zero);
        levelPlay.transform.DOKill();

        levelPlay.transform.localScale = Vector3.one;
        levelPlay.transform.localPosition = Vector3.zero;

        GameController.instance.gameCapture.levelParent
            .gameObject.SetActive(false);

        crossAds.HideCrossAds();

        this.gameObject.SetActive(false);
    }
    public void OnClickNextLevel()
    {
        if (!GameController.instance.IsReadyToLoadLevel())
            return;

        StartCoroutine(C_OnClickNextLevel());
    }
    IEnumerator C_OnClickNextLevel()
    {
        if (GameController.instance.currentLevelID >=
            GameController.instance.GetTotalRealLevel() &&
            !GameController.instance.playBeforeLastLevel)
        {
            UIController.instance.listLevel.OnOpen(true);
            GameController.instance.TrashAll();
        }
        else
        {
            GameController.instance.LoadLevelByGameID(
                GameController.instance.currentLevelID);
            yield return new WaitUntil(() => GameController.instance.DoneLoadLevel == true);

            if (GameController.instance.currentLevelID >= 3)
            {
                Bridge.instance.ShowInterstitial(InterstitialPosition.NextButtonClick);
            }
            UIController.instance.gamePlay.OnOpen();
        }

        OnClose();
    }
    public void OnClickButtonShare()
    {
        /*
        Show0();
        GameController.instance.
            currentLevel.gameObject.SetActive(false);
        GameController.instance.gameCapture.
            currentLevel.gameObject.SetActive(true);
        GameController.instance.gameCapture.
            levelParent.gameObject.SetActive(true);
        
        TaskUtil.Delay(this, delegate
        {
            GameController.instance.CaptureShare();
        }, 0.01f);
        */
        GameController.instance.imageCapturing.NativeShare();
    }

    public void OnClickButtonReplace()
    {
        if (!GameController.instance.IsReadyToLoadLevel())
            return;

        StartCoroutine(C_OnClickButtonReplace());
    }
    IEnumerator C_OnClickButtonReplace()
    {
        GameController.instance.levelAsync.Trash();

        if (GameController.instance.CanNextAfterWin)
            GameController.instance.LoadLevelByGameID(
                GameController.instance.currentLevelID - 1);
        else
            GameController.instance.LoadLevelByGameID(
                GameController.instance.currentLevelID);

        yield return new WaitUntil(() => GameController.instance.DoneLoadLevel == true);

        UIController.instance.popupLoadingAds.OnClose();
        UIController.instance.gamePlay.OnOpen();
        OnClose();
    }

    public void OnClickBackToHome()
    {
        UIController.instance.StartFadeScreen(delegate
        {
            UIController.instance.BackToMain();

            SoundManager.instance.PlayBackgroundSound(SoundManager.instance.listMusicMainMenu);

            OnClose();
        });
    }

    public void OnClickRemoveADS()
    {
        Bridge.instance.RemoveAds(delegate
        {
            SectionSettings.RemoveAds = true;
            RemoveAdsButtonState();
            UIController.instance.RemoveAdsButtonState();
        });
    }

    void RemoveAdsButtonState()
    {
        //removeAdsButton.interactable = !SectionSettings.RemoveAds;
        //if (SectionSettings.RemoveAds)
        //{
        //    removeAdsButton.gameObject.GetComponent<SkeletonGraphic>().AnimationState.SetAnimation(0, "off", false);
        //}
        //else
        //{
        //    removeAdsButton.gameObject.GetComponent<SkeletonGraphic>().AnimationState.SetAnimation(0, "idle", true);
        //}
    }
}
