using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SwitchProgramar : MonoBehaviour
{
    public Toggle toggle;

    public TMP_InputField acenderInput;
    public TMP_InputField apagarInput;

    public TextMeshProUGUI acenderTexto;
    public TextMeshProUGUI apagarTexto;

    [Header("Cores - Modo Claro")]
    public Color ativoClaroColor = new Color(0f, 0f, 0f, 1f);
    public Color inativoClaroColor = new Color(0f, 0f, 0f, 0.4f);

    [Header("Cores - Modo Escuro")]
    public Color ativoEscuroColor = new Color(1f, 1f, 1f, 1f);
    public Color inativoEscuroColor = new Color(1f, 1f, 1f, 0.4f);

    void Start()
    {
        toggle.onValueChanged.AddListener(OnToggleChanged);

        OnToggleChanged(toggle.isOn);

        // Atualiza as cores quando o tema for alterado
        AtualizarCoresTema();
    }

    void OnToggleChanged(bool isOn)
    {
        acenderInput.interactable = isOn;
        apagarInput.interactable = isOn;

        AtualizarCoresTema();
    }

    void AtualizarCoresTema()
    {
        bool modoEscuro = ControleTema.Instance != null &&
                           ControleTema.Instance.darkMode;

        Color ativoColor;
        Color inativoColor;

        if (modoEscuro)
        {
            ativoColor = ativoEscuroColor;
            inativoColor = inativoEscuroColor;
        }
        else
        {
            ativoColor = ativoClaroColor;
            inativoColor = inativoClaroColor;
        }

        bool isOn = toggle.isOn;

        if (acenderTexto != null)
            acenderTexto.color = isOn ? ativoColor : inativoColor;

        if (apagarTexto != null)
            apagarTexto.color = isOn ? ativoColor : inativoColor;
    }
}