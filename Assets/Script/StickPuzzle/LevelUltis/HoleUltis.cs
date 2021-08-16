using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class HoleUltis : MonoBehaviour
{
    public List<Level> levels;
    public TextAsset holeDataText;

    [Serializable]
    public struct InitHole
    {
        public string level;
        public string attackable;
        public string hole;
        public string ToJson()
        {
            return JsonUtility.ToJson(this);
        }
    }
    [Serializable]
    public struct HoleData
    {
        public List<InitHole> initHoles;
        public string ToJson()
        {
            return JsonUtility.ToJson(this);
        }
    }
    [ContextMenu("DebugInitHole")]
    public void DebugInitHole()
    {
        levels = FindObjectsOfType<Level>().ToList();
        HoleData gameData = new HoleData();
        foreach (Level level in levels)
        {
            foreach (Hole hole in level.listHoles)
            {
                InitHole initHole = new InitHole();
                initHole.level = level.gameObject.name;
                if (hole.enemyAttackable == null) continue;
                initHole.attackable = hole.enemyAttackable.ToString();
                initHole.hole = hole.gameObject.name;
                gameData.initHoles.Add(initHole);
            }
        }
        File.WriteAllText(Application.persistentDataPath + "/holeTextData.txt", gameData.ToJson());
        Debug.Log(Application.persistentDataPath + "/holeTextData.txt");
    }
    [ContextMenu("SyncInitHole")]
    public void SyncInitHole()
    {
        levels = FindObjectsOfType<Level>().ToList();
        HoleData gameData = JsonUtility.FromJson<HoleData>(holeDataText.text);
        foreach (InitHole hole in gameData.initHoles)
        {
            Level level = this.transform.Find(hole.level)?.GetComponent<Level>();
            if (level == null)
                Debug.Log(hole.level + " ko tim thay ");
            else
            {
                Transform holeGameObject = level.transform.Find(hole.hole);
                if (holeGameObject == null) Debug.Log(hole.hole + " ko tim thay " + hole.level);
                Transform attackableGameObject = level.transform.Find(hole.attackable);
                if (attackableGameObject == null) Debug.Log(hole.attackable + " ko tim thay " + hole.level);

                if (holeGameObject != null && attackableGameObject != null)
                {
                    Hole holeComponent = holeGameObject.GetComponent<Hole>();
                    holeComponent.enemyAttackable = attackableGameObject.GetComponent<Attackable>();
                }
            }
        }
    }
}
