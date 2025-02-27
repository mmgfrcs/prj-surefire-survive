﻿using Affdex;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.GameFoundation;
using UnityEngine.UI;

public enum GameState
{
    Relax, BuildUp1, BuildUp2, BuildUp3, Peak, Final
}

public class GameManager : MonoBehaviour {
    [Header("Debug"), SerializeField] public bool debugMode = true;
    [SerializeField] public bool printData = true;
    [SerializeField] public float printInterval = 1f;
    [SerializeField] private PrintType printFormat = PrintType.CSV;
    [SerializeField] private TextMeshProUGUI debugText;
    [SerializeField] private TextMeshProUGUI objectiveDebugText, timerDebugText;

    [Header("Game - General"), SerializeField]
    private Player player; 
    [SerializeField] private ObjectiveBase[] objectives;
    [SerializeField] private AudioSource dividerSource;
    [SerializeField] private GameObject[] dividers;
    [SerializeField] private GameObject finalWall;
    [SerializeField] private NavMeshSurface pt1Surface;
    [SerializeField] private NavMeshSurface pt2Surface;
    [SerializeField] private float hordeDelay = 100, hordeTime = 30;
    [SerializeField] private CarePackageSettings carePackageSettings;
    [SerializeField] private GameScore scoreConfiguration;

    [Header("Game - Meta AI"), SerializeField]
    private GameState defaultState = GameState.Relax;
    [SerializeField] private float buildUp1Probability = 0.334f;
    [SerializeField] private float buildUp2Probability = 0.333f;
    [SerializeField] private float probabilityChange = 0.033f;
    [SerializeField] private float maxEnemyAtBU1 = 100, maxEnemyAtBU3 = 25f, spawnRateAtBU1 = 0.667f, spawnRateAtBU3 = 0.167f;
    [SerializeField] private float timeToMax = 30, timeToMin = 15;
    [SerializeField] private AnimationCurve joyCurve, angerCurve, fearCurve, surpriseCurve, disgustCurve;
    [SerializeField] private RateOfChange rateOfMaxEnemyChange, rateOfSpawnRateChange;
    [SerializeField] private Detector detector;

    [Header("UI"), SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI objectiveText;
    [SerializeField] private GameObject logObject;
    [SerializeField] private Transform logPanel, logPanelDebug;
    [SerializeField] private ConsumableIcon bigPotionIcon;
    [SerializeField] private ConsumableIcon smallPotionIcon;
    [SerializeField] private ConsumableIcon medkitIcon;
    [SerializeField] private ConsumableIcon grenadeIcon;
    [SerializeField] private Image faceStatusImage;

    public static GameManager Instance;
    public static bool FEREnabled;

    private float hordeTimer;

    //Gameplay
    private Map currentMap;
    private float playerStressLevel;
    private int initRifleAmmo, initHandgunAmmo, totalInitAmmo;
    private float stateWeight = 0;
    private float startingMaxEnemy = 0, startingSpawnRate = 0;

    //Objective
    private Queue<ObjectiveBase> objectiveQueue = new Queue<ObjectiveBase>();
    private ObjectiveBase currentObjective;

    //Statistics
    private int enemyDefeated = 0;
    private int bossDefeated = 0;
    private float flowPerSecond = 0;

    private float varHP;
    private float varAmmo;
    private float varFER;
    private float stressRate;
    private float joy = 0, anger = 0, fear = 0, disgust = 0, sadness = 0, surprise = 0, valence = 0, contempt = 0, engagement = 0;

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

    internal float CurrentStateTime { get; private set; }
    internal bool IsInPart2 { get; private set; }
    internal bool IsInMaze { get; private set; }
    internal bool IsHordeMode { get { return hordeTimer < 0; } }
    public GameState CurrentState { get; private set; }
    public float PeakStateTimer { get; private set; }

    public Player PlayerObject
    {
        get { return player; }
        private set { player = value; }
    }

    public bool GrenadeAvailable { get; private set; } = true;
    public bool BigPotionAvailable { get; private set; } = true;
    public bool SmallPotionAvailable { get; private set; } = true;
    public bool MedkitAvailable { get; private set; } = true;

    private bool BGMState = true;
    internal bool gameEnd = false;
    private DataPrinter printer;
    private float[] buildUpProbabilities = new float[2];
    public float GameTime { get; private set; }

    private void Awake()
    {
        GameFoundation.Initialize(); //Initialize Game Foundation
        if (Instance == null)
            Instance = this;
        else Destroy(this);

        currentMap = FindObjectOfType<Map>();
        print("GameManager Initializing");
        //Hey

    }
    private void Start()
    {
        print(currentMap != null ? $"Map found on {currentMap.gameObject.name}, starting {(!FEREnabled ? "non-" : "")}FER game" : "Map not found. Game cannot start");
        if (currentMap == null) { 
            enabled = false; 
            return; 
        }
       
        if(printData) printer = new DataPrinter();

        StartCoroutine(PrintGameDataPeriodic());

        hordeDelay /= ExperimentManager.Instance.DifficultyFactor;
        maxEnemyAtBU1 *= ExperimentManager.Instance.DifficultyFactor;
        maxEnemyAtBU3 *= ExperimentManager.Instance.DifficultyFactor;
        spawnRateAtBU1 *= ExperimentManager.Instance.DifficultyFactor;
        spawnRateAtBU3 *= ExperimentManager.Instance.DifficultyFactor;
        
        hordeTimer = hordeDelay;
        initHandgunAmmo = player.handgunScript.ammo + player.handgunScript.magazine;
        initRifleAmmo = player.autoGunScript.ammo + player.autoGunScript.magazine;
        totalInitAmmo = initHandgunAmmo + initRifleAmmo;
        CurrentState = defaultState;
        finalWall.SetActive(false);

        if(!FEREnabled)
        {
            detector.enabled = false;
            faceStatusImage.color = Color.green;
        }

        buildUpProbabilities[0] = buildUp1Probability;
        buildUpProbabilities[1] = buildUp2Probability;

        foreach (ObjectiveBase objective in objectives)
            objectiveQueue.Enqueue(objective);
        currentObjective = objectiveQueue.Dequeue();

    }

    private void Update()
    {
        GameTime += Time.deltaTime;
        ProcessInput();
        ProcessStates();
        CalculateStressLevel();
        UpdateUI();

        if(carePackageSettings.CurrentDelivery != null && carePackageSettings.CurrentDelivery.time <= GameTime)
        {
            //Deliver Package
            Vector3 spawnLoc = carePackageSettings.CurrentDelivery.deliveryLocation.position;
            spawnLoc.y += 100;
            Instantiate(carePackageSettings.packagePrefab, spawnLoc, carePackageSettings.CurrentDelivery.deliveryLocation.rotation);
            carePackageSettings.GetNextDelivery();
            Announce("A Care Package has been dropped!");
        }

        if (currentObjective is FinalObjective && !gameEnd)
        {
            CompleteObjective();
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
        CurrentStateTime += Time.deltaTime;
        switch(CurrentState)
        {
            case GameState.Relax:
                {
                    currentMap.spawnEnemy = false;
                    stateWeight = -5;
                    if(playerStressLevel <= 0)
                    {
                        CurrentStateTime = 0;
                        startingSpawnRate = currentMap.currentSpawnRate;
                        startingMaxEnemy = currentMap.currentMaxEnemy;
                        float rnd = UnityEngine.Random.Range(0, precision);
                        float p1 = buildUpProbabilities[0] * precision;
                        float p2 = buildUpProbabilities[1] * precision;
                        if (rnd < p1) CurrentState = GameState.BuildUp1;
                        else if (rnd >= p1 && rnd < p1 + p2) CurrentState = GameState.BuildUp2;
                        else CurrentState = GameState.BuildUp3;
                    }
                    break;

                }
            case GameState.BuildUp1:
                {
                    //buildUpProbabilities[0] -= probabilityChange;
                    //buildUpProbabilities[1] -= probabilityChange;
                    currentMap.spawnEnemy = true;
                    currentMap.currentSpawnRate = Mathf.Min(currentMap.currentSpawnRate + (spawnRateAtBU1 - startingSpawnRate) / timeToMax * rateOfSpawnRateChange.toMax.Evaluate(CurrentStateTime) * Time.deltaTime, spawnRateAtBU1);
                    currentMap.currentMaxEnemy = Mathf.Min(currentMap.currentMaxEnemy + (maxEnemyAtBU1 - startingMaxEnemy) / timeToMax * rateOfMaxEnemyChange.toMax.Evaluate(CurrentStateTime) * Time.deltaTime, maxEnemyAtBU1);
                    stateWeight = 3;
                    if (playerStressLevel >= 100)
                    {
                        CurrentStateTime = 0;
                        CurrentState = GameState.Peak;
                        PeakStateTimer = 10;
                    }
                    break;
                }
            case GameState.BuildUp2:
                {
                    //buildUpProbabilities[0] = buildUp1Probability;
                    //buildUpProbabilities[1] = buildUp2Probability;
                    currentMap.spawnEnemy = true;

                    if(currentMap.currentSpawnRate >= currentMap.spawnRate) 
                        currentMap.currentSpawnRate = Mathf.Max(currentMap.currentSpawnRate - (startingSpawnRate - spawnRateAtBU3) / timeToMin * rateOfSpawnRateChange.toMin.Evaluate(CurrentStateTime) * Time.deltaTime, currentMap.spawnRate);
                    else currentMap.currentSpawnRate = Mathf.Min(currentMap.currentSpawnRate + (spawnRateAtBU1 - startingSpawnRate) / timeToMax * rateOfSpawnRateChange.toMax.Evaluate(CurrentStateTime) * Time.deltaTime, currentMap.spawnRate);
                    
                    if (currentMap.currentMaxEnemy >= currentMap.maximumEnemy) 
                        currentMap.currentMaxEnemy = Mathf.Max(currentMap.currentMaxEnemy - (startingMaxEnemy - maxEnemyAtBU3) / timeToMin * rateOfMaxEnemyChange.toMin.Evaluate(CurrentStateTime) * Time.deltaTime, currentMap.maximumEnemy);
                    else currentMap.currentMaxEnemy = Mathf.Min(currentMap.currentMaxEnemy + (maxEnemyAtBU1 - startingMaxEnemy) / timeToMax * rateOfSpawnRateChange.toMax.Evaluate(CurrentStateTime) * Time.deltaTime, currentMap.maximumEnemy);
                    
                    stateWeight = 3;
                    if (playerStressLevel >= 50)
                    {
                        CurrentStateTime = 0;
                        CurrentState = GameState.Peak;
                        PeakStateTimer = 15;
                    }
                    break;
                }
            case GameState.BuildUp3:
                {
                    //buildUpProbabilities[0] += probabilityChange;
                    //buildUpProbabilities[1] += probabilityChange;
                    currentMap.spawnEnemy = true;
                    currentMap.currentSpawnRate = Mathf.Max(currentMap.currentSpawnRate - (startingSpawnRate - spawnRateAtBU3) / timeToMin * rateOfSpawnRateChange.toMin.Evaluate(CurrentStateTime) * Time.deltaTime, spawnRateAtBU3);
                    currentMap.currentMaxEnemy = Mathf.Max(currentMap.currentMaxEnemy - (startingMaxEnemy - maxEnemyAtBU3) / timeToMin * rateOfMaxEnemyChange.toMin.Evaluate(CurrentStateTime) * Time.deltaTime, maxEnemyAtBU3);
                    stateWeight = 3;
                    if (playerStressLevel >= 25)
                    {
                        CurrentStateTime = 0;
                        CurrentState = GameState.Peak;
                        PeakStateTimer = 7;
                    }
                    break;
                }
            case GameState.Peak:
                {
                    stateWeight = 0;
                    PeakStateTimer -= Time.deltaTime;
                    if (PeakStateTimer <= 0)
                    {
                        CurrentStateTime = 0;
                        CurrentState = GameState.Relax;
                    }
                    break;
                }
        }
    }

    private void CalculateStressLevel()
    {
        if (ExpressionManager.FaceResults != null)
        {
            ExpressionManager.FaceResults.Emotions.TryGetValue(Emotions.Joy, out joy);
            ExpressionManager.FaceResults.Emotions.TryGetValue(Emotions.Anger, out anger);
            ExpressionManager.FaceResults.Emotions.TryGetValue(Emotions.Fear, out fear);
            ExpressionManager.FaceResults.Emotions.TryGetValue(Emotions.Disgust, out disgust);
            ExpressionManager.FaceResults.Emotions.TryGetValue(Emotions.Sadness, out sadness);
            ExpressionManager.FaceResults.Emotions.TryGetValue(Emotions.Surprise, out surprise);
            ExpressionManager.FaceResults.Emotions.TryGetValue(Emotions.Valence, out valence);
            ExpressionManager.FaceResults.Emotions.TryGetValue(Emotions.Contempt, out contempt);
            ExpressionManager.FaceResults.Emotions.TryGetValue(Emotions.Engagement, out engagement);
        }

        float varHPWeight = 1;
        float varAmmoWeight = 1;
        float maxHp = player.playerItem.GetStatFloat(DEF_HEALTH);

        varHP = ((maxHp - player.CurrentHealth) / (maxHp / 2) - 1) * varHPWeight;
        varAmmo = ((initRifleAmmo + initHandgunAmmo - (player.autoGunScript.CurrentAmmo + player.autoGunScript.CurrentMagazine) - (player.handgunScript.CurrentAmmo + player.handgunScript.CurrentMagazine)) / (totalInitAmmo / 2) - 1) * varAmmoWeight;//player.autoGunScript
        if (FEREnabled)
        {
            float varJoy = joyCurve.Evaluate(joy);
            float varAnger = angerCurve.Evaluate(anger);
            float varFear = fearCurve.Evaluate(fear);
            float varSurprise = surpriseCurve.Evaluate(surprise);
            float varDisgust = disgustCurve.Evaluate(disgust);
            if (varHP + varAmmo >= 0) varFER = varAnger * varFear * varSurprise * varDisgust;
            else varFER = varJoy;
        }
        else varFER = 1;
        stressRate = (varHP + varAmmo) * varFER + stateWeight;
        //print(stressRate);
        playerStressLevel = Mathf.Clamp(playerStressLevel + stressRate * Time.deltaTime, 0, 100);
    }

    private void ProcessInput()
    {

    }

    private void UpdateUI()
    {
        objectiveDebugText.text = currentObjective.ObjectiveText;
        objectiveText.text = currentObjective.ObjectiveText;

        //Timer
        if (currentObjective is FinalObjective)
        {
            FinalObjective objective = currentObjective as FinalObjective;
            float minutes = Mathf.Floor(objective.RemainingTime / 60);
            float seconds = Mathf.Floor(objective.RemainingTime % 60);
            int milliseconds = Mathf.RoundToInt((objective.RemainingTime - (minutes * 60) - Mathf.Floor(seconds)) * 100) % 100;
        
            timerDebugText.gameObject.SetActive(true);
            timerText.gameObject.SetActive(true);
            timerDebugText.text = string.Format("{0:N0}:{1,2:00}:<size=40>{2,2:00}</size>", minutes, seconds, milliseconds);
            timerText.text = string.Format("{0:N0}:{1,2:00}:<size=40>{2,2:00}</size>", minutes, seconds, milliseconds);
        }
        else
        {
            timerDebugText.gameObject.SetActive(false);
            timerText.gameObject.SetActive(false);
        }

        //Consumables
        if (GrenadeAvailable) grenadeIcon.Enable();
        else grenadeIcon.Disable();
        if (BigPotionAvailable) bigPotionIcon.Enable();
        else bigPotionIcon.Disable();
        if (SmallPotionAvailable) smallPotionIcon.Enable();
        else smallPotionIcon.Disable();
        if (MedkitAvailable) medkitIcon.Enable();
        else medkitIcon.Disable();

        //Debug
        if (debugMode)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine($"Enemies: {currentMap.EnemyCount}");
            sb.AppendLine($"- Average Enemy HP: {currentMap.AverageEnemyHP.ToString("n1")}");
            sb.AppendLine($"- Average Distance: {currentMap.AverageEnemyDistance.ToString("n1")}m");
            sb.AppendLine($"  - Max: {currentMap.MaxEnemyDistance.ToString("n1")}m");
            sb.AppendLine($"  - Min: {currentMap.MinEnemyDistance.ToString("n1")}m");
            sb.AppendLine($"Current State: {(CurrentState != GameState.Peak ? CurrentState.ToString() : $"{CurrentState}, {PeakStateTimer.ToString("n0")}s left")}, Spawnrate: {currentMap.currentSpawnRate.ToString("n3")} e/s, Max: {currentMap.currentMaxEnemy.ToString("n0")}");
            sb.AppendLine($"Current Objective: {currentObjective.ObjectiveName}");
            sb.AppendLine();
            sb.AppendLine(!IsHordeMode ? $"Next horde in {Mathf.FloorToInt(hordeTimer)}s" : $"Horde is coming, ending in {hordeTime - Mathf.FloorToInt(Mathf.Abs(hordeTimer))}s");
            sb.AppendLine($"Stress: {playerStressLevel.ToString("n2")}%, varHP {varHP.ToString("n2")}, varAmmo {varAmmo.ToString("n2")}, varFER {varFER.ToString("n2")}, Stressrate {stressRate.ToString("n2")}");
            sb.AppendLine();
            List<string> powerUps = new List<string>();
            if (BigPotionAvailable) powerUps.Add("Big Potion");
            if (SmallPotionAvailable) powerUps.Add("Small Potion");
            if (GrenadeAvailable) powerUps.Add("Grenade");
            sb.AppendLine($"Power-ups Available: { (powerUps.Count > 0 ? string.Join(", ", powerUps) : "(none)") }");
            sb.AppendLine($"Enemies killed: {enemyDefeated}");
            sb.AppendLine($"Current Score: {scoreConfiguration.GetTotalScore().ToString("n0")}");
            
            sb.AppendLine();
            if(FEREnabled)
            {
                sb.AppendLine($"Face {(ExpressionManager.FaceFound ? "detected" : "not detected")}");
                if (ExpressionManager.FaceFound)
                {
                    sb.AppendLine($"Joy: { joy.ToString("n2")}, Anger: { anger.ToString("n2")}, Fear: { fear.ToString("n2")}, Disgust: {disgust.ToString("n2")}, Engagement: {engagement.ToString("n2")}");
                    sb.AppendLine($"Sadness: {sadness.ToString("n2")}, Surprise: { surprise.ToString("n2")}, Valence: { valence.ToString("n2")}, Contempt: {contempt.ToString("n2")}");
                }
                sb.AppendLine($"varJoy: {joyCurve.Evaluate(joy).ToString("n2")}, varAnger: { angerCurve.Evaluate(anger).ToString("n2")}, varFear: { fearCurve.Evaluate(fear).ToString("n2")}, varSurprise: {surpriseCurve.Evaluate(surprise).ToString("n2")} , varDisgust: {disgustCurve.Evaluate(disgust).ToString("n2")}");
            }
            else sb.AppendLine($"FER Disabled");

            debugText.text = sb.ToString();
        }

        if(FEREnabled)
        {
            if (ExpressionManager.FaceFound) faceStatusImage.color = Color.green;
            else faceStatusImage.color = Color.red;
        }
    }

    internal void CompleteObjective(params object[] data)
    {
        if (currentObjective.GetObjectiveCompletion(data))
        {
            if(!IsInPart2)
                IsInPart2 = currentObjective.GotoNextPartUponCompletion;

            scoreConfiguration.AddEntry("Objectives", currentObjective.ObjectiveName, currentObjective.ScoreReward);

            if(currentObjective is FindObjectObjective)
            {
                foreach (GameObject obj in dividers) Destroy(obj);
                pt1Surface.enabled = false;
                pt2Surface.enabled = true;
                SoundManager.PlaySound(dividerSource, SoundManager.SoundType.DoorRumble);
            }

            if (objectiveQueue.Count > 0)
            {
                currentObjective = objectiveQueue.Dequeue();
                currentObjective.Prepare();
                if (currentObjective is FinalObjective) StartBoss();
            }
            else EndGame(true);
        }
    }

    internal void EnemyDie(Enemy e)
    {
        if (e.type == EnemyType.Boss) bossDefeated++;
        else if (e.type == EnemyType.Mob) enemyDefeated++;
        scoreConfiguration.AddEnemyKillEntry(e.type);
        currentMap.EnemyDie(e);
    }

    internal void PlayerDie()
    {
        if(enabled)
        {
            EndGame();
        }
    }

    internal void EndGame(bool victory = false)
    {
        OnGameEnd.Invoke();
        gameEnd = true;
        currentMap.spawnEnemy = false;
        Cursor.lockState = CursorLockMode.None;
        player.autoGunScript.enabled = false;
        player.handgunScript.enabled = false;
        player.controller.enabled = false;
        enabled = false;
        if (printData) printer.NextFile();
        scoreConfiguration.FinalizeScore(victory, player.RifleTotalAmmo, player.HandgunTotalAmmo, BigPotionAvailable, SmallPotionAvailable, MedkitAvailable, GrenadeAvailable);
        GameOverManager.ShowPanel(victory, 
            scoreConfiguration.GetAllScores(), 
            scoreConfiguration.GetPerCategoryScore, 
            GetScoresInCategory
        );
    }

    internal List<(string name, float value)> GetScoresInCategory(string category)
        => scoreConfiguration.GetScoresInCategory(category);

    internal void Announce(string text)
    {
        TextMeshProUGUI textMesh = Instantiate(logObject, logPanel).GetComponent<TextMeshProUGUI>();
        textMesh.text = text;
        TextMeshProUGUI textMesh2 = Instantiate(logObject, logPanelDebug).GetComponent<TextMeshProUGUI>();
        textMesh2.text = text;
    }

    internal void StartBoss()
    {
        if (CurrentState != GameState.Final)
        {
            CurrentState = GameState.Final;
            currentMap.spawnEnemy = true;
            finalWall.SetActive(true);
        }
    }

    internal bool GetItem(ChestType item)
    {
        if(item == ChestType.BigPotion && !BigPotionAvailable)
        {
            Announce($"Big Potion Added!");
            BigPotionAvailable = true;
            return true;
        }
        else if (item == ChestType.SmallPotion && !SmallPotionAvailable)
        {
            Announce($"Small Potion Added!");
            SmallPotionAvailable = true;
            return true;
        }
        else if (item == ChestType.Grenade && !GrenadeAvailable)
        {
            Announce($"Grenade Added!");
            GrenadeAvailable = true;
            return true;
        }
        else if (item == ChestType.Medkit && !MedkitAvailable)
        {
            Announce($"Medikit Added!");
            MedkitAvailable = true;
            return true;
        }
        else return false;
    }

    internal void UseItem(ChestType item)
    {
        if (item == ChestType.BigPotion)
        {
            BigPotionAvailable = false;
            bigPotionIcon.EnableBar();
        }
        else if (item == ChestType.SmallPotion)
        {
            SmallPotionAvailable = false;
            smallPotionIcon.EnableBar();
        }
        else if (item == ChestType.Grenade)
        {
            GrenadeAvailable = false;
        }
        else if (item == ChestType.Medkit)
        {
            MedkitAvailable = false;
        }
    }

    public void SetItemBarValue(ChestType item, float value)
    {
        if (item == ChestType.BigPotion) bigPotionIcon.SetBarValue(value);
        else if (item == ChestType.SmallPotion) smallPotionIcon.SetBarValue(value);
    }

    public void SetItemBarVisibility(ChestType item, bool visible)
    {
        if (item == ChestType.BigPotion)
        {
            if (visible) bigPotionIcon.EnableBar(); 
            else bigPotionIcon.DisableBar();
        }
        else if (item == ChestType.SmallPotion)
        {
            if (visible) smallPotionIcon.EnableBar();
            else smallPotionIcon.DisableBar();
        }
    }

    private IEnumerator PrintGameDataPeriodic()
    {
        while(!gameEnd)
        {
            yield return new WaitForSeconds(printInterval);
            Print();
        }

        Print();
    }

    private void Print()
    {
        PrintData data = new PrintData()
        {
            angerVal = anger,
            bigPotionAvailable = BigPotionAvailable,
            contemptVal = contempt,
            currentState = CurrentState,
            disgustVal = disgust,
            distance = new Range(currentMap.AverageEnemyDistance, currentMap.MaxEnemyDistance, currentMap.MinEnemyDistance),
            enemiesKilled = enemyDefeated,
            enemyCount = currentMap.EnemyCount,
            engagementVal = engagement,
            faceDetected = ExpressionManager.FaceFound,
            fearVal = fear,
            FEREnabled = FEREnabled,
            grenadeAvailable = GrenadeAvailable,
            health = player.CurrentHealth,
            hordeMode = hordeTimer < 1,
            hordeTimer = Mathf.Abs(hordeTimer),
            joyVal = joy,
            maxEnemies = Mathf.RoundToInt(currentMap.currentMaxEnemy),
            peakTimer = PeakStateTimer,
            primaryAmmo = player.autoGunScript.CurrentAmmo,
            primaryClip = player.autoGunScript.CurrentMagazine,
            sadnessVal = sadness,
            score = scoreConfiguration.GetTotalScore(),
            secondaryAmmo = player.handgunScript.CurrentAmmo,
            secondaryClip = player.handgunScript.CurrentMagazine,
            smallPotionAvailable = SmallPotionAvailable,
            spawnRate = currentMap.currentSpawnRate,
            stressLevel = playerStressLevel,
            stressRate = stressRate,
            surpriseVal = surprise,
            valenceVal = valence,
            varAmmo = varAmmo,
            varFER = varFER,
            varHP = varHP
        };

        if (ExperimentManager.Instance.LockGame) ExperimentManager.Instance.SendExperimentData(data);

        if(printData) printer.Print(printFormat, data);
    }

}

[Serializable]
public struct RateOfChange
{
    public AnimationCurve toMax, toMin;
}