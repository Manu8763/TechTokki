using UnityEngine;
using UnityEngine.EventSystems;

public class SVPicker : MonoBehaviour,
    IPointerDownHandler,
    IDragHandler
{
    public ColorPicker picker;

    public RectTransform rectTransform;
    public RectTransform selector;

    public void OnPointerDown(PointerEventData eventData)
    {
        UpdatePicker(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        UpdatePicker(eventData);
    }

    void UpdatePicker(PointerEventData eventData)
    {
        Vector2 localPoint;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out localPoint
        );

        Rect rect = rectTransform.rect;

        // Limites considerando o tamanho do selector
        float halfSelectorWidth = selector.rect.width * 0.5f;
        float halfSelectorHeight = selector.rect.height * 0.5f;

        float minX = rect.xMin + halfSelectorWidth;
        float maxX = rect.xMax - halfSelectorWidth;

        float minY = rect.yMin + halfSelectorHeight;
        float maxY = rect.yMax - halfSelectorHeight;

        // Limita o selector dentro do SVArea
        float x = Mathf.Clamp(localPoint.x, minX, maxX);
        float y = Mathf.Clamp(localPoint.y, minY, maxY);

        // Move o selector
        selector.localPosition = new Vector2(x, y);

        // Converte para 0–1
        float s = Mathf.InverseLerp(rect.xMin, rect.xMax, x);
        float v = Mathf.InverseLerp(rect.yMin, rect.yMax, y);

        // Atualiza a cor
        picker.PickColor(new Vector2(s, v));
    }
}