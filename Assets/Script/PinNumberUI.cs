using UnityEngine;

public class PinNumberUI : MonoBehaviour
{
    public Transform target;
    public Vector3 worldOffset;

    private RectTransform rect;
    private Canvas canvas;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        UpdatePosition();
    }

    public void UpdatePosition()
    {
        if (rect == null || canvas == null || Camera.main == null)
        {
            return;
        }

        RectTransform canvasRect = canvas.transform as RectTransform;
        Vector2 screenPosition = Camera.main.WorldToScreenPoint(target.position + worldOffset);
        Camera canvasCamera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPosition, canvasCamera, out Vector2 localPosition))
        {
            rect.anchoredPosition = localPosition;
        }
    }
}
