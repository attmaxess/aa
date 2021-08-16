using EnhancedUI;
using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GameControllerReference", menuName = "Scriptable/GameControllerRefernce")]
public class GameControllerSO : ScriptableObject
{
    const string LevelPath = "Levels/Level";
    public string GetLevelPath()
    {
        return LevelPath;
    }
    public List<LevelData> levelsData
    {
        get
        {
            if (_levelsData.Count == 0)
                LoadData();
            return _levelsData;
        }
    }
    public List<LevelData> _levelsData = new List<LevelData>();
    [ContextMenu("LoadData")]
    public void LoadData()
    {
        if (_levelsData.Count > 0) return;
        // cached

        _levelsData = new List<LevelData>();
        // set up some simple data
        int gameID = 0;
        for (var i = 0; i < SectionSettings.TotalLevel; i++)
        {
            int levelID = i + 1;
            if (IsExisted(levelID, out string typeLevel))
            {
                gameID++;
                _levelsData.Add(new LevelData()
                {
                    GameID = gameID,
                    ResourceID = levelID,
                    NameType = typeLevel
                }); ;
            }
        }
    }
    bool IsExisted(int LevelID, out string TypeLevel)
    {
        string path = GetLevelPath() + LevelID.ToString();
        GameObject level = Resources.Load<GameObject>(path);
        if (level != null)
        {
            TypeLevel = level.GetComponent<Level>().GetLevelNameType();
            return true;
        }
        else
        {
            TypeLevel = string.Empty;
            return false;
        }
    }
    public int GetGameID(int sourceID)
    {
        int index = -1;
        LevelData levelData = null;
        for (int i = 0; i < levelsData.Count; i++)
        {
            if (levelsData[i].ResourceID == sourceID)
            {
                levelData = levelsData[i];
                index = i;
                i = levelsData.Count;
            }
        }

        return index != -1 ? levelsData[index].GameID : -1;
    }
    public int GetResourceID(int gameID)
    {
        int index = -1;
        LevelData levelData = null;
        for (int i = 0; i < levelsData.Count; i++)
        {
            if (levelsData[i].GameID == gameID)
            {
                levelData = levelsData[i];
                index = i;
                i = levelsData.Count;
            }
        }

        return index != -1 ? levelsData[index].ResourceID : -1;
    }
}
