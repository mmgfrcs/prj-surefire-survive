using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Audio;

public enum Menus
{
    MainMenu, HowToPlay, Credits, Options
}

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] bool FEREnabled = true;
    [SerializeField] AudioMixer mixer;
    [SerializeField] CanvasGroup mainUI, howToPlayUI, creditsUI, optionsUI;
    [SerializeField] Image fader;
    [SerializeField] TextMeshProUGUI versionText;

    //Controls for setting default values
    [SerializeField] Slider BGMVolumeSlider, SFXVolumeSlider;
    [SerializeField] Toggle FERToggle;

    Menus currentMenu = Menus.MainMenu;
    void Start()
    {
        //Setup Scene
        //Activate Screens
        mainUI.gameObject.SetActive(true);
        howToPlayUI.gameObject.SetActive(true);
        creditsUI.gameObject.SetActive(true);
        optionsUI.gameObject.SetActive(true);

        //Setup alpha
        mainUI.alpha = 1;
        howToPlayUI.alpha = 0;
        creditsUI.alpha = 0;
        optionsUI.alpha = 0;

        //Get values for Options
        BGMVolumeSlider.value = PlayerPrefs.GetFloat("BGM", DbToLinear(-3));
        SFXVolumeSlider.value = PlayerPrefs.GetFloat("SFX", DbToLinear(0));
        FERToggle.isOn = PlayerPrefs.GetInt("FER", FEREnabled ? 1 : 0) == 1;

        //Apply values
        mixer.SetFloat("bgmVol", LinearToDb(BGMVolumeSlider.value));
        mixer.SetFloat("sfxVol", LinearToDb(SFXVolumeSlider.value));
        FEREnabled = FERToggle.isOn;

        StartCoroutine(InitialFade());
        versionText.text = $"Version {Application.version}";
    }

    // Update is called once per frame
    public void OnPlay()
    {
        StartCoroutine(FadeToMenu(Menus.HowToPlay));
    }

    public void OnOptions()
    {
        StartCoroutine(FadeToMenu(Menus.Options));
    }

    public void OnCredits()
    {
        StartCoroutine(FadeToMenu(Menus.Credits));
    }

    public void SaveOptions()
    {
        PlayerPrefs.SetFloat("BGM", BGMVolumeSlider.value);
        PlayerPrefs.SetFloat("SFX", SFXVolumeSlider.value);
        PlayerPrefs.SetInt("FER", FERToggle.isOn ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void OnMenuExit()
    {
        StartCoroutine(FadeToMenu(Menus.MainMenu));
    }

    public void OnActualStart()
    {
        StartCoroutine(FadeOut());
    }

    public void OnGameExit()
    {
        Application.Quit();
    }

    IEnumerator InitialFade()
    {
        fader.CrossFadeAlpha(0, 2, false);
        yield return new WaitForSeconds(2f);
        fader.gameObject.SetActive(false);
        while (mainUI.alpha < 1)
        {
            mainUI.alpha += Time.deltaTime / 2;
            yield return null;
        }
    }

    IEnumerator FadeOut()
    {
        fader.gameObject.SetActive(true);
        fader.canvasRenderer.SetAlpha(0f);
        fader.CrossFadeAlpha(1, 2, false);
        yield return new WaitForSeconds(2);
        SceneManager.LoadScene("Game");
    }

    IEnumerator FadeToMenu(Menus to)
    {
        CanvasGroup source, dest;

        //Figure out source and destination Menu
        if (currentMenu == Menus.HowToPlay) source = howToPlayUI;
        else if (currentMenu == Menus.Credits) source = creditsUI;
        else if (currentMenu == Menus.Options) source = optionsUI;
        else source = mainUI;
        if (to == Menus.HowToPlay) dest = howToPlayUI;
        else if (to == Menus.Credits) dest = creditsUI;
        else if (to == Menus.Options) dest = optionsUI;
        else dest = mainUI;

        //Start fading menu
        source.blocksRaycasts = false;
        while (source.alpha > 0)
        {
            source.alpha -= Time.deltaTime;
            dest.alpha += Time.deltaTime;
            yield return null;
        }
        dest.blocksRaycasts = true;
        currentMenu = to;
    }

    public void OnBGMVolumeChange(float value)
    {
        mixer.SetFloat("bgmVol", LinearToDb(value));
    }

    public void OnSFXVolumeChange(float value)
    {
        mixer.SetFloat("sfxVol", LinearToDb(value));
    }

    public void OnToggleFER(bool active)
    {
        FEREnabled = active;
    }

    float LinearToDb(float val)
    {
        if (val != 0)
            return 20.0f * Mathf.Log10(val);
        else
            return -144.0f;
    }

    float DbToLinear(float val)
    {
        return Mathf.Pow(10.0f, val / 20.0f);
    }
}
