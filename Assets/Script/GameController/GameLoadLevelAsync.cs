using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameLoadLevelAsync : GameControllerBaseProperties
{
    public bool DoneLoadLevel = true;
    [Space(20)]
    public Level currentLevel;
    public Transform levelParent;
    public int currentLevelID = -1;

    public void LoadLevelByGameID(int gameID, bool forceShowHint = false)
    {
        int resourceID = gameController.gameSO.GetResourceID(gameID);
        if (resourceID == -1) return;
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

        DoneLoadLevel = true;
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
