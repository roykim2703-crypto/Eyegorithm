using UnityEngine;

public sealed class DeleteBall : TreeBall
{
    public float moveSpeed = 3.2f;
    public float fallSpeed = 3.2f;
    public float deletedFallSpeed = 4.5f;
    public float startDelay = 0.45f;
    public float bounceHeight = 0.5f;
    public float reboundHeight = 0.14f;
    public float spinAngle = 120f;
    public float destroyY = -4.8f;

    private TreeManager treeManager;
    private TreeSpot targetSpot;
    private bool isFalling;
    private Vector3 moveStart;
    private Vector3 normalScale;
    private float moveTime;
    private float moveDuration;
    private float moveStartAngle;
    private bool canMove;
    private bool impactPlayed;
    private float currentFallSpeed;

    public override BallOperation Operation => BallOperation.Delete;

    private void Start()
    {
        PrepareBall();
        SetBallSize();
        normalScale = transform.localScale;
        ShowNumber(new Vector3(0, 0.65f, 0));
        treeManager = FindAnyObjectByType<TreeManager>();
        targetSpot = treeManager == null ? null : treeManager.RootSpot;

        StartCoroutine(StartMoving());
    }

    private System.Collections.IEnumerator StartMoving()
    {
        yield return new WaitForSeconds(startDelay);
        canMove = true;

        if (targetSpot == null || !targetSpot.isFull)
        {
            StartFalling();
        }
        else
        {
            StartBounce(targetSpot);
        }
    }

    private void Update()
    {
        if (!canMove)
        {
            return;
        }

        if (isFalling)
        {
            transform.position += Vector3.down * currentFallSpeed * Time.deltaTime;
            if (transform.position.y < destroyY)
            {
                FinishOperation();
                Destroy(gameObject);
            }

            return;
        }

        if (BounceToTarget())
        {
            CompareAtSpot();
        }
    }

    private void CompareAtSpot()
    {
        if (Number == targetSpot.number)
        {
            treeManager.DeletePin(targetSpot, moveSpeed, null);
            StartFalling(true);
            return;
        }

        TreeSpot nextSpot = treeManager.GetNextSpot(targetSpot, Number);
        if (nextSpot == null || !nextSpot.isFull)
        {
            StartFalling();
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
        moveDuration = Mathf.Max(0.35f, Vector3.Distance(moveStart, targetSpot.transform.position) / moveSpeed);
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

    private void StartFalling(bool keepNumber = false)
    {
        isFalling = true;
        currentFallSpeed = keepNumber ? deletedFallSpeed : fallSpeed;
        targetSpot = null;
        transform.localScale = normalScale;
        transform.rotation = Quaternion.identity;

        if (!keepNumber)
        {
            HideNumber();
        }
    }

    private void PrepareBall()
    {
        Rigidbody2D rigid = GetComponent<Rigidbody2D>();
        if (rigid != null)
        {
            rigid.bodyType = RigidbodyType2D.Kinematic;
            rigid.linearVelocity = Vector2.zero;
            rigid.gravityScale = 0;
        }

        Collider2D ballCollider = GetComponent<Collider2D>();
        if (ballCollider != null)
        {
            ballCollider.isTrigger = true;
        }
    }
}
