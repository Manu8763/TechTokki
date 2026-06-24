using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SwitchProgramar : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public Toggle toggle;

    public TMP_InputField acenderInput;
    public TMP_InputField apagarInput;

    public TextMeshProUGUI acenderTexto;
    public TextMeshProUGUI apagarTexto;

    public Color ativoColor = Color.white;
    public Color inativoColor = new Color(1f, 1f, 1f, 0.4f);
    void Start()
    {
        toggle.onValueChanged.AddListener(OnToggleChanged);
        OnToggleChanged(toggle.isOn);
    }

    void OnToggleChanged(bool isOn)
    {
        acenderInput.interactable = isOn;
        apagarInput.interactable = isOn;

        if (acenderTexto != null)
            acenderTexto.color = isOn ? ativoColor : inativoColor;

        if (apagarTexto != null)
            apagarTexto.color = isOn ? ativoColor : inativoColor;
    }
}
