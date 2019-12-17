using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LogFade : MonoBehaviour
{
    public float fadeDelay = 3, fadeTime = 2;
    TextMeshProUGUI text;
    void Start()
    {
        text = GetComponent<TextMeshProUGUI>();
        StartCoroutine(FadeAndKill());
    }

    private IEnumerator FadeAndKill()
    {
        yield return new WaitForSeconds(fadeDelay);
        text.CrossFadeAlpha(0, fadeTime, false);
        yield return new WaitForSeconds(fadeTime);
        Destroy(gameObject);
    }

}
