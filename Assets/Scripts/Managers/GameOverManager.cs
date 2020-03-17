using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameOverManager : MonoBehaviour
{
    [SerializeField, Header("General")] float animationTime = 2;
    [SerializeField] string victoryString = "Congratulations!";
    [SerializeField] string defeatString = "Game Over";
    [SerializeField, Header("UI")] TextMeshProUGUI titleText;
    [SerializeField] TextMeshProUGUI score1AmountText;
    [SerializeField] TextMeshProUGUI score2AmountText;
    [SerializeField] TextMeshProUGUI score3AmountText;
    [SerializeField] TextMeshProUGUI totalScoreText;
    [SerializeField] GameObject panel;

    static GameOverManager instance;

    // Start is called before the first frame update
    private void Start()
    {
        if (instance == null) instance = this;
        else Destroy(this);

        panel.SetActive(false);
    }

    public static void ShowPanel(bool victory, float score1, float score2, float score3)
    {
        if(!instance.panel.activeInHierarchy)
        {
            if (victory) instance.titleText.text = instance.victoryString;
            else instance.titleText.text = instance.defeatString;
            instance.StartCoroutine(instance.AnimateNumbers(score1, score2, score3));
            instance.panel.SetActive(true);
        }

    }

    public void OnBack()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Main Menu");
        GameManager.Instance = null;
        instance = null;
        
    }

    IEnumerator AnimateNumbers(float score1, float score2, float score3)
    {
        float s1 = 0, s2 = 0, s3 = 0;
        score1AmountText.text = "0";
        score2AmountText.text = "0";
        score3AmountText.text = "0";
        yield return new WaitForSeconds(0.5f);
        while (true)
        {
            s1 += score1 * Time.deltaTime / animationTime;
            s2 += score2 * Time.deltaTime / animationTime;
            s3 += score3 * Time.deltaTime / animationTime;
            score1AmountText.text = s1.ToString("n0");
            score2AmountText.text = s2.ToString("n0");
            score3AmountText.text = s3.ToString("n0");
            totalScoreText.text = (s1 + s2 + s3).ToString("n0");
            if(s1 >= score1)
            {
                score1AmountText.text = score1.ToString("n0");
                score2AmountText.text = score2.ToString("n0");
                score3AmountText.text = score3.ToString("n0");
                totalScoreText.text = (score1 + score2 + score3).ToString("n0");
                break;
            }
            yield return null;
        }
    }
}
