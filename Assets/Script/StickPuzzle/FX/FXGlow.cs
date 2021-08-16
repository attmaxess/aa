using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FXGlow : MonoBehaviour
{
    public Material glowMat;
    public Image img;

    private void Awake()
    {
        SetNoGlow();
    }
    public void SetGlow()
    {
        img.material = glowMat;
    }
    public void SetNoGlow()
    {
        img.material = null;
    }
}
