/// Credit Titinious (https://github.com/Titinious)
/// Sourced from - https://github.com/Titinious/CurlyUI

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.UI.Extensions
{
    [RequireComponent(typeof(RectTransform))]
    public class CUIMapBound : MonoBehaviour
    {
        #region Descriptions

        [SerializeField]
        protected Vector3[] controlPoints;

        public Vector3[] ControlPoints
        {
            get
            {
                return controlPoints;
            }

        }

        PolygonCollider2D polygon
        {
            get
            {
                if (this._polygon == null) _polygon = GetComponent<PolygonCollider2D>();
                return this._polygon;
            }
        }
        PolygonCollider2D _polygon;

        #endregion

        #region Events

        [ContextMenu("Refresh")]
        public void Refresh()
        {
            Vector2[] points = new Vector2[ControlPoints.Length];
            for (int i = 0; i < ControlPoints.Length; i++)
            {
                points[i] = ControlPoints[i];
            }
            polygon.points = points;
        }

        #endregion        
    }
}