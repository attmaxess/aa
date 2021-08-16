using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameTrash : Singleton<GameTrash>
{
    List<Transform> trash = new List<Transform>();
    public void AddTrash(Transform tr)
    {
        tr.gameObject.SetActive(false);
        tr.SetParent(transform);
        trash.Add(tr);
        trash.RemoveAll((x) => x == null);
        DeleteAll();
    }
    void DeleteAll()
    {
        trash.RemoveAll((x) => x == null);
        for (int i = 0; i < trash.Count; i++)
            if (trash[i] != null)
                DestroyDelay(trash[i]);
    }
    void DestroyDelay(Transform tr)
    {
        Destroy(tr.gameObject, .3f);
    }
}
