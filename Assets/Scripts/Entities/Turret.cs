using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.GameFoundation;

public class Turret : Enemy
{
    [Header("Turret"), SerializeField] float rotationRate = 60;
    [SerializeField] Transform turretHead;
    [SerializeField] Transform gunEnd;
    [SerializeField] GameObject bullet;
    [Header("Visuals"), SerializeField] Light muzzleLight;
    [SerializeField] ParticleSystem muzzleParticle, sparkParticle;

    float timeToShoot;
    protected override void Start()
    {
        type = EnemyType.Turret;
        base.Start();
        timeToShoot = enemyItem.GetStatFloat("attackSpeed");
    }
    protected override void Die()
    {
        Destroy(gameObject);
        GameManager.Instance.EnemyDie(this);
        
    }

    protected override void InitializeAgentAndAnimator()
    {
        
    }

    protected override void Update()
    {
        base.Update();
        timeToShoot -= Time.deltaTime;
        if(!gameEnd) //We only do this if the game's still running
        {
            //Tracking player: See if the player is visible
            //Raycast out towards the player's position
            Vector3 playerPos = gm.PlayerObject.transform.position;
            playerPos.y = turretHead.position.y;
            
            if (Physics.Raycast(transform.position, playerPos - transform.position, out RaycastHit hit, detectDistance))
            {
                //Check whether it is a player we're colliding by its tag
                if(hit.collider.gameObject.CompareTag("Player"))
                {
                    //Player is visible. Start turning the turret's head
                    //Find angle between player and turret

                    Quaternion angle = Quaternion.LookRotation(playerPos - turretHead.position, transform.up);
                    //Rotate the turret
                    turretHead.rotation = Quaternion.RotateTowards(turretHead.rotation, angle, rotationRate * Time.deltaTime);

                    //If the distance and angle is small enough...
                    if (distance <= detectDistance && Vector3.Angle(turretHead.forward, playerPos - transform.position) < 4f)
                    {
                        //Attack the player if it's time to shoot
                        if (timeToShoot <= 0)
                            Shoot();
                    }
                }

            }


        }

    }

    private IEnumerator MuzzleFlashLight()
    {
        muzzleLight.enabled = true;
        yield return new WaitForSeconds(0.02f);
        muzzleLight.enabled = false;
    }

    private void Shoot()
    {
        timeToShoot = enemyItem.GetStatFloat("attackSpeed");

        //Spawn bullet from bullet spawnpoint
        var bulletTrans = Instantiate(bullet, gunEnd.position, gunEnd.rotation).transform;
        bulletTrans.GetComponent<TurretBullet>().damage = enemyItem.GetStatFloat("damage");
        //Add velocity to the bullet
        bulletTrans.GetComponent<Rigidbody>().velocity = bulletTrans.forward * 400;

        audioSource.Play();
        muzzleParticle.Emit(1);
        sparkParticle.Emit(Random.Range(5, 20));
        StartCoroutine(MuzzleFlashLight());
    }
}
