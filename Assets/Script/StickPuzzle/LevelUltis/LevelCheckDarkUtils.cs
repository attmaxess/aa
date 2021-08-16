using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class LevelCheckDarkUtils : MonoBehaviour
{
    public List<LevelData> _data = new List<LevelData>();
    const string LevelPath = "Levels/Level";

    GameObject currentLevelPrefab = null;

    public void CheckDark(Level level)
    {
        Dark dark = level.GetComponentInChildren<Dark>();
        if (dark != null)
        {
            Debug.Log("Dark : " + level.transform.name);
        }
    }
    [ContextMenu("LoadData")]
    public void LoadData()
    {
        _data = new List<LevelData>();
        for (int i = 0; i < GameController.instance.gameSO.levelsData.Count; i++)
            _data.Add(GameController.instance.gameSO.levelsData[i]);
    }
    bool IsExisted(int LevelID)
    ///Phải gọi trước tất cả các hàm khác nếu có
    {
        string path = LevelPath + LevelID.ToString();
        currentLevelPrefab = Resources.Load<GameObject>(path);
        return currentLevelPrefab != null;
    }
    [ContextMenu("CheckDarkAllLevel")]
    public void CheckDarkAllLevel()
    {
        StartCoroutine(C_CheckDarkAllLevel());
    }
    IEnumerator C_CheckDarkAllLevel()
    {
        for (int i = 0; i < _data.Count; i++)
        {
            GameController.instance.LoadLevelByResourceID(_data[i].ResourceID);
            yield return new WaitUntil(() => GameController.instance.currentLevel != null);
            yield return new WaitUntil(() => GameController.instance.DoneLoadLevel == true);
            yield return new WaitForEndOfFrame();

            string fileName = "Level" + _data[i].ResourceID.ToString() + _data[i].NameType;
            CheckDark(GameController.instance.currentLevel);

            yield return new WaitForEndOfFrame();
        }
        yield break;
    }
    [ContextMenu("DebugPath")]
    public void DebugPath()
    {
        Debug.Log(Application.persistentDataPath);
    }
    [ContextMenu("DebugPath")]
    public void DebugLevel()
    {
        Debug.Log(Application.persistentDataPath);
    }
}
