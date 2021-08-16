using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI.Extensions;

public class BarrierLineController : LineController
{
    protected override void OnPostSetLinesValue(List<RectTransform> value)
    {
        base.OnPostSetLinesValue(value);

        BarrierIsometricController isometric = GetComponent<BarrierIsometricController>();
        List<Canvas> listCanvas = new List<Canvas>();
        foreach (RectTransform rt in linetransforms)
        {
            Canvas canvas = rt.GetComponent<Canvas>();
            if (canvas != null) listCanvas.Add(canvas);
        }
        isometric.SetCanvas(listCanvas);
    }
}
