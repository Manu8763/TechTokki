
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

    [Header("Controle individual do comodo")]
    public ESP32Controller esp32Controller;

    // Marque esta opcao somente no switch da luz.
    public bool controlarLampadaDoComodo = false;

    void Start()
    {
        if (toggle != null)
        {
            toggle.onValueChanged.AddListener(
                AoAlterarToggle
            );

            // Apenas atualiza a interface inicial.
            // Nao envia comandos ao ESP32.
            UpdateSwitch(toggle.isOn);
        }
    }

    void AoAlterarToggle(bool ligado)
    {
        UpdateSwitch(ligado);

        if (!controlarLampadaDoComodo)
            return;

        if (esp32Controller == null)
            return;

        if (GerenciadorComodos.Instance == null)
            return;

        int numeroComodo =
            GerenciadorComodos.Instance.NumeroComodoAtual;

        if (ligado)
        {
            esp32Controller.LigarComodo(numeroComodo);
        }
        else
        {
            esp32Controller.DesligarComodo(numeroComodo);
        }
    }

    // Atualiza somente a aparencia do switch.
    // Pode ser chamado ao trocar de comodo.
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

            label.rectTransform.anchoredPosition =
                new Vector2(
                    isOn ? onLabelX : offLabelX,
                    label.rectTransform.anchoredPosition.y
                );
        }
    }

    void OnDestroy()
    {
        if (toggle != null)
        {
            toggle.onValueChanged.RemoveListener(
                AoAlterarToggle
            );
        }
    }
}
