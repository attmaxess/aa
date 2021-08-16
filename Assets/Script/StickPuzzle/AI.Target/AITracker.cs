using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AITracker : MonoBehaviour
{
    public RectTransform aiRT;
    public Rigidbody2D rigidbody2;
    [Space(20)]
    public bool CanTrack = true;

    Seeker seeker
    {
        get
        {
            if (_seeker == null && aiRT != null) _seeker = aiRT.GetComponent<Seeker>();
            return _seeker;
        }
    }
    Seeker _seeker = null;

    IAstarAI Iseeker
    {
        get
        {
            if (_Iseeker == null && seeker != null) _Iseeker = seeker.GetComponent<IAstarAI>();
            return _Iseeker;
        }
    }
    IAstarAI _Iseeker = null;

    AILerp aILerp
    {
        get
        {
            if (_aILerp == null && seeker != null) _aILerp = seeker.GetComponent<AILerp>();
            return _aILerp;
        }
    }
    AILerp _aILerp = null;

    ColliderDetection colliderDetection
    {
        get
        {
            if (_colliderDetection == null && seeker != null) _colliderDetection = seeker.GetComponent<ColliderDetection>();
            return _colliderDetection;
        }
    }
    ColliderDetection _colliderDetection = null;

    public float nearDistance = 0.01f;
    public float farDistance = 10f;
    public float multipleForce = 10;
    private void Start()
    {
        //StartCoroutine(C_Move());
    }
    IEnumerator C_Move()
    {
        while (true)
        {
            //Debug.Log(this.gameObject.name + " " + Distance(aiRT.gameObject, this.gameObject));
            if (CanTrack)
            {
                float distance = Distance(aiRT.gameObject, this.gameObject);
                Vector3 direction = Direction(this.gameObject, aiRT.gameObject);

                if (distance > farDistance)
                {
                    aILerp.canMove = false;
                    aILerp.isStopped = true;
                    Iseeker.canMove = false;
                    ResetForce();
                }
                else if (distance <= nearDistance)
                {
                    ResetForce();
                }
                else if (colliderDetection.IsContain(aiRT.transform))
                {
                    ResetForce();
                }
                else
                {
                    aILerp.canMove = true;
                    aILerp.isStopped = false;
                    Iseeker.canMove = true;
                }

                if (distance > nearDistance)
                {
                    rigidbody2.AddForce(direction.normalized / Time.deltaTime * multipleForce);
                }
            }
            yield return new WaitForEndOfFrame();
        }
    }
    private void Update()
    {
        if (CanTrack)
        {
            float distance = Distance(aiRT.gameObject, this.gameObject);
            Vector3 direction = Direction(this.gameObject, aiRT.gameObject);

            if (distance > farDistance)
            {
                aILerp.canMove = false;
                aILerp.isStopped = true;
                Iseeker.canMove = false;
                ResetForce();
            }
            else if (distance <= nearDistance)
            {
                ResetForce();
            }
            else if (colliderDetection.IsContain(aiRT.transform))
            {
                ResetForce();
            }
            else
            {
                aILerp.canMove = true;
                aILerp.isStopped = false;
                Iseeker.canMove = true;
            }

            if (distance > nearDistance && Time.deltaTime != 0)
            {
                rigidbody2.AddForce(direction / Time.deltaTime * multipleForce);
            }
        }
    }
    float Distance(GameObject start, GameObject end)
    {
        return (start.transform.position - end.transform.position).sqrMagnitude;
    }
    Vector3 Direction(GameObject start, GameObject end)
    {
        return (end.transform.position - start.transform.position);
    }
    [ContextMenu("Snap")]
    public void Snap()
    {
        this.transform.position = aiRT.transform.position;
    }
    [ContextMenu("ResetForce")]
    public void ResetForce()
    {
        rigidbody2.velocity = Vector3.zero;
        rigidbody2.angularVelocity = 0;
    }
}
