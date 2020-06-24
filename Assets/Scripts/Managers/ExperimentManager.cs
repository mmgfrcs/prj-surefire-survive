using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ExperimentManager : MonoBehaviour
{
    internal static string SubjectName { get; private set; }
    [Header("Server"), SerializeField] string serverAddress;
    [SerializeField] string researchID, accessToken, secret;
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
        if (Application.platform != RuntimePlatform.WindowsEditor)
            CurrentAttempts = PlayerPrefs.GetInt("attempts", 0);
        else CurrentAttempts = 0;

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
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("Config Set: ");
        sb.AppendLine("Attempts: " + attempts);
        sb.AppendLine("FER: " + FEREnabled);
        sb.AppendLine("Final Stage Time: " + finalStageTime);
        sb.AppendLine("Difficulty Factor: " + difficultyFactor);
        sb.AppendLine("Lock Game: " + lockGame);
        sb.AppendLine("Lock FER: " + lockFER);
        sb.AppendLine("Override: " + overrideClient);
        Debug.Log("[ExperimentManager] " + sb.ToString());
        enabled = false;
    }

    public void AddAttempt()
    {
        PlayerPrefs.SetInt("attempts", ++CurrentAttempts);
        if (Application.platform != RuntimePlatform.WindowsEditor)
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

    internal void SendExperimentData(PrintData data)
    {
        StartCoroutine(SendData(data));
    }

    IEnumerator SendData(PrintData data)
    {
        data.subjectName = SubjectName;

        UnityWebRequest webRequest = new UnityWebRequest(
            $"{serverAddress}/api/research/{researchID}/data",
            "POST",
            new DownloadHandlerBuffer(),
            new UploadHandlerRaw(Encoding.UTF8.GetBytes(JsonUtility.ToJson(new SentData(accessToken, data))))
        );
        webRequest.SetRequestHeader("Content-Type", "application/json");
        webRequest.SetRequestHeader("Authorization", $"Bearer {secret}");

        yield return webRequest.SendWebRequest();

        if (webRequest.isNetworkError || webRequest.isHttpError)
        {
            Debug.LogWarning($"[ExperimentManager] Error Connecting to {serverAddress}: {(webRequest.error != null ? webRequest.error : webRequest.responseCode.ToString())}");
        }
    }

}
[System.Serializable]
struct SentData
{
    public string token;
    public PrintData data;

    public SentData(string token, PrintData data)
    {
        this.token = token;
        this.data = data;
    }
}