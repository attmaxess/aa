using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SnapRotation : MonoBehaviour
{
    Quaternion qua0 = Quaternion.identity;
    Quaternion qua90 = new Quaternion(0, 0, -.7f, .7f);
    Quaternion qua_90 = new Quaternion(0, 0, .7f, .7f);
    Quaternion qua180 = new Quaternion(0, 0, 1f, 0);

    Quaternion original = Quaternion.identity;

    public bool IgnoreParent = false;

    private void Awake()
    {
        original = transform.rotation;
    }

    [ContextMenu("DebugRotation")]
    public void DebugRotation()
    {
        Debug.Log(transform.rotation);
    }
    [ContextMenu("SetOriginal")]
    public void SetOriginal()
    {
        SetRotation(original);
    }
    [ContextMenu("Set0")]
    public void Set0()
    {
        SetRotation(qua0);
    }
    [ContextMenu("Set90")]
    public void Set90()
    {
        SetRotation(qua90);
    }
    [ContextMenu("Set_90")]
    public void Set_90()
    {
        SetRotation(qua_90);
    }
    [ContextMenu("Set180")]
    public void Set180()
    {
        SetRotation(qua180);
    }
    void SetRotation(Quaternion q)
    {
        transform.rotation = q;
        CheckChildrenIgnore();
    }
    void CheckChildrenIgnore()
    {
        List<SnapRotation> snaps = GetComponentsInChildren<SnapRotation>().ToList();
        foreach (var snap in snaps)
        {
            if (snap == this) continue;
            if (snap.IgnoreParent == true)
                snap.SetOriginal();
        }
    }
}
