using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class LevelCheckSlashHole : BaseLevelProperties
{
    [ContextMenu("FixSlash")]
    public void FixSlash()
    {
        List<Hole> holes = level.GetComponentsInChildren<Hole>().ToList();
        foreach (var hole in holes)
        {
            if (IsSlashHole(hole) && hole.IsPassed == false)
            {
                Debug.Log("Fixed hole " + hole.transform.name);
                hole.IsPassed = true;
            }
        }
    }
    bool IsSlashHole(Hole hole)
    {
        return hole.transform.name.Contains("_");
    }
}
