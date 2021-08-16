using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI.Extensions;

[ExecuteInEditMode]
public class BarrierRay : BarrierBaseProperties
{
    public bool CanCheckCollision
    {
        get { return this._CanCheckCollision; }
        set
        {
            this._CanCheckCollision = value;
            OnpostSetCanCheckCollision(value);
        }
    }
    private void OnpostSetCanCheckCollision(bool value)
    ///Tắt tất cả các collider của bản thân
    {
        //foreach (BoxCollider2D box in GetComponentsInChildren<BoxCollider2D>().ToList())
        //{
        //    box.enabled = !value;
        //}
    }
    [ReadOnly] public bool _CanCheckCollision = false;
    public LayerMask mask;
    private void Update()
    {
        #region Check collision
        if (CanCheckCollision)
        {
            if (barrier.lineController.linetransforms.Count == 2)
            {
                Vector3 direction = barrier.lineController.linetransforms[1].position
                    - barrier.lineController.linetransforms[0].position;
                Ray ray = new Ray(barrier.lineController.linetransforms[0].position
                    , direction);

                Debug.DrawRay(ray.origin, direction, Color.blue);

                RaycastHit hit;
                if (Physics.Raycast(ray.origin, ray.direction, out hit, direction.magnitude, mask))
                {
                    barrier.skin = Barrier.eSkin.collide;
                }
                else
                {
                    barrier.skin = Barrier.eSkin.fine;
                }
            }
        }
        #endregion
    }
}
