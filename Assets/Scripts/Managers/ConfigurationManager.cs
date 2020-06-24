using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.RemoteConfig;
using System;
using System.Text;

public class ConfigurationManager : MonoBehaviour
{
    public RuntimeConfig CurrentConfig { get; private set; }
    public bool IsConfigReady { get; private set; }
    public static ConfigurationManager Instance;
    private struct UserConfig
    {

    }

    private struct AppConfig
    {

    }

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
        if (Instance == null) Instance = this;
        else Destroy(this);

        ConfigManager.FetchCompleted += ConfigManager_FetchCompleted;
        ConfigManager.FetchConfigs(new UserConfig(), new AppConfig());
    }

    private void ConfigManager_FetchCompleted(ConfigResponse obj)
    {

        if(obj.requestOrigin == ConfigOrigin.Remote || obj.requestOrigin == ConfigOrigin.Cached)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(obj.requestOrigin + " Config Retrieved: ");
            sb.AppendLine("Attempts: " + ConfigManager.appConfig.GetInt("attempts"));
            sb.AppendLine("FER: " + ConfigManager.appConfig.GetBool("ferEnabled"));
            sb.AppendLine("Final Stage Time: " + ConfigManager.appConfig.GetFloat("finalStageTime"));
            sb.AppendLine("Difficulty Factor: " + ConfigManager.appConfig.GetFloat("difficultyFactor"));
            sb.AppendLine("Lock Game: " + ConfigManager.appConfig.GetBool("lockGame"));
            sb.AppendLine("Lock FER: " + ConfigManager.appConfig.GetBool("lockFER"));
            sb.AppendLine("Override: " + ConfigManager.appConfig.GetBool("override"));
            Debug.Log("[ConfigurationManager] " + sb.ToString());
            CurrentConfig = ConfigManager.appConfig;
            IsConfigReady = true;
        }
        else
        {
            Debug.Log("Config Not Retrieved");
        }
    }
}
