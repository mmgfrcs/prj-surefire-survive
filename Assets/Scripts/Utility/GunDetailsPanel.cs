using UnityEngine.UI;
using UnityEngine;

public class GunDetailsPanel : MonoBehaviour {
    public Color panelColor = new Color(131f / 255, 131f / 255, 131f / 255);
    public Text gunRemaining, clipRemaining;
    public Slider gunSlider;

    Transform fArea;
    Image fill, back;
    private void Start()
    {
        fArea = gunSlider.transform.Find("Fill Area");
        fill = fArea.Find("Fill").GetComponent<Image>();
        back = fArea.parent.Find("Background").GetComponent<Image>();
        SetColor(panelColor);
    }
    public void SetColor(Color color)
    {
        panelColor = color;

        gunRemaining.color = clipRemaining.color = fill.color = fArea.parent.parent.Find("GunIcon").GetComponent<Image>().color = panelColor;

        back.color = new Color(color.r / 5, color.g / 5, color.b / 5, 0.25f);
    }
}
