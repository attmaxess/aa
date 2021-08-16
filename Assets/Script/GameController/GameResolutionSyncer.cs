using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameResolutionSyncer : MonoBehaviour
///Should be parent objects
///To check game resolution and decide what to appear
///At awake and enable
{
    public List<Transform> normals;
    public List<Transform> longs;
    public List<Transform> squares;
    public bool IsSync = false;

    private void Awake()
    {
        StartCoroutine(C_SyncWithGameResolution());
    }

    IEnumerator C_SyncWithGameResolution()
    {
        yield return new WaitUntil(() => GameResolution.instance != null);
        switch (GameResolution.instance.eRes)
        {
            case eResolution.normal:
                Toggle(normals, new List<List<Transform>>() { longs, squares });
                break;
            case eResolution.longscreen:
                Toggle(longs, new List<List<Transform>>() { normals, squares });
                break;
            case eResolution.squarescreen:
                Toggle(squares, new List<List<Transform>>() { longs, normals });
                break;
        }
        IsSync = true;
    }
    void Toggle(List<Transform> on, List<List<Transform>> offs)
    {
        on.ForEach((item) => item.gameObject.SetActive(true));
        offs.ForEach((list) =>
        list.ForEach((item) => item.gameObject.SetActive(false)));
    }
}
