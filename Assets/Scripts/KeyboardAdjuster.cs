using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class KeyboardAdjuster : MonoBehaviour
{
    [Header("Referências")]
    public ScrollRect scrollRect;

    [Header("Campos de Input")]
    public TMP_InputField[] inputFields;

    [Header("Configuração")]
    public float atraso = 0.3f;
    public float margem = 100f;

    private TMP_InputField campoAtual;

    void Start()
    {
        if (scrollRect == null)
            scrollRect = GetComponent<ScrollRect>();

        foreach (TMP_InputField input in inputFields)
        {
            if (input != null)
            {
                input.onSelect.AddListener(
                    delegate
                    {
                        CampoSelecionado(input);
                    }
                );
            }
        }
    }

    void CampoSelecionado(TMP_InputField campo)
    {
        campoAtual = campo;

        StopAllCoroutines();
        StartCoroutine(RolarParaCampo());
    }

    IEnumerator RolarParaCampo()
    {
        yield return new WaitForSeconds(atraso);

        Canvas.ForceUpdateCanvases();

        yield return null;

        if (campoAtual == null)
            yield break;

        RectTransform content = scrollRect.content;
        RectTransform viewport = scrollRect.viewport;

        if (content == null || viewport == null)
            yield break;

        RectTransform campo =
            campoAtual.GetComponent<RectTransform>();

        Vector3 campoLocal =
            content.InverseTransformPoint(
                campo.position
            );

        float contentHeight =
            content.rect.height;

        float viewportHeight =
            viewport.rect.height;

        float campoY =
            -campoLocal.y;

        float limiteInferior =
            contentHeight - viewportHeight - margem;

        float novaPosicao = campoY - margem;

        novaPosicao =
            Mathf.Clamp(
                novaPosicao,
                0,
                Mathf.Max(
                    0,
                    contentHeight - viewportHeight
                )
            );

        float maxScroll =
            contentHeight - viewportHeight;

        if (maxScroll > 0)
        {
            float normalized =
                1f - (novaPosicao / maxScroll);

            normalized =
                Mathf.Clamp01(normalized);

            scrollRect.verticalNormalizedPosition =
                normalized;
        }

        Canvas.ForceUpdateCanvases();
    }

    void OnDisable()
    {
        StopAllCoroutines();
    }
}