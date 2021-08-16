using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

public class Teleport : BaseLevelProperties
{
    [SerializeField] List<TeleportItem> teleportItems = null;

    public delegate void OnPostTeleport();
    public OnPostTeleport onPostTeleport;

    public static Teleport instance;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        EventDispacher<Hole>.AddListener(EventName.OnMoveToHole, OnMoveToHole);
        level.onPostPassingHole += OnPassingHole;
    }

    private void OnDestroy()
    {
        EventDispacher<Hole>.RemoveListener(EventName.OnMoveToHole, OnMoveToHole);
        //EventDispacher<Hole>.RemoveListener(EventName.OnPassingHole, OnPassingHole);
    }

    public bool CanTeleport(Hole hole)
    {
        TeleportItem item = teleportItems.Find(i => i.hole1 == hole || i.hole2 == hole);
        var result = item != null && !item.teleported;
        return result;
    }

    //public void CheckShowTeleport(Hole hole)
    //{
    //    if (hole == holeToShowTeleport)
    //    {
    //        teleportItems.ForEach(item => item.Show());
    //    }
    //}

    void OnMoveToHole(Hole hole)
    {
        TeleportItem item = teleportItems.Find(i => i.hole1 == hole || i.hole2 == hole);
        if (item != null && item.isShowed && !item.teleported)
            DoTeleport(item);
    }

    void OnPassingHole(Hole hole)
    {
        ///Kiểm tra tập 1 hole
        TeleportItem item = teleportItems.Find(i => i.holeToShow == hole);
        if (item != null) item.Show();

        ///Kiểm tra tập nhiều hole
        foreach (TeleportItem tele in teleportItems)
            if (tele.IsAllHolesPassed()) tele.Show();
    }

    public void DoTeleport(TeleportItem item)
    {
        item.teleported = true;

        Charactor charactor = GameController.instance.currentLevel.charactor;

        Hole targetHole = item.hole1 == charactor.hole ? item.hole1 : item.hole2;
        TaskUtil.Delay(this, delegate
        {
            charactor.hole = targetHole;
            //charactor.targetHole = targetHole;
            //Phòng trường hợp di chuyển không có target hole nhưng vẫn chạm sói
            //Cẩn thận trường hợp : teleport xong không đi tiếp target hole

            charactor.transform.position = targetHole.transform.position;

            List<GameObject> objs = new List<GameObject>();
            charactor.listSkeleton.ForEach(i => objs.Add(i.gameObject));

            targetHole.Place(objs, true);

            charactor.DoIdle();

            item.hole1.IsPassed = true;
            item.hole2.IsPassed = true;

            TaskUtil.Delay(this, delegate
            {
                item.Hide(true);

                if (onPostTeleport != null)
                    ///Chỗ này gọi sync hint tiếp
                    onPostTeleport.Invoke();
            }, 0.25f);
        }, 0.25f);
    }
    public TeleportItem GetTeleportOfHole(Hole hole)
    {
        if (teleportItems == null || teleportItems.Count == 0) return null;
        TeleportItem teleport = teleportItems.Find((x) => x.hole1 == hole || x.hole2 == hole);
        return teleport;
    }
}
