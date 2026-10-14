
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

    [Header("ESP32")]
    public ESP32Controller esp32Controller;

    private Texture2D svTexture;
    private Color currentColor = Color.white;
    private float intensidade = 1f;

    void Start()
    {
        if (hueSlider != null)
            hueSlider.onValueChanged.AddListener(OnHueChanged);

        if (rInput != null)
            rInput.onEndEdit.AddListener(delegate { UpdateFromRGB(); });

        if (gInput != null)
            gInput.onEndEdit.AddListener(delegate { UpdateFromRGB(); });

        if (bInput != null)
            bInput.onEndEdit.AddListener(delegate { UpdateFromRGB(); });

        GenerateSVTexture();

        SetColor(
            Color.HSVToRGB(hue, saturation, value),
            false
        );
    }

    void OnDestroy()
    {
        if (hueSlider != null)
            hueSlider.onValueChanged.RemoveListener(OnHueChanged);

        if (rInput != null)
            rInput.onEndEdit.RemoveAllListeners();

        if (gInput != null)
            gInput.onEndEdit.RemoveAllListeners();

        if (bInput != null)
            bInput.onEndEdit.RemoveAllListeners();

        if (svTexture != null)
            Destroy(svTexture);
    }

    void OnHueChanged(float h)
    {
        hue = h;
        GenerateSVTexture();
        UpdateColor();
    }

    public void PickColor(Vector2 uv)
    {
        saturation = Mathf.Clamp01(uv.x);
        value = Mathf.Clamp01(uv.y);
        UpdateColor();
    }

    void UpdateColor()
    {
        SetColor(
            Color.HSVToRGB(hue, saturation, value),
            true
        );
    }

    public void SetColor(Color color)
    {
        SetColor(color, true);
    }

    public void SetColor(Color color, bool salvarNoComodo)
    {
        currentColor = color;

        Color.RGBToHSV(
            currentColor,
            out hue,
            out saturation,
            out value
        );

        if (hueSlider != null)
            hueSlider.SetValueWithoutNotify(hue);

        GenerateSVTexture();
        AtualizarInterfaceRGB();

        if (!salvarNoComodo)
            return;

        if (GerenciadorComodos.Instance != null)
            GerenciadorComodos.Instance.SalvarCor(currentColor);

        EnviarCorDoComodo();
    }

    public void UpdateFromRGB()
    {
        int r = ClampInput(rInput != null ? rInput.text : "0");
        int g = ClampInput(gInput != null ? gInput.text : "0");
        int b = ClampInput(bInput != null ? bInput.text : "0");

        currentColor = new Color(
            r / 255f,
            g / 255f,
            b / 255f
        );

        Color.RGBToHSV(
            currentColor,
            out hue,
            out saturation,
            out value
        );

        if (hueSlider != null)
            hueSlider.SetValueWithoutNotify(hue);

        GenerateSVTexture();
        AtualizarInterfaceRGB();

        if (GerenciadorComodos.Instance != null)
            GerenciadorComodos.Instance.SalvarCor(currentColor);

        EnviarCorDoComodo();
    }

    // Carrega a cor salva sem enviar comandos ao ESP32.
    public void CarregarCor(Color color)
    {
        currentColor = color;

        Color.RGBToHSV(
            color,
            out hue,
            out saturation,
            out value
        );

        if (hueSlider != null)
            hueSlider.SetValueWithoutNotify(hue);

        GenerateSVTexture();
        AtualizarInterfaceRGB();

        if (selector != null && svImage != null)
        {
            RectTransform area = svImage.rectTransform;

            float x = (saturation - 0.5f) * area.rect.width;
            float y = (value - 0.5f) * area.rect.height;

            selector.anchoredPosition = new Vector2(x, y);
            selector.gameObject.SetActive(true);
        }
    }

    // Carrega a intensidade salva sem enviar comandos.
    public void CarregarIntensidade(float valor)
    {
        intensidade = Mathf.Clamp01(valor);
    }

    public void SetIntensidade(float valor)
    {
        intensidade = Mathf.Clamp01(valor);

        Debug.Log(
            "Intensidade: " +
            Mathf.RoundToInt(intensidade * 100f) +
            "%"
        );

        EnviarCorDoComodo();
    }

    void EnviarCorDoComodo()
    {
        if (esp32Controller == null)
            return;

        if (GerenciadorComodos.Instance == null)
            return;

        int numeroComodo =
            GerenciadorComodos.Instance.NumeroComodoAtual;

        esp32Controller.EnviarCorComodo(
            numeroComodo,
            currentColor,
            intensidade
        );
    }

    void AtualizarInterfaceRGB()
    {
        if (preview != null)
            preview.color = currentColor;

        if (rInput != null)
            rInput.SetTextWithoutNotify(
                Mathf.RoundToInt(currentColor.r * 255f).ToString()
            );

        if (gInput != null)
            gInput.SetTextWithoutNotify(
                Mathf.RoundToInt(currentColor.g * 255f).ToString()
            );

        if (bInput != null)
            bInput.SetTextWithoutNotify(
                Mathf.RoundToInt(currentColor.b * 255f).ToString()
            );
    }

    int ClampInput(string texto)
    {
        if (!int.TryParse(texto, out int numero))
            numero = 0;

        return Mathf.Clamp(numero, 0, 255);
    }

    void GenerateSVTexture()
    {
        int width = 256;
        int height = 256;

        if (svTexture == null)
        {
            svTexture = new Texture2D(
                width,
                height,
                TextureFormat.RGB24,
                false
            );

            svTexture.wrapMode = TextureWrapMode.Clamp;
            svTexture.filterMode = FilterMode.Bilinear;
        }

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float s = (float)x / (width - 1);
                float v = (float)y / (height - 1);

                svTexture.SetPixel(
                    x,
                    y,
                    Color.HSVToRGB(hue, s, v)
                );
            }
        }

        svTexture.Apply();

        if (svImage != null)
            svImage.texture = svTexture;
    }
}
