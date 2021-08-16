using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialWaterHole : MonoBehaviour
{
    [SerializeField] GameObject tap1Obj = null;

    private void Start()
    {
        EventDispacher.AddListener(EventName.OnTap, OnTap);

        if (SectionSettings.BiggestPlayLevel <= 9)
        {
            TaskUtil.Delay(this, delegate
            {
                ShowTutorial();
            }, 0.1f);
        }
    }

    private void OnDestroy()
    {
        EventDispacher.RemoveListener(EventName.OnTap, OnTap);
    }

    void OnTap()
    {
        if (SectionSettings.BiggestPlayLevel > 9) return;

        tap1Obj.SetActive(false);
    }

    void ShowTutorial()
    {
        tap1Obj.SetActive(true);
    }
}
