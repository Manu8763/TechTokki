using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ControleTema : MonoBehaviour
{
    public static ControleTema Instance;

    public bool darkMode = true;

    [Header("Cores")]
    public Color backgroundDark;
    public Color backgroundLight;

    public Color headerDark;
    public Color headerLight;

    public Color textDark;
    public Color textLight;

    [Header("Referências")]
    public Image[] backgrounds;
    public Image headerBackground;
    public TextMeshProUGUI[] texts;

    void Awake()
    {
        if (Instance == null){
            Instance = this;
            ApplyTheme();
        }
        else
            Destroy(gameObject);
    }

    public void SetTheme(bool isDark)
    {
        darkMode = isDark;
        ApplyTheme();
    }

    void ApplyTheme()
    {
        // Textos
        foreach (var text in texts)
        {
            if (text != null)
                text.color = darkMode ? textDark : textLight;
        }

        // Fundos
        foreach (var bg in backgrounds)
        {
            if (bg != null)
                bg.color = darkMode ? backgroundDark : backgroundLight;
        }

        if (headerBackground != null)
        {
            headerBackground.color = darkMode ? headerDark : headerLight;
        }
    }
}