using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class LevelRowCellView : MonoBehaviour
{
    public int gameID;
    public int resourceID;

    [SerializeField] GameObject container = null;
    [SerializeField] Text itemLevelIdText = null;
    [Space(10)]
    [SerializeField] Image bgImage = null;
    [SerializeField] Sprite spritePass = null;
    [SerializeField] Sprite spriteNotPass = null;
    [SerializeField] Sprite SpriteProcessing = null;

    [SerializeField] GameObject lockObj = null;
    [SerializeField] GameObject doneObj = null;
    [SerializeField] GameObject playingObj = null;
    [SerializeField] Button button = null;

    public void SetData(LevelData data)
    {
        container.SetActive(data != null);

        if (data != null)
        {
            resourceID = data.ResourceID;
            gameID = data.GameID;

            itemLevelIdText.text = gameID.ToString();

            if (gameID == SectionSettings.BiggestPlayLevel &&
                gameID == GameController.instance.GetTotalRealLevel() &&
                //SectionSettings.TotalLevel &&
                SectionSettings.IsWinAll)
            {
                bgImage.sprite = spritePass;
                doneObj.SetActive(true);
                lockObj.SetActive(false);
                playingObj.SetActive(false);
                button.enabled = true;
            }
            else if (gameID < SectionSettings.BiggestPlayLevel)
            {
                bgImage.sprite = spritePass;
                doneObj.SetActive(true);
                lockObj.SetActive(false);
                playingObj.SetActive(false);
                button.enabled = true;
            }
            else if (gameID > SectionSettings.BiggestPlayLevel)
            {
                bgImage.sprite = spriteNotPass;
                doneObj.SetActive(false);
                lockObj.SetActive(true);
                playingObj.SetActive(false);
                button.enabled = false;
            }
            else if (gameID == SectionSettings.BiggestPlayLevel)
            {
                bgImage.sprite = SpriteProcessing;
                lockObj.SetActive(false);
                playingObj.SetActive(true);
                doneObj.SetActive(false);
                button.enabled = true;
            }
        }
    }

    public void OnLevelButtonClick()
    {
        if (!GameController.instance.IsReadyToLoadLevel())
            return;

        StartCoroutine(C_OnLevelButtonClick());
    }
    IEnumerator C_OnLevelButtonClick()
    { 
        Bridge.instance.ShowInterstitial(InterstitialPosition.LoadLevel);

        GameController.instance.currentState = GameController.State.Play;
        GameController.instance.LoadLevelByGameID(gameID);
        UIController.instance.listLevel.OnClose();
        UIController.instance.gamePlay.OnOpen();
        yield break;
    }

    public void SoundLevelClick()
    {
        SoundManager.instance.PlayAudioClip(SoundManager.instance.choiceLevelButton);
    }
}
