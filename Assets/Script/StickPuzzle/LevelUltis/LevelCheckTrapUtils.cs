using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class LevelCheckTrapUtils : MonoBehaviour
{
    public List<LevelData> _data = new List<LevelData>();
    const string LevelPath = "Levels/Level";

    GameObject currentLevelPrefab = null;

    public void CheckTrap(Level level)
    {
        List<Trap> traps = level.GetComponentsInChildren<Trap>().ToList();
        foreach (var trap in traps)
        {
            if (IsTrapNoType(trap))
                Debug.Log(level.transform.name + " : " + trap.transform.name);
        }
    }
    bool IsTrapNoType(Trap trap)
    {
        return trap.type == eTrapType.None;
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
    [ContextMenu("CheckTrapAllLevel")]
    public void CheckTrapAllLevel()
    {
        StartCoroutine(C_CheckTrapAllLevel());
    }
    IEnumerator C_CheckTrapAllLevel()
    {
        for (int i = 0; i < _data.Count; i++)
        {
            GameController.instance.LoadLevelByResourceID(_data[i].ResourceID);
            yield return new WaitUntil(() => GameController.instance.currentLevel != null);
            yield return new WaitUntil(() => GameController.instance.DoneLoadLevel == true);
            yield return new WaitForEndOfFrame();

            string fileName = "Level" + _data[i].ResourceID.ToString() + _data[i].NameType;
            CheckTrap(GameController.instance.currentLevel);

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
