using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Spine.Unity;
using UnityEngine.UI;
using System.Linq;
using EnhancedUI;

public class GameController : GameControllerBaseProperties
{
    public static GameController instance;
    public enum State
    {
        Menu,
        Play,
    }
    public State currentState;
    public Level currentLevel;

    [HideInInspector] public bool isZoom = true;
    [SerializeField] Transform levelParent = null;

    [Space(10), SerializeField] CanvasScaler canvasScaler = null;

    public bool playBeforeLastLevel;
    public int currentLevelID;
    /// <summary>
    /// store GameID
    /// </summary>

    const string LevelPath = "Levels/Level";

    [Space(10)]
    public ImageCapturing imageCapturing = null;

    public bool isWinLastLevel;

    [SerializeField] List<SkeletonGraphic> listHitBlueSkeleton = null;
    [SerializeField] List<SkeletonGraphic> listHitRedSkeleton = null;
    [SerializeField] SkeletonGraphic levelUpSkeleton = null;

    [Space(10)]
    [SerializeField] SkeletonGraphic mergeSkeleton = null;

    [SerializeField] LineRenderer wayLine = null;
    [SerializeField] GameObject arrowObj = null;

    [Space(10)]
    [SerializeField] SkeletonDataAsset swordDataAsset = null;
    [SerializeField] SkeletonDataAsset archeryDataAsset = null;
    [SerializeField] SkeletonDataAsset spearDataAsset = null;
    [SerializeField] SkeletonDataAsset maceDataAsset = null;
    [SerializeField] SkeletonDataAsset shieldDataAsset = null;
    [SerializeField] SkeletonDataAsset swordShieldDataAsset = null;

    [Space(10), Header("Test")]
    [SerializeField] bool unlockAllLevel = false;
    [SerializeField] int GameID = -1;
    [SerializeField] int ResourceID = -1;

    public GameControllerSO gameSO;

    public bool DoneLoadLevel = true;
    public bool DoneDestroyLevel = true;

    Coroutine coLoadLevel;
    Coroutine coUpdateWayline;

    [HideInInspector]
    public bool CanNextAfterWin = true;

    void Awake()
    {
        Input.multiTouchEnabled = false;
        MakeInstance();

        if (unlockAllLevel)
        {
            SectionSettings.BiggestPlayLevel = GetTotalRealLevel();
            //SectionSettings.TotalLevel;
            SectionSettings.CurrentLevel = GetTotalRealLevel();
            //SectionSettings.TotalLevel;
        }

        currentLevelID = SectionSettings.CurrentLevel;
    }
    public void SwapCharactorWeapon(WeaponType weaponType)
    {
        switch (weaponType)
        {
            case WeaponType.Sword:
                currentLevel.charactor.SwapWeapon(weaponType, swordDataAsset);
                break;
            case WeaponType.Archery:
                currentLevel.charactor.SwapWeapon(weaponType, archeryDataAsset);
                break;
            case WeaponType.Spear:
                currentLevel.charactor.SwapWeapon(weaponType, spearDataAsset);
                break;
            case WeaponType.Mace:
                currentLevel.charactor.SwapWeapon(weaponType, maceDataAsset);
                break;
            case WeaponType.Shield:
                currentLevel.charactor.SwapWeapon(weaponType, shieldDataAsset);
                break;
            case WeaponType.SwordShield:
                currentLevel.charactor.SwapWeapon(weaponType, swordShieldDataAsset);
                break;
        }
    }

    public void PlaceWayLine(List<Vector3> listPoint)
    {
        wayLine.positionCount = 0;
        wayLine.positionCount = listPoint.Count;
        wayLine.SetPositions(listPoint.ToArray());

        Vector3 v = listPoint[listPoint.Count - 1] - listPoint[listPoint.Count - 2];
        arrowObj.transform.eulerAngles = new Vector3(0, 0, Mathf.Atan2(v.y, v.x) * Mathf.Rad2Deg);
        arrowObj.transform.position = listPoint[listPoint.Count - 1];
        arrowObj.SetActive(true);

        coUpdateWayline = StartCoroutine(IEUpdateLine());
    }

    IEnumerator IEUpdateLine()
    {
        while (wayLine.positionCount > 0)
        {
            Vector3[] points = new Vector3[wayLine.positionCount];
            wayLine.GetPositions(points);

            List<Vector3> pointsList = points.ToList();

            for (int i = pointsList.Count - 1; i >= 0; i--)
            {
                if (Vector2.Distance(pointsList[i], currentLevel.charactor.transform.position) < 0.3f)
                {
                    pointsList.RemoveRange(0, i);
                    break;
                }
            }

            if (pointsList.Count == 1) pointsList.Clear();

            wayLine.positionCount = pointsList.Count;
            wayLine.SetPositions(pointsList.ToArray());
            if (pointsList.Count == 0) arrowObj.SetActive(false);

            yield return null;
        }
    }
    public void HideArrowObj()
    {
        arrowObj.SetActive(false);
        wayLine.positionCount = 0;
    }
    public void PlaceHitAnim(bool isPlayer, List<GameObject> listObj)
    {
        for (int i = 0; i < listObj.Count; i++)
        {
            if (isPlayer)
            {
                Vector3 pos = listObj[i].transform.position;
                pos.y += 0.45f;
                pos.z = 0f;
                listHitBlueSkeleton[i].transform.position = pos;
                if (listHitBlueSkeleton[i].gameObject.activeSelf == true)
                    listHitBlueSkeleton[i].AnimationState.SetAnimation(0, "animation", false);
            }
            else
            {
                Vector3 pos = listObj[i].transform.position;
                pos.y += 0.5f;
                pos.z = 0f;
                listHitRedSkeleton[i].transform.position = pos;
                if (listHitRedSkeleton[i].gameObject.activeSelf == true)
                    listHitRedSkeleton[i].AnimationState.SetAnimation(0, "animation", false);
            }
        }
    }

    public void PlaceLevelUpAnim(GameObject obj)
    {
        Vector3 pos = obj.transform.position;
        pos.y += 0.3f;
        pos.z = 0f;
        levelUpSkeleton.transform.position = pos;
        if (levelUpSkeleton.gameObject.activeSelf == true)
            levelUpSkeleton.AnimationState.SetAnimation(0, "animation", false);
    }

    public void PlaceMergeAnim(GameObject obj)
    {
        Vector3 pos = obj.transform.position;
        pos.y -= 0.1f;
        pos.x -= 0.1f;
        pos.z = 0f;
        mergeSkeleton.transform.position = pos;
        if (mergeSkeleton.gameObject.activeSelf == true)
            mergeSkeleton.AnimationState.SetAnimation(0, "animation", false);
    }

    public void UpdateText(float startText, float endText, Text text, float time, Callback callback)
    {
        StartCoroutine(IEUpdateHealth(startText, endText, text, time, callback));
    }

    IEnumerator IEUpdateHealth(float startHealth, float endHealth, Text healthText, float time, Callback callback)
    {
        float distance = endHealth - startHealth;
        float t = 0;

        while (t < time)
        {
            healthText.text = TaskUtil.Round((startHealth + (t / time) * distance), 2).ToString();

            t += Time.deltaTime;
            yield return null;
        }

        if (callback != null) callback.Invoke();
    }

    Vector2 effectEraserOldPos;

    void MakeInstance()
    {
        instance = this;
    }

    int totalPointInside = 0;

    public void CaptureShare()
    {
        Rect fullScreenRect = new Rect(0, 0, Screen.width, Screen.height);
        imageCapturing.TakeLevelScreenshot(fullScreenRect);
    }
    void CaptureCurrentLevel()
    {
        StartCoroutine(C_CaptureCurrentLevel());
    }
    IEnumerator C_CaptureCurrentLevel()
    {
        yield return new WaitUntil(() =>
        currentLevel != null &&
        currentLevel.DoneStartLevel == true &&
        UIController.instance.winPopup.gameObject.activeSelf == false &&
        UIController.instance.losePopup.gameObject.activeSelf == false);

        CaptureShare();
    }
    public bool IsReadyToLoadLevel()
    {
        return DoneLoadLevel == true &&
            coLoadLevel == null &&
            (currentLevel == null ||
            (currentLevel != null && currentLevel.DoneStartLevel == true));
    }
    public void LoadLevelByGameID(int gameID, bool forceShowHint = false)
    {
        TrashAll();
        if (gameID == -1)
        {
            ListLevel.instance.OnOpen();
            return;
        }
        int resourceID = gameSO.GetResourceID(gameID);
        coLoadLevel = StartCoroutine(C_LoadLevel(resourceID, forceShowHint));
    }
    public void LoadLevelByResourceID(int resourceID, bool forceShowHint = false)
    {
        TrashAll();
        int gameID = gameSO.GetGameID(resourceID);
        if (gameID == -1)
        {
            ListLevel.instance.OnOpen();
            return;
        }
        coLoadLevel = StartCoroutine(C_LoadLevel(resourceID, forceShowHint));
    }
    IEnumerator C_LoadLevel(int resourceID, bool forceShowHint = false)
    {
        DoneLoadLevel = false;
        float startLoad = Time.time;

        Transform lastLevelAstar = null;
        if (currentLevel != null)
        {
            lastLevelAstar = currentLevel.insideAstar;
            currentLevel.UnReady();
            GameTrash.instance.AddTrash(currentLevel.transform);
        }

        currentLevelID = gameSO.GetGameID(resourceID);

        UIController.instance.gamePlay.levelName = "Level " + currentLevelID.ToString();
        totalPointInside = 0;

        wayLine.positionCount = 0;
        arrowObj.SetActive(false);

        InstantiateLevelFromSource(
        resourceID: resourceID,
        parent: levelParent,
        forceInactive: false,
        showHint: forceShowHint,
        out currentLevelID,
        out currentLevel);

        Transform currentLevelAstar = currentLevel.insideAstar;
        if (currentLevelAstar != null)
            currentLevelAstar.gameObject.SetActive(false);

        currentLevel.gameObject.SetActive(true);
        currentLevel.GetReady();

        if (forceShowHint)
            currentLevel.ShowHint();

        PlayLevelSound(currentLevel);

        gameCapture.LoadLevelByGameID(currentLevelID);

        if (currentLevelID == GetTotalRealLevel() - 1)
            playBeforeLastLevel = true;
        else
            playBeforeLastLevel = false;

        if (currentLevelID == 1 &&
            SectionSettings.BiggestPlayLevel <= currentLevelID)
        {
            UIController.instance.hintAdsIcon.gameObject.SetActive(false);
        }

        Bridge.instance.SendEventLoadLevel(currentLevelID);
        Bridge.instance.ButtonHintAvailable(currentLevelID);
        Bridge.instance.ButtonSkipAvailable(currentLevelID);

        //levelAsync.LoadLevelByGameID(currentLevelID + 1);

        if (lastLevelAstar != null)
            yield return new WaitUntil(() => lastLevelAstar == null);
        if (currentLevelAstar != null)
            currentLevelAstar.gameObject.SetActive(true);

        CaptureCurrentLevel();

        DoneLoadLevel = true;
        coLoadLevel = null;

        yield break;
    }
    public void Trash()
    {
        if (currentLevel != null)
            GameTrash.instance.AddTrash(currentLevel.transform);
        currentLevel = null;
        currentLevelID = -1;
    }
    public void TrashAll()
    {
        Trash();
        levelAsync.Trash();
        gameCapture.Trash();
    }
    void PlayLevelSound(Level level)
    {
        switch (level.typeLevel)
        {
            case TypeLevel.Normal:
                SoundManager.instance.PlayBackgroundSound(
                    SoundManager.instance.listMusicGamePlayNormal);
                break;
            case TypeLevel.Boss:
                SoundManager.instance.PlayBackgroundSound(
                    SoundManager.instance.listMusicGamePlayBoss);
                break;
        }

        SoundManager.instance.StopMoving();
    }
    public void InstantiateLevelFromSource(
        int resourceID,
        Transform parent,
        bool forceInactive,
        bool showHint,
        out int GameID,
        out Level level)
    {
        GameID = gameSO.GetGameID(resourceID);

        string path = LevelPath + resourceID;
        level = Resources.Load<GameObject>(path)?.GetComponent<Level>();
        if (level == null)
            return;
        if (forceInactive)
            level.gameObject.SetActive(false);
        if (!showHint)
            level.levelHint.HideHint();
        //else
        //level.levelHint.ShowHint();
        level = Instantiate(level.gameObject, parent).GetComponent<Level>();
        level.transform.localPosition = Vector3.zero;
        level.transform.localScale = Vector3.one;
    }
    [ContextMenu("DoWin")]
    public void DoWin()
    {
        if (currentLevel.isWin) return;

        HideArrowObj();
        //bool upperIQ = currentLevelID == SectionSettings.BiggestPlayLevel;

        TaskUtil.Delay(this, delegate
        {
            UIController.instance.gamePlay.OnClose();
            UIController.instance.winPopup.OnOpen(true);
            Bridge.instance.ShowInterstitial(InterstitialPosition.ShowWinPopup);
        }, .75f);

        Bridge.instance.SendEventCompleteLevel(currentLevelID);

        if (UIController.instance.popupLoadingAds.isOpen)
        {
            UIController.instance.popupLoadingAds.OnClose();
        }

        EventDispacher.Dispatch(EventName.DoWin);

        CanNextAfterWin = NextLevelData();
    }
    [ContextMenu("DoLose")]
    public void DoLose()
    {
        HideArrowObj();

        TaskUtil.Delay(this, delegate
        {
            UIController.instance.gamePlay.OnClose();
            UIController.instance.losePopup.OnOpen();
            Bridge.instance.ShowInterstitial(InterstitialPosition.ShowLosePopup);
        }, .75f);

        Bridge.instance.SendEventFailLevel(currentLevelID);
        if (UIController.instance.popupLoadingAds.isOpen)
        {
            UIController.instance.popupLoadingAds.OnClose();
        }
    }
    bool NextLevelData()
    {
        if (currentLevelID == SectionSettings.BiggestPlayLevel)
        {
            SectionSettings.BiggestPlayLevel = Mathf.Clamp(
                SectionSettings.BiggestPlayLevel + 1, 0, GetTotalRealLevel());
        }
        int next = ListLevel.instance.NextLevel(currentLevelID);
        if (next != -1)
        {
            currentLevelID = next;
            SectionSettings.CurrentLevel = currentLevelID;
            return true;
        }
        else
        {
            return false;
        }
    }
    [ContextMenu("SetCurrentLevelPlayerInt1")]
    public void SetCurrentLevelPlayerInt1()
    {
        SectionSettings.CurrentLevel = 1;
        SectionSettings.BiggestPlayLevel = 1;
    }
    public bool CanSkipNewLevel()
    {
        return currentLevelID < GetTotalRealLevel();
    }
    public void SkipNewLevel()
    {
        if (!IsReadyToLoadLevel())
            return;

        if (NextLevelData())
            LoadLevelByGameID(currentLevelID);
    }
    public void DestroyLevel()
    {
        if (currentLevel == null)
            return;

        currentLevel.UnReady();
    }
    public void OnSkipLevelButtonClick()
    {
        if (currentState == State.Menu ||
            currentLevel.isWin ||
            currentLevel.isLose) return;

        Bridge.instance.ButtonSkipOffIn(currentLevelID);
        UIController.instance.popupLoadingAds.OnOpen(LoadingAdsPopup.LoadingVideoType.ButtonSkip);
        Bridge.instance.ShowRewardForSkipLevel(delegate
            {
                if (CanSkipNewLevel())
                {
                    SkipNewLevel();
                    UIController.instance.gamePlay.OnOpen();
                    UIController.instance.popupLoadingAds.OnClose();
                }
                else
                {
                    UIController.instance.listLevel.OnOpen();
                    TrashAll();
                }

                //SkipNewLevel();
                //UIController.instance.popupLoadingAds.OnClose();
            }, currentLevelID);
    }
    public void WatchHint()
    {
        currentLevel.ShowHint();
    }
    [ContextMenu("DebugGameID")]
    public void DebugGameID()
    {
        Debug.Log(gameSO.GetGameID(this.ResourceID));
    }
    [ContextMenu("DebugResourecID")]
    public void DebugResourecID()
    {
        Debug.Log(gameSO.GetResourceID(this.GameID));
    }
    public int GetTotalRealLevel()
    {
        return gameSO.levelsData.Count;
    }
}
