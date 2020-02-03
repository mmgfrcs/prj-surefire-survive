using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.GameFoundation;
using TMPro;
using System;
using UnityEngine.AI;

public enum GameState
{
    Relax, BuildUp1, BuildUp2, BuildUp3, Peak, Final
}

public class GameManager : MonoBehaviour {
    [Header("Debug")]
    public bool debugMode = true;
    public TextMeshProUGUI debugText;

    [Header("Game - General")]
    public int keys = 2;
    public AudioSource dividerSource;
    
    public GameObject[] dividers;
    public NavMeshSurface pt1Surface;
    public NavMeshSurface pt2Surface;
    public float timerDuration = 300;
    public float hordeDelay = 100, hordeTime = 30;
    public AnimationCurve FERCurve;

    [Header("Game - Score")]
    public float scorePerEnemy = 100;
    public float baseScorePt1 = 2000;
    public float baseScorePt2 = 5000;
    public float baseScoreFinal = 10000;
    public float baseScoreVictory = 20000;

    [Header("Game - Meta AI")]
    public GameState defaultState = GameState.Relax;
    public float buildUp1Probability = 0.334f;
    public float buildUp2Probability = 0.333f;
    public float buildUp3Probability = 0.333f;
    public float maxFactor = 2, minFactor = 0.5f;
    public float timeToMax = 30, timeToMin = 15;

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
    float hordeTimer;

    //Gameplay
    Map currentMap;
    float playerStressLevel;
    int initRifleAmmo, initHandgunAmmo;
    float stateWeight = 0;

    //Statistics
    List<Enemy> enemies = new List<Enemy>();
    int enemyDefeated = 0;
    float maxDist = 0;
    float minDist = 0;
    float flowPerSecond = 0;
    private float varHP;
    private float varAmmo;
    private float varFER;
    private float stressRate;
    int objectives = 0;
    bool timer = false;
    float joy = 0, anger = 0, fear = 0, disgust = 0, sadness = 0, surprise = 0, valence = 0, contempt = 0, engagement = 0;

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

    public event Action OnGameEnd;

    internal bool IsInPart2 { get; private set; }
    internal bool IsHordeMode { get { return hordeTimer < 0; } }
    public GameState CurrentState { get; private set; }
    public float PeakStateTimer { get; private set; }

    public Player PlayerObject
    {
        get { return player; }
        private set { player = value; }
    }

    public bool GrenadeAvailable { get; private set; } = false;
    public bool BigPotionAvailable { get; private set; } = false;
    public bool SmallPotionAvailable { get; private set; } = true;

    bool BGMState = true;

    private void Awake()
    {
        GameFoundation.Initialize(); //Initialize Game Foundation
        time = timerDuration;
        if (Instance == null)
            Instance = this;
        else Destroy(this);

        currentMap = FindObjectOfType<Map>();
        print("GameManager Initializing");
        //Hey

    }
    private void Start()
    {
        print(currentMap != null ? $"Map found on {currentMap.gameObject.name}" : "Map not found. Game cannot start");
        hordeTimer = hordeDelay;
        initHandgunAmmo = player.handgunScript.ammo + player.handgunScript.magazine;
        initRifleAmmo = player.autoGunScript.ammo + player.autoGunScript.magazine;
        CurrentState = defaultState;
    }

    private void OnGUI()
    {

    }


    private void Update()
    {
        ProcessInput();
        ProcessStates();
        CalculateStressLevel();
        UpdateUI();

        if (timer)
        {
            timerText.gameObject.SetActive(true);
            time = Mathf.Max(time - Time.deltaTime, 0);
            if (time <= 0)
            {
                //Victory
                timer = false;
                currentMap.spawnEnemy = false;
                GameOverManager.ShowPanel(true, GetBaseScore(true), enemyDefeated * scorePerEnemy, 0);
                EndGame();
            }
        }

        if (CurrentState != GameState.Relax && CurrentState != GameState.Final) hordeTimer -= Time.deltaTime;
        else if (CurrentState == GameState.Final) hordeTimer = -1;

        if (hordeTimer < 1)
        {
            if (BGMState)
            {
                SoundManager.CrossfadeBGM();
                BGMState = false;
            }
            if (hordeTimer > 0) hordeTimer = 0;
            if (hordeTimer < -hordeTime)
            {
                hordeTimer = hordeDelay;
                SoundManager.CrossfadeBGM();
                BGMState = true;
            }
        }

    }

    private void ProcessStates()
    {
        float precision = 1000;
        switch(CurrentState)
        {
            case GameState.Relax:
                {
                    currentMap.spawnEnemy = false;
                    stateWeight = -5;
                    if(playerStressLevel <= 0)
                    {
                        float rnd = UnityEngine.Random.Range(0, precision);
                        float p1 = buildUp1Probability * precision;
                        float p2 = buildUp2Probability * precision;
                        if (rnd < p1) CurrentState = GameState.BuildUp1;
                        else if (rnd >= p1 && rnd < p1 + p2) CurrentState = GameState.BuildUp2;
                        else CurrentState = GameState.BuildUp3;
                    }
                    break;
                }
            case GameState.BuildUp1:
                {
                    currentMap.spawnEnemy = true;
                    currentMap.currentSpawnRate = Mathf.Min(currentMap.currentSpawnRate + (currentMap.spawnRate * Time.deltaTime / timeToMax), currentMap.spawnRate * maxFactor);
                    currentMap.currentMaxEnemy = Mathf.Min(currentMap.currentMaxEnemy + (currentMap.maximumEnemy * Time.deltaTime / timeToMax), currentMap.maximumEnemy * maxFactor);
                    stateWeight = 3;
                    if (playerStressLevel >= 100)
                    {
                        CurrentState = GameState.Peak;
                        PeakStateTimer = 10;
                    }
                    break;
                }
            case GameState.BuildUp2:
                {
                    currentMap.spawnEnemy = true;
                    if(currentMap.currentSpawnRate >= currentMap.spawnRate) currentMap.currentSpawnRate = Mathf.Max(currentMap.currentSpawnRate - (currentMap.spawnRate * Time.deltaTime / timeToMin), currentMap.spawnRate * minFactor);
                    else currentMap.currentSpawnRate = Mathf.Min(currentMap.currentSpawnRate + (currentMap.spawnRate * Time.deltaTime / timeToMax), currentMap.spawnRate * maxFactor);
                    if (currentMap.currentMaxEnemy >= currentMap.maximumEnemy) currentMap.currentMaxEnemy = Mathf.Max(currentMap.currentMaxEnemy - (currentMap.maximumEnemy * Time.deltaTime / timeToMin), currentMap.maximumEnemy * minFactor);
                    else currentMap.currentMaxEnemy = Mathf.Min(currentMap.currentMaxEnemy + (currentMap.maximumEnemy * Time.deltaTime / timeToMax), currentMap.maximumEnemy * maxFactor);
                    stateWeight = 3;
                    if (playerStressLevel >= 50)
                    {
                        CurrentState = GameState.Peak;
                        PeakStateTimer = 15;
                    }
                    break;
                }
            case GameState.BuildUp3:
                {
                    currentMap.spawnEnemy = true;
                    currentMap.currentSpawnRate = Mathf.Max(currentMap.currentSpawnRate - (currentMap.spawnRate * Time.deltaTime / timeToMin), currentMap.spawnRate * minFactor);
                    currentMap.currentMaxEnemy = Mathf.Max(currentMap.currentMaxEnemy - (currentMap.maximumEnemy * Time.deltaTime / timeToMin), currentMap.maximumEnemy * minFactor);
                    stateWeight = 3;
                    if (playerStressLevel >= 25)
                    {
                        CurrentState = GameState.Peak;
                        PeakStateTimer = 7;
                    }
                    break;
                }
            case GameState.Peak:
                {
                    stateWeight = 0;
                    PeakStateTimer -= Time.deltaTime;
                    if (PeakStateTimer <= 0) CurrentState = GameState.Relax;
                    break;
                }
        }
    }

    private void CalculateStressLevel()
    {
        if (ExpressionManager.FaceResults != null)
        {
            ExpressionManager.FaceResults.Emotions.TryGetValue(Affdex.Emotions.Joy, out joy);
            ExpressionManager.FaceResults.Emotions.TryGetValue(Affdex.Emotions.Anger, out anger);
            ExpressionManager.FaceResults.Emotions.TryGetValue(Affdex.Emotions.Fear, out fear);
            ExpressionManager.FaceResults.Emotions.TryGetValue(Affdex.Emotions.Disgust, out disgust);
            ExpressionManager.FaceResults.Emotions.TryGetValue(Affdex.Emotions.Sadness, out sadness);
            ExpressionManager.FaceResults.Emotions.TryGetValue(Affdex.Emotions.Surprise, out surprise);
            ExpressionManager.FaceResults.Emotions.TryGetValue(Affdex.Emotions.Valence, out valence);
            ExpressionManager.FaceResults.Emotions.TryGetValue(Affdex.Emotions.Contempt, out contempt);
            ExpressionManager.FaceResults.Emotions.TryGetValue(Affdex.Emotions.Engagement, out engagement);
        }

        float varHPWeight = 1;
        float varAmmoWeight = 1;
        varHP = ((player.playerItem.GetStatFloat(DEF_HEALTH) - player.CurrentHealth) / 50 - 1) * varHPWeight;
        varAmmo = ((initRifleAmmo + initHandgunAmmo - (player.autoGunScript.CurrentAmmo + player.autoGunScript.CurrentMagazine) - (player.handgunScript.CurrentAmmo + player.handgunScript.CurrentMagazine)) / 50f - 1) * varAmmoWeight;//player.autoGunScript
        varFER = FERCurve.Evaluate(joy + anger / 2);
        stressRate = (varHP + varAmmo) * varFER + stateWeight;
        //print(stressRate);
        playerStressLevel = Mathf.Clamp(playerStressLevel + stressRate * Time.deltaTime, 0, 100);
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

    private float GetBaseScore(bool victory = false)
    {
        if (victory) return baseScoreVictory;
        else if (CurrentState == GameState.Final) return baseScoreFinal;
        else if (IsInPart2) return baseScorePt2;
        else return baseScorePt1;
    }

    private void UpdateUI()
    {
        //Timer
        float minutes = Mathf.Floor(time / 60);
        float seconds = Mathf.Floor(time % 60);
        int milliseconds = Mathf.RoundToInt((time - (minutes * 60) - Mathf.Floor(seconds)) * 100) % 100;
        if (timer)
        {
            timerText.gameObject.SetActive(true);
            timerText.text = string.Format("{0:N0}:{1,2:00}:<size=40>{2,2:00}</size>", minutes, seconds, milliseconds);
        }
        else timerText.gameObject.SetActive(false);

        //Consumables
        if (GrenadeAvailable) grenadeIcon.Enable();
        else grenadeIcon.Disable();
        if (BigPotionAvailable) bigPotionIcon.Enable();
        else bigPotionIcon.Disable();
        if (SmallPotionAvailable) smallPotionIcon.Enable();
        else smallPotionIcon.Disable();

        //Debug
        if(debugMode)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            sb.AppendLine($"Enemies: {currentMap.EnemyCount}");
            sb.AppendLine($"- Average Enemy HP: {currentMap.AverageEnemyHP.ToString("n1")}");
            sb.AppendLine($"- Average Distance: {currentMap.AverageEnemyDistance.ToString("n1")}m");
            sb.AppendLine($"  - Max: {currentMap.MaxEnemyDistance.ToString("n1")}m");
            sb.AppendLine($"  - Min: {currentMap.MinEnemyDistance.ToString("n1")}m");
            sb.AppendLine($"Current State: {(CurrentState != GameState.Peak ? CurrentState.ToString() : $"{CurrentState}, {PeakStateTimer.ToString("n0")}s left")}, Spawnrate: {currentMap.currentSpawnRate.ToString("n3")} e/s, Max: {currentMap.currentMaxEnemy.ToString("n0")}");
            sb.AppendLine();
            sb.AppendLine(!IsHordeMode ? $"Next horde in {Mathf.FloorToInt(hordeTimer)}s" : $"Horde is coming, ending in {hordeTime - Mathf.FloorToInt(Mathf.Abs(hordeTimer))}s");
            sb.AppendLine($"Stress: {playerStressLevel.ToString("n2")}%, varHP {varHP.ToString("n2")}, varAmmo {varAmmo.ToString("n2")}, varFER {varFER.ToString("n2")}, Stressrate {stressRate.ToString("n2")}");

            sb.AppendLine();
            sb.AppendLine($"Face {(ExpressionManager.FaceFound ? "detected" : "not detected")}");
            if (ExpressionManager.FaceFound)
            {
                sb.AppendLine($"Joy: { joy.ToString("n2")}, Anger: { anger.ToString("n2")}, Fear: { fear.ToString("n2")}, Disgust: {disgust.ToString("n2")}, Engagement: {engagement.ToString("n2")}");
                sb.AppendLine($"Sadness: {sadness.ToString("n2")}, Surprise: { surprise.ToString("n2")}, Valence: { valence.ToString("n2")}, Contempt: {contempt.ToString("n2")}");
            }

            debugText.text = sb.ToString();
        }
    }

    internal void CompleteObjective()
    {
        objectives++;
        Announce("Found a key!");
        if (objectives == keys)
        {
            IsInPart2 = true;
            foreach(GameObject obj in dividers)
            {
                Destroy(obj);
            }
            pt1Surface.enabled = false;
            pt2Surface.enabled = true;
            SoundManager.PlaySound(dividerSource, SoundManager.SoundType.DoorRumble);
            Announce("A new path has just opened!");
        }
    }

    internal void EnemyDie(Enemy e)
    {
        enemyDefeated++;
        currentMap.EnemyDie(e);
    }

    internal void PlayerDie()
    {
        if(enabled)
        {
            GameOverManager.ShowPanel(false, GetBaseScore(), enemyDefeated * scorePerEnemy, 0);
            EndGame();
        }

    }

    internal void EndGame()
    {
        OnGameEnd.Invoke();
        player.autoGunScript.enabled = false;
        player.handgunScript.enabled = false;
        player.controller.enabled = false;
        enabled = false;
    }


    internal void Announce(string text)
    {
        TextMeshProUGUI textMesh = Instantiate(logObject, logPanel).GetComponent<TextMeshProUGUI>();
        textMesh.text = text;
    }

    internal void StartBoss()
    {
        if (CurrentState != GameState.Final)
        {
            CurrentState = GameState.Final;
            timer = true;
            currentMap.spawnEnemy = true;
            
        }
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