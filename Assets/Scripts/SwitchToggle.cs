using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SwitchToggle : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public Toggle toggle;
    public RectTransform handle;

    public float onX = 10f;
    public float offX = -10f;

    public Image background;
    public Color offColor = Color.gray;
    public Color onColor = Color.yellow;

    public GameObject intensidadeECor;

    public TextMeshProUGUI label;
    public float onLabelX = -10f;
    public float offLabelX = 10f;
    
    void Start()
    {
        toggle.onValueChanged.AddListener(UpdateSwitch);
        UpdateSwitch(toggle.isOn);
    }

    public void UpdateSwitch(bool isOn)
    {
        if (handle != null)
        {
            handle.anchoredPosition = new Vector2(
                isOn ? onX : offX,
                handle.anchoredPosition.y
            );
        }

        // Muda cor do fundo
        if (background != null)
        {
            background.color = isOn ? onColor : offColor;
        }

        // MOSTRA / OCULTA só se existir
        if (intensidadeECor != null)
        {
            intensidadeECor.SetActive(isOn);
        }
        
        if (label != null)
        {
            label.text = isOn ? "ON" : "OFF";

            label.rectTransform.anchoredPosition = new Vector2(
                isOn ? onLabelX : offLabelX,
                label.rectTransform.anchoredPosition.y
            );
        }
    }
}
