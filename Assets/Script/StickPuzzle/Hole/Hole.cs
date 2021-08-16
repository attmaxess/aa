using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Spine.Unity;
using Pathfinding;
using UnityEngine.UI.Extensions;
using System;
using UnityEngine.UI;
#if UNITY_EDITOR    
using UnityEditor;
#endif

public class Hole : HoleBaseProperties
{
    public Image holeImage;
    [SerializeField] List<HoleSlot> listSlot = new List<HoleSlot>();
    public Attackable enemyAttackable;

    public bool placeWhenAttack = true;
    public bool blockWeapon = false; // Block all character weapon but archery

    int totalObj = 0;

    [HideInInspector] public FightingDirection fightingDirection;
    public bool IsPassed
    {
        get { return this._IsPassed; }
        set
        {
            OnPostSetIsPass(_IsPassed, value);
        }
    }
    public bool _IsPassed = false;
    private void OnPostSetIsPass(bool lastValue, bool value)
    //Xet truong hop pass
    {
        if (lastValue == false &&
            value == true &&
            level?.onPostPassingHole != null)
        {
            _IsPassed = value;
            level?.onPostPassingHole.Invoke(this);
        }
        else
        {
            _IsPassed = value;
        }
    }

    [Space(10)]
    public List<Hole> connections;

    public delegate void OnPostAIMeetTarget(Hole tr, ColliderDetection detection);
    public OnPostAIMeetTarget postMeetHoleTarget;

    [Space(10)]
    public HoleHint hint;
    public Transform focus;
    public Transform glowImg;

    [ContextMenu("Place")]
    void PlaceAtEditor()
    {
        List<GameObject> objs = new List<GameObject>();

        Enemy enemy = (Enemy)enemyAttackable;
        enemy.listSkeleton.ForEach(item => objs.Add(item.gameObject));

        Place(objs, false);
    }
    public void Place(List<GameObject> listObj, bool animationMoving = true)
    {
        totalObj = listObj.Count;
        if (totalObj == 0) return;

        if (GameController.instance != null)
        {
            GameController.instance.currentLevel.HideFocusAnim();
        }

        if (animationMoving)
        {
            if (totalObj == 1) listObj[0].transform.DOMove(listSlot[4].transform.position, 0.2f);
            else if (totalObj == 2)
            {
                listObj[0].transform.DOMove(listSlot[3].transform.position, 0.2f);
                listObj[1].transform.DOMove(listSlot[5].transform.position, 0.2f);
            }
            else if (totalObj == 3)
            {
                listObj[0].transform.DOMove(listSlot[3].transform.position, 0.2f);
                listObj[1].transform.DOMove(listSlot[7].transform.position, 0.2f);
                listObj[2].transform.DOMove(listSlot[5].transform.position, 0.2f);
            }
            else if (totalObj == 4)
            {
                listObj[0].transform.DOMove(listSlot[0].transform.position, 0.2f);
                listObj[1].transform.DOMove(listSlot[6].transform.position, 0.2f);
                listObj[2].transform.DOMove(listSlot[2].transform.position, 0.2f);
                listObj[3].transform.DOMove(listSlot[8].transform.position, 0.2f);
            }
            else if (totalObj == 5)
            {
                listObj[0].transform.DOMove(listSlot[1].transform.position, 0.2f);
                listObj[1].transform.DOMove(listSlot[3].transform.position, 0.2f);
                listObj[2].transform.DOMove(listSlot[4].transform.position, 0.2f);
                listObj[3].transform.DOMove(listSlot[5].transform.position, 0.2f);
                listObj[4].transform.DOMove(listSlot[7].transform.position, 0.2f);
            }
            else if (totalObj == 6)
            {
                listObj[0].transform.DOMove(listSlot[0].transform.position, 0.2f);
                listObj[1].transform.DOMove(listSlot[1].transform.position, 0.2f);
                listObj[2].transform.DOMove(listSlot[3].transform.position, 0.2f);
                listObj[3].transform.DOMove(listSlot[5].transform.position, 0.2f);
                listObj[4].transform.DOMove(listSlot[7].transform.position, 0.2f);
                listObj[5].transform.DOMove(listSlot[8].transform.position, 0.2f);
            }
            else
            {
                Debug.LogError("Full hole slot");
            }
        }
        else
        {
            if (totalObj == 1) listObj[0].transform.position = listSlot[4].transform.position;
            else if (totalObj == 2)
            {
                listObj[0].transform.position = listSlot[3].transform.position;
                listObj[1].transform.position = listSlot[5].transform.position;
            }
            else if (totalObj == 3)
            {
                listObj[0].transform.position = listSlot[3].transform.position;
                listObj[1].transform.position = listSlot[7].transform.position;
                listObj[2].transform.position = listSlot[5].transform.position;
            }
            else if (totalObj == 4)
            {
                listObj[0].transform.position = listSlot[0].transform.position;
                listObj[1].transform.position = listSlot[6].transform.position;
                listObj[2].transform.position = listSlot[2].transform.position;
                listObj[3].transform.position = listSlot[8].transform.position;
            }
            else if (totalObj == 5)
            {
                listObj[0].transform.position = listSlot[1].transform.position;
                listObj[1].transform.position = listSlot[3].transform.position;
                listObj[2].transform.position = listSlot[4].transform.position;
                listObj[3].transform.position = listSlot[5].transform.position;
                listObj[4].transform.position = listSlot[7].transform.position;
            }
            else if (totalObj == 6)
            {
                listObj[0].transform.position = listSlot[0].transform.position;
                listObj[1].transform.position = listSlot[1].transform.position;
                listObj[2].transform.position = listSlot[3].transform.position;
                listObj[3].transform.position = listSlot[5].transform.position;
                listObj[4].transform.position = listSlot[7].transform.position;
                listObj[5].transform.position = listSlot[8].transform.position;
            }
            else
            {
                Debug.LogError("Full hole slot");
            }
        }
    }

    public void Place(SkeletonController charactorSkeleton, SkeletonController enemySkeleton)
    {
        GameController.instance.currentLevel.HideFocusAnim();

        if (placeWhenAttack)
            Place(enemySkeleton, false);
        ///Đặt tất cả enemy vào slot

        TaskUtil.Delay(this, delegate
        {
            SoundManager.instance.StopMoving();
            Place(charactorSkeleton, true);
        }, 0.2f);
        ///Đặt tất cả charactor vào slot
    }
    public void Place(SkeletonController skeleton, int lookDirection)
    ///Dung cho may con mammalAI
    {
        Place(skeleton, lookDirection == -1 ? true : false);
    }
    void Place(SkeletonController skeleton, bool isPlaceCharactor)
    {
        List<SkeletonGraphic> objs = skeleton.actives;
        if (isPlaceCharactor)
        {
            if (objs.Count == 0) return;

            if (objs.Count == 1) objs[0].transform.DOMove(listSlot[3].transform.position, 0.2f);
            else if (objs.Count == 2)
            {
                objs[0].transform.DOMove(listSlot[6].transform.position, 0.2f);
                objs[1].transform.DOMove(listSlot[0].transform.position, 0.2f);
            }
            else if (objs.Count == 3)
            {
                objs[0].transform.DOMove(listSlot[6].transform.position, 0.2f);
                objs[1].transform.DOMove(listSlot[3].transform.position, 0.2f);
                objs[2].transform.DOMove(listSlot[0].transform.position, 0.2f);
            }
        }
        else
        {
            if (objs.Count == 1) objs[0].transform.DOMove(listSlot[5].transform.position, 0.2f);
            else if (objs.Count == 2)
            {
                objs[0].transform.DOMove(listSlot[8].transform.position, 0.2f);
                objs[1].transform.DOMove(listSlot[2].transform.position, 0.2f);
            }
            else if (objs.Count == 3)
            {
                objs[0].transform.DOMove(listSlot[8].transform.position, 0.2f);
                objs[1].transform.DOMove(listSlot[5].transform.position, 0.2f);
                objs[2].transform.DOMove(listSlot[2].transform.position, 0.2f);
            }
            else if (objs.Count == 4)
            {
                objs[0].transform.DOMove(listSlot[8].transform.position, 0.2f);
                objs[1].transform.DOMove(listSlot[5].transform.position, 0.2f);
                objs[2].transform.DOMove(listSlot[2].transform.position, 0.2f);
                objs[3].transform.DOMove(listSlot[4].transform.position, 0.2f);
            }
        }
    }    
    public void DoWin(BaseMammal helper = null)
    {
        if (enemyAttackable != null)
        {
            Enemy enemy = (Enemy)enemyAttackable;
            if (enemy != null)
            {
                enemy.DoWin(helper);
                List<GameObject> objs = new List<GameObject>();
                enemy.skeletonController.listSkeleton.ForEach(item => objs.Add(item.gameObject));

                Place(objs);
            }

            IsPassed = true;
        }
    }
    public void DoLose(BaseMammal killer = null, bool placeCharactorToHole = true)
    {
        if (enemyAttackable != null)
        {
            if (enemyAttackable.IsEnemy())
            {
                enemyAttackable.enemy.DoLose(this, killer);
            }
            else if (enemyAttackable.charactor != null)
            {
                enemyAttackable.charactor.DoLose(this, killer);
            }
            else if (enemyAttackable.IsTrap())
            {
                enemyAttackable.trap.DoLose(this, killer);
            }
        }
        IsPassed = true;

        List<GameObject> objs = new List<GameObject>();
        GameController.instance.currentLevel.charactor.listSkeleton.ForEach(item => objs.Add(item.gameObject));

        if (placeCharactorToHole)
            Place(objs);
    }
    public void MeetHoleTarget(Transform target)
    {
        if (this.transform == target)
        {
            ColliderDetection detection = target.GetComponent<ColliderDetection>();
            //Debug.Log(this.gameObject.name + " meet " + targetHealth.gameObject.name);
            if (postMeetHoleTarget != null)
                postMeetHoleTarget.Invoke(this, detection);
        }
    }
    public bool ContainWaterTrap()
    {
        return enemyAttackable != null && enemyAttackable.trap != null && enemyAttackable.trap.type == eTrapType.WaterHole;
    }
    public bool ContainTrap()
    {
        return enemyAttackable != null && enemyAttackable.trap != null;
    }
    public bool ContainCharactor()
    {
        return enemyAttackable != null && enemyAttackable.charactor != null;
    }
    [ContextMenu("TestHeadToward")]
    public virtual void TestHeadToward()
    {
        this.enemyAttackable.head.Toward(transform);
    }
    [ContextMenu("UpdateAppearance")]
    public void UpdateAppearance()
    {
        UpdateAppearance(true, true);
    }
    public void UpdateAppearance(
        bool CanPositionHint,
        bool CanToggleImageHole)
    {
        //if (CanPositionHint)
            //hint.transform.position = HoleCenter() + transform.InverseTransformVector(new Vector3(60, -20, 0));

        focus.transform.position = HoleCenter();
        hint.gameObject.SetActive(true);

        if (enemyAttackable != null && enemyAttackable.IsTrap())
        {
            enemyAttackable.trap.InitHoleIsPassed(this);
            try
            {
                hint.gameObject.SetActive(!enemyAttackable.trap.hole.IsPassed);
            }
            catch { }
        }
        else if (enemyAttackable == null)
        {
            hole.IsPassed = true;
            //hint.gameObject.SetActive(false);
        }

        if (IsCharactorHole())
            hint.gameObject.SetActive(false);

        if (hint.GetIntHint() ==  0)
            hint.gameObject.SetActive(false);

        if (enemyAttackable != null)
            enemyAttackable.transform.position = HoleCenter();

        UpdateHoleCollider();

        if (appearance != null)
            appearance.nameText.transform.position = HoleCenter() + transform.InverseTransformVector(new Vector3(0, -40, 0));
    }
    void OffsetAttackableToCenterHole()
    {
        if (enemyAttackable == null) return;
        /*
        if (enemyAttackable.skeletonController.listSkeleton.Count == 0) return;
        Vector3 offset = enemyAttackable.skeletonController.ActiveCenter()
            - holeImage.transform.position;
        foreach (var skeleton in enemyAttackable.skeletonController.listSkeleton)
            skeleton.transform.position -= offset;
        */
    }
    [ContextMenu("UpdateHoleCollider")]
    public void UpdateHoleCollider()
    {
        Vector3 offset = holeImage.GetComponent<RectTransform>().anchoredPosition;
        colliderController.pivotOffset = offset;
        colliderController.UpdateCollider();
    }
    public bool IsContainHint(HoleHint hint)
    {
        return this.hint != null && this.hint.transform == hint.transform;
    }
    public void SetFocus(bool value)
    {
        //focus.gameObject.SetActive(value);        
    }
    public Vector3 HoleCenter()
    {
        return this.holeImage.transform.position;
    }
    public bool IsCharactorHole()
    {
        if (level.charactor.hole != null)
            return level.charactor.hole == this;
        else return false;
    }
    [ContextMenu("HideHoleImage")]
    public void HideHoleImage()
    {
        holeImage.gameObject.SetActive(false);
    }
    [ContextMenu("ShowHoleImage")]
    public void ShowHoleImage()
    {
        holeImage.gameObject.SetActive(true);
    }
    public bool ContainTeleport()
    {
        if (teleport != null) return teleport.GetTeleportOfHole(this) != null;
        else return false;
    }
    [ContextMenu("CenterAttackable")]
    public void CenterAttackable()
    {
        if (enemyAttackable != null)
            enemyAttackable.transform.position = HoleCenter();
    }
    [ContextMenu("1")] public void SetHint1() { hint.hintText.text = "1"; hint.gameObject.SetActive(true); }
    [ContextMenu("2")] public void SetHint2() { hint.hintText.text = "2"; hint.gameObject.SetActive(true); }
    [ContextMenu("3")] public void SetHint3() { hint.hintText.text = "3"; hint.gameObject.SetActive(true); }
    [ContextMenu("4")] public void SetHint4() { hint.hintText.text = "4"; hint.gameObject.SetActive(true); }
    [ContextMenu("5")] public void SetHint5() { hint.hintText.text = "5"; hint.gameObject.SetActive(true); }
    [ContextMenu("6")] public void SetHint6() { hint.hintText.text = "6"; hint.gameObject.SetActive(true); }
    [ContextMenu("7")] public void SetHint7() { hint.hintText.text = "7"; hint.gameObject.SetActive(true); }
    [ContextMenu("8")] public void SetHint8() { hint.hintText.text = "8"; hint.gameObject.SetActive(true); }
    [ContextMenu("9")] public void SetHint9() { hint.hintText.text = "9"; hint.gameObject.SetActive(true); }
    [ContextMenu("10")] public void SetHint10() { hint.hintText.text = "10"; hint.gameObject.SetActive(true); }
    [ContextMenu("0")] public void SetHint0() { hint.hintText.text = "0"; hint.gameObject.SetActive(false); }
#if UNITY_EDITOR    
    public void DrawConnections(bool drawLine, bool drawText)
    {
        foreach (Hole hole in connections)
        {
            if (hole != null)
            {
                Vector3 dir = hole.HoleCenter() - HoleCenter();
                Vector3 end = HoleCenter() + dir.normalized * dir.magnitude / 3f;

                if (drawLine)
                {
                    Color lineColor = Color.gray;
                    if (hole.IsPassed) lineColor = Color.black;
                    else if (!hole.IsPassed) lineColor = Color.blue;
                    var thickness = 10;
                    Handles.DrawBezier(HoleCenter(), end, HoleCenter(), end, lineColor, null, thickness);
                }

                if (drawText)
                {
                    Color textColor = Color.gray;
                    if (hole.IsPassed) textColor = Color.black;
                    else if (!hole.IsPassed) textColor = Color.blue;
                    GUIStyle style = new GUIStyle();
                    style.normal.textColor = textColor;
                    style.alignment = TextAnchor.MiddleCenter;
                    Handles.Label(end, hole.name, style);
                }
            }
        }
    }
#endif
}

public enum FightingDirection
{
    Left,
    Right
}
