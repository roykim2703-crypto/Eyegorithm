using UnityEngine;

public sealed class SearchBall : TreeBall
{
    public float moveSpeed = 3.2f;
    public float fallSpeed = 3.2f;
    public float startDelay = 0.45f;
    public float bounceHeight = 0.5f;
    public float reboundHeight = 0.14f;
    public float spinAngle = 120f;
    public float destroyY = -4.8f;

    private TreeManager treeManager;
    private TreeSpot targetSpot;
    private InsertBall foundPin;
    private LineRenderer thread;
    private bool isFalling;
    private float swingTime;
    private Vector3 moveStart;
    private Vector3 normalScale;
    private float moveTime;
    private float moveDuration;
    private float moveStartAngle;
    private bool canMove;
    private bool impactPlayed;
    private bool resultShown;
    private float fallStartX;

    public override BallOperation Operation => BallOperation.Search;

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

        if (treeManager == null || targetSpot == null || !targetSpot.isFull || targetSpot.pin == null)
        {
            FinishSearch(false);
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
            Fall();
            return;
        }

        if (BounceToTarget())
        {
            CompareAtSpot();
        }
    }

    private void CompareAtSpot()
    {
        if (treeManager == null || targetSpot == null || !targetSpot.isFull || targetSpot.pin == null)
        {
            FinishSearch(false);
            return;
        }

        if (Number == targetSpot.number)
        {
            foundPin = targetSpot.pin;
            CreateThread();
            FinishSearch(true);
            return;
        }

        TreeSpot nextSpot = treeManager.GetNextSpot(targetSpot, Number);
        if (nextSpot == null || !nextSpot.isFull || nextSpot.pin == null)
        {
            FinishSearch(false);
            return;
        }

        StartBounce(nextSpot);
    }

    private void StartBounce(TreeSpot nextSpot)
    {
        if (nextSpot == null)
        {
            FinishSearch(false);
            return;
        }

        targetSpot = nextSpot;
        moveStart = transform.position;
        moveTime = 0;
        moveStartAngle = transform.eulerAngles.z;
        impactPlayed = false;
        moveDuration = Mathf.Max(0.35f, Vector3.Distance(moveStart, targetSpot.transform.position) / moveSpeed);
    }

    private bool BounceToTarget()
    {
        if (targetSpot == null)
        {
            FinishSearch(false);
            return false;
        }

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

    private void FinishSearch(bool found)
    {
        if (!resultShown && treeManager != null)
        {
            treeManager.ShowSearchResult(found, Number);
        }

        resultShown = true;
        StartFalling(found);
    }

    private void StartFalling(bool found)
    {
        isFalling = true;
        targetSpot = null;
        fallStartX = transform.position.x;
        swingTime = 0;
        transform.localScale = normalScale;
        transform.rotation = Quaternion.identity;

        if (!found)
        {
            HideNumber();
        }
    }

    private void Fall()
    {
        swingTime += Time.deltaTime;
        float swing = foundPin == null ? 0 : Mathf.Sin(swingTime * 4f) * 0.35f;
        Vector3 position = transform.position;
        position.x = fallStartX + swing;
        position.y -= fallSpeed * Time.deltaTime;
        transform.position = position;

        if (thread != null && foundPin != null)
        {
            thread.SetPosition(0, foundPin.transform.position);
            thread.SetPosition(1, transform.position);
        }

        if (transform.position.y < destroyY)
        {
            FinishOperation();
            Destroy(gameObject);
        }
    }

    private void CreateThread()
    {
        if (foundPin == null)
        {
            return;
        }

        thread = gameObject.AddComponent<LineRenderer>();
        thread.useWorldSpace = true;
        thread.positionCount = 2;
        thread.startWidth = 0.045f;
        thread.endWidth = 0.025f;
        thread.startColor = new Color(1f, 0.9f, 0.25f, 1f);
        thread.endColor = Color.white;
        Shader lineShader = Shader.Find("Sprites/Default");
        if (lineShader != null)
        {
            thread.material = new Material(lineShader);
        }
        thread.sortingOrder = 9;
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
