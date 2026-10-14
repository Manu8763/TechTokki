
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GerenciadorComodos : MonoBehaviour
{
    public static GerenciadorComodos Instance;

    [Header("Controle ESP32")]
    public ESP32Controller esp32Controller;

    [Header("Cor")]
    public ColorPicker colorPicker;

    [Header("Cômodos")]
    public DadosComodo[] comodos;

    [Header("Seleção de cômodo")]
    public TMP_Dropdown selecionarComodo;

    [Header("Luz")]
    public Toggle luzToggle;
    public Slider intensidadeSlider;
    public SwitchToggle switchLuz;

    [Header("Programação")]
    public Toggle programacaoToggle;
    public SwitchToggle switchProgramacao;
    public TMP_InputField acenderInput;
    public TMP_InputField apagarInput;

    [Header("Sensores")]
    public Toggle movimentoToggle;
    public SwitchToggle switchMovimento;

    public Toggle luminosidadeToggle;
    public SwitchToggle switchLuminosidade;

    private int comodoAtual = 0;

    // Número enviado ao ESP32: de 1 a 4.
    public int NumeroComodoAtual
    {
        get { return comodoAtual + 1; }
    }

    // Intensidade do cômodo selecionado.
    public float IntensidadeAtual
    {
        get
        {
            if (
                comodos == null ||
                comodoAtual < 0 ||
                comodoAtual >= comodos.Length ||
                comodos[comodoAtual] == null
            )
                return 1f;

            return comodos[comodoAtual].intensidade;
        }
    }

    // =====================================================
    // INICIALIZAÇÃO
    // =====================================================

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (comodos == null || comodos.Length == 0)
        {
            Debug.LogError(
                "GerenciadorComodos: nenhum cômodo foi configurado."
            );
            return;
        }

        if (selecionarComodo == null)
        {
            Debug.LogError(
                "GerenciadorComodos: o Dropdown não foi atribuído."
            );
            return;
        }

        ConfigurarDropdown();

        selecionarComodo.onValueChanged.AddListener(MudarComodo);

        if (luzToggle != null)
            luzToggle.onValueChanged.AddListener(SalvarLuz);

        if (intensidadeSlider != null)
            intensidadeSlider.onValueChanged.AddListener(SalvarIntensidade);

        if (programacaoToggle != null)
            programacaoToggle.onValueChanged.AddListener(SalvarProgramacao);

        if (movimentoToggle != null)
            movimentoToggle.onValueChanged.AddListener(SalvarMovimento);

        if (luminosidadeToggle != null)
            luminosidadeToggle.onValueChanged.AddListener(SalvarLuminosidade);

        if (acenderInput != null)
            acenderInput.onEndEdit.AddListener(SalvarHorarioAcender);

        if (apagarInput != null)
            apagarInput.onEndEdit.AddListener(SalvarHorarioApagar);

        // Carrega o primeiro cômodo.
        MudarComodo(0);
    }

    // =====================================================
    // CONFIGURAR DROPDOWN
    // =====================================================

    private void ConfigurarDropdown()
    {
        selecionarComodo.ClearOptions();

        foreach (DadosComodo comodo in comodos)
        {
            if (comodo == null)
            {
                Debug.LogError(
                    "Existe um elemento vazio no array de cômodos."
                );
                continue;
            }

            selecionarComodo.options.Add(
                new TMP_Dropdown.OptionData(comodo.nome)
            );
        }

        selecionarComodo.RefreshShownValue();
    }

    // =====================================================
    // MUDAR CÔMODO
    // =====================================================

    private void MudarComodo(int indice)
    {
        if (
            comodos == null ||
            indice < 0 ||
            indice >= comodos.Length
        )
        {
            Debug.LogWarning("Índice de cômodo inválido.");
            return;
        }

        if (comodos[indice] == null)
        {
            Debug.LogWarning(
                "O cômodo selecionado não foi configurado."
            );
            return;
        }

        comodoAtual = indice;

        CarregarDadosComodo();
    }

    // =====================================================
    // CARREGAR DADOS DO CÔMODO
    // =====================================================

    private void CarregarDadosComodo()
    {
        DadosComodo dados = comodos[comodoAtual];

        // Estado da lâmpada.
        if (luzToggle != null)
            luzToggle.SetIsOnWithoutNotify(dados.luzLigada);

        if (switchLuz != null)
            switchLuz.UpdateSwitch(dados.luzLigada);

        // Intensidade.
        if (intensidadeSlider != null)
        {
            intensidadeSlider.SetValueWithoutNotify(
                dados.intensidade
            );
        }

        // Cor e intensidade do ColorPicker.
        if (colorPicker != null)
        {
            colorPicker.CarregarCor(dados.cor);
            colorPicker.CarregarIntensidade(dados.intensidade);
        }

        // Programação.
        if (programacaoToggle != null)
        {
            programacaoToggle.SetIsOnWithoutNotify(
                dados.programacaoAtiva
            );
        }

        if (switchProgramacao != null)
        {
            switchProgramacao.UpdateSwitch(
                dados.programacaoAtiva
            );
        }

        if (acenderInput != null)
        {
            acenderInput.SetTextWithoutNotify(
                dados.horarioAcender
            );
        }

        if (apagarInput != null)
        {
            apagarInput.SetTextWithoutNotify(
                dados.horarioApagar
            );
        }

        // Sensor de movimento.
        if (movimentoToggle != null)
        {
            movimentoToggle.SetIsOnWithoutNotify(
                dados.sensorMovimento
            );
        }

        if (switchMovimento != null)
        {
            switchMovimento.UpdateSwitch(
                dados.sensorMovimento
            );
        }

        // Sensor de luminosidade.
        if (luminosidadeToggle != null)
        {
            luminosidadeToggle.SetIsOnWithoutNotify(
                dados.sensorLuminosidade
            );
        }

        if (switchLuminosidade != null)
        {
            switchLuminosidade.UpdateSwitch(
                dados.sensorLuminosidade
            );
        }
    }

    // =====================================================
    // SALVAR ALTERAÇÕES
    // =====================================================

    private void SalvarLuz(bool valor)
    {
        if (!ComodoAtualValido())
            return;

        comodos[comodoAtual].luzLigada = valor;
    }

    private void SalvarIntensidade(float valor)
    {
        if (!ComodoAtualValido())
            return;

        comodos[comodoAtual].intensidade =
            Mathf.Clamp01(valor);
    }

    private void SalvarProgramacao(bool valor)
    {
        if (!ComodoAtualValido())
            return;

        comodos[comodoAtual].programacaoAtiva = valor;
    }

    private void SalvarHorarioAcender(string valor)
    {
        if (!ComodoAtualValido())
            return;

        comodos[comodoAtual].horarioAcender = valor;
    }

    private void SalvarHorarioApagar(string valor)
    {
        if (!ComodoAtualValido())
            return;

        comodos[comodoAtual].horarioApagar = valor;
    }

    
private void SalvarMovimento(bool valor)
{
    if (!ComodoAtualValido())
        return;

    comodos[comodoAtual].sensorMovimento = valor;

    if (esp32Controller != null)
    {
        esp32Controller.ConfigurarSensorMovimento(valor);
    }
}


    private void SalvarLuminosidade(bool valor)
    {
        if (!ComodoAtualValido())
            return;

        comodos[comodoAtual].sensorLuminosidade = valor;
    }

    // =====================================================
    // SALVAR COR
    // =====================================================

    public void SalvarCor(Color novaCor)
    {
        if (!ComodoAtualValido())
            return;

        comodos[comodoAtual].cor = novaCor;
    }

    // =====================================================
    // VALIDAR CÔMODO ATUAL
    // =====================================================

    private bool ComodoAtualValido()
    {
        return
            comodos != null &&
            comodoAtual >= 0 &&
            comodoAtual < comodos.Length &&
            comodos[comodoAtual] != null;
    }

    // =====================================================
    // LIMPAR EVENTOS
    // =====================================================

    private void OnDestroy()
    {
        if (selecionarComodo != null)
            selecionarComodo.onValueChanged.RemoveListener(MudarComodo);

        if (luzToggle != null)
            luzToggle.onValueChanged.RemoveListener(SalvarLuz);

        if (intensidadeSlider != null)
            intensidadeSlider.onValueChanged.RemoveListener(SalvarIntensidade);

        if (programacaoToggle != null)
            programacaoToggle.onValueChanged.RemoveListener(SalvarProgramacao);

        if (movimentoToggle != null)
            movimentoToggle.onValueChanged.RemoveListener(SalvarMovimento);

        if (luminosidadeToggle != null)
            luminosidadeToggle.onValueChanged.RemoveListener(SalvarLuminosidade);

        if (acenderInput != null)
            acenderInput.onEndEdit.RemoveListener(SalvarHorarioAcender);

        if (apagarInput != null)
            apagarInput.onEndEdit.RemoveListener(SalvarHorarioApagar);

        if (Instance == this)
            Instance = null;
    }
}
