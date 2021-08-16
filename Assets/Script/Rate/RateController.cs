using UnityEngine;

public class RateController : MonoBehaviour
{
    public static RateController instance;

    [SerializeField] RatePopup ratePopup = null;

    const string HasRateKey = "HasRateKey";
    const string RateLevelKey = "RateLevelKey";

    public bool HasRate
    {
        get
        {
            return PlayerPrefsX.GetBool(HasRateKey, false);
        }
        set
        {
            PlayerPrefsX.SetBool(HasRateKey, value);
        }
    }

    int RateLevel
    {
        get
        {
            return PlayerPrefs.GetInt(RateLevelKey, 0);
        }
        set
        {
            PlayerPrefs.SetInt(RateLevelKey, value);
        }
    }

    private void Awake()
    {
        instance = this;
    }

    public void CheckRateStatus()
    {
        int currentLevel = GameController.instance.currentLevelID;

        bool condition1 = (currentLevel == 6 || currentLevel == 16 || currentLevel == 51) && RateLevel < currentLevel;
        if (condition1 && !HasRate)
        {
            ShowRatePopup();
            RateLevel = currentLevel;
        }
    }


    [ContextMenu("Show Rate Popup")]
    void ShowRatePopup()
    {
        TaskUtil.Delay(this, delegate
        {
            ratePopup.OnOpen();
        }, 0.1f);
    }
}
