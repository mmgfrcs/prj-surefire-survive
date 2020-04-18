using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretExplosion : MonoBehaviour
{
    public float explosionForce = 100;
    public float despawnDuration = 10;
    public Rigidbody[] rigidbodies;
    // Start is called before the first frame update
    void Start()
    {
        foreach(var rb in rigidbodies)
        {
            rb.AddForce(Random.onUnitSphere * explosionForce);
        }
        Destroy(gameObject, despawnDuration);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
