using UnityEngine;
using TMPro;

public enum BallOperation
{
    Insert,
    Delete,
    Search
}

public abstract class TreeBall : MonoBehaviour
{
    private static AudioClip metalHitClip;

    [SerializeField] private int number;
    private BallManager ballManager;
    private bool operationFinished;
    private bool skipFinishOnDestroy;
    private GameObject numberObject;

    public int Number => number;
    public abstract BallOperation Operation { get; }

    public void Initialize(int value)
    {
        number = value;
        gameObject.name = $"{Operation} Ball ({number})";

        ballManager = FindAnyObjectByType<BallManager>();
        if (ballManager != null)
        {
            ballManager.ShowOperationUI(false);
        }
    }

    protected void FinishOperation()
    {
        if (operationFinished)
        {
            return;
        }

        operationFinished = true;

        if (ballManager == null)
        {
            ballManager = FindAnyObjectByType<BallManager>();
        }

        if (ballManager != null)
        {
            ballManager.ShowOperationUI(true);
        }
    }

    protected void SetBallSize(float ballScale = 0.23f, float sizeRange = 0.05f)
    {
        float value = Mathf.Abs((float)Number);
        float normalizedSize = Mathf.Clamp01(Mathf.Log10(value + 1f) / 3f);
        float sizeValue = normalizedSize * 2f - 1f;
        transform.localScale = Vector3.one * (ballScale + sizeValue * sizeRange);
    }

    protected void ShowNumber(Vector3 worldOffset)
    {
        if (numberObject != null)
        {
            return;
        }

        if (ballManager == null)
        {
            ballManager = FindAnyObjectByType<BallManager>();
        }

        Canvas canvas = ballManager == null ? null : ballManager.GetComponent<Canvas>();
        if (canvas == null)
        {
            return;
        }

        numberObject = new GameObject("BallNumberUI " + Number, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        numberObject.transform.SetParent(canvas.transform, false);
        numberObject.transform.SetAsLastSibling();

        RectTransform rect = numberObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(120, 55);

        TextMeshProUGUI numberText = numberObject.GetComponent<TextMeshProUGUI>();
        numberText.text = Number.ToString();
        numberText.fontSize = 34;
        numberText.fontStyle = FontStyles.Bold;
        numberText.alignment = TextAlignmentOptions.Center;
        numberText.color = Color.white;
        numberText.outlineColor = Color.black;
        numberText.outlineWidth = 0.2f;
        numberText.raycastTarget = false;

        PinNumberUI numberUI = numberObject.AddComponent<PinNumberUI>();
        numberUI.target = transform;
        numberUI.worldOffset = worldOffset;
        numberUI.UpdatePosition();
    }

    protected void HideNumber()
    {
        if (numberObject != null)
        {
            Destroy(numberObject);
            numberObject = null;
        }
    }

    protected void PlayMetalHit()
    {
        if (metalHitClip == null)
        {
            metalHitClip = CreateMetalHitClip();
        }

        AudioSource source = GetComponent<AudioSource>();
        if (source == null)
        {
            source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.spatialBlend = 0;
        }

        source.PlayOneShot(metalHitClip, 0.28f);
    }

    private static AudioClip CreateMetalHitClip()
    {
        const int sampleRate = 44100;
        const float duration = 0.12f;
        int sampleCount = Mathf.CeilToInt(sampleRate * duration);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float time = (float)i / sampleRate;
            float attack = Mathf.Clamp01(time / 0.002f);
            float decay = Mathf.Exp(-time * 38f);
            float ring = Mathf.Sin(time * Mathf.PI * 2f * 1550f) * 0.55f
                + Mathf.Sin(time * Mathf.PI * 2f * 2370f) * 0.3f
                + Mathf.Sin(time * Mathf.PI * 2f * 3180f) * 0.15f;
            samples[i] = ring * attack * decay;
        }

        AudioClip clip = AudioClip.Create("Metal Hit", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    public void ReplaceWithAnotherOperation()
    {
        skipFinishOnDestroy = true;
    }

    protected virtual void OnDestroy()
    {
        HideNumber();

        if (!skipFinishOnDestroy)
        {
            FinishOperation();
        }
    }
}
