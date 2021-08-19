using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class HealthController : HealthControllerBaseProperties
{
    public bool CanBeUpdate = true;
    public bool CanCalculatePosition = true;
    public bool CanSyncSkeleton = true;

    public delegate void OnPostUpdateHealth();
    public OnPostUpdateHealth onPostUpdateHealth;

    public Transform healthPosition;
    public Animator animtor;
    public Text healthText
    {
        get
        {
            if (_healthText == null) _healthText = GetComponentInChildren<Text>();
            if (_healthText == null) Debug.Log(gameObject.name + " ko co healthText ");
            return this._healthText;
        }
    }
    [SerializeField] Text _healthText;
    public float Health
    {
        get
        {
            return health;
        }
        set
        {
            health = Mathf.Max(0, value);
            UpdateHealth(true, false);
        }
    }
    [SerializeField] float health;

    [Space(10)]
    [SerializeField] Color hitColor = Color.black;

    private void Start()
    {
        UpdateHealth();
        //CalculateFirstPosition();
        if (CanSyncSkeleton)
            skeletonController.SyncWithHealth(Health);
    }
    [ContextMenu("CalculateFirstPosition")]
    public void CalculateFirstPosition()
    {
        if (!CanCalculatePosition) return;

        SkeletonController skeletonController = GetComponentInParent<SkeletonController>();
        if (skeletonController != null)
        {
            bool isMatch = skeletonController.GetActiveSkeletons()[0].MatchRectTransformWithBounds();
            if (isMatch)
            {
                RectTransform rt = healthPosition.GetComponent<RectTransform>();
                rt.pivot = new Vector2(.5f, 0);
                RectTransform skeletonRT = skeletonController.GetActiveSkeletons()[0].GetComponent<RectTransform>();

                rt.position = skeletonRT.position;

                Vector3 newV = new Vector3(rt.anchoredPosition.x,
                    rt.anchoredPosition.y + skeletonRT.sizeDelta.y,
                    0);

                rt.anchoredPosition = newV;
            }
        }
    }
    [ContextMenu("UpdateHealth")]
    public void UpdateHealthEditor()
    {
        UpdateHealth(true, true, true);
        if (CanSyncSkeleton)
            skeletonController.SyncWithHealth(Health);
    }
    public void UpdateHealth()
    {
        UpdateHealth(true, true);
    }
    public void UpdateHealth(
        bool updateNumber,
        bool updatePosition,
        bool updateColor = false)
    {
        if (!CanBeUpdate) return;

        if (updateNumber)
            healthText.text = Health != 0 ? health.ToString("#") : string.Empty;

        if (updateColor &&
            skeletonController != null && skeletonController.listSkeleton.Count > 0)
            healthText.color = skeletonController.listSkeleton[0].color;

        if (updatePosition)
            healthText.transform.position = healthPosition.position;        

        if (onPostUpdateHealth != null)
            onPostUpdateHealth.Invoke();
    }

    [ContextMenu("AnimTextHitTrap")]
    public void AnimTextHitTrap()
    {
        healthText.color = hitColor;
        TaskUtil.Delay(this, delegate
        {
            healthText.color = Color.white;
            TaskUtil.Delay(this, delegate
            {
                healthText.color = hitColor;
                TaskUtil.Delay(this, delegate
                {
                    healthText.color = Color.white;
                    TaskUtil.Delay(this, delegate
                    {
                        healthText.color = hitColor;
                        TaskUtil.Delay(this, delegate
                        {
                            healthText.color = Color.white;
                        }, 0.1f);
                    }, 0.1f);
                }, 0.1f);
            }, 0.1f);
        }, 0.1f);
    }
    [ContextMenu("Show0")]
    public void Show0()
    {
        ToggleAppearance(-1);
    }
    [ContextMenu("Show100")]
    public void Show100()
    {
        ToggleAppearance(1);
    }
    void ToggleAppearance(int toogleType = 0)
    {
        float alpha = 0;
        switch (toogleType)
        {
            case 0: alpha = healthText.color.a > 0 ? 0 : 1; break;
            case 1: alpha = 1; break;
            case -1: alpha = 0; break;
        }
        Color color = healthText.color;
        healthText.color = new Color(color.r, color.g, color.b, alpha);        
    }
    public void PlaceWhenFight()
    {
        HeadTowardController head = GetComponent<HeadTowardController>();
        if (head == null) return;
        int.TryParse(healthText.text, out int intHealth);
        if (intHealth <= 0) return;
        Vector3 pos = healthPosition.transform.position;
        if (head.Direction() == -1) pos += new Vector3(.3f, 0, 0);
        else if (head.Direction() == 1) pos += new Vector3(-.3f, 0, 0);
        healthText.transform.DOMove(pos, 0.2f);
        healthText.transform.position = healthPosition.transform.position;
    }
    [ContextMenu("on")] public void SetOn() { animtor.SetBool("IsOn", true); }
    [ContextMenu("off")] public void SetOff() { animtor.SetBool("IsOn", false); }
}
