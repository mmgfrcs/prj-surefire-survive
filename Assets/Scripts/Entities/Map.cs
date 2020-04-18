using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Map : MonoBehaviour
{
    [Header("Enemy Spawns")]
    public EnemySpawnData[] enemyObjects;
    public GameObject pt1EnemySpawnParent, pt2EnemySpawnParent, turretSpawnParent;
    public GameObject bossPrefab, turretPrefab, turretDestroyedPrefab;
    public Transform bossSpawnPoint;
    public float spawnRate = 1;
    public int maximumEnemy = 30;

    [Header("Item Spawns")]
    public GameObject itemSpawnParent;
    public GameObject rifleAmmoPrefab, handGunAmmoPrefab, assortedAmmoPrefab, bigHealthPrefab, smallHealthPrefab, grenadePrefab, medkitPrefab;

    List<Enemy> enemies = new List<Enemy>();
    List<SpawnPosition> pt1EnemySpawnPoints = new List<SpawnPosition>();
    List<SpawnPosition> pt2EnemySpawnPoints = new List<SpawnPosition>();
    List<SpawnPosition> itemSpawnPoints = new List<SpawnPosition>();
    List<SpawnPosition> turretSpawnPoints = new List<SpawnPosition>();

    public int EnemyCount { get { return enemies.Count; } }
    public float AverageEnemyHP { get; private set; }
    public float AverageEnemyDistance { get; private set; }
    public float MaxEnemyDistance { get; private set; }
    public float MinEnemyDistance { get; private set; }
    public SpawnPosition[] Pt1EnemySpawnPoints { get { return pt1EnemySpawnPoints.ToArray(); } }
    public SpawnPosition[] Pt2EnemySpawnPoints { get { return pt2EnemySpawnPoints.ToArray(); } }
    public SpawnPosition[] ItemSpawnPoints { get { return null; } }

    internal bool spawnEnemy = true;
    internal float currentSpawnRate;
    internal float currentMaxEnemy;
    bool boss = false;
    float totalEnemyWeight = 0;
    Camera mainCam;

    // Start is called before the first frame update
    void Start()
    {
        List<string> sPos1 = new List<string>();
        List<string> sPos2 = new List<string>();
        currentSpawnRate = spawnRate;
        currentMaxEnemy = maximumEnemy;
        mainCam = Camera.main;
        foreach (Transform t in pt1EnemySpawnParent.GetComponentsInChildren<Transform>())
        {
            if (t.gameObject.name != pt1EnemySpawnParent.name)
            {
                sPos1.Add(t.position.ToString());
                pt1EnemySpawnPoints.Add(new SpawnPosition(t));
            }
        }
        foreach (Transform t in pt2EnemySpawnParent.GetComponentsInChildren<Transform>())
        {
            if (t.gameObject.name != pt2EnemySpawnParent.name)
            {
                sPos2.Add(t.position.ToString());
                pt2EnemySpawnPoints.Add(new SpawnPosition(t));
            }
        }
        foreach(Transform t in itemSpawnParent.GetComponentsInChildren<Transform>())
        {
            if (t.gameObject.name != itemSpawnParent.name)
            {
                SpawnPosition pos = new SpawnPosition(t);
                itemSpawnPoints.Add(pos);
                ItemSpawn(pos);
            }
        }
        foreach (Transform t in turretSpawnParent.GetComponentsInChildren<Transform>())
        {
            if (t.gameObject.name != turretSpawnParent.name)
            {
                SpawnPosition pos = new SpawnPosition(t);
                turretSpawnPoints.Add(pos);
            }
        }
        for (int i = 0; i<enemyObjects.Length;i++)
        {
            totalEnemyWeight += enemyObjects[i].weight;
        }
        print($"Part 1 Enemy Spawn Positions:\n - {string.Join("\n - ", sPos1)}");
        print($"Part 2 Enemy Spawn Positions:\n - {string.Join("\n - ", sPos2)}");
        StartCoroutine(EnemySpawn());
    }

    // Update is called once per frame
    void Update()
    {
        CalculateStatistics();
        foreach (SpawnPosition sp in pt2EnemySpawnPoints)
        {
            sp.canSpawnHere = !CheckSpawnPointVisible(GameManager.Instance.PlayerObject.transform.position, sp.transform.position);
        }
    }

    private void OnDrawGizmos()
    {
        foreach(SpawnPosition sp in pt2EnemySpawnPoints)
        {
            if (!CheckSpawnPointVisible(GameManager.Instance.PlayerObject.transform.position, sp.transform.position)) Gizmos.color = Color.green;
            else Gizmos.color = Color.red;
            Gizmos.DrawLine(sp.transform.position, sp.transform.position + (GameManager.Instance.PlayerObject.transform.position - sp.transform.position).normalized * 100);
        }
    }

    private void CalculateStatistics()
    {
        AverageEnemyHP = 0;
        AverageEnemyDistance = 0;
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
        Vector3 viewportPt = mainCam.WorldToViewportPoint(spawnPos);
        //Check whether the point can be seen by the camera. Splitting the If is unnecessary but it's a bit clear
        if (viewportPt.x > 0 && viewportPt.x < 1) //Inside camera viewport on X axis
        {
            if (viewportPt.y > 0 && viewportPt.y < 1) //Inside camera viewport on Y axis
            {
                if(viewportPt.z >= 0) //Inside camera viewport on the Z axis (i.e in front and not behind)
                {
                    Ray ray = new Ray(spawnPos, dir);
                    if (Physics.Raycast(ray,
                        out RaycastHit hit,
                        100f,
                        LayerMask.GetMask("Map", "WeaponLayer")) &&
                        hit.collider.gameObject.tag == "Player") return true;
                }
            }
        }

        return false;
    }

    private IEnumerator EnemySpawn()
    {
        for(int i = 0; i < turretSpawnPoints.Count; i++)
        {
            enemies.Add(Instantiate(turretPrefab, turretSpawnPoints[i].transform.position, turretSpawnPoints[i].transform.rotation).GetComponent<Enemy>());
        }
        //Spawn enemy
        while (true)
        {
            if (GameManager.Instance.CurrentState == GameState.Final && !boss)
            {
                GameObject bossObj = Instantiate(bossPrefab, bossSpawnPoint.position, Quaternion.identity);
                enemies.Add(bossObj.GetComponent<Enemy>());
                SoundManager.PlaySound(bossObj.GetComponent<AudioSource>(), SoundManager.SoundType.TrollRoar);
                boss = true;
            } 

            if (enemies.Count < Mathf.RoundToInt(currentMaxEnemy) && spawnEnemy)
            {
                SpawnPosition spawn;
                if (GameManager.Instance.IsInPart2)
                    spawn = pt2EnemySpawnPoints[Random.Range(0, pt2EnemySpawnPoints.Count)];
                else spawn = pt1EnemySpawnPoints[Random.Range(0, pt1EnemySpawnPoints.Count)];

                while (CheckSpawnPointVisible(GameManager.Instance.PlayerObject.transform.position, spawn.transform.position))
                {
                    if (GameManager.Instance.IsInPart2)
                        spawn = pt2EnemySpawnPoints[Random.Range(0, pt2EnemySpawnPoints.Count)];
                    else spawn = pt1EnemySpawnPoints[Random.Range(0, pt1EnemySpawnPoints.Count)];
                    yield return null;
                }

                spawn.spawns++;
                GameObject selectedEnemy = null;
                float prevWeight = 0;
                float rnd = Random.Range(0, totalEnemyWeight);
                for (int i = 0; i<enemyObjects.Length;i++)
                {
                    if(rnd < enemyObjects[i].weight + prevWeight)
                    {
                        selectedEnemy = enemyObjects[i].enemyObject;
                        break;
                    }
                    prevWeight += enemyObjects[i].weight;
                }

                enemies.Add(Instantiate(selectedEnemy, spawn.transform.position, Quaternion.identity).GetComponent<Enemy>());
                yield return new WaitForSeconds(1f / currentSpawnRate);
            }
            yield return null;
        }
    }

    private void ItemSpawn(SpawnPosition pos)
    {
        if(pos.transform.gameObject.name.Contains("Health"))
        {
            if (Random.Range(0, 100f) < 40f)
                Instantiate(bigHealthPrefab, pos.transform);
            else if (Random.Range(0, 100f) < 80f) Instantiate(smallHealthPrefab, pos.transform);
            else Instantiate(medkitPrefab, pos.transform);
            pos.spawns++;
        }
        else if (pos.transform.gameObject.name.Contains("Ammo"))
        {
            float rnd = Random.Range(0, 100f);
            if (rnd < 33f) Instantiate(assortedAmmoPrefab, pos.transform);
            else
            {
                rnd = Random.Range(0, 100f);
                if (rnd < 33f)
                    Instantiate(rifleAmmoPrefab, pos.transform);
                else if (rnd < 67f) Instantiate(handGunAmmoPrefab, pos.transform);
                else Instantiate(grenadePrefab, pos.transform);
            }
            pos.spawns++;
        }
    }

    private void ItemSpawn(SpawnPosition pos, ChestType type)
    {
        GameObject spawn;

        switch (type) {
            case ChestType.AssortedAmmo: { spawn = assortedAmmoPrefab; break; }
            case ChestType.BigPotion: { spawn = bigHealthPrefab; break; }
            case ChestType.Grenade: { spawn = grenadePrefab; break; }
            case ChestType.HandgunAmmo: { spawn = handGunAmmoPrefab; break; }
            case ChestType.RifleAmmo: { spawn = rifleAmmoPrefab; break; }
            case ChestType.SmallPotion: { spawn = smallHealthPrefab; break; }
            case ChestType.Medkit: { spawn = medkitPrefab; break; }
            default: { spawn = smallHealthPrefab; break; }
        }

        Instantiate(spawn, pos.transform);
        pos.spawns++;
    }

    internal void EnemyDie(Enemy e)
    {
        enemies.Remove(e);
        if (e.type == EnemyType.Turret) Instantiate(turretDestroyedPrefab, e.transform.position, Quaternion.identity);
    }
}

[System.Serializable]
public class SpawnPosition
{
    public Transform transform;

    internal int spawns = 0;
    internal bool canSpawnHere = false;

    public SpawnPosition(Transform t)
    {
        transform = t;
    }
}

[System.Serializable]
public struct EnemySpawnData
{
    public GameObject enemyObject;
    public float weight;
}