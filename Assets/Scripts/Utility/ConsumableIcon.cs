using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConsumableIcon : MonoBehaviour
{
    public UnityEngine.UI.Image icon;
    public GameObject hotkeyText;

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
