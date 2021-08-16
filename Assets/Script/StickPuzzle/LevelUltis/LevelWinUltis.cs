using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class LevelWinUltis : MonoBehaviour
{
    public List<Level> levels;
    public TextAsset levelwinDataText;

    [Serializable]
    public struct InitLevelWin
    {
        public string level;
        public List<string> listEnemies;
        public List<string> listHoles;
        public List<string> listWinHoles;
        public string ToJson()
        {
            return JsonUtility.ToJson(this);
        }
    }
    [Serializable]
    public struct LevelWinData
    {
        public List<InitLevelWin> initLevelWins;
        public string ToJson()
        {
            return JsonUtility.ToJson(this);
        }
    }
    [ContextMenu("DebugInitLevelWin")]
    public void DebugInitLevelWin()
    {
        levels = FindObjectsOfType<Level>().ToList();
        LevelWinData gameData = new LevelWinData();
        gameData.initLevelWins = new List<InitLevelWin>();

        foreach (Level level in levels)
        {
            InitLevelWin initLevelwin = new InitLevelWin();
            initLevelwin.level = level.gameObject.name;

            initLevelwin.listEnemies = new List<string>();
            foreach (Enemy enemy in level.listEnemies)
                initLevelwin.listEnemies.Add(enemy.gameObject.name);

            initLevelwin.listHoles = new List<string>();
            foreach (Hole hole in level.listHoles)
                initLevelwin.listHoles.Add(hole.gameObject.name);

            initLevelwin.listWinHoles = new List<string>();
            foreach (Hole winHole in level.listWinHole)
                initLevelwin.listWinHoles.Add(winHole.gameObject.name);

            gameData.initLevelWins.Add(initLevelwin);
        }
        File.WriteAllText(Application.persistentDataPath + "/levelwinTextData.txt", gameData.ToJson());
        Debug.Log(Application.persistentDataPath + "/levelwinTextData.txt");
    }
    [ContextMenu("SyncInitLevelWins")]
    public void SyncInitLevelWins()
    {
        levels = FindObjectsOfType<Level>().ToList();
        LevelWinData winData = JsonUtility.FromJson<LevelWinData>(levelwinDataText.text);
        foreach (InitLevelWin win in winData.initLevelWins)
        {
            Level level = this.transform.Find(win.level)?.GetComponent<Level>();
            if (level == null)
                Debug.Log(win.level + " ko tim thay ");
            else
            {
                level.listEnemies = new List<Enemy>();
                foreach (string enemyStr in win.listEnemies)
                {
                    Transform enemyGO = level.transform.Find(enemyStr);
                    if (enemyGO == null) Debug.Log(enemyStr + " ko tim thay trong " + win.level);
                    if (enemyGO != null) level.listEnemies.Add(enemyGO.GetComponent<Enemy>());
                }

                level.listHoles = new List<Hole>();
                foreach (string holeStr in win.listHoles)
                {
                    Transform holeGO = level.transform.Find(holeStr);
                    if (holeGO == null) Debug.Log(holeStr + " ko tim thay trong " + win.level);
                    if (holeGO != null) level.listHoles.Add(holeGO.GetComponent<Hole>());
                }

                level.listWinHole = new List<Hole>();
                foreach (string winholeStr in win.listWinHoles)
                {
                    Transform winGO = level.transform.Find(winholeStr);
                    if (winGO == null) Debug.Log(winholeStr + " ko tim thay trong " + win.level);
                    if (winGO != null) level.listWinHole.Add(winGO.GetComponent<Hole>());
                }
            }
        }
    }
    [ContextMenu("SelfInitLevelWins")]
    public void SelfInitLevelWins()
    {
        LevelWinData winData = JsonUtility.FromJson<LevelWinData>(levelwinDataText.text);
        InitLevelWin levelWin = winData.initLevelWins.Find((x) => x.level == this.transform.name);
        Level level = GetComponent<Level>();
        level.listEnemies = new List<Enemy>(10);
        level.listEnemies = new List<Enemy>();
        foreach (string enemyStr in levelWin.listEnemies)
        {
            Transform enemyGO = level.transform.Find(enemyStr);
            if (enemyGO == null) Debug.Log(enemyStr + " ko tim thay trong " + levelWin.level);
            if (enemyGO != null) level.listEnemies.Add(enemyGO.GetComponent<Enemy>());
        }

        level.listHoles = new List<Hole>(10);
        level.listHoles = new List<Hole>();
        foreach (string holeStr in levelWin.listHoles)
        {
            Transform holeGO = level.transform.Find(holeStr);
            if (holeGO == null) Debug.Log(holeStr + " ko tim thay trong " + levelWin.level);
            if (holeGO != null) level.listHoles.Add(holeGO.GetComponent<Hole>());
        }

        level.listWinHole = new List<Hole>(10);
        level.listWinHole = new List<Hole>();
        foreach (string winholeStr in levelWin.listWinHoles)
        {
            Transform winGO = level.transform.Find(winholeStr);
            if (winGO == null) Debug.Log(winholeStr + " ko tim thay trong " + levelWin.level);
            if (winGO != null) level.listWinHole.Add(winGO.GetComponent<Hole>());
        }

    }
}
