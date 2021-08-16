using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class HealthUltis : MonoBehaviour
{
    public List<Level> levels;
    public TextAsset gameDataText;

    [ContextMenu("FixLevelPrefabToGetHealth")]
    public void FixLevelPrefabToGetHealth()
    {
        levels = FindObjectsOfType<Level>().ToList();
        foreach (Level level in levels)
        {
            GameObject go = level.gameObject;
            Charactor charactor = level.charactor;
            if (charactor == null) continue;
            HealthController healthController = charactor.GetComponent<HealthController>();
            if (healthController == null) healthController = go.AddComponent<HealthController>();
        }
    }
    [Serializable]
    public class InitHealth
    {
        public string level;
        public string health;
        public string gameobject;      //name
        public string ToJson()
        {
            return JsonUtility.ToJson(this);
        }
    }
    [Serializable]
    public class GameData
    {
        public List<InitHealth> initHealths = new List<InitHealth>();
        public string ToJson()
        {
            return JsonUtility.ToJson(this);
        }
    }
    [ContextMenu("DebugInitHealth")]
    public void DebugInitHealth()
    {
        levels = FindObjectsOfType<Level>().ToList();
        GameData gameData = new GameData();
        foreach (Level level in levels)
        {
            InitHealth initHealth = new InitHealth();

            initHealth.level = level.gameObject.name;
            initHealth.health = level.charactor.Health.ToString();
            initHealth.gameobject = level.charactor.gameObject.name;
            gameData.initHealths.Add(initHealth);

            List<Enemy> enemies = level.listEnemies;

            foreach (Enemy enemy in enemies)
            {
                initHealth = new InitHealth();
                initHealth.level = level.gameObject.name;
                initHealth.health = enemy.Health.ToString();
                initHealth.gameobject = enemy.gameObject.name;
                gameData.initHealths.Add(initHealth);
            }
        }
        File.WriteAllText(Application.persistentDataPath + "/gameTextData.txt", gameData.ToJson());
        Debug.Log(Application.persistentDataPath + "/gameTextData.txt");
    }
    [ContextMenu("SyncInitHealth")]
    public void SyncInitHealth()
    {
        levels = FindObjectsOfType<Level>().ToList();
        GameData gameData = JsonUtility.FromJson<GameData>(gameDataText.text);
        foreach (InitHealth health in gameData.initHealths)
        {
            Level level = this.transform.Find(health.level)?.GetComponent<Level>();
            if (level == null)
                Debug.Log(health.level + " ko tim thay ");
            else
            {

                Transform ObjectNeedHealth = level.transform.Find(health.gameobject);
                if (ObjectNeedHealth == null) Debug.Log(health.gameobject + " ko tim thay " + health.level);

                if (ObjectNeedHealth != null)
                {
                    HealthController healthController = ObjectNeedHealth.GetComponentInChildren<HealthController>();
                    if (healthController == null) healthController = ObjectNeedHealth.gameObject.AddComponent<HealthController>();

                    int h = -1;
                    int.TryParse(health.health, out h);
                    if (h != -1) healthController.Health = h;
                    else Debug.Log("bug " + health.level);
                }
            }
        }
    }
}
