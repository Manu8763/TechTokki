using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GerenciadorComodos : MonoBehaviour
{
    public static GerenciadorComodos Instance;

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

    void Awake()
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

    void Start()
    {
        ConfigurarDropdown();

        selecionarComodo.onValueChanged.AddListener(MudarComodo);

        // Detecta alterações feitas pela interface
        luzToggle.onValueChanged.AddListener(SalvarLuz);
        intensidadeSlider.onValueChanged.AddListener(SalvarIntensidade);
        programacaoToggle.onValueChanged.AddListener(SalvarProgramacao);
        movimentoToggle.onValueChanged.AddListener(SalvarMovimento);
        luminosidadeToggle.onValueChanged.AddListener(SalvarLuminosidade);

        acenderInput.onEndEdit.AddListener(SalvarHorarioAcender);
        apagarInput.onEndEdit.AddListener(SalvarHorarioApagar);

        // Carrega o primeiro cômodo
        MudarComodo(0);
    }

    void ConfigurarDropdown()
    {
        selecionarComodo.ClearOptions();

        foreach (DadosComodo comodo in comodos)
        {
            selecionarComodo.options.Add(
                new TMP_Dropdown.OptionData(comodo.nome)
            );
        }

        selecionarComodo.RefreshShownValue();
    }

    void MudarComodo(int indice)
    {
        comodoAtual = indice;

        CarregarDadosComodo();
    }

    void CarregarDadosComodo()
    {
        DadosComodo dados = comodos[comodoAtual];

        luzToggle.SetIsOnWithoutNotify(dados.luzLigada);
        if (switchLuz != null)
            switchLuz.UpdateSwitch(dados.luzLigada);

        intensidadeSlider.SetValueWithoutNotify(dados.intensidade);

        if (colorPicker != null)
            colorPicker.CarregarCor(dados.cor);

        programacaoToggle.SetIsOnWithoutNotify(dados.programacaoAtiva);
        if (switchProgramacao != null)
            switchProgramacao.UpdateSwitch(dados.programacaoAtiva);

        acenderInput.SetTextWithoutNotify(dados.horarioAcender);
        apagarInput.SetTextWithoutNotify(dados.horarioApagar);

        movimentoToggle.SetIsOnWithoutNotify(dados.sensorMovimento);
        if (switchMovimento != null)
            switchMovimento.UpdateSwitch(dados.sensorMovimento);

        luminosidadeToggle.SetIsOnWithoutNotify(dados.sensorLuminosidade);
        if (switchLuminosidade != null)
            switchLuminosidade.UpdateSwitch(dados.sensorLuminosidade);
    }

    // =========================
    // SALVAR ALTERAÇÕES
    // =========================

    void SalvarLuz(bool valor)
    {
        comodos[comodoAtual].luzLigada = valor;
    }

    void SalvarIntensidade(float valor)
    {
        comodos[comodoAtual].intensidade = Mathf.RoundToInt(valor);
    }

    void SalvarProgramacao(bool valor)
    {
        comodos[comodoAtual].programacaoAtiva = valor;
    }

    void SalvarHorarioAcender(string valor)
    {
        comodos[comodoAtual].horarioAcender = valor;
    }

    void SalvarHorarioApagar(string valor)
    {
        comodos[comodoAtual].horarioApagar = valor;
    }

    void SalvarMovimento(bool valor)
    {
        comodos[comodoAtual].sensorMovimento = valor;
    }

    void SalvarLuminosidade(bool valor)
    {
        comodos[comodoAtual].sensorLuminosidade = valor;
    }

    // =========================
    // COR
    // =========================

    public void SalvarCor(Color novaCor)
    {
        comodos[comodoAtual].cor = novaCor;
    }
}