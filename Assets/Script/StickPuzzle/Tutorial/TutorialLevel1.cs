using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialLevel1 : MonoBehaviour
{
    [SerializeField] GameObject tap1Obj = null;

    private void Start()
    {
        EventDispacher.AddListener(EventName.OnTap, OnTap);

        if (SectionSettings.BiggestPlayLevel <= 1)
        {
            TaskUtil.Delay(this, delegate
            {
                ShowTutorial();
            }, 0.1f);
        }
    }

    void OnTap()
    {
        if (SectionSettings.BiggestPlayLevel > 1) return;

        HideTutorial();
    }

    private void OnDestroy()
    {
        EventDispacher.RemoveListener(EventName.OnTap, OnTap);
    }

    void ShowTutorial()
    {
        tap1Obj.SetActive(true);
    }

    void HideTutorial()
    {
        tap1Obj.SetActive(false);
    }
}
