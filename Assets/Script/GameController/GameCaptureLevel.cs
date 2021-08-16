using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameCaptureLevel : GameControllerBaseProperties
{
    public bool DoneLoadLevel = true;
    public bool DoneDestroyLevel = true;
    [Space(20)]
    public Level currentLevel;
    public Transform levelParent;
    public int currentLevelID;

    public void LoadLevelByGameID(int gameID, bool forceShowHint = false)
    {
        int resourceID = gameController.gameSO.GetResourceID(gameID);
        StartCoroutine(C_LoadLevel(resourceID));
    }
    IEnumerator C_LoadLevel(int resourceID)
    {
        DoneLoadLevel = false;

        gameController.InstantiateLevelFromSource(
            resourceID: resourceID,
            parent: levelParent,
            forceInactive: true,
            showHint: false,
            out currentLevelID,
            out currentLevel);

        currentLevel.HideHealthText();

        DoneLoadLevel = true;
        yield break;
    }
    public void DestroyLevel()
    {
        if (currentLevel == null)
            return;

        StartCoroutine(C_DestroyLevel());
    }
    IEnumerator C_DestroyLevel()
    {
        DoneDestroyLevel = false;
        Destroy(currentLevel.gameObject, .3f);
        DoneDestroyLevel = true;
        yield break;
    }
    public void Trash()
    {
        if (currentLevel != null)
            GameTrash.instance.AddTrash(currentLevel.transform);
        currentLevel = null;
        currentLevelID = -1;
    }
}
