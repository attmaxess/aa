using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EnhancedUI.EnhancedScroller;
using EnhancedUI;

public class LevelCellView : EnhancedScrollerCellView
{
    public LevelRowCellView[] rowCellViews;
   
    public void SetData(ref SmallList<LevelData> data, int startingIndex)
    {
        for (var i = 0; i < rowCellViews.Length; i++)
        {
            rowCellViews[i].SetData(startingIndex + i < data.Count ? data[startingIndex + i] : null);
        }
    }
}
