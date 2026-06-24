using UnityEngine;
using UnityEngine.UI;

public class ThemeToggle : MonoBehaviour
{
    public Toggle toggle;
    public Image icon;

    public Sprite sunIcon;
    public Sprite moonIcon;

    void Start()
    {
        toggle.onValueChanged.AddListener(OnThemeChanged);
        OnThemeChanged(toggle.isOn);
    }

    void OnThemeChanged(bool isOn)
    {
        // isOn = modo escuro
        icon.sprite = isOn ? sunIcon : moonIcon;

        ControleTema.Instance.SetTheme(isOn);
    }
}