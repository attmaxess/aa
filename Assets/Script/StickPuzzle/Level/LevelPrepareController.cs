using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LevelPrepareController : MonoBehaviour
{
    public bool CanPositionHint = false;
    public bool CanSortHint = false;
    public bool CanToggleImageHole = true;
    public bool CanFitMammalToHoleCenterAtStart = true;

    public Sprite holeSprite;
    [ContextMenu("FixHoleImage")]
    public void FixHoleImage()
    {
        List<Hole> holes = transform.GetComponentsInChildren<Hole>().ToList();
        if (holeSprite != null)
            foreach (var hole in holes)
                hole.holeImage.sprite = holeSprite;
        else
            foreach (var hole in holes)
                hole.holeImage.gameObject.SetActive(false);

    }    
}
