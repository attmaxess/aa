using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelAICheckMapBreach : BaseLevelProperties
{
    List<LevelData> _data = new List<LevelData>();

    [ContextMenu("LoadData")]
    public void LoadData()
    {
        _data = new List<LevelData>();
        for (int i = 0; i < GameController.instance.gameSO.levelsData.Count; i++)
            _data.Add(GameController.instance.gameSO.levelsData[i]);
    }
    [ContextMenu("CheckBreach")]
    public void CheckBreach()
    {
        StartCoroutine(C_CheckBreach());
    }

    IEnumerator C_CheckBreach()
    {
        for (int i = 0; i < _data.Count; i++)
            yield return C_CheckBreach(i);

        yield break;
    }
    IEnumerator C_CheckBreach(int GameID)
    {
        GameController.instance.LoadLevelByResourceID(_data[GameID].ResourceID);
        yield return new WaitUntil(() => GameController.instance.currentLevel != null);
        yield return new WaitUntil(() => GameController.instance.DoneLoadLevel == true);

        Level currentLevel = GameController.instance.currentLevel;

        PillarManager pillar = currentLevel.levelController as PillarManager;
        WallManager wall = currentLevel.levelController as WallManager;
        if (pillar == null && wall == null) yield break;

        yield return new WaitUntil(() => currentLevel.readyPlay == true);
        yield return new WaitForEndOfFrame();

        currentLevel.ReScanPath(true);
        yield return new WaitUntil(() => currentLevel.levelController.levelStatus == eLevelStatus.Idle);
        if (level.charactorAI.mammalReachables.Count > 0)
        {
            Debug.Log(currentLevel.transform.name + " Wrong map");
        }

        yield break;
    }
}
