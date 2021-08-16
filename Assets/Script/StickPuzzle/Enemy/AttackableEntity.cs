using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackableEntity : MonoBehaviour
{
    public List<Attackable> attackables = new List<Attackable>();
    public HealthController healthController;

    private void Start()
    {
        healthController.onPostUpdateHealth += OnPostUpdateHealth;
    }

    public void AddAttack(Attackable attackable)
    {
        if (attackables.Contains(attackable)) return;
        attackables.Add(attackable);
        Enemy enemy = attackable.enemy;
        if (enemy != null)
        {
            healthController.Health += enemy.Health;
            //enemy.healthText.gameObject.SetActive(false);
        }
    }
    public void RemoveAttack(Attackable attackable)
    {
        if (!attackables.Contains(attackable)) return;
        attackable.enemy.healthController.healthText.gameObject.SetActive(true);
        attackables.Remove(attackable);
    }
    public void Damage(float damage)
    {
        healthController.Health -= damage;
    }
    void OnPostUpdateHealth()
    {
        if (attackables.Count == 0) return;
        //Transform healthText = (attackables[0] as Enemy).healthText.transform;
        //healthController.healthText.transform.position = healthText.position;        
    }
}
