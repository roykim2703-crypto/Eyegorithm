using UnityEngine;
using System.Collections;

public sealed class InsertBall : TreeBall
{
    public Sprite pinSprite;
    public float ballScale = 0.23f;
    public float ballSizeRange = 0.05f;
    public float pinScale = 0.15f;
    public float pinSizeRange = 0.03f;
    public float moveSpeed = 3.4f;
    public float failedFallSpeed = 3.2f;
    public float startDelay = 0.45f;
    public float bounceHeight = 0.5f;
    public float reboundHeight = 0.14f;
    public float spinAngle = 120f;
    public float destroyY = -4.8f;
    public bool isPin;
    public bool isSameNumber;
    public bool isTreeFull;
    public TreeSpot currentSpot;
    public TreeSpot targetSpot;

    private Rigidbody2D rigid;
    private TreeManager treeManager;
    private Vector3 moveStart;
    private Vector3 normalScale;
    private float moveTime;
    private float moveDuration;
    private float moveStartAngle;
    private bool canMove;
    private bool isFalling;
    private bool impactPlayed;

    public override BallOperation Operation => BallOperation.Insert;

    private void Start()
    {
        rigid = GetComponent<Rigidbody2D>();
        normalScale = Vector3.one * GetBallSize();
        transform.localScale = normalScale;
        PrepareBall();
        treeManager = FindAnyObjectByType<TreeManager>();

        if (treeManager != null && treeManager.spots != null && treeManager.spots.Length > 0)
        {
            targetSpot = treeManager.spots[0];
        }

        StartCoroutine(StartMoving());
    }

    private IEnumerator StartMoving()
    {
        yield return new WaitForSeconds(startDelay);

        if (targetSpot == null)
        {
            FinishOperation();
            Destroy(gameObject);
            yield break;
        }

        canMove = true;
        StartBounce(targetSpot);
    }

    private void Update()
    {
        if (!canMove || isPin)
        {
            return;
        }

        if (isFalling)
        {
            Fall();
            return;
        }

        if (BounceToTarget())
        {
            CheckSpot();
        }
    }

    private void CheckSpot()
    {
        if (targetSpot == null)
        {
            return;
        }

        if (!targetSpot.isFull || targetSpot.pin == null)
        {
            BecomePin(targetSpot);
            return;
        }

        if (Number == targetSpot.number)
        {
            isSameNumber = true;
            StartFailureFall();
            return;
        }

        TreeSpot nextSpot = treeManager.GetNextSpot(targetSpot, Number);

        if (nextSpot == null)
        {
            isTreeFull = true;
            StartFailureFall();
            return;
        }

        StartBounce(nextSpot);
    }

    private void StartBounce(TreeSpot nextSpot)
    {
        targetSpot = nextSpot;
        moveStart = transform.position;
        moveTime = 0;
        moveStartAngle = transform.eulerAngles.z;
        impactPlayed = false;
        float distance = Vector3.Distance(moveStart, targetSpot.transform.position);
        moveDuration = Mathf.Max(0.35f, distance / Mathf.Min(moveSpeed, 3.4f));
    }

    private bool BounceToTarget()
    {
        moveTime += Time.deltaTime;
        float value = Mathf.Clamp01(moveTime / moveDuration);
        const float mainBounceEnd = 0.84f;

        if (!impactPlayed && value >= mainBounceEnd)
        {
            impactPlayed = true;
            PlayMetalHit();
        }

        float bounce;
        float travelValue;

        if (value < mainBounceEnd)
        {
            float mainValue = value / mainBounceEnd;
            travelValue = mainValue;
            bounce = Mathf.Sin(mainValue * Mathf.PI);
        }
        else
        {
            float smallValue = (value - mainBounceEnd) / (1f - mainBounceEnd);
            travelValue = 1;
            bounce = Mathf.Sin(smallValue * Mathf.PI) * (reboundHeight / Mathf.Max(0.01f, bounceHeight));
        }

        Vector3 position = Vector3.Lerp(moveStart, targetSpot.transform.position, travelValue);
        position.y += bounce * bounceHeight;
        transform.position = position;

        float direction = Mathf.Sign(targetSpot.transform.position.x - moveStart.x);
        if (direction == 0)
        {
            direction = 1;
        }

        float spinValue = value < mainBounceEnd
            ? travelValue
            : 1f + (value - mainBounceEnd) * 0.5f;
        transform.localScale = normalScale;
        transform.rotation = Quaternion.Euler(0, 0, moveStartAngle - direction * spinValue * spinAngle);

        if (value < 1f)
        {
            return false;
        }

        transform.position = targetSpot.transform.position;
        transform.localScale = normalScale;
        return true;
    }

    private void StartFailureFall()
    {
        isFalling = true;
        targetSpot = null;
        transform.localScale = normalScale;
        transform.rotation = Quaternion.identity;
    }

    private void Fall()
    {
        transform.position += Vector3.down * failedFallSpeed * Time.deltaTime;

        if (transform.position.y >= destroyY)
        {
            return;
        }

        if (treeManager != null)
        {
            if (isSameNumber)
            {
                treeManager.ShowAlreadyExists(Number);
            }
            else if (isTreeFull)
            {
                treeManager.ShowFull();
            }
        }

        FinishOperation();
        Destroy(gameObject);
    }

    private void PrepareBall()
    {
        if (rigid != null)
        {
            rigid.bodyType = RigidbodyType2D.Kinematic;
            rigid.linearVelocity = Vector2.zero;
            rigid.angularVelocity = 0;
            rigid.gravityScale = 0;
        }

        Collider2D ballCollider = GetComponent<Collider2D>();
        if (ballCollider != null)
        {
            ballCollider.enabled = false;
        }
    }

    public void BecomePin(TreeSpot spot)
    {
        if (isPin)
        {
            return;
        }

        isPin = true;
        canMove = false;
        isFalling = false;
        currentSpot = spot;
        spot.SetPin(this);
        targetSpot = null;
        transform.position = spot.transform.position;
        float pinSize = GetPinSize();
        transform.localScale = Vector3.one * pinSize;
        gameObject.name = "Pin " + Number;

        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null && pinSprite != null)
        {
            spriteRenderer.sprite = pinSprite;
        }

        Rigidbody2D rigid = GetComponent<Rigidbody2D>();
        if (rigid != null)
        {
            rigid.linearVelocity = Vector2.zero;
            rigid.angularVelocity = 0;
            rigid.bodyType = RigidbodyType2D.Static;
        }

        CircleCollider2D circle = GetComponent<CircleCollider2D>();
        if (circle != null)
        {
            circle.enabled = true;
            circle.radius = 2.7f;
            circle.isTrigger = false;
        }

        FinishOperation();
        ShowNumber(new Vector3(0, GetPinSize() * 3.2f, 0));
    }

    public void MoveToSpot(TreeSpot spot)
    {
        currentSpot = spot;
        targetSpot = null;
        transform.position = spot.transform.position;
    }

    private float GetBallSize()
    {
        float sizeValue = GetNormalizedSize() * 2f - 1f;
        return ballScale + sizeValue * ballSizeRange;
    }

    private float GetPinSize()
    {
        float sizeValue = GetNormalizedSize() * 2f - 1f;
        return pinScale + sizeValue * pinSizeRange;
    }

    private float GetNormalizedSize()
    {
        float value = Mathf.Abs((float)Number);
        return Mathf.Clamp01(Mathf.Log10(value + 1f) / 3f);
    }
}
