using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColliderDetection : MonoBehaviour
{
    public Collider2D col2D
    {
        get
        {
            if (_col2D == null) _col2D = GetComponent<Collider2D>();
            return this._col2D;
        }
    }
    [SerializeField] Collider2D _col2D;

    [SerializeField] List<Transform> trs = new List<Transform>();    
    [SerializeField] List<Collision2D> collisions = new List<Collision2D>();
    [SerializeField] List<Collider2D> colliders = new List<Collider2D>();

    public delegate void OnAddOnce(ColliderDetection detection, Transform tr);
    public OnAddOnce onAddOnce;
    public delegate void OnRemoceOnce(ColliderDetection detection, Transform tr);
    public OnRemoceOnce onRemoceOnce;
    public delegate void OnChange(ColliderDetection detection);
    public OnChange onChange;

    void AddOnce(Transform tr)
    {
        if (!trs.Contains(tr))
        {
            trs.Add(tr);
            if (onAddOnce != null) onAddOnce.Invoke(this, tr);
            if (onChange != null) onChange.Invoke(this);
        }        
    }
    void RemoveOnce(Transform tr)
    {
        if (trs.Contains(tr))
        {
            trs.Remove(tr);
            if (onRemoceOnce != null) onRemoceOnce.Invoke(this, tr);
            if (onChange != null) onChange.Invoke(this);
        }
    }

    void AddOnce(Collision2D collision)
    {
        if (!collisions.Contains(collision))
        {
            collisions.Add(collision);
            AddOnce(collision.transform);
        }
    }
    void AddOnce(Collider2D collider)
    {
        if (!colliders.Contains(collider))
        {
            colliders.Add(collider);
            AddOnce(collider.transform);
        }
    }
    void RemoveOnce(Collision2D collision)
    {
        if (collisions.Contains(collision))
        {
            collisions.Remove(collision);
            RemoveOnce(collision.transform);
        }
    }
    void RemoveOnce(Collider2D collider)
    {
        if (colliders.Contains(collider))
        {
            colliders.Remove(collider);
            RemoveOnce(collider.transform);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        AddOnce(collision);
    }
    private void OnCollisionExit2D(Collision2D collision)
    {
        RemoveOnce(collision);
    }
    private void OnTriggerEnter2D(Collider2D collider)
    {
        AddOnce(collider);
    }
    private void OnTriggerExit2D(Collider2D collider)
    {
        RemoveOnce(collider);
    }
    public bool IsContain(Collision2D collision)
    {
        return this.collisions.Contains(collision);
    }
    public bool IsContain(Collider2D collider)
    {
        return this.colliders.Contains(collider);
    }    
    public bool IsContain(Transform tr)
    {
        return this.trs.Contains(tr);
    }
    public int TrCount()
    {
        return this.trs.Count;
    }
    public Transform GetTr(int index)
    {
        return (index >= 0 && index < TrCount()) ? this.trs[index] : null;
    }
    public List<Transform> GetTrs()
    {
        return this.trs;
    }
    public void Clear()
    {
        this.colliders = new List<Collider2D>();
        this.collisions = new List<Collision2D>();
        this.trs = new List<Transform>();
    }    
    /// <summary>
    /// Hạn chế dùng
    /// </summary>
    public void TryRemove(Transform tr)
    {
        RemoveOnce(tr);
    }
    public Vector2 GetHitPosition(Transform tr)
    {
        if (!trs.Contains(tr)) return -Vector2.one;
        Collision2D collision = collisions.Find((x) => x.transform == tr);
        if (collision != null) return collision.GetContact(0).point;
        Collider2D collider = colliders.Find((x) => x.transform == tr);
        if (collider != null) return collider.ClosestPoint(tr.transform.position);
        return -Vector2.one;
    }
}
