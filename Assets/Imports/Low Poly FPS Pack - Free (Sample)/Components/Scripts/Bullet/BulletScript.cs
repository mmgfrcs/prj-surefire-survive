﻿using UnityEngine;
using System.Collections;

// ----- Low Poly FPS Pack Free Version -----
public class BulletScript : MonoBehaviour {

	[Range(5, 100)]
	[Tooltip("After how long time should the bullet prefab be destroyed?")]
	public float destroyAfter;
	[Tooltip("If enabled the bullet destroys on impact")]
	public bool destroyOnImpact = false;
	[Tooltip("Minimum time after impact that the bullet is destroyed")]
	public float minDestroyTime;
	[Tooltip("Maximum time after impact that the bullet is destroyed")]
	public float maxDestroyTime;

	[Header("Impact Effect Prefabs")]
	public Transform [] metalImpactPrefabs;

    internal float damage;

	protected virtual void Start () 
	{
		//Start destroy timer
		StartCoroutine (DestroyAfter ());
	}

	//If the bullet collides with anything
	protected virtual void OnCollisionEnter (Collision collision) 
	{
		//If bullet collides with "Metal" tag
		if (collision.transform.tag == "Metal") 
		{
			//Instantiate random impact prefab from array
			Instantiate (metalImpactPrefabs [Random.Range 
				(0, metalImpactPrefabs.Length)], transform.position, 
				Quaternion.LookRotation (collision.contacts [0].normal));
		}

		//If bullet collides with "Target" tag
		if (collision.transform.tag == "Target") 
		{
			//Toggle "isHit" on target object
			collision.transform.gameObject.GetComponent
				<TargetScript>().isHit = true;
		}
			
		//If bullet collides with "ExplosiveBarrel" tag
		if (collision.transform.tag == "ExplosiveBarrel") 
		{
			//Toggle "explode" on explosive barrel object
			collision.transform.gameObject.GetComponent
				<ExplosiveBarrelScript>().explode = true;
		}

        //If bullet collides with "Enemy" tag
        if (collision.transform.tag == "Enemy")
        {
			//Toggle "explode" on explosive barrel object
			Enemy e = collision.transform.gameObject.GetComponent<Enemy>();
			if(e.type == EnemyType.Turret)
			{
				Instantiate(metalImpactPrefabs[Random.Range
				(0, metalImpactPrefabs.Length)], transform.position,
				Quaternion.LookRotation(collision.contacts[0].normal));
			}
			e.Damage(damage);
        }

        //If destroy on impact is false, start 
        //coroutine with random destroy timer
        if (!destroyOnImpact) StartCoroutine(DestroyTimer());
        //Otherwise, destroy bullet on impact
        else Destroy(gameObject);
        
    }

	protected IEnumerator DestroyTimer () 
	{
		//Wait random time based on min and max values
		yield return new WaitForSeconds
			(Random.Range(minDestroyTime, maxDestroyTime));
		//Destroy bullet object
		Destroy(gameObject);
	}

	protected IEnumerator DestroyAfter () 
	{
		//Wait for set amount of time
		yield return new WaitForSeconds (destroyAfter);
		//Destroy bullet object
		Destroy (gameObject);
	}
}
// ----- Low Poly FPS Pack Free Version -----