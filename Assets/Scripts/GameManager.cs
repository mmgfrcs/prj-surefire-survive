using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.GameFoundation;
using TMPro;
using System;

public class GameManager : MonoBehaviour {
    public bool debugMode = true;
    public Font debugFont;
    public float timerDuration = 300;

    [Header("Spawner")]
    public GameObject mobPrefab;
    public EnemySpawnPosition[] spawnPoints;
    public float spawnRate = 1;
    public int maximumEnemy = 30;
    
    [Header("UI")]
    public TextMeshProUGUI timerText;
    public GameObject logObject;
    public Transform logPanel;
    public ConsumableIcon bigPotionIcon;
    public ConsumableIcon smallPotionIcon;
    public ConsumableIcon grenadeIcon;

    public static GameManager Instance;

    [SerializeField]
    Player player;

    GUIStyle style;
    float time;

    //Gameplay
    Map currentMap;

    //Statistics
    List<Enemy> enemies = new List<Enemy>();
    float avgEnemyHp = 0;
    float avgDist = 0;
    float maxDist = 0;
    float minDist = 0;
    float flowPerSecond = 0;

    //Stat Definition
    public const string DEF_HEALTH = "health";
    public const string DEF_STAMINA = "stamina";
    public const string DEF_DAMAGE = "damage";
    public const string DEF_CRITICAL = "crit";
    public const string DEF_MOVESPEED = "moveSpeed";
    public const string DEF_ATKSPEED = "attackSpeed";
    public const string DEF_DMGOVERTIME = "dot";
    public const string DEF_HEAL = "heal";
    public const string DEF_AK47AMMO = "rifleAmmoRec";
    public const string DEF_HANDGUNAMMO = "handgunAmmoRec";

    public Player PlayerObject
    {
        get { return player; }
        private set { player = value; }
    }

    public bool GrenadeAvailable { get; private set; } = false;
    public bool BigPotionAvailable { get; private set; } = false;
    public bool SmallPotionAvailable { get; private set; } = false;

    private void Awake()
    {
        GameFoundation.Initialize(); //Initialize Game Foundation
        time = timerDuration;
        if (Instance == null)
            Instance = this;
        else Destroy(this);

        style = new GUIStyle();
        style.normal.textColor = Color.white;
        style.font = debugFont;
        style.fontSize = 20;
        //Hey
    }

    private void Start()
    {
        StartCoroutine(EnemySpawn());
    }

    private void OnGUI()
    {
        GUI.Label(new Rect(32, 32, 150, 32), $"Enemies: {enemies.Count}", style);
        GUI.Label(new Rect(32, 64, 150, 32), $"- Average Enemy HP: {avgEnemyHp.ToString("n1")}", style);
        GUI.Label(new Rect(32, 96, 220, 32), $"- Average Distance: {avgDist.ToString("n1")}m", style);
        GUI.Label(new Rect(32, 128, 220, 32), $"  - Max: {maxDist.ToString("n1")}m", style);
        GUI.Label(new Rect(32, 160, 220, 32), $"  - Min: {minDist.ToString("n1")}m", style);

        GUI.Label(new Rect(32, 224, 150, 32), "Spawn Points:", style);
        for(int i = 0; i < spawnPoints.Length; i++)
        {
            GUI.Label(new Rect(32, 256 + (32 * i), 332, 32), $"- {spawnPoints[i].transform.position.ToString()}: {(CheckSpawnPointVisible(spawnPoints[i].transform.position) ? "Not Spawning" : "Spawning")}, {spawnPoints[i].spawns}", style);
        }
    }

    private void OnDrawGizmosSelected()
    {
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            Vector3 dir = player.transform.position - spawnPoints[i].transform.position;
            Ray ray = new Ray(spawnPoints[i].transform.position, dir);
            Gizmos.DrawRay(ray);
        }
    }

    private void Update()
    {
        UpdateUI();
        ProcessInput();
        CalculateStatistics();
        time -= Time.deltaTime;

    }

    private bool CheckSpawnPointVisible(Vector3 spawnPos)
    {
        Vector3 dir = player.transform.position - spawnPos;
        Ray ray = new Ray(spawnPos, dir);
        if (Physics.Raycast(ray,
            out RaycastHit hit,
            Mathf.Infinity,
            LayerMask.GetMask("Map", "WeaponLayer")) &&
            hit.collider.gameObject.tag == "Player") return true;
        else return false;
    }

    private void ProcessInput()
    {

    }

    private void UpdateUI()
    {
        //Timer
        float minutes = Mathf.Floor(time / 60);
        float seconds = Mathf.Floor(time % 60);
        int milliseconds = Mathf.RoundToInt((time - (minutes * 60) - Mathf.Floor(seconds)) * 100) % 100;
        timerText.text = string.Format("{0:N0}:{1,2:00}:<size=40>{2,2:00}</size>", minutes, seconds, milliseconds);

        //Consumables
        if (GrenadeAvailable) grenadeIcon.Enable();
        else grenadeIcon.Disable();
        if (BigPotionAvailable) bigPotionIcon.Enable();
        else bigPotionIcon.Disable();
        if (SmallPotionAvailable) smallPotionIcon.Enable();
        else smallPotionIcon.Disable();
    }

    private void CalculateStatistics()
    {
        avgEnemyHp = 0;
        bool first = true;
        foreach (Enemy e in enemies) {
            //Sum all HP and distance
            avgEnemyHp += e.CurrentHealth;
            avgDist += e.distance;
            if(first) //Calculate max/min distance
            {
                maxDist = e.distance;
                minDist = e.distance;
                first = false;
            }
            else
            {
                if (e.distance > maxDist) maxDist = e.distance;
                else if (e.distance < minDist) minDist = e.distance;
            }
        }

        //Divide to find average
        avgDist /= enemies.Count;
        avgEnemyHp /= enemies.Count;
    }

    private IEnumerator EnemySpawn()
    {
        //Spawn enemy
        while(true)
        {
            if(enemies.Count < maximumEnemy)
            {
                EnemySpawnPosition spawn = spawnPoints[UnityEngine.Random.Range(0, spawnPoints.Length)];
                while(CheckSpawnPointVisible(spawn.transform.position))
                {
                    spawn = spawnPoints[UnityEngine.Random.Range(0, spawnPoints.Length)];
                    yield return null;
                }

                spawn.spawns++;
                enemies.Add(Instantiate(mobPrefab, spawn.transform.position, Quaternion.identity).GetComponent<Enemy>());
                yield return new WaitForSeconds(1f / spawnRate);
            }
            yield return null;
        }
    }

    internal void EnemyDie(Enemy e)
    {
        enemies.Remove(e);
    }

    internal void Announce(string text)
    {
        TextMeshProUGUI textMesh = Instantiate(logObject, logPanel).GetComponent<TextMeshProUGUI>();
        textMesh.text = text;
    }

    internal void GetBigPotion()
    {
        BigPotionAvailable = true;
    }

    internal void GetSmallPotion()
    {
        SmallPotionAvailable = true;
    }

    internal void GetGrenade()
    {
        GrenadeAvailable = true;
    }

    internal void UseBigPotion()
    {
        BigPotionAvailable = false;
    }

    internal void UseSmallPotion()
    {
        SmallPotionAvailable = false;
    }

    internal void UseGrenade()
    {
        GrenadeAvailable = false;
    }

    
}
