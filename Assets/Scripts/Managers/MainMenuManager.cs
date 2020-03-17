using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] CanvasGroup mainUI;
    [SerializeField] CanvasGroup howToPlayUI, creditsUI;
    [SerializeField] Image fader;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(InitialFade());
    }

    // Update is called once per frame
    public void OnPlay()
    {
        StartCoroutine(FadeToMenu(false));
    }

    public void OnCredits()
    {
        StartCoroutine(FadeToMenu(true));
    }

    public void OnActualStart()
    {
        StartCoroutine(FadeOut());
    }

    public void OnCreditsExit()
    {
        StartCoroutine(FadeToMainUI(true));
    }

    public void OnExit()
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

    IEnumerator FadeToMenu(bool creditsMenu)
    {
        if(creditsMenu)
        {
            mainUI.blocksRaycasts = false;
            while (mainUI.alpha > 0)
            {
                mainUI.alpha -= Time.deltaTime;
                creditsUI.alpha += Time.deltaTime;
                yield return null;
            }
            creditsUI.blocksRaycasts = true;
        }
        else
        {
            mainUI.blocksRaycasts = false;
            while(mainUI.alpha > 0)
            {
                mainUI.alpha -= Time.deltaTime;
                howToPlayUI.alpha += Time.deltaTime;
                yield return null;
            }
            howToPlayUI.blocksRaycasts = true;
        }
    }

    IEnumerator FadeToMainUI(bool creditsMenu)
    {
        if (creditsMenu)
        {
            creditsUI.blocksRaycasts = false;
            while (creditsUI.alpha > 0)
            {
                mainUI.alpha += Time.deltaTime;
                creditsUI.alpha -= Time.deltaTime;
                yield return null;
            }
            mainUI.blocksRaycasts = true;
        }
        else
        {
            howToPlayUI.blocksRaycasts = false;
            while (howToPlayUI.alpha > 0)
            {
                mainUI.alpha += Time.deltaTime;
                howToPlayUI.alpha -= Time.deltaTime;
                yield return null;
            }
            mainUI.blocksRaycasts = true;
        }
    }
}
