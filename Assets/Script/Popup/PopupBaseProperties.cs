using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopupBaseProperties : MonoBehaviour
{
    public CanvasGroup canvasGroup
    {
        get
        {
            if (_canvasGroup == null)
                _canvasGroup = GetComponentInChildren<CanvasGroup>();
            return _canvasGroup;
        }
    }
    CanvasGroup _canvasGroup;
}
