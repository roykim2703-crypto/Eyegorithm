using UnityEngine;

public class TreeSpot : MonoBehaviour
{
    public TreeSpot parentSpot;
    public TreeSpot leftSpot;
    public TreeSpot rightSpot;
    public bool isFull;
    public int number;
    public InsertBall pin;

    public void SetPin(InsertBall newPin)
    {
        pin = newPin;
        isFull = newPin != null;
        number = newPin == null ? 0 : newPin.Number;
    }

    public void ClearPin()
    {
        pin = null;
        isFull = false;
        number = 0;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        InsertBall ball = other.GetComponent<InsertBall>();

        if (ball == null || ball.isPin || ball.isSameNumber || isFull)
        {
            return;
        }

        if (ball.targetSpot != this)
        {
            return;
        }

        if (parentSpot != null && !parentSpot.isFull)
        {
            return;
        }

        ball.BecomePin(this);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = isFull ? Color.red : Color.green;
        Gizmos.DrawWireSphere(transform.position, 0.4f);
    }
}
