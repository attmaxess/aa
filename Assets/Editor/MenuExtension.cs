using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class MenuExtension
{
    [MenuItem("Tools/Remove All Data")]
    static void RemoveAllData()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
    }
}
