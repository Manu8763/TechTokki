using UnityEngine;
using UnityEngine.EventSystems;

public class SVSelectorController : MonoBehaviour, IDragHandler, IPointerDownHandler
{
    public RectTransform svArea;        // Quadrado branco
    public ColorPicker colorPicker;     // Seu script atual

    RectTransform selector;

    void Awake()
    {
        selector = GetComponent<RectTransform>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        UpdateSelector(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        UpdateSelector(eventData);
    }

    void UpdateSelector(PointerEventData eventData)
    {
        Vector2 localPoint;

        // Converte o toque para coordenada local do quadrado
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            svArea,
            eventData.position,
            eventData.pressEventCamera,
            out localPoint
        );

        // Metade do tamanho do quadrado
        Vector2 halfSize = svArea.rect.size * 0.5f;

        // Metade do tamanho da bolinha
        Vector2 selectorHalf = selector.rect.size * 0.5f;

        // Limita movimento
        float x = Mathf.Clamp(
            localPoint.x,
            -halfSize.x + selectorHalf.x,
            halfSize.x - selectorHalf.x
        );

        float y = Mathf.Clamp(
            localPoint.y,
            -halfSize.y + selectorHalf.y,
            halfSize.y - selectorHalf.y
        );

        // Aplica posição
        selector.localPosition = new Vector2(x, y);

        // Normaliza para 0–1 (Saturation / Value)
        float s = (x + halfSize.x) / (halfSize.x * 2f);
        float v = (y + halfSize.y) / (halfSize.y * 2f);

        // Envia para o ColorPicker
        colorPicker.PickColor(new Vector2(s, v));
    }
}