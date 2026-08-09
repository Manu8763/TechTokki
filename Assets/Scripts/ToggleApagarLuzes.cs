using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ApagarAcenderToggle : MonoBehaviour
{
    public Toggle toggle;
    public Image background;
    public TextMeshProUGUI Texto;

    public ESP32Controller esp32;

    public Color apagarColor = Color.yellow;
    public Color acenderColor = Color.gray;

    void Start()
    {
        toggle.onValueChanged.AddListener(OnToggleChanged);
        OnToggleChanged(toggle.isOn);
    }

    void OnToggleChanged(bool isOn)
    {
        if (isOn)
        {
            background.color = apagarColor;
            Texto.text = "Apagar todas";

            esp32.LigarLED();
        }
        else
        {
            background.color = acenderColor;
            Texto.text = "Acender todas";

            esp32.DesligarLED();
        }
    }
}