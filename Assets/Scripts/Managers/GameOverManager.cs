using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class GameOverManager : MonoBehaviour
{
    [SerializeField, Header("General")] float animationTime = 2;
    [SerializeField] string victoryString = "Congratulations!";
    [SerializeField] string defeatString = "Game Over";
    [SerializeField] float waitTime = 5;
    [SerializeField, Header("UI")] TextMeshProUGUI titleText;
    [SerializeField] GameOverPanelText[] scoreAmountTexts;
    [SerializeField] TextMeshProUGUI totalScoreText;
    [SerializeField] GameObject panel;
    [SerializeField] GameObject backButton;

    List<(string category, string name, float score)> scoreData;

    static GameOverManager instance;

    // Start is called before the first frame update
    private void Start()
    {
        if (instance == null) instance = this;
        else Destroy(this);

        panel.SetActive(false);
    }

    public static void ShowPanel(bool victory, List<(string category, string name, float score)> scores, Func<List<(string, string, float)>, List<(string, float)>> categoryOverviewFunc, Func<string, List<(string, float)>> perCategoryFunc)
    {
        if(!instance.panel.activeInHierarchy)
        {
            if (victory) instance.titleText.text = instance.victoryString;
            else instance.titleText.text = instance.defeatString;
            instance.scoreData = scores;
            instance.StartCoroutine(instance.AnimateNumbers(categoryOverviewFunc(scores)));
            instance.StartCoroutine(instance.BackButtonActivate());
            instance.panel.SetActive(true);
            instance.backButton.SetActive(false);
        }

    }

    public void OnBack()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Main Menu");
        GameManager.Instance = null;
        instance = null;
        
    }

    IEnumerator AnimateNumbers(List<(string category, float score)> scores)
    {

        float[] scoreArr = new float[scores.Count];
        for (int i = 0; i < Mathf.Min(scoreAmountTexts.Length, scores.Count); i++)
        {
            scoreAmountTexts[i].nameText.transform.parent.gameObject.SetActive(true);
            scoreAmountTexts[i].nameText.text = scores[i].category;
            scoreAmountTexts[i].valueText.text = "0";
        }
        for (int i = (scoreAmountTexts.Length > scores.Count ? scores.Count : scoreAmountTexts.Length); i < (scoreAmountTexts.Length > scores.Count ? scoreAmountTexts.Length : scores.Count); i++)
        {
            scoreAmountTexts[i].nameText.transform.parent.gameObject.SetActive(false);
        }

        yield return new WaitForSeconds(0.5f);
        while (true)
        {
            for (int i = 0; i < Mathf.Min(scoreAmountTexts.Length, scores.Count); i++)
            {
                scoreArr[i] += scores[i].score * Time.deltaTime / animationTime;
                scoreAmountTexts[i].valueText.text = scoreArr[i].ToString("n0");
            }

            totalScoreText.text = scoreArr.Sum().ToString("n0");
            
            if(scoreArr[0] > scores[0].score)
            {
                for (int i = 0; i < Mathf.Min(scoreAmountTexts.Length, scores.Count); i++)
                    scoreAmountTexts[i].valueText.text = scores[i].score.ToString("n0");

                totalScoreText.text = scores.Sum(x=> x.score).ToString("n0");
                break;
            }

            yield return null;
        }
    }

    IEnumerator BackButtonActivate()
    {
        yield return new WaitForSeconds(waitTime);
        backButton.SetActive(true);
    }
}

[Serializable]
public struct GameOverPanelText
{
    public TextMeshProUGUI nameText, valueText;
}