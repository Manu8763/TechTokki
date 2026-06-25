using UnityEngine;
using TMPro;

public class TimeInputFormatter : MonoBehaviour
{
    public TMP_InputField input;

    void Start()
    {
        input.onValueChanged.AddListener(OnTyping);
        input.onEndEdit.AddListener(FormatFinalTime);
    }

    void OnTyping(string value)
    {
        string digits = "";
        foreach (char c in value)
        {
            if (char.IsDigit(c))
                digits += c;
        }

        if (digits.Length > 4)
            digits = digits.Substring(0, 4);

        string display = digits;

        if (digits.Length > 2)
            display = digits.Insert(2, ":");

        if (input.text != display)
        {
            input.text = display;
            input.caretPosition = display.Length;
        }
    }

void FormatFinalTime(string value)
{
    if (string.IsNullOrEmpty(value))
        return;

    // Remove tudo que não for número
    string digits = "";
    foreach (char c in value)
    {
        if (char.IsDigit(c))
            digits += c;
    }

    if (digits.Length == 0)
        return;

    // Garante 4 dígitos
    digits = digits.PadLeft(4, '0');

    int hours = int.Parse(digits.Substring(0, 2));
    int minutes = int.Parse(digits.Substring(2, 2));

    hours = Mathf.Clamp(hours, 0, 23);
    minutes = Mathf.Clamp(minutes, 0, 59);

    input.text = $"{hours:00}:{minutes:00}";
}

    public bool IsValidTime()
    {
        return System.TimeSpan.TryParse(input.text, out _);
    }
}