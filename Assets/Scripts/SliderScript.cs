using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SliderScript : MonoBehaviour
{
    [SerializeField] private Slider _slider;
    [SerializeField] private TextMeshProUGUI _sliderText;

    void Start()
    {
        if (_slider == null)
            _slider = GetComponent<Slider>();

        if (_slider != null)
        {
            _slider.onValueChanged.AddListener(UpdateSliderText);

            UpdateSliderText(_slider.value);
        }
    }

    public void UpdateSliderText(float value)
    {
        if (_sliderText != null)
        {
            _sliderText.text = Mathf.RoundToInt(value) + "%";
        }
    }

    void OnDestroy()
    {
        if (_slider != null)
            _slider.onValueChanged.RemoveListener(UpdateSliderText);
    }
}