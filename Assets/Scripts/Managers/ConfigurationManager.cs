using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.RemoteConfig;
using System;

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
            Debug.Log(obj.requestOrigin + " Config Retrieved");
            CurrentConfig = ConfigManager.appConfig;
        }
        else
        {
            Debug.Log("Config Not Retrieved");
        }
    }
}
