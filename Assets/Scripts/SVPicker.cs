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
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 localPoint
        );

        Rect rect = rectTransform.rect;

        float x = Mathf.Clamp01(
            (localPoint.x - rect.x) / rect.width
        );

        float y = Mathf.Clamp01(
            (localPoint.y - rect.y) / rect.height
        );

        // Atualiza HSV
        picker.PickColor(new Vector2(x, y));

        // Move bolinha
        selector.anchoredPosition = new Vector2(
            localPoint.x,
            localPoint.y
        );
    }
}