using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;
using DG.Tweening;

public class EnemyDarkIcon : MonoBehaviour
{
    public Hole hole;
    [SerializeField] SkeletonGraphic skeletonGraphic = null;
    public void Show()
    {
        skeletonGraphic.DOFade(1, 0.25f);
    }
    public void Hide()
    {
        skeletonGraphic.DOFade(0, 0.25f);
    }
    public void GetCloseHole(List<Hole> holes)
    {
        if (holes.Count == 0) return;       

        holes.Sort((x, y) => (x.transform.position - transform.position).magnitude.
        CompareTo((y.transform.position - transform.position).magnitude));

        hole = holes[0];
    }
}
