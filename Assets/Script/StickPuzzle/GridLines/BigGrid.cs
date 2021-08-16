using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class BigGrid : GridBaseProperties, iLevelHint
{
    public GameObject bigGridPrefab;
    public GameObject oneGridPrefab;
    public GameObject playerGridPrefab;
    public Transform currentBigGrid;
    public GameObject flyHealhPrefab;

    [Space(20)]
    public Vector2 playerCord = Vector2.zero;

    [Serializable]
    public class ObstacleCord
    {
        public Vector2 cord = Vector2.zero;
        public eGridType type = eGridType.Rock;
    }
    public List<ObstacleCord> obstacles;

    [ContextMenu("DeleteAll")]
    public void DeleteAll()
    {
        layerController.DeleteAll();

        if (currentBigGrid)
            if (Application.isPlaying) Destroy(currentBigGrid.gameObject);
            else DestroyImmediate(currentBigGrid.gameObject);

        currentBigGrid = Instantiate(bigGridPrefab as GameObject, transform).transform;
        currentBigGrid.gameObject.SetActive(true);
    }

    [Space(10)]
    public int columns;
    public int rows;

    public List<OneGrid> gridsData;

    public void CreateAll(BigGridDefinition bigGridData)
    {
        DeleteAll();
        if (!currentBigGrid) return;
        this.gridsData = new List<OneGrid>();
        for (int i = 0; i < columns; i++)
        {
            for (int j = 0; j < rows; j++)
            {
                CreateOneGrid(currentBigGrid, new Vector2(i, j));
            }
        }
    }

    [ContextMenu("CreateFirstGrid")]
    public void CreateFirstGrid()
    {
        DeleteAll();
        if (!currentBigGrid) return;
        gridsData = new List<OneGrid>();
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                CreateOneGrid(currentBigGrid, new Vector2(i, j));
            }
        }
        layerController.SelfSetup();

        OneGrid playerGrid = gridsData.Find((x) => x.cordinate == playerCord);
        playerGrid.gridType = eGridType.Player;

        foreach (ObstacleCord obstacle in obstacles)
        {
            OneGrid obsGrid = gridsData.Find((x) => x.cordinate == obstacle.cord);
            obsGrid.gridType = obstacle.type;
        }

        foreach (OneGrid grid in gridsData)
            grid.SelfSetup();
    }

    void CreateOneGrid(Transform parent, Vector2 cordinate)
    {
        OneGrid newGrid = cordinate != playerCord ?
            Instantiate(oneGridPrefab, currentBigGrid).GetComponent<OneGrid>() :
            Instantiate(playerGridPrefab as GameObject, currentBigGrid).GetComponent<OneGrid>();

        newGrid.Setup(cordinate);
        newGrid.gameObject.SetActive(true);
        gridsData.Add(newGrid);
    }

    [ContextMenu("UpdateDataFromScene")]
    public void UpdateDataFromScene()
    {
        gridsData = FindObjectsOfType<OneGrid>().ToList();
    }

    [ContextMenu("ChangeGridName")]
    public void ChangeGridName()
    {
        foreach (OneGrid grid in this.gridsData)
        {
            grid.gameObject.name = "G " + grid.cordinate;
        }
    }

    public void CreateFlyHealth(OneGrid grid, Transform charactorHealth)
    {
        FlyingHealth flyingHealth = Instantiate(flyHealhPrefab as GameObject, this.transform).GetComponent<FlyingHealth>();
        flyingHealth.transform.position = grid.transform.position;
        flyingHealth.targetHealth = charactorHealth;
        flyingHealth.postMeetTarget += scalingController.AddScaleFromFlyingHealth;
        flyingHealth.postMeetTarget += AddPointFromFlyingHealth;

        flyingHealth.Health = grid.point;
        grid.point = 0;

        grid.currentFlyHealth = flyingHealth.gameObject;


    }
    void AddPointFromFlyingHealth(FlyingHealth flying, UnityAction action)
    {
        level.charactor.healthController.Health += flying.Health;
        level.charactor.healthController.UpdateHealth(true, true);
        if (action != null) action.Invoke();
    }
    [ContextMenu("ShowHint")]
    public void ShowHint()
    {
        LevelDebug.DebugLog(transform.name + " BigGrid ShowHint");
    }
    public OneGrid GetPlayerGrid()
    {
        return gridsData.Find((x) => x.gridType == eGridType.Player);
    }
}
