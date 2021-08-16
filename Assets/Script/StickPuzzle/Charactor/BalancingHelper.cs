using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI.Extensions;
#if UNITY_EDITOR    
using UnityEditor;
#endif

public class BalancingHelper : MonoBehaviour
{
    public Coroutine coBalancing = null;
#if UNITY_EDITOR
    public bool CanDraw = true;
    Vector3 drawPosition = Vector3.zero;    
#endif
    const float minBalanceY = 0.1f;
    const float durationBalance = 0.7f;

    [ReadOnly] float SelfBalanceSpeed = 3f;

    public static void StaticBalance(Transform tr, Vector3 position)
    {
        BalancingHelper helper = tr.GetComponent<BalancingHelper>();
        if (helper == null) helper = tr.gameObject.AddComponent<BalancingHelper>();
        helper.Balance(position);
    }
    public static bool StaticIsDoneBalance(Transform tr)
    {
        BalancingHelper helper = tr.GetComponent<BalancingHelper>();
        if (helper == null) helper = tr.gameObject.AddComponent<BalancingHelper>();
        return helper.coBalancing == null;
    }
    public static void StaticStopBalance(Transform tr)
    {
        BalancingHelper helper = tr.GetComponent<BalancingHelper>();
        if (helper == null) helper = tr.gameObject.AddComponent<BalancingHelper>();
        helper.StopBalance();
    }
    public void Balance(Vector3 position)
    {
        if (coBalancing != null)
        {
            StopCoroutine(coBalancing);
            coBalancing = null;
        }
        coBalancing = StartCoroutine(C_Balance(position));
    }
    IEnumerator C_Balance(Vector3 position)
    {
        float timer = Time.time;
#if UNITY_EDITOR
        drawPosition = position;
#endif
        while ((transform.position - position).magnitude > minBalanceY && Time.time - timer < durationBalance)
        {
            yield return new WaitForEndOfFrame();            
            transform.position = Vector3.Lerp(transform.position, position, Time.deltaTime * SelfBalanceSpeed);
        }
        coBalancing = null;

        yield break;
    }
    public void StopBalance()
    {
        if (coBalancing != null)
        {
            StopCoroutine(coBalancing);
            coBalancing = null;
        }
    }
    public bool IsDoneBalance(Transform tr)
    {
        return coBalancing == null;
    }
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {        
        if (CanDraw && coBalancing != null)
        {
            GUIStyle style = new GUIStyle();
            style.normal.textColor = Color.red;
            style.alignment = TextAnchor.MiddleCenter;
            Handles.Label(drawPosition, "x\n" + transform.name, style);            
        }
    }
#endif
}
