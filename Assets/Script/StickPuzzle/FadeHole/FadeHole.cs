using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeHole : BaseLevelProperties
{
    [SerializeField] List<FadeHoleItem> fadeHoleItems = new List<FadeHoleItem>();

    [ContextMenu("Awake")]
    public void Awake()
    {
        foreach (FadeHoleItem fade in fadeHoleItems)
            fade.FadeAtAwake();
    }
    void Start()
    {
        //EventDispacher<Hole>.AddListener(EventName.OnPassingHole, OnPassingHole);
        level.onPostPassingHole += OnPassingHole;
    }
    private void OnDestroy()
    {
        //EventDispacher<Hole>.RemoveListener(EventName.OnMoveToHole, OnPassingHole);
    }
    void OnPassingHole(Hole hole)
    ///Chi kiem tra 1 trong 2
    {        
        ///Kiểm tra tập 1 hole
        FadeHoleItem item = fadeHoleItems.Find(i => i.isShowed == false && i.targetHole == hole);
        if (item != null)
        {            
            item.Show();
            return;
        }

        ///Kiểm tra tập nhiều hole
        foreach (FadeHoleItem fade in fadeHoleItems)
            if (fade.IsAllHolesPassed())
            {
                ///Tap thi phai co nhieu phan tu                
                fade.Show();
                return;
            }
    }
}
