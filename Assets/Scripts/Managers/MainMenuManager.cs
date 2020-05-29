using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum Menus
{
    MainMenu, HowToPlay, Credits, Options
}

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] bool FEREnabled = true;
    [SerializeField] CanvasGroup mainUI, howToPlayUI, creditsUI, optionsUI;
    [SerializeField] Image fader;
    [SerializeField] TextMeshProUGUI versionText;
    // Start is called before the first frame update

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
}
