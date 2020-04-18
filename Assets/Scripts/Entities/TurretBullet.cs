using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretBullet : BulletScript
{
    protected override void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.tag == "Player")
        {
            GameManager.Instance.PlayerObject.Damage(damage);
        }
        //If destroy on impact is false, start 
        //coroutine with random destroy timer
        if (!destroyOnImpact) StartCoroutine(DestroyTimer());
        //Otherwise, destroy bullet on impact
        else Destroy(gameObject);
    }
}
