using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopupBase : PopupBaseProperties
{

    public virtual void Show100()
    {
        if (canvasGroup != null)
            canvasGroup.alpha = 1;
    }

    public virtual void Show0()
    {
        if (canvasGroup != null)
            canvasGroup.alpha = 0;
    }
}
