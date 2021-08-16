using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(Image))]
public class BaseTouchController : MonoBehaviour
{
    public Collider2D col2D
    {
        get
        {
            if (_col2D == null) _col2D = GetComponentInParent<Collider2D>();
            if (_col2D == null) _col2D = GetComponent<Collider2D>();
            if (_col2D == null) _col2D = GetComponentInChildren<Collider2D>();
            return this._col2D;
        }
    }
    Collider2D _col2D;

    [ContextMenu("ResetTrigger")]
    public void ResetTrigger()
    {
        //StartCoroutine(C_ResetTrigger());
    }

    public IEnumerator C_ResetTrigger()
    {
        //yield return new WaitForEndOfFrame();
        //col2D.isTrigger = false;
        //yield return new WaitForEndOfFrame();
        //yield return new WaitUntil(() => col2D.isTrigger == false);
        //col2D.isTrigger = true;
        //yield return new WaitForEndOfFrame();
        //yield return new WaitUntil(() => col2D.isTrigger == true);
        //col2D.isTrigger = false;
        //yield return new WaitForEndOfFrame();
        //yield return new WaitUntil(() => col2D.isTrigger == false);

        yield break;
    }
}
