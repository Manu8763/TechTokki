using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SliderScript : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] private Slider _slider;
    [SerializeField] private TextMeshProUGUI _sliderText;
    void Start()
    {
       UpdateSliderText(_slider.value);
    }

    public void UpdateSliderText(float value)
    {
        _sliderText.text = value.ToString() + "%";
    }
}
