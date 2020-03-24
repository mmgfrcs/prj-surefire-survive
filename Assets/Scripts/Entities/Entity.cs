using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity : MonoBehaviour
{
    public float CurrentHealth { get; protected set; }
    // Start is called before the first frame update
    
    public virtual void Damage(float amount)
    {
        if(CurrentHealth > amount) CurrentHealth -= amount;
        else
        {
            CurrentHealth = 0;
            Die();
        }
    }

    protected virtual void Die()
    {

    }
}
