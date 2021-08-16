using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class BigGridDefinition
{
    public List<OneGridDefinition> bigGrid = new List<OneGridDefinition>();
    public string ToJson()
    {
        return JsonUtility.ToJson(this);
    }
}
