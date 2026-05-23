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

    private Texture2D svTexture;
    private Color currentColor;

    void Start()
    {
        // Slider do Hue
        hueSlider.onValueChanged.AddListener(OnHueChanged);
        // Inputs RGB
        rInput.onEndEdit.AddListener(delegate { UpdateFromRGB(); });
        gInput.onEndEdit.AddListener(delegate { UpdateFromRGB(); });
        bInput.onEndEdit.AddListener(delegate { UpdateFromRGB(); });
        // Gera textura inicial
        GenerateSVTexture();
        // Cor inicial (amarelo)
        SetColor(Color.HSVToRGB(hue, saturation, value));
    }
    // =========================
    // HUE
    // =========================
    void OnHueChanged(float h)
    {
        hue = h;
        GenerateSVTexture();
        UpdateColor();
    }

    // =========================
    // SATURATION / VALUE
    // =========================
    public void PickColor(Vector2 uv)
    {
        saturation = Mathf.Clamp01(uv.x);
        value = Mathf.Clamp01(uv.y);
        UpdateColor();
    }
    // =========================
    // UPDATE COLOR
    // =========================
    void UpdateColor()
    {
        Color finalColor = Color.HSVToRGB(
            hue,
            saturation,
            value
        );
        SetColor(finalColor);
    }
    // =========================
    // SET COLOR
    // =========================
    public void SetColor(Color color)
    {
        currentColor = color;
        // Preview
        preview.color = color;
        // Atualiza Inputs RGB
        rInput.text = Mathf.RoundToInt(color.r * 255).ToString();
        gInput.text = Mathf.RoundToInt(color.g * 255).ToString();
        bInput.text = Mathf.RoundToInt(color.b * 255).ToString();
    }

    // =========================
    // RGB -> COLOR
    // =========================

    public void UpdateFromRGB()
    {
        int r = ClampInput(rInput.text);
        int g = ClampInput(gInput.text);
        int b = ClampInput(bInput.text);

        currentColor = new Color(
            r / 255f,
            g / 255f,
            b / 255f
        );
        // Atualiza Preview
        preview.color = currentColor;
        // Atualiza HSV
        Color.RGBToHSV(currentColor, out hue, out saturation, out value);
        // Atualiza slider
        hueSlider.value = hue;
        // Atualiza textura
        GenerateSVTexture();
    }
    // =========================
    // INPUT LIMIT
    // =========================
    int ClampInput(string value)
    {
        int number;
        if (!int.TryParse(value, out number))
        {
            number = 0;
        }
        return Mathf.Clamp(number, 0, 255);
    }
    // =========================
    // GENERATE TEXTURE
    // =========================
    void GenerateSVTexture()
    {
        int width = 256;
        int height = 256;
        if (svTexture == null)
        {
            svTexture = new Texture2D(width, height);
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
                svTexture.SetPixel(x, y, color);
            }
        }
        svTexture.Apply();
        svImage.texture = svTexture;
    }
}