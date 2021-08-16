using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelFixHint : BaseLevelProperties
{
    [ContextMenu("FixHintTapAtCharactor")]
    public void FixHintTapAtCharactor()
    {
        try
        {
            TapKillManager tapKillManager = level.GetComponentInChildren<TapKillManager>();
            if (tapKillManager == null)
            {
                return;
            }
            Hole holeCharactor = level.listHoles.Find((x) => x.enemyAttackable != null &&
            x.enemyAttackable.transform == level.charactor.transform);
            if (holeCharactor == null)
            {
                Debug.Log(level.transform.name + " khong co hole Charactor");
                return;
            }
            holeCharactor.SetHint0();
            Hole holerWater = level.listHoles.Find((x) => x.enemyAttackable != null &&
            x.enemyAttackable.IsTrap() && x.enemyAttackable.trap.type == eTrapType.WaterHole);
            if (holerWater != null)
            {
                holerWater.SetHint0();
            }
            //tapKillManager.SyncHolesFromScene();
            //tapKillManager.levelHint.GetAllHint();
        }
        catch (Exception e)
        {
            Debug.Log(level.transform.name + " co loi " + e.ToString());
        }
    }
    [ContextMenu("FixHintWater")]
    public void FixHintWater()
    {
        try
        {
            TapKillManager tapKillManager = level.GetComponentInChildren<TapKillManager>();
            if (tapKillManager == null)
            {
                return;
            }
            List<Hole> holerWater = level.listHoles.FindAll((x) => x.enemyAttackable != null &&
            x.enemyAttackable.IsTrap() && x.enemyAttackable.trap.type == eTrapType.WaterHole);
            foreach (Hole water in holerWater)
            {
                Debug.Log(level.transform.name + " co water");
                water.SetHint0();
            }
        }
        catch (Exception e)
        {
            Debug.Log(level.transform.name + " co loi " + e.ToString());
        }
    }
    [ContextMenu("FixHintHoleNullEnemy")]
    public void FixHintHoleNullEnemy()
    {
        try
        {
            TapKillManager tapKillManager = level.GetComponentInChildren<TapKillManager>();
            if (tapKillManager == null)
            {
                return;
            }
            List<Hole> nullHole = level.listHoles.FindAll((x) => x.enemyAttackable == null);
            foreach (Hole hole in nullHole)
            {
                Debug.Log(level.transform.name + " co null hole");
                hole.SetHint0();
            }
        }
        catch (Exception e)
        {
            Debug.Log(level.transform.name + " co loi " + e.ToString());
        }
    }
    [ContextMenu("Set0HintNotActive")]
    public void Set0HintNotActive()
    {
        try
        {
            TapKillManager tapKillManager = level.GetComponentInChildren<TapKillManager>();
            if (tapKillManager == null)
            {
                return;
            }
            List<Hole> incctiveHintHoles = level.listHoles.FindAll((x) => x.hint != null &&
            x.hint.gameObject.activeSelf == false);
            foreach (Hole hole in incctiveHintHoles)
            {
                Debug.Log(level.transform.name + " co inactive hole");
                hole.SetHint0();
            }
        }
        catch (Exception e)
        {
            Debug.Log(level.transform.name + " co loi " + e.ToString());
        }
    }
    [ContextMenu("FixCharactorAndHoleCharactor")]
    public void FixCharactorAndHoleCharactor()
    {
        TapKillManager tapKillManager = level.GetComponentInChildren<TapKillManager>();
        if (tapKillManager == null)
        {
            return;
        }
        level.FitCharactorIntoFirstHole();
    }
}
