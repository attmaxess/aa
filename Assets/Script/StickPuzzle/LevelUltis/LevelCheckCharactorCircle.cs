using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class LevelCheckCharactorCircle : BaseLevelProperties
{
    public List<LevelData> _data = new List<LevelData>();
    [ContextMenu("LoadData")]
    public void LoadData()
    {
        _data = new List<LevelData>();
        for (int i = 0; i < GameController.instance.gameSO.levelsData.Count; i++)
            _data.Add(GameController.instance.gameSO.levelsData[i]);
    }
    [ContextMenu("DebugCircle")]
    public void DebugCircle()
    {
        StartCoroutine(C_DebugCircle());
    }

    IEnumerator C_DebugCircle()
    {
        for (int i = 0; i < _data.Count; i++)
            yield return C_DebugCircle(i);
        yield break;
    }
    IEnumerator C_DebugCircle(int GameID)
    {
        GameController.instance.LoadLevelByResourceID(_data[GameID].ResourceID);
        yield return new WaitUntil(() => GameController.instance.currentLevel != null);
        yield return new WaitUntil(() => GameController.instance.DoneLoadLevel == true);

        Level currentLevel = GameController.instance.currentLevel;
        if (currentLevel.charactor.circle == null) yield break;
        currentLevel.charactor.circle.gameObject.SetActive(true);

        List<Image> circles = currentLevel.charactor.GetComponentsInChildren<Image>().
            ToList().FindAll((x) => x.transform.name.ToLower().Contains("circle"));

        if (circles.Count > 1)
            Debug.Log(currentLevel.transform.name + " has 2 circle");
    }
}
