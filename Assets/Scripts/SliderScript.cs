using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SliderScript : MonoBehaviour
{
    [SerializeField] private Slider _slider;
    [SerializeField] private TextMeshProUGUI _sliderText;

    [Header("Color Picker")]
    public ColorPicker colorPicker;

    void Start()
    {
        if (_slider.value <= 0)
            _slider.value = 100;

        _slider.onValueChanged.AddListener(UpdateSliderText);

        UpdateSliderText(_slider.value);
    }

    public void UpdateSliderText(float value)
    {
        _sliderText.text = Mathf.RoundToInt(value) + "%";

        AplicarIntensidade(value);
    }

    void AplicarIntensidade(float intensidade)
    {
        if (colorPicker != null)
        {
            colorPicker.SetIntensidade(intensidade / 100f);
        }
    }
}
