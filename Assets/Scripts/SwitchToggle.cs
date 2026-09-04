using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SwitchToggle : MonoBehaviour
{
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

    // ADICIONE ISSO
    public ESP32Controller esp32Controller;

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

        if (background != null)
        {
            background.color = isOn ? onColor : offColor;
        }

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

        // AVISA O ESP32 CONTROLLER SE O CONTROLE DE COR ESTÁ ATIVO
        if (esp32Controller != null)
        {
            esp32Controller.AlternarCor(isOn);
        }
    }
}