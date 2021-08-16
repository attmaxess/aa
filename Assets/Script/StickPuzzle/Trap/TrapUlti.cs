using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class TrapUlti : MonoBehaviour
{
    public Material matDefault;
    public Material matHighlight;

    [ContextMenu("SetupAllTrap")]
    public void SetupAllTrap()
    {
        List<Trap> traps = GetComponentsInChildren<Trap>().ToList();
        foreach (Trap trap in traps)
        {
            if (trap.selectController == null)
                trap.gameObject.AddComponent<SelectController>();

            if (trap.selectController != null)
            {
                trap.selectController.image = trap.transform.GetComponentInChildren<Image>();
                trap.selectController.spriteDefault = trap.selectController.image.sprite;
                trap.selectController.spriteHighlight = trap.selectController.image.sprite;
                trap.selectController.matDefault = matDefault;
                trap.selectController.matHighlight = matHighlight;
            }
        }
    }
}
