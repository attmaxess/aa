using UnityEngine;

public class TutorialLevel2 : MonoBehaviour
{
    [SerializeField] GameObject tap1Obj = null;
    [SerializeField] GameObject tap2Obj = null;

    int count = 0;

    private void Start()
    {
        EventDispacher.AddListener(EventName.OnTap, OnTap);

        if (SectionSettings.BiggestPlayLevel <= 2)
        {
            TaskUtil.Delay(this, delegate
            {
                ShowTutorial();
            }, 0.1f);
        }
    }

    void OnTap()
    {
        if (SectionSettings.BiggestPlayLevel > 2) return;

        if (count == 0)
        {
            tap1Obj.SetActive(false);
            TaskUtil.Delay(this, delegate
            {
                if (!GameController.instance.currentLevel.isWin)
                    tap2Obj.SetActive(true);
            }, 3.5f);
        }
        else if (count == 1)
        {
            tap2Obj.SetActive(false);
        }
        count += 1;
    }

    private void OnDestroy()
    {
        EventDispacher.RemoveListener(EventName.OnTap, OnTap);
    }

    void ShowTutorial()
    {
        tap1Obj.SetActive(true);
    }
}
