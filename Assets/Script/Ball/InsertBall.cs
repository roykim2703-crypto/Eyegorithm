using UnityEngine;
using TMPro;

public sealed class InsertBall : TreeBall
{
    public Sprite pinSprite;
    public float pinScale = 0.15f;
    public float moveSpeed = 4.5f;
    public bool isPin;
    public bool isSameNumber;
    public TreeSpot currentSpot;

    public override BallOperation Operation => BallOperation.Insert;

    private void Start()
    {
        Rigidbody2D rigid = GetComponent<Rigidbody2D>();

        if (rigid != null)
        {
            rigid.AddForce(new Vector2(Random.Range(-0.08f, 0.08f), 0), ForceMode2D.Impulse);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isPin)
        {
            return;
        }

        InsertBall pin = collision.gameObject.GetComponent<InsertBall>();

        if (pin == null || !pin.isPin || pin.currentSpot == null)
        {
            return;
        }

        if (isSameNumber || Number == pin.Number)
        {
            isSameNumber = true;

            Collider2D myCollider = GetComponent<Collider2D>();
            if (myCollider != null)
            {
                Physics2D.IgnoreCollision(myCollider, collision.collider);
            }

            Rigidbody2D sameRigid = GetComponent<Rigidbody2D>();
            if (sameRigid != null)
            {
                sameRigid.linearVelocity = Vector2.down * moveSpeed;
            }

            return;
        }

        TreeSpot nextSpot;

        if (Number < pin.Number)
        {
            nextSpot = pin.currentSpot.leftSpot;
        }
        else
        {
            nextSpot = pin.currentSpot.rightSpot;
        }

        if (nextSpot == null)
        {
            return;
        }

        Rigidbody2D rigid = GetComponent<Rigidbody2D>();
        if (rigid != null)
        {
            Vector2 direction = (nextSpot.transform.position - transform.position).normalized;
            rigid.linearVelocity = direction * moveSpeed;
        }
    }

    public void BecomePin(TreeSpot spot)
    {
        if (isPin)
        {
            return;
        }

        isPin = true;
        currentSpot = spot;
        transform.position = spot.transform.position;
        transform.localScale = Vector3.one * pinScale;
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
            circle.radius = 2.7f;
            circle.isTrigger = false;
        }

        ShowNumber();
    }

    private void ShowNumber()
    {
        GameObject numberObject = new GameObject("NumberText");
        numberObject.transform.SetParent(transform);
        numberObject.transform.localPosition = new Vector3(0, 3.2f, -0.1f);
        numberObject.transform.localScale = Vector3.one;

        TextMeshPro numberText = numberObject.AddComponent<TextMeshPro>();
        numberText.text = Number.ToString();
        numberText.fontSize = 4;
        numberText.alignment = TextAlignmentOptions.Center;
        numberText.color = Color.white;
        numberText.sortingOrder = 11;
        numberText.rectTransform.sizeDelta = new Vector2(8, 3);
    }
}
