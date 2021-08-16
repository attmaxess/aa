using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class Bridge : MonoBehaviour
{
    public bool useDebug = false;

    private static Bridge mInstance = null;
    public static Bridge instance
    {
        get
        {
            if (mInstance == null)
                mInstance = GameObject.FindObjectOfType<Bridge>();
            return mInstance;
        }
    }

    public Queue<Action> ExecuteOnMainThread = new Queue<Action>();

    // Use this for initialization
    void Awake()
    {
        Bridge[] mytypes = FindObjectsOfType(typeof(Bridge)) as Bridge[];
        if (mytypes.Length > 1)
        {
            Destroy(gameObject);
            return;
        }
        mInstance = this;
        DontDestroyOnLoad(this);
    }

    //public Queue<Action> QueueFirebaseEvent = new Queue<Action>();
    void Update()
    {
        while (ExecuteOnMainThread.Count > 0)
        {
            ExecuteOnMainThread.Dequeue().Invoke();
        }

        //#if BUILD
        //        while (QueueFirebaseEvent.Count > 0)
        //        {
        //            if (ServiceControl.Instance.isInitFirebaseDone == false) return;
        //            QueueFirebaseEvent.Dequeue().Invoke();
        //        }
        //#endif
    }

    public void PurchaseItem(UnityAction e, string itemID)
    {
#if !BUILD
        e.Invoke();
        return;
#endif
#if BUILD
        UnityEvent unityEvent = new UnityEvent();
        unityEvent.AddListener(() =>
        {

            ExecuteOnMainThread.Enqueue(delegate ()
            {
                ShowAds.Instance.setremoveads();
                e.Invoke();
            });
        });
#if UNITY_ANDROID
        PurchaseInappHandler.mInstance.BuyConsumable(unityEvent, itemID);
#elif UNITY_IOS
        PurchaseInappHandler.mInstance.BuyNonConsumable(unityEvent, itemID);
#endif
#endif

    }

    public void OpenRate()
    {
        Application.OpenURL("https://play.google.com/store/apps/details?id=com.weegoon.drawpuzzle2onelineonepart");
    }

    public void OpenInstagram()
    {
        Application.OpenURL("https://www.instagram.com/weegoonstudio/");
    }

    public void Facebook()
    {
        Application.OpenURL("https://www.facebook.com/Weegoon-337477433663758");
    }

    public void Youtube()
    {
        Application.OpenURL("https://www.youtube.com/channel/UCLwuF4EwRHbGQlLLWdomPeg/videos");
    }

    public void Twitter()
    {
        Application.OpenURL("https://twitter.com/WEEGOON1");

    }

    public void MoreGame()
    {
        Application.OpenURL("https://play.google.com/store/apps/dev?id=8316470523921261984");
    }

    //    public void LogLevelLoaded(int lv)
    //    {

    //#if !BUILD
    //        return;
    //#else

    //        string level = "0";
    //        if (lv < 10)
    //        {
    //            level = "0" + lv;
    //        }
    //        else
    //        {
    //            level = "" + lv;
    //        }
    //        Firebase.Analytics.FirebaseAnalytics.LogEvent("LevelLoaded_" + level);
    //        Firebase.Analytics.FirebaseAnalytics.SetUserProperty("LevelLoaded", "lvl_" + level);

    //		//QueueFirebaseEvent.Enqueue(() =>
    //        //{
    //        //    Firebase.Analytics.FirebaseAnalytics.LogEvent("LevelLoaded" + level);
    //        //});   
    //#endif
    //    }

    public void RestorePurchase()
    {

    }

    public void RemoveAds(UnityAction e)
    {
        //e.Invoke();
        PurchaseItem(e, ITEM_ID.NO_ADS);
    }

    public void ShowReward(UnityAction e)
    {
#if !BUILD
        e.Invoke();
#else
        UnityEvent onSuccessEvent = new UnityEvent();
        onSuccessEvent.AddListener(() =>
        {
            ExecuteOnMainThread.Enqueue(() =>
            {
                e.Invoke();
            });
        });
        ShowAds.Instance.ShowAd(onSuccessEvent);
#endif
    }

    public void ShowRewardForHint(UnityAction e, int lv)
    {
        string level = GetLevelByInt(lv);
        if (useDebug)
            Debug.Log("Show Reward Hint level " + level);

#if !BUILD
        e.Invoke();
#else
        UnityEvent onSuccessEvent = new UnityEvent();
        onSuccessEvent.AddListener(() =>
        {
            ExecuteOnMainThread.Enqueue(() =>
            {
                e.Invoke();
            });
        });
        ShowAds.Instance.ShowAd(onSuccessEvent);
#endif
    }

    public void ShowRewardForSkipLevel(UnityAction e, int lv)
    {
        string level = GetLevelByInt(lv);
        if (useDebug)
            Debug.Log("Show Reward Skip level " + level);

#if !BUILD
        e.Invoke();
#else
        UnityEvent onSuccessEvent = new UnityEvent();
        onSuccessEvent.AddListener(() =>
        {
            ExecuteOnMainThread.Enqueue(() =>
            {
                e.Invoke();
            });
        });
        ShowAds.Instance.ShowAd(onSuccessEvent);
#endif
    }

    public void ShowInterstitial()
    {
        if (useDebug)
            Debug.Log("ShowInterstitial");
#if BUILD
        ShowAds.Instance.ShowInterstitial();
#endif
    }

    public void ShowInterstitial(InterstitialPosition showAtPosition)
    {
        switch (showAtPosition)
        {
            case InterstitialPosition.LoadLevel:

                break;
            case InterstitialPosition.NextButtonClick:

                break;
            case InterstitialPosition.ShowWinPopup:

                break;
            case InterstitialPosition.ShowLosePopup:

                break;
            case InterstitialPosition.BackButtonClick:

                break;
        }
        if (useDebug)
            Debug.Log(showAtPosition);
#if BUILD
        ShowAds.Instance.ShowInterstitial();
#endif
    }

    public void ShowBanner()
    {
        if (useDebug)
            Debug.Log("Show banner");
#if BUILD
        ShowAds.Instance.showBanner();
#endif
    }
    public void HideBanner()
    {
        if (useDebug)
            Debug.Log("Hide banner");
#if BUILD
        ShowAds.Instance.hideBanner();
#endif
    }

    public static List<Item> Items = new List<Item>()
    {
        new Item(){ itemID = ITEM_ID.NO_ADS, Price = 2.99f, PriceString = "$2.99"},
    };

    public void SendEventLoadLevel(int lv)
    {
        string level = "0";
        if (lv < 10)
        {
            level = "0" + lv;
        }
        else
        {
            level = "" + lv;
        }

        if (useDebug)
            Debug.Log("Load " + level);
    }

    public void SendEventCompleteLevel(int lv)
    {
        string level = "0";
        if (lv < 10)
        {
            level = "0" + lv;
        }
        else
        {
            level = "" + lv;
        }

        if (useDebug)
            Debug.Log("Complete " + level);
    }

    public void SendEventFailLevel(int lv)
    {
        string level = "0";
        if (lv < 10)
        {
            level = "0" + lv;
        }
        else
        {
            level = "" + lv;
        }

        if (useDebug)
            Debug.Log("Fail " + level);
    }

    public void SendEventWatchHintLevel(int lv)
    {
        string level = "0";
        if (lv < 10)
        {
            level = "0" + lv;
        }
        else
        {
            level = "" + lv;
        }

        if (useDebug)
            Debug.Log("Watch Hint " + level);
    }

    RectTransform g_container = null;
    public void ShowCrossAds(RectTransform container, float delayShow)
    {
        if (useDebug)
            Debug.Log("ShowCrossAds");

        g_container = container;
        CanvasScaler canvasScaler = container.transform.GetComponentInParent<CanvasScaler>();
        float scale = 1;
        if (canvasScaler != null && canvasScaler.uiScaleMode == CanvasScaler.ScaleMode.ScaleWithScreenSize)
        {
            if (canvasScaler.referenceResolution.x / canvasScaler.referenceResolution.y > 1)
            {
                scale = canvasScaler.referenceResolution.x / 1280f;
            }
            else
            {
                scale = canvasScaler.referenceResolution.x / 720f;
            }
        }
        container.transform.localScale = Vector3.one * scale;

        if (container.gameObject.GetComponent<Image>() != null)
            DestroyImmediate(container.gameObject.GetComponent<Image>());
#if BUILD
        WEcrossinstall.instance.ShowCrossAds(container, delayShow);
#else
        container.localScale = Vector3.zero;
        Image tmp = container.gameObject.AddComponent<Image>();
        tmp.type = Image.Type.Simple;
        tmp.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
        tmp.GetComponent<RectTransform>().sizeDelta = new Vector2(139, 161);
        container.transform.DOScale(1 * scale, 0.35f).SetEase(Ease.OutBounce).SetDelay(delayShow);
        tmp.DOFade(1, 0.35f).SetDelay(delayShow);
#endif
    }

    public void HideCrossAds()
    {
#if BUILD
        WEcrossinstall.intance.HideAds();
#endif
    }

    string GetLevelByInt(int lv)
    {
        string level = "0";
        if (lv < 10)
        {
            level = "0" + lv;
        }
        else
        {
            level = "" + lv;
        }

        return level;
    }

    public bool isAvailableVideoAds()
    {
        return false;
    }

    public void ButtonHintAvailable(int lv)
    {
        string level = GetLevelByInt(lv);

        if (useDebug)
            Debug.Log("ButtonHintAvailable " + level);
    }

    public void ButtonSkipAvailable(int lv)
    {
        string level = GetLevelByInt(lv);

        if (useDebug)
            Debug.Log("ButtonSkipAvailable " + level);
    }

    public void ButtonHintOffIn(int lv)
    {
        string level = GetLevelByInt(lv);

        if (useDebug)
            Debug.Log("ButtonHintOffIn " + level);
    }

    public void ButtonSkipOffIn(int lv)
    {
        string level = GetLevelByInt(lv);

        if (useDebug)
            Debug.Log("ButtonSkipOffIn " + level);
    }

    #region Log Story Level Event

    string GetStoryLevelString(string chapter, int lv)
    {
        string level = GetLevelByInt(lv);
        chapter = chapter.ToLower();
        chapter = chapter.Replace(" ", "_");
        string eventSendString = chapter + "_" + level;
        return eventSendString;
    }

    public void SendEventLoadStoryLevel(string chapter, int lv)
    {
        string sendEventString = "loadlevel_" + GetStoryLevelString(chapter, lv);// send cai nay len firebase
        Debug.Log(sendEventString);
    }

    public void SendEventCompleteStoryLevel(string chapter, int lv)
    {
        string sendEventString = "completelevel_" + GetStoryLevelString(chapter, lv);// send cai nay len firebase
        Debug.Log(sendEventString);
    }

    public void SendEventWatchHintStoryLevel(string chapter, int lv)
    {
        string sendEventString = "hint_" + GetStoryLevelString(chapter, lv);// send cai nay len firebase
        Debug.Log(sendEventString);
    }

    #endregion

    public void ReloadVideo()
    {

    }
}

public class Item
{
    public string itemID;
    public int index;
    public float Price;
    public string PriceString;
    public double usdprice;
}

public class ITEM_ID
{
    public const string NO_ADS = "draw2_removeads";
}

public enum InterstitialPosition
{
    LoadLevel = 0, //Click vao choi 1 level trong man SelectLevel
    NextButtonClick = 1, //Click vao button next
    ShowWinPopup = 2, //Luc popup win bat dau hien len
    ShowLosePopup = 3, //Luc popup lose bat dau hien len
    BackButtonClick = 4 //Click vao button back de show man SelectLevel
}
