using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;
using Spine.Unity;
using Spine;
using UnityEngine.UI.Extensions;

public class UIController : MonoBehaviour
{
    public static UIController instance;
    public ListLevel listLevel;
    public WinPopup winPopup;
    public LosePopup losePopup;
    public Image hintObj;
    public Image hintAdsIcon;

    [SerializeField] Button removeAdsButton = null;

    [Space(10)]
    [SerializeField] Image panelPopup = null;

    [Space(10)]
    [SerializeField] GameObject menuObj = null;
    [SerializeField] Image fadeScreen = null;

    [SerializeField] CrossAdsEvent crossAds = null;
    public LoadingAdsPopup popupLoadingAds;

    public GamePlay gamePlay;
    [Header("Home")]
    [Space(10)]
    [SerializeField] List<Transform> listSettingButton = new List<Transform>();
    [Space(10)]
    [SerializeField] Image soundImage = null;
    [SerializeField] Sprite soundEnableSprite = null;
    [SerializeField] Sprite soundDisableSprite = null;
    [SerializeField] Text levelText = null;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }

        Application.targetFrameRate = 60;
    }

    void Start()
    {
        levelText.text = "Level " + "<color=#f1e870>" +

            (((SectionSettings.BiggestPlayLevel - 1) < 10 &&
            (SectionSettings.BiggestPlayLevel - 1) > 0) ? "0" : "") +

            (SectionSettings.BiggestPlayLevel == GameController.instance.GetTotalRealLevel() ?
            SectionSettings.BiggestPlayLevel :
            SectionSettings.BiggestPlayLevel - 1).ToString() +

            "</color>" + "/" + GameController.instance.GetTotalRealLevel();

        RemoveAdsButtonState();
        UpdateSoundState();
        SoundManager.instance.PlayBackgroundSound(SoundManager.instance.listMusicMainMenu);

        if (SectionSettings.RemoveAds)
            removeAdsButton.interactable = false;

        if (SectionSettings.FirstOpen)
        {
            SectionSettings.FirstOpen = false;
            HideMainMenu();
            gamePlay.OnOpen();
            GameController.instance.LoadLevelByGameID(
                GameController.instance.currentLevelID);
        }
        else
        {
            if (GameController.instance.currentLevelID ==
                GameController.instance.GetTotalRealLevel())
            {
                listLevel.OnOpen(true);
                HideMainMenu();
            }
        }

        TaskUtil.Delay(this, delegate
        {
            crossAds.ShowCrossAds();
        }, 0.5f);
    }

    public void BackToMain()
    {
        menuObj.SetActive(true);
        ResetZoomSetting();
        levelText.text = "Level " + "<color=#f1e870>" +

            (((SectionSettings.BiggestPlayLevel - 1) < 10 &&
            (SectionSettings.BiggestPlayLevel - 1) > 0) ? "0" : "") +

            (SectionSettings.BiggestPlayLevel == GameController.instance.GetTotalRealLevel() ?
            SectionSettings.BiggestPlayLevel :
            SectionSettings.BiggestPlayLevel - 1).ToString() + "</color>" + "/" +
            GameController.instance.GetTotalRealLevel();

        GameController.instance.DestroyLevel();
        GameController.instance.levelAsync.Trash();

        crossAds.ShowCrossAds();
    }

    public void HideMainMenu()
    {
        menuObj.SetActive(false);
    }

    public void StartFadeScreen(Callback callback)
    {
        fadeScreen.gameObject.SetActive(true);
        fadeScreen.DOFade(1, 0.45f).OnComplete(delegate
        {
            EndFadeScreen();

            if (callback != null)
            {
                callback.Invoke();
            }
        }).SetEase(Ease.InBack);
    }

    public void EndFadeScreen()
    {
        fadeScreen.DOFade(0, 0.45f).OnComplete(delegate
        {
            fadeScreen.gameObject.SetActive(false);
        }).SetEase(Ease.OutBack);
    }

    bool animLogoComplete = false;

    public void OnPlayButtonClick()
    {
        if (!GameController.instance.IsReadyToLoadLevel())
            return;

        SoundManager.instance.PlayAudioClip(SoundManager.instance.playButton);
        StartFadeScreen(delegate
        {
            GameController.instance.LoadLevelByGameID(SectionSettings.BiggestPlayLevel);

            animLogoComplete = true;

            HideMainMenu();
            gamePlay.OnOpen();
        });

        crossAds.HideCrossAds();
    }

    [ContextMenu("PlayCurrentEditorLevel")]
    public void PlayCurrentEditorLevel()
    {
        if (!GameController.instance.IsReadyToLoadLevel())
            return;

        SoundManager.instance.PlayAudioClip(SoundManager.instance.playButton);
        StartFadeScreen(delegate
        {
            GameController.instance.LoadLevelByGameID(GameController.instance.currentLevelID);

            animLogoComplete = true;

            HideMainMenu();
            gamePlay.OnOpen();
        });

        crossAds.HideCrossAds();
    }
    public void OnClickSelectLevel()
    {
        StartFadeScreen(delegate
        {
            animLogoComplete = true;
            HideMainMenu();
            listLevel.OnOpen(scrollToFirst:false, firstOpen:true);
        });

        crossAds.HideCrossAds();
    }
    public void OnClickRemoveADS()
    {
        Bridge.instance.RemoveAds(delegate
        {
            SectionSettings.RemoveAds = true;
            RemoveAdsButtonState();
        });
    }

    public void RemoveAdsButtonState()
    {
        removeAdsButton.gameObject.SetActive(!SectionSettings.RemoveAds);
        ListLevel.instance.RemoveAdsButtonState(SectionSettings.RemoveAds);
    }

    public void FadeInPanel()
    {
        panelPopup.gameObject.SetActive(true);
        StartCoroutine(FadeIn(190f / 255f, panelPopup, 0.2f));
    }

    public void FadeInPanelForLoading()
    {
        panelPopup.gameObject.SetActive(true);
        StartCoroutine(FadeIn(150f / 255f, panelPopup, 0.1f));
    }

    public void FadeOutPanel()
    {
        StartCoroutine(FadeOut(panelPopup, 0.2f, delegate
        {
            panelPopup.gameObject.SetActive(false);
        }));
    }

    IEnumerator FadeIn(float target, Image image, float time, Callback callback = null)
    {
        var color = image.color;
        float alpha = 0;
        float currentTime = 0f;
        float targetAlpha = target;
        while (currentTime < time)
        {
            alpha = Mathf.Lerp(0, targetAlpha, currentTime / time);
            color.a = alpha;
            image.color = color;
            currentTime += Time.deltaTime;
            yield return null;
        }
        color.a = targetAlpha;
        image.color = color;

        if (callback != null)
        {
            callback.Invoke();
        }
    }

    IEnumerator FadeOut(Image image, float time, Action onComplete = null)
    {
        var alpha = image.color.a;
        for (var t = 0.0f; t < 1.0f; t += Time.deltaTime / time)
        {
            var color = image.color;
            color.a = Mathf.Lerp(alpha, 0, t);
            image.color = color;
            yield return null;
        }
        if (onComplete != null)
        {
            onComplete();
        }
    }


    /// <summary>
    /// Dev Thắng ->|||
    /// </summary>
    bool settingGroupShowing = false;
    bool isPlaySettingAnim = false;

    void ZoomInSetting()
    {
        isPlaySettingAnim = true;
        settingGroupShowing = false;

        listSettingButton.ForEach(s => s.GetComponent<DOTweenAnimation>().DORestartById("In"));
        TaskUtil.Delay(this, delegate
        {
            isPlaySettingAnim = false;
        }, 0.5f);
    }

    void ZoomOutSetting()
    {
        isPlaySettingAnim = true;
        settingGroupShowing = true;
        listSettingButton.ForEach(s => s.GetComponent<DOTweenAnimation>().DORestartById("Out"));
        TaskUtil.Delay(this, delegate
        {
            isPlaySettingAnim = false;
        }, 0.5f);
    }

    public void ResetZoomSetting()
    {
        isPlaySettingAnim = false;
        settingGroupShowing = false;
        listSettingButton.ForEach(s => s.localScale = Vector3.zero);
    }

    void UpdateSoundState()
    {
        soundImage.sprite = SoundManager.instance.SoundEnable ? soundEnableSprite : soundDisableSprite;
    }

    public void OnSoundButtonClick()
    {
        SoundManager.instance.SoundEnable = !SoundManager.instance.SoundEnable;
        UpdateSoundState();
        SoundManager.instance.UpdateSoundBG();
    }

    public void OnSettingButtonClick()
    {
        if (isPlaySettingAnim) return;

        if (settingGroupShowing)
        {
            ZoomInSetting();
        }
        else
        {
            ZoomOutSetting();
        }
    }
}
