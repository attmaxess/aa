using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Appearance : MonoBehaviour
{
    public Text nameText;
    [ContextMenu("UpdateName")]
    public void UpdateName()
    {
        nameText.text = transform.name;
    }
    [ContextMenu("Toggle")]
    public void Toggle()
    {
        nameText.gameObject.SetActive(!nameText.gameObject.activeSelf);
    }
    [ContextMenu("Hide")]
    public void Hide()
    {
        nameText.gameObject.SetActive(false);
    }
    private void OnValidate()
    {
        //UpdateName();
    }
    private void Awake()
    {
        Hide();
    }
}
