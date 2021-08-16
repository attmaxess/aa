using UnityEngine;
using System;

public class SectionSettings
{
    const string SoundKey = "SoundKey";
    const string MusicKey = "MusicKey";
    const string BiggestPlayLevelKey = "BiggestPlayLevelKey";
    const string RemoveAdsKey = "RemoveAdsKey";
    const string FirstOpenKey = "FirstOpenKey";
    const string WinAll = "WinAllKey";

    const string CurrentLevelkey = "CurrentLevelKey";

    const string IQKey = "IQKey";

    public const float fightingTime = 0.867f;

    public static bool RemoveAds
    {
        get
        {
            return PlayerPrefsX.GetBool(RemoveAdsKey, false);
        }
        set
        {
            PlayerPrefsX.SetBool(RemoveAdsKey, value);
        }
    }
    public static bool IsWinAll
    {
        get
        {
            return PlayerPrefsX.GetBool(WinAll, false);
        }
        set
        {
            PlayerPrefsX.SetBool(WinAll, value);
        }
    }

    public static float IQ
    {
        get
        {
            return PlayerPrefs.GetFloat(IQKey, 85f);
        }
        set
        {
            PlayerPrefs.SetFloat(IQKey, value);
        }
    }

    public static bool FirstOpen
    {
        get
        {
            return PlayerPrefsX.GetBool(FirstOpenKey, true);
        }
        set
        {
            PlayerPrefsX.SetBool(FirstOpenKey, value);
        }
    }

    public static int TotalLevel
    {
        get
        {
            return 175;
        }
    }

    public static bool SoundEnable
    {
        get
        {
            return PlayerPrefsX.GetBool(SoundKey, true);
        }
        set
        {
            PlayerPrefsX.SetBool(SoundKey, value);
        }
    }

    public static bool MusicEnable
    {
        get
        {
            return PlayerPrefsX.GetBool(MusicKey, true);
        }
        set
        {
            PlayerPrefsX.SetBool(MusicKey, value);
        }
    }

    public static int BiggestPlayLevel
    {
        get
        {
            return PlayerPrefs.GetInt(BiggestPlayLevelKey, 1);
        }
        set
        {
            PlayerPrefs.SetInt(BiggestPlayLevelKey, value);
        }
    }    

    public static int CurrentLevel
    {
        get
        {
            return PlayerPrefs.GetInt(CurrentLevelkey, BiggestPlayLevel);
        }
        set
        {
            PlayerPrefs.SetInt(CurrentLevelkey, value);
        }
    }
       
    public static int GetLevelReferenceSpecialChapter(int chapterIndex)
    {
        switch (chapterIndex)
        {
            case 1:
                return 10;
            case 2:
                return 26;
            case 3:
                return 46;
            case 4:
                return 70;
            case 5:
                return 100;
            case 6:
                return 136;
        }
        Debug.LogError(chapterIndex);
        Debug.LogError("Lỗi rồi");
        return -1;
    }
}
