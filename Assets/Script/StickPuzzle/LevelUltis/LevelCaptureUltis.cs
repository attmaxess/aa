using System.Collections;
using System.Collections.Generic;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

public class LevelCaptureUltis : MonoBehaviour
{
    public List<LevelData> _data = new List<LevelData>();
    const string LevelPath = "Levels/Level";
    public bool WaitReady = false;
    public bool CaptureHint = true;
    public bool CaptureTapKill = false;
    public bool CaptureLevel11 = false;
    public bool CaptureDark = true;
    public bool CaptureTeleport = false;
    public bool CaptureGrids = false;
    public bool CapturePillar = false;
    public bool CaptureWall = false;
    public bool CaptureWays = false;
    public bool CaptureScene = false;
    GameObject currentLevelPrefab = null;

    [Space(20)]
    public int GameID;

    public void Capture(string filename)
    {
        ScreenCapture.CaptureScreenshot(GeneratePathFromFileName(filename));
    }
#if UNITY_EDITOR
    public void CaptureSceneView(
        Level level,
        string filename)
    {
        StartCoroutine(C_CaptureSceneView(level, filename));
    }
    IEnumerator C_CaptureSceneView(
        Level level,
        string filename)
    { 
        yield return new WaitUntil(() => level.DoneStartLevel == true);
        yield return new WaitForSeconds(1f);

        SceneView sw = SceneView.lastActiveSceneView;
        if (sw == null)
        {
            Debug.LogError("Unable to capture editor screenshot, no scene view found");
            yield break;
        }

        Camera cam = sw.camera;

        if (cam == null)
        {
            Debug.LogError("Unable to capture editor screenshot, no camera attached to current scene view");
            yield break;
        }

        RenderTexture renderTexture = new RenderTexture(
            Mathf.FloorToInt(cam.pixelRect.size.x),
            Mathf.FloorToInt(cam.pixelRect.size.y), 300);

        cam.targetTexture = renderTexture;
        RenderTexture.active = renderTexture;
        cam.Render();
        
        int width = renderTexture.width;
        int height = renderTexture.height;

        var outputTexture = new Texture2D(width, height, TextureFormat.RGB24, false);
        outputTexture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        outputTexture.Apply();
        File.WriteAllBytes(filename, outputTexture.EncodeToPNG());

        RenderTexture.active = null;
        cam.targetTexture = null;

        Destroy(outputTexture);
        yield break;
    }
#endif
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
    [ContextMenu("CaptureAll")]
    public void CaptureAll()
    {
        StartCoroutine(C_CaptureAll());
    }
    IEnumerator C_CaptureAll()
    {
        for (int i = 0; i < _data.Count; i++)
            yield return C_CaptureLevel(i);

        yield break;
    }
    [ContextMenu("CapturGameID")]
    public void CapturGameID()
    {
        StartCoroutine(C_CapturGameID());
    }
    IEnumerator C_CapturGameID()
    {
        yield return C_CaptureLevel(GameID);
        yield break;
    }
    IEnumerator C_CaptureLevel(int GameID)
    {
        GameController.instance.LoadLevelByResourceID(_data[GameID].ResourceID);
        yield return new WaitUntil(() => GameController.instance.currentLevel != null);
        yield return new WaitUntil(() => GameController.instance.DoneLoadLevel == true);

        if (CaptureTapKill)
        {
            TapKillManager tapKill = GameController.instance.currentLevel.levelController as TapKillManager;
            if (tapKill == null) yield break;
        }

        if (CaptureLevel11)
        {
            Level1_1 level11 = GameController.instance.currentLevel as Level1_1;
            if (level11 == null) yield break;
        }

        if (CaptureTeleport)
        {
            Teleport teleport = GameController.instance.currentLevel.GetComponentInChildren<Teleport>();
            if (teleport == null) yield break;
        }

        if (CaptureGrids)
        {
            BigGrid bigGrid = GameController.instance.currentLevel.levelController.GetComponent<BigGrid>();
            if (bigGrid == null) yield break;
        }

        if (CapturePillar)
        {
            PillarManager pillar = GameController.instance.currentLevel.levelController as PillarManager;
            if (pillar == null) yield break;
        }

        if (CaptureDark)
        {
            Dark dark = GameController.instance.currentLevel.GetComponentInChildren<Dark>();
            if (dark == null) yield break;
        }

        if (CaptureWall)
        {
            WallManager wall = GameController.instance.currentLevel.levelController as WallManager;
            if (wall == null) yield break;
        }

        if (CaptureWays)
        {
            Way way = GameController.instance.currentLevel.GetComponentInChildren<Way>();
            if (way == null) yield break;
        }

        //yield return new WaitUntil(() => GameCameraController.instance.IsClear() == true);
        yield return new WaitForEndOfFrame();

        if (WaitReady)
        {
            yield return new WaitUntil(() => GameController.instance.currentLevel.readyPlay == true);
            yield return new WaitForEndOfFrame();
        }

        if (CaptureHint)
        {
            GameController.instance.WatchHint();
            yield return new WaitUntil(() => GameController.instance.currentLevel.levelHint.DoneShow == true);
            yield return new WaitForEndOfFrame();
        }

        yield return new WaitUntil(() => GameController.instance.currentLevel.levelController.levelStatus == eLevelStatus.Idle);

        string fileName = "Level" +
            _data[GameID].ResourceID.ToString() + "_" +
            _data[GameID].GameID.ToString() + "_" +
            _data[GameID].NameType;
#if UNITY_EDITOR
        if (CaptureScene)
        {
            yield return new WaitForSeconds(1f);
            CaptureSceneView(GameController.instance.currentLevel, GeneratePathFromFileName(fileName));
        }
        else
            Capture(fileName);
#endif
        yield return new WaitUntil(() => File.Exists(GeneratePathFromFileName(fileName)) == true);
        yield return new WaitForEndOfFrame();
    }
    string GeneratePathFromFileName(string filename)
    {
        return Application.persistentDataPath + "/" + filename + ".png";
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
