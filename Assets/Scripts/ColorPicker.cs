using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ColorPicker : MonoBehaviour
{
    [Header("HSV")]
    [Range(0, 1)] public float hue = 0f;
    [Range(0, 1)] public float saturation = 1f;
    [Range(0, 1)] public float value = 1f;

    [Header("UI References")]
    public Slider hueSlider;
    public RawImage svImage;
    public Image preview;

    [Header("RGB Inputs")]
    public TMP_InputField rInput;
    public TMP_InputField gInput;
    public TMP_InputField bInput;

    [Header("Selector")]
    public RectTransform selector;

    // ESP32
    public ESP32Controller esp32Controller;

    // VARIÁVEIS
    private Texture2D svTexture;

    // Cor escolhida pelo usuário
    // SEM considerar intensidade
    private Color currentColor;

    // Intensidade atual
    private float intensidade = 1f;

    // START
    void Start()
    {
        // Slider do Hue
        hueSlider.onValueChanged.AddListener(OnHueChanged);

        // Inputs RGB
        rInput.onEndEdit.AddListener(delegate
        {
            UpdateFromRGB();
        });

        gInput.onEndEdit.AddListener(delegate
        {
            UpdateFromRGB();
        });

        bInput.onEndEdit.AddListener(delegate
        {
            UpdateFromRGB();
        });

        // Gera textura inicial
        GenerateSVTexture();

        // Cor inicial
        SetColor(
            Color.HSVToRGB(
                hue,
                saturation,
                value
            ),
            false
        );
    }

    // HUE
    void OnHueChanged(float h)
    {
        hue = h;

        GenerateSVTexture();
        UpdateColor();
    }

    // SATURATION / VALUE
    public void PickColor(Vector2 uv)
    {
        saturation = Mathf.Clamp01(uv.x);
        value = Mathf.Clamp01(uv.y);

        UpdateColor();
    }

    // UPDATE COLOR
    void UpdateColor()
    {
        Color finalColor = Color.HSVToRGB(
            hue,
            saturation,
            value
        );

        SetColor(finalColor, true);
    }

    // SET COLOR
    public void SetColor(Color color)
    {
        SetColor(color, true);
    }

    public void SetColor(Color color, bool salvarNoComodo)
    {
        // Guarda a cor ORIGINAL
        currentColor = color;

        // ENVIA PARA O ESP32
        if (salvarNoComodo && esp32Controller != null)
        {
            esp32Controller.EnviarCor(currentColor);
        }

        // PREVIEW
        if (preview != null)
            preview.color = color;

        // ATUALIZA INPUTS RGB
        if (rInput != null)
        {
            rInput.SetTextWithoutNotify(
                Mathf.RoundToInt(color.r * 255).ToString()
            );
        }

        if (gInput != null)
        {
            gInput.SetTextWithoutNotify(
                Mathf.RoundToInt(color.g * 255).ToString()
            );
        }

        if (bInput != null)
        {
            bInput.SetTextWithoutNotify(
                Mathf.RoundToInt(color.b * 255).ToString()
            );
        }

        // SALVAR COR
        if (
            salvarNoComodo &&
            GerenciadorComodos.Instance != null
        )
        {
            GerenciadorComodos.Instance.SalvarCor(color);
        }
    }

    // RGB -> COLOR
    public void UpdateFromRGB()
    {
        int r = ClampInput(rInput.text);
        int g = ClampInput(gInput.text);
        int b = ClampInput(bInput.text);

        if (rInput != null)
            rInput.SetTextWithoutNotify(r.ToString());

        if (gInput != null)
            gInput.SetTextWithoutNotify(g.ToString());

        if (bInput != null)
            bInput.SetTextWithoutNotify(b.ToString());

        currentColor = new Color(
            r / 255f,
            g / 255f,
            b / 255f
        );

        // PREVIEW
        if (preview != null)
            preview.color = currentColor;

        // ATUALIZA HSV
        Color.RGBToHSV(
            currentColor,
            out hue,
            out saturation,
            out value
        );

        // ATUALIZA SLIDER
        if (hueSlider != null)
        hueSlider.SetValueWithoutNotify(hue);

        // ATUALIZA TEXTURA
        GenerateSVTexture();

        // ENVIA PARA ESP32
        if (esp32Controller != null)
            esp32Controller.EnviarCor(currentColor);

        // SALVA NO CÔMODO
        if (GerenciadorComodos.Instance != null)
            GerenciadorComodos.Instance.SalvarCor(currentColor);
    }

    // CARREGAR COR
    public void CarregarCor(Color color)
    {
    Color.RGBToHSV(
        color,
        out hue,
        out saturation,
        out value
    );

    if (hueSlider != null)
        hueSlider.SetValueWithoutNotify(hue);

    GenerateSVTexture();

    // Posiciona o selector
    if (selector != null && svImage != null)
    {
        RectTransform area = svImage.rectTransform;

        float x = (saturation - 0.5f) * area.rect.width;
        float y = (value - 0.5f) * area.rect.height;

        selector.anchoredPosition = new Vector2(x, y);

        selector.gameObject.SetActive(true);
    }

    currentColor = color;

    if (preview != null)
        preview.color = color;

    if (rInput != null)
        rInput.SetTextWithoutNotify(
            Mathf.RoundToInt(color.r * 255).ToString()
        );

    if (gInput != null)
        gInput.SetTextWithoutNotify(
            Mathf.RoundToInt(color.g * 255).ToString()
        );

    if (bInput != null)
        bInput.SetTextWithoutNotify(
            Mathf.RoundToInt(color.b * 255).ToString()
        );

    if (esp32Controller != null)
        esp32Controller.EnviarCor(currentColor);
    }

    // INTENSIDADE
    public void SetIntensidade(float valor)
    {
        intensidade = Mathf.Clamp01(valor);

        Debug.Log(
            "Intensidade selecionada: " +
            Mathf.RoundToInt(intensidade * 100) +
            "%"
        );

        if (esp32Controller != null)
        {
            esp32Controller.DefinirIntensidade(intensidade);
        }
    }

    // INPUT LIMIT
    int ClampInput(string valor)
    {
        int number;

        if (!int.TryParse(valor, out number))
        {
            number = 0;
        }

        return Mathf.Clamp(number, 0, 255);
    }

    // GENERATE TEXTURE
    void GenerateSVTexture()
    {
        int width = 256;
        int height = 256;

        if (svTexture == null)
        {
            svTexture = new Texture2D(
                width,
                height
            );
        }

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float s = (float)x / width;
                float v = (float)y / height;

                Color color = Color.HSVToRGB(
                    hue,
                    s,
                    v
                );

                svTexture.SetPixel(
                    x,
                    y,
                    color
                );
            }
        }

        svTexture.Apply();

        svImage.texture = svTexture;
    }
}