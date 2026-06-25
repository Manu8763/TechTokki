using UnityEngine;
using UnityEngine.UI;

public class AlertController : MonoBehaviour
{
    [SerializeField] private Button closeButton;

    void Awake()
    {
        if (closeButton == null)
        {
            Debug.LogError("❌ CloseButton NÃO foi atribuído no Inspector");
            return;
        }

        closeButton.onClick.AddListener(CloseAlert);
    }

    void CloseAlert()
    {
        Debug.Log("🟢 ALERTA FECHADO");
        gameObject.SetActive(false);
    }
}