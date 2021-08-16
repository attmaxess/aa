using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Spine.Unity;
using EnhancedUI.EnhancedScroller;
using EnhancedUI;
using System.Collections;

public class ListLevel : MonoBehaviour, IEnhancedScrollerDelegate
{
    [Space(10)]
    [SerializeField] Button removeAdsButton = null;

    [Space(10)]
    [SerializeField] GameObject container = null;

    [Space(10)]
    SmallList<LevelData> _data = new SmallList<LevelData>();
    [SerializeField] EnhancedScroller scroller = null;
    [SerializeField] EnhancedScrollerCellView cellViewPrefab = null;

    int numberOfCellsPerRow = 3;

    public bool showing;

    public static ListLevel instance;

    private void Awake()
    {
        instance = this;
    }
    private void Start()
    {
        scroller.Delegate = this;
        StartCoroutine(C_Start());        
    }
    IEnumerator C_Start()
    {
        yield return new WaitForEndOfFrame();
        LoadData();
        RemoveAdsButtonState(SectionSettings.RemoveAds);
    }
    void LoadData()
    {
        _data = new SmallList<LevelData>();
        for (int i = 0; i < GameController.instance.gameSO.levelsData.Count;i++)        
            _data.Add(GameController.instance.gameSO.levelsData[i]);

        scroller.ReloadData();
    }    
    public void OnBeginDrag()
    {
        SoundManager.instance.PlayNextPageLevel();
    }

    public void OnOpen(
        bool scrollToFirst = false,
        bool firstOpen = false)
    {
        UIController.instance.gamePlay.OnClose();
        container.SetActive(true);
        container.transform.localPosition = Vector3.zero;
        GameController.instance.currentState = GameController.State.Menu;
        UIController.instance.gamePlay.OnClose();
        scroller.ReloadData();
        if (firstOpen)
        {
            if ((GameController.instance.GetTotalRealLevel() - 1) / numberOfCellsPerRow -
                //SectionSettings.TotalLevel - 1) / numberOfCellsPerRow -
                (SectionSettings.BiggestPlayLevel - 1) / numberOfCellsPerRow <= 2)
            {
                scroller.JumpToDataIndex((
                    GameController.instance.GetTotalRealLevel() - 1) / numberOfCellsPerRow,
                    //SectionSettings.TotalLevel - 1) / numberOfCellsPerRow,
                    1f, 1f);
            }
            else
            {
                scroller.JumpToDataIndex((
                    SectionSettings.BiggestPlayLevel - 1) / numberOfCellsPerRow,
                    //SectionSettings.BiggestPlayLevel - 1) / numberOfCellsPerRow,
                    0.5f, 0.5f);
            }
        }
        else
        {
            if (scrollToFirst)
            {
                scroller.JumpToDataIndex(0, 0.5f, 0.5f);
            }
            else
            {
                if ((GameController.instance.GetTotalRealLevel() - 1) / numberOfCellsPerRow -
                    (GameController.instance.currentLevelID - 1) / numberOfCellsPerRow <= 2)
                    //SectionSettings.TotalLevel - 1) / numberOfCellsPerRow - (GameController.instance.currentLevelID - 1) / numberOfCellsPerRow <= 2)
                {
                    scroller.JumpToDataIndex((
                        GameController.instance.GetTotalRealLevel() - 1) / numberOfCellsPerRow,
                        1f, 1f);
                    //SectionSettings.TotalLevel - 1) / numberOfCellsPerRow, 1f, 1f);
                }
                else
                {
                    scroller.JumpToDataIndex((
                        GameController.instance.currentLevelID - 1) / numberOfCellsPerRow,
                        0.5f, 0.5f);
                }
            }
        }

        showing = true;
    }

    public void OnClose()
    {
        showing = false;
        container.SetActive(false);
    }

    public void OnClickBackToMainMenu()
    {
        UIController.instance.StartFadeScreen(delegate
        {
            UIController.instance.BackToMain();            
            UIController.instance.gamePlay.OnClose();
            OnClose();
        });
    }

    public void RemoveAdsButtonState(bool removeAds)
    {
        removeAdsButton.gameObject.SetActive(!removeAds);
        if (removeAds)
        {
            //removeAdsButton.GetComponent<SkeletonGraphic>().AnimationState.SetAnimation(0, "off", true);
        }
    }
    #region EnhancedScroller Handlers
    public int GetNumberOfCells(EnhancedScroller scroller)
    {
        return Mathf.CeilToInt((float)_data.Count / (float)numberOfCellsPerRow);
    }
    public float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
    {
        return 275f;
    }
    public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
    {
        LevelCellView cellView = scroller.GetCellView(cellViewPrefab) as LevelCellView;

        cellView.name = "Cell " + (dataIndex * numberOfCellsPerRow).ToString() + " to " + ((dataIndex * numberOfCellsPerRow) + numberOfCellsPerRow - 1).ToString();

        cellView.SetData(ref _data, dataIndex * numberOfCellsPerRow);

        return cellView;
    }
    public int GameID(int sourceID)
    {
        int index = -1;
        LevelData levelData = null;
        for (int i = 0; i < _data.Count; i++)
        {
            if (_data[i].ResourceID == sourceID)
            {
                levelData = _data[i];
                index = i;
                i = _data.Count;
            }
        }

        return index != -1 ? _data[index].GameID : -1;
    }
    public int ResourceID(int gameID)
    {
        int index = -1;
        LevelData levelData = null;
        for (int i = 0; i < _data.Count; i++)
        {
            if (_data[i].GameID == gameID)
            {
                levelData = _data[i];
                index = i;
                i = _data.Count;
            }
        }

        return index != -1 ? _data[index].ResourceID : -1;
    }
    public int NextLevel(int gameID)
    {
        int indexNext = -1;
        LevelData levelData = null;
        for (int i = 0; i < _data.Count; i++)
        {
            if (_data[i].GameID == gameID + 1)
            {
                levelData = _data[i];
                indexNext = i;
                i = _data.Count;
            }
        }

        return indexNext != -1 ? _data[indexNext].GameID : -1;
    }
    #endregion
}
