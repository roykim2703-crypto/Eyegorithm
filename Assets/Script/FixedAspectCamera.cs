using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Camera))]
public sealed class FixedAspectCamera : MonoBehaviour
{
    [Header("Reference Screen")]
    [SerializeField] private Vector2Int referenceResolution = new Vector2Int(1920, 1080);

    [Header("2D Camera")]
    [SerializeField, Min(0.01f)] private float fixedOrthographicSize = 5f;
    [SerializeField] private Color barColor = Color.black;

    private Camera targetCamera;
    private Camera backgroundCamera;
    private int previousScreenWidth;
    private int previousScreenHeight;

    private void Awake()
    {
        targetCamera = GetComponent<Camera>();
        targetCamera.orthographic = true;

        CreateBackgroundCamera();
        ApplyAspectRatio();
    }

    private void Update()
    {
        if (Screen.width != previousScreenWidth || Screen.height != previousScreenHeight)
        {
            ApplyAspectRatio();
        }
    }

    private void CreateBackgroundCamera()
    {
        GameObject backgroundObject = new GameObject("Black Bar Camera");
        backgroundObject.transform.SetParent(transform, false);

        backgroundCamera = backgroundObject.AddComponent<Camera>();
        backgroundCamera.clearFlags = CameraClearFlags.SolidColor;
        backgroundCamera.backgroundColor = barColor;
        backgroundCamera.cullingMask = 0;
        backgroundCamera.depth = targetCamera.depth - 1f;
        backgroundCamera.rect = new Rect(0f, 0f, 1f, 1f);
        backgroundCamera.targetDisplay = targetCamera.targetDisplay;
    }

    private void ApplyAspectRatio()
    {
        previousScreenWidth = Screen.width;
        previousScreenHeight = Screen.height;

        if (referenceResolution.x <= 0 || referenceResolution.y <= 0 || Screen.height <= 0)
        {
            return;
        }

        float targetAspect = (float)referenceResolution.x / referenceResolution.y;
        float screenAspect = (float)Screen.width / Screen.height;

        if (screenAspect > targetAspect)
        {
            float viewportWidth = targetAspect / screenAspect;
            targetCamera.rect = new Rect(
                (1f - viewportWidth) * 0.5f,
                0f,
                viewportWidth,
                1f);
        }
        else
        {
            float viewportHeight = screenAspect / targetAspect;
            targetCamera.rect = new Rect(
                0f,
                (1f - viewportHeight) * 0.5f,
                1f,
                viewportHeight);
        }

        targetCamera.orthographicSize = fixedOrthographicSize;

        if (backgroundCamera != null)
        {
            backgroundCamera.backgroundColor = barColor;
            backgroundCamera.depth = targetCamera.depth - 1f;
        }
    }
}
