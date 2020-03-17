using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConsumableIcon : MonoBehaviour
{
    public UnityEngine.UI.Image icon;
    public GameObject hotkeyText;
    public GameObject durationBarParent;
    public UnityEngine.UI.Image durationBar;

    public void SetBarValue(float value)
    {
        durationBar.fillAmount = value;
    }

    public void DisableBar()
    {
        durationBarParent.SetActive(false);
    }

    public void EnableBar()
    {
        durationBarParent.SetActive(true);
    }

    public void Disable()
    {
        icon.color = new Color(0.8f, 0.8f, 0.8f, 0.4f);
        hotkeyText.SetActive(false);
    }

    public void Enable()
    {
        icon.color = Color.white;
        hotkeyText.SetActive(true);
    }
}
