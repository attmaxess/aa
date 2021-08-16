using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class FlyingHealth : MonoBehaviour
{
    public int Health
    {
        get { return this._Health; }
        set { this._Health = value; UpdateText(value); }
    }
    int _Health = 0;

    public Text uiText;
    public ColliderDetection detection;
    public Rigidbody2D rigidbody2;
    [Space(20)]
    public Vector3 spawnOffsetMin = new Vector3(1f, 1f, 1f);
    public Vector3 spawnOffsetMax = new Vector3(3f, 3f, 3f);
    public float multipleSpawnSpeed = 10f;
    public float timeJumpSpawn = 0.5f;
    public bool doneJumpWhenSpawn = true;
    [Space(20)]
    public Transform targetHealth;
    public bool CanTargetHealth = false;
    public float multipleFlySpeed = 1f;
    public float distanceMin = 0.1f;

    public delegate void PostMeetTarget(FlyingHealth flying, UnityAction action);
    public PostMeetTarget postMeetTarget;

    IEnumerator Start()
    {
        //Physics2D.gravity = Vector2.zero;
        CanTargetHealth = false;
        detection.onAddOnce += MeetTarget;

        JumpWhenSpawn();
        yield return new WaitUntil(() => doneJumpWhenSpawn == true);

        rigidbody2.velocity = Vector2.zero;
        rigidbody2.angularVelocity = 0;
        CanTargetHealth = true;
    }

    public void JumpWhenSpawn()
    {
        StartCoroutine(C_JumpWhenSpawn());
    }

    IEnumerator C_JumpWhenSpawn()
    {
        doneJumpWhenSpawn = false;
        Vector3 randomV = new Vector3(
            Random.Range(spawnOffsetMin.x * FloatDirection(), spawnOffsetMax.x * FloatDirection()),
            Random.Range(spawnOffsetMin.y * FloatDirection(), spawnOffsetMax.y * FloatDirection()),
            Random.Range(spawnOffsetMin.z * FloatDirection(), spawnOffsetMax.z * FloatDirection()));
        Vector3 newPosition = this.transform.position + randomV;
        Vector3 direction = newPosition - this.transform.position;
        rigidbody2.AddForce(direction * multipleSpawnSpeed);
        yield return new WaitForSeconds(timeJumpSpawn);
        doneJumpWhenSpawn = true;
        yield break;
    }

    float FloatDirection()
    {
        return Random.Range(0, 2) == 0 ? -1 : 1;
    }
    float Distance(GameObject start, GameObject end)
    {
        return (start.transform.position - end.transform.position).sqrMagnitude;
    }
    Vector3 Direction(GameObject start, GameObject end)
    {
        return (end.transform.position - start.transform.position);
    }
    private void Update()
    {
        if (CanTargetHealth)
        {
            if (targetHealth != null &&
            Time.deltaTime != 0 &&
            Distance(targetHealth.gameObject, this.gameObject) > distanceMin)
            {
                //rigidbody2.AddForce((targetHealth.transform.position - this.transform.position) / Time.deltaTime * multipleFlySpeed);
                this.transform.position = Vector3.Lerp(this.transform.position, targetHealth.transform.TransformPoint(targetHealth.transform.position), multipleFlySpeed * Time.deltaTime);
            }
            else
            {
                CanTargetHealth = false;
            }
        }
    }
    void MeetTarget(ColliderDetection detection, Transform tr)
    {
        if (tr == targetHealth)
        {
            //Debug.Log(this.gameObject.name + " meet " + targetHealth.gameObject.name);
            CanTargetHealth = false;
            rigidbody2.velocity = Vector2.zero;
            rigidbody2.angularVelocity = 0;

            if (postMeetTarget != null)
                postMeetTarget.Invoke(this, new UnityAction(() => SelfDestroy()));
        }
    }
    void SelfDestroy()
    {
        Destroy(this.gameObject);
    }
    void UpdateText(int value)
    {
        uiText.text = value != 0 ? value.ToString() : string.Empty;
    }
}
