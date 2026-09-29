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

    [Header("Ball Follow")]
    [SerializeField] private Vector2 followOffset = new Vector2(0, -0.45f);
    [SerializeField, Min(0.01f)] private float followSmoothTime = 0.16f;
    [SerializeField, Min(0.01f)] private float returnSmoothTime = 0.3f;
    [SerializeField, Min(0.01f)] private float followOrthographicSize = 3.25f;
    [SerializeField, Min(0.01f)] private float zoomInSmoothTime = 0.18f;
    [SerializeField, Min(0.01f)] private float zoomOutSmoothTime = 0.32f;

    private Camera targetCamera;
    private Camera backgroundCamera;
    private Vector3 startPosition;
    private Vector3 followVelocity;
    private float zoomVelocity;
    private int previousScreenWidth;
    private int previousScreenHeight;

    private void Awake()
    {
        targetCamera = GetComponent<Camera>();
        targetCamera.orthographic = true;
        startPosition = transform.position;

        CreateBackgroundCamera();
        ApplyAspectRatio();
    }

    private void LateUpdate()
    {
        Transform ball = FindMovingBall();
        Vector3 targetPosition = startPosition;
        float smoothTime = returnSmoothTime;

        if (ball != null)
        {
            targetPosition.x = ball.position.x + followOffset.x;
            targetPosition.y = ball.position.y + followOffset.y;
            smoothTime = followSmoothTime;
        }

        targetPosition.z = startPosition.z;
        transform.position = Vector3.SmoothDamp(
            transform.position,
            targetPosition,
            ref followVelocity,
            smoothTime);

        float targetSize = ball == null ? fixedOrthographicSize : followOrthographicSize;
        float zoomTime = ball == null ? zoomOutSmoothTime : zoomInSmoothTime;
        targetCamera.orthographicSize = Mathf.SmoothDamp(
            targetCamera.orthographicSize,
            targetSize,
            ref zoomVelocity,
            zoomTime);
    }

    private Transform FindMovingBall()
    {
        TreeBall[] balls = FindObjectsByType<TreeBall>();

        for (int i = 0; i < balls.Length; i++)
        {
            InsertBall insertBall = balls[i] as InsertBall;

            if (insertBall == null || !insertBall.isPin)
            {
                return balls[i].transform;
            }
        }

        return null;
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
