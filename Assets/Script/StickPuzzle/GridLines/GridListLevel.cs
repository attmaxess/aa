using System.Collections;
using System.Collections.Generic;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

public class GridListLevel : MonoBehaviour
{   
    public List<OneGridDefinition> grid;    

#if UNITY_EDITOR
    [ContextMenu("SaveFile")]
    public void SaveFile()
    {
        //BigGridDefinition bigGridDefinition = FindObjectOfType<BigGrid>()?._bigGridData;
        //File.WriteAllText(Application.dataPath + "/" + gameObject.name + ".txt", bigGridDefinition.ToJson());
        //Debug.Log(Application.dataPath + "/Script/StickPuzzle/GridLines/" + gameObject.name + ".txt");
        //AssetDatabase.Refresh();
    }
#endif
}