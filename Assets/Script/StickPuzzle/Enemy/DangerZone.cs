using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DangerZone : MonoBehaviour
{
    public Transform target;
    public bool IsRotate = false;
    public float speed = 1f;
    private void Update()
    {
        if (IsRotate && target != null)
        {
            transform.LookAt(target);
        }
    }
}
