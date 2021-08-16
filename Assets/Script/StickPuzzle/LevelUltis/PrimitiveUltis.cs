using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PrimitiveUltis
{
    public static GameObject CreateGameObject(
        string goName, 
        Transform parent, 
        Quaternion localRotation,
        Vector3 localScale)
    {
        GameObject newGO = new GameObject();
        newGO.name = goName;
        newGO.transform.parent = parent;
        newGO.transform.localRotation = localRotation;
        newGO.transform.localScale = localScale;
        return newGO;
    }
    public static RectTransform CreateRectObject(
        string goName,
        Transform parent,
        Quaternion localRotation,
        Vector3 localScale)
    {
        GameObject newGO = new GameObject();
        newGO.name = goName;
        newGO.transform.parent = parent;
        newGO.transform.localRotation = localRotation;
        newGO.transform.localScale = localScale;
        RectTransform rt = newGO.AddComponent<RectTransform>();
        return rt;
    }    
}
