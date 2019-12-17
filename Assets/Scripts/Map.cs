using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Map : MonoBehaviour
{
    [Header("Enemy Spawns")]
    public GameObject mobPrefab;
    public EnemySpawnPosition[] enemySpawnPoints;
    public float spawnRate = 1;
    public int maximumEnemy = 30;

    [Header("Item Spawns")]
    public EnemySpawnPosition[] itemSpawnPoints;
    public GameObject rifleAmmoPrefab, handGunAmmoPrefab, assortedAmmoPrefab, bigHealthPrefab, smallHealthPrefab, grenadePrefab;

    List<Enemy> enemies = new List<Enemy>();

    public int EnemyCount { get { return enemies.Count; } }
    public float AverageEnemyHP { get; private set; }
    public float AverageEnemyDistance { get; private set; }
    public float MaxEnemyDistance { get; private set; }
    public float MinEnemyDistance { get; private set; }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        CalculateStatistics();
        foreach(EnemySpawnPosition sp in enemySpawnPoints)
        {
            sp.canSpawnHere = CheckSpawnPointVisible(GameManager.Instance.PlayerObject.transform.position, sp.transform.position);
        }
    }

    private void CalculateStatistics()
    {
        AverageEnemyHP = 0;
        bool first = true;
        foreach (Enemy e in enemies)
        {
            //Sum all HP and distance
            AverageEnemyHP += e.CurrentHealth;
            AverageEnemyDistance += e.distance;
            if (first) //Calculate max/min distance
            {
                MaxEnemyDistance = e.distance;
                MinEnemyDistance = e.distance;
                first = false;
            }
            else
            {
                if (e.distance > MaxEnemyDistance) MaxEnemyDistance = e.distance;
                else if (e.distance < MinEnemyDistance) MinEnemyDistance = e.distance;
            }
        }

        //Divide to find average
        AverageEnemyDistance /= enemies.Count;
        AverageEnemyHP /= enemies.Count;
    }

    private bool CheckSpawnPointVisible(Vector3 playerPos, Vector3 spawnPos)
    {
        Vector3 dir = playerPos - spawnPos;
        Ray ray = new Ray(spawnPos, dir);
        if (Physics.Raycast(ray,
            out RaycastHit hit,
            Mathf.Infinity,
            LayerMask.GetMask("Map", "WeaponLayer")) &&
            hit.collider.gameObject.tag == "Player") return true;
        else return false;
    }

    private IEnumerator EnemySpawn()
    {
        //Spawn enemy
        while (true)
        {
            if (enemies.Count < maximumEnemy)
            {
                EnemySpawnPosition spawn = enemySpawnPoints[Random.Range(0, enemySpawnPoints.Length)];
                while (CheckSpawnPointVisible(GameManager.Instance.PlayerObject.transform.position, spawn.transform.position))
                {
                    spawn = enemySpawnPoints[Random.Range(0, enemySpawnPoints.Length)];
                    yield return null;
                }

                spawn.spawns++;
                enemies.Add(Instantiate(mobPrefab, spawn.transform.position, Quaternion.identity).GetComponent<Enemy>());
                yield return new WaitForSeconds(1f / spawnRate);
            }
            yield return null;
        }
    }
}

[System.Serializable]
public class EnemySpawnPosition
{
    public Transform transform;

    internal int spawns = 0;
    internal bool canSpawnHere = false;
}