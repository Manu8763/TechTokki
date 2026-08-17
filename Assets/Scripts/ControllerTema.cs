using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ControleTema : MonoBehaviour
{
    public static ControleTema Instance;

    public bool darkMode = true;

    [Header("Cores - Background Geral")]
    public Color backgroundDark;
    public Color backgroundLight;

    [Header("Cores - Blocos/Painéis")]
    public Color panelDark;
    public Color panelLight;

    [Header("Cores - Header")]
    public Color headerDark;
    public Color headerLight;

    [Header("Cores - Textos")]
    public Color textDark;
    public Color textLight;

    [Header("Referências")]
    public Image[] backgrounds;
    public Image[] panels;
    public Image headerBackground;
    public TextMeshProUGUI[] texts;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            ApplyTheme();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetTheme(bool isDark)
    {
        darkMode = isDark;
        ApplyTheme();
    }

    void ApplyTheme()
    {
        // =========================
        // TEXTOS
        // =========================

        foreach (var text in texts)
        {
            if (text != null)
                text.color = darkMode ? textDark : textLight;
        }


        // =========================
        // BACKGROUND GERAL
        // =========================

        foreach (var bg in backgrounds)
        {
            if (bg != null)
                bg.color = darkMode ? backgroundDark : backgroundLight;
        }


        // =========================
        // BLOCOS / PAINÉIS
        // =========================

        foreach (var panel in panels)
        {
            if (panel != null)
                panel.color = darkMode ? panelDark : panelLight;
        }


        // =========================
        // HEADER
        // =========================

        if (headerBackground != null)
        {
            headerBackground.color = darkMode ? headerDark : headerLight;
        }
    }
}