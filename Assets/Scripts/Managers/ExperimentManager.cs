using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ExperimentManager : MonoBehaviour
{
    internal static string SubjectName { get; private set; }
    [Header("Server"), SerializeField] string serverAddress;
    [SerializeField] string accessToken, secret;
    [Header("Experiment Constraints"), SerializeField] int attempts = 3;
    [SerializeField] bool FEREnabled = true;
    [SerializeField] float finalStageTime = 300;
    [SerializeField] float difficultyFactor = 1;
    [SerializeField] bool lockGame = true;
    [SerializeField] bool lockFER = true, overrideClient = true;

    public static ExperimentManager Instance;
    public int Attempts { get => attempts; }
    public bool IsFEREnabled { get => FEREnabled; }
    public float FinalStageTime { get => finalStageTime; }
    public float DifficultyFactor { get => difficultyFactor; }
    public bool LockGame { get => lockGame; }
    public bool LockFER { get => lockFER; }
    public bool OverrideClient { get => overrideClient; }

    public int CurrentAttempts { get; private set; }
    public bool IsPlayable { get { return !lockGame || CurrentAttempts < attempts; } }

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
        if (Instance == null) Instance = this;
        else Destroy(this);

        CurrentAttempts = PlayerPrefs.GetInt("attempts", 0);

        if (ConfigurationManager.Instance.IsConfigReady) SetValuesFromConfig();
    }

    private void Update()
    {
        if (ConfigurationManager.Instance.IsConfigReady) SetValuesFromConfig();
    }

    void SetValuesFromConfig()
    {
        attempts = ConfigurationManager.Instance.CurrentConfig.GetInt("attempts", 3);
        FEREnabled = ConfigurationManager.Instance.CurrentConfig.GetBool("ferEnabled", true);
        finalStageTime = ConfigurationManager.Instance.CurrentConfig.GetFloat("finalStageTime", 300);
        difficultyFactor = ConfigurationManager.Instance.CurrentConfig.GetFloat("difficultyFactor", 1);
        lockGame = ConfigurationManager.Instance.CurrentConfig.GetBool("lockGame", true);
        lockFER = ConfigurationManager.Instance.CurrentConfig.GetBool("lockFER", true);
        overrideClient = ConfigurationManager.Instance.CurrentConfig.GetBool("override", true);
        enabled = false;
    }

    public void AddAttempt()
    {
        PlayerPrefs.SetInt("attempts", ++CurrentAttempts);
        PlayerPrefs.Save();
    }

    public void OnContinue(TMP_InputField input)
    {
        SubjectName = input.text;
        if (SubjectName.Length < 4) input.textComponent.color = Color.red;
        else
        {
            input.interactable = false;
            input.textComponent.color = Color.white;
            StartCoroutine(LoadNextScene());
        }
    }

    public void OnExit()
    {
        Application.Quit();
    }

    IEnumerator LoadNextScene()
    {
        yield return SceneManager.LoadSceneAsync("Main Menu");
    }
}
