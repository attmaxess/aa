using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class OneGridDefinition
{
    public eGridType gridType;
    public Sprite spriteDefault;
    public Sprite spriteHighlight;
    
    public Sprite gridDefault;
    public Sprite gridHighlight;

    public Color colorDefault;
    public Color colorHighlight;

    public int point = 10;

    public void SyncData(OneGridDefinition definition = null)
    {
        if (definition == null)
        {
            Debug.Log("def null");
            return;
        }

        this.gridType = definition.gridType;
        this.spriteDefault = definition.spriteDefault;
        this.spriteHighlight = definition.spriteHighlight;
        this.gridDefault = definition.gridDefault;
        this.gridHighlight = definition.gridHighlight;
        this.colorDefault = definition.colorDefault;
        this.colorHighlight = definition.colorHighlight;
        this.point = definition.point;
    }
}