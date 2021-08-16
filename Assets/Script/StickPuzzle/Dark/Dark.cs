using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Dark : BaseLevelProperties
{
    [SerializeField] RectTransform circle = null;
    [SerializeField] List<EnemyDarkIcon> enemyIcons = null;
    [SerializeField] GameObject enemyIconPrefab;
    [SerializeField] Text countdown;

    public int CountInt
    {
        get { return _CountInt; }
        set
        {
            _CountInt = value;
            countdown.text = _CountInt.ToString();
        }
    }
    public int _CountInt = 3;

    private void Start()
    {
        circle = level.charactor.circle.GetComponent<RectTransform>();
        circle.gameObject.SetActive(true);

        EventDispacher<Hole>.AddListener(EventName.OnMoveToHole, OnMoveToHole);
        EventDispacher.AddListener(EventName.DoWin, DoWin);

        StartCoroutine(C_CountDown());
    }
    IEnumerator C_CountDown()
    {
        yield return new WaitUntil(() => level.DoneStartLevel == true);
        yield return new WaitForEndOfFrame();
        while (CountInt >= 0)
        {
            yield return new WaitForSeconds(1f);
            CountInt--;
        }
        countdown.gameObject.SetActive(false);
        ShowDark();
    }
    private void OnDestroy()
    {
        EventDispacher<Hole>.RemoveListener(EventName.OnMoveToHole, OnMoveToHole);
        EventDispacher.RemoveListener(EventName.DoWin, DoWin);
    }
    void DoWin()
    {
        HideDark();
    }
    void OnMoveToHole(Hole hole)
    {
        EnemyDarkIcon icon = enemyIcons.Find(item => item.hole == hole);
        if (icon != null) icon.Hide();
    }
    public void ShowDark()
    {
        if (circle == null)
            circle = level.charactor.circle.GetComponent<RectTransform>();
        if (circle == null)
            return;

        circle.DOSizeDelta(new Vector2(450f, 450f), 0.5f).OnComplete(delegate
        {
            TaskUtil.Delay(this, delegate
            {
                enemyIcons.ForEach(item => item.Show());
                GameController.instance.currentLevel.readyPlay = true;
            }, 0.25f);
        });
    }
    public void HideDark()
    {
        circle.DOSizeDelta(new Vector2(5000f, 5000f), 0.5f);
    }
    [ContextMenu("FindHole")]
    public void FindHole()
    {
        foreach (var icon in enemyIcons)
            icon.GetCloseHole(level.listHoles);
    }
    [ContextMenu("CreateEnemyIcons")]
    public void CreateEnemyIcons()
    {
        enemyIcons = GetComponentsInChildren<EnemyDarkIcon>().ToList();
    }
}
