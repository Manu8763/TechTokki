using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ApagarAcenderToggle : MonoBehaviour
{
    public Toggle toggle;
    public Image background;
    public Text label;

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
            label.text = "Apagar todas";
        }
        else
        {
            background.color = acenderColor;
            label.text = "Acender todas";
        }
    }
}