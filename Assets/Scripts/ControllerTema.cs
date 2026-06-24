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

    public Color textDark;
    public Color textLight;

    [Header("Referências")]
    public Image[] backgrounds;
    public TextMeshProUGUI[] texts;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
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
    }
}