using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GamePlay : MonoBehaviour
{
    public string levelName
    {
        get { return _levelName; }
        set { _levelName = value; SetLevelText(value); }
    }
    string _levelName = string.Empty;

    public Text levelNameSquare;
    public Text levelNameNormal;
    public Text levelNameLong;

    public Button btnHint;

    void SetLevelText(string value)
    {
        levelNameSquare.text = value;
        levelNameNormal.text = value;
        levelNameLong.text = value;
    }

    public void OnOpen()
    {
        this.gameObject.SetActive(true);
        GameController.instance.currentState = GameController.State.Play;
        //Debug.LogError("Open");
    }
    public void OnClose()
    {
        GameController.instance.currentState = GameController.State.Menu;
        SoundManager.instance.StopCrownBattle();
        SoundManager.instance.StopMoving();
        this.gameObject.SetActive(false);
        //Debug.LogError("Close");
    }

    public void OnClickSelectLevelFromWinPopup()
    {
        if (UIController.instance.popupLoadingAds.isOpen)
        {
            UIController.instance.popupLoadingAds.OnClose();
        }

        UIController.instance.StartFadeScreen(delegate
        {
            UIController.instance.listLevel.OnOpen();
            UIController.instance.winPopup.OnClose();
            OnClose();

            if (GameController.instance.currentLevelID >= 3)
            {
                Bridge.instance.ShowInterstitial(InterstitialPosition.BackButtonClick);
            }
        });

        SoundManager.instance.PlayBackgroundSound(SoundManager.instance.listMusicSelectLevel);
    }

    public void OnClickSelectLevel()
    {
        StartCoroutine(C_OnClickSelectLevel());
    }
    IEnumerator C_OnClickSelectLevel()
    {
        if (GameController.instance.currentLevel == null)
        {
            ShowListLevel();
            yield break;
        }

        if (GameController.instance.currentState == GameController.State.Menu ||
            GameController.instance.currentLevel.isWin ||
            GameController.instance.currentLevel.isLose)
            yield break;

        ShowListLevel();

        yield break;
    }
    void ShowListLevel()
    {
        if (UIController.instance.popupLoadingAds.isOpen)
        {
            UIController.instance.popupLoadingAds.OnClose();
        }

        UIController.instance.StartFadeScreen(delegate
        {
            UIController.instance.listLevel.OnOpen();
            OnClose();

            GameController.instance.TrashAll();

            if (GameController.instance.currentLevelID >= 3)
            {
                Bridge.instance.ShowInterstitial(InterstitialPosition.BackButtonClick);
            }
        });

        SoundManager.instance.PlayBackgroundSound(SoundManager.instance.listMusicSelectLevel);
    }
    public void OnClickRemoveADS()
    {
        if (GameController.instance.currentState == GameController.State.Menu)
        {
            return;
        }
    }
    public void ShowButtonHint()
    {
        btnHint.gameObject.SetActive(true);

    }
    public void HideButtonHint()
    {
        btnHint.gameObject.SetActive(false);
    }
}
