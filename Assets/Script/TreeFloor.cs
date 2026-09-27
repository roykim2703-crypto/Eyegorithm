using UnityEngine;

public class TreeFloor : MonoBehaviour
{
    public TreeManager treeManager;

    private void OnTriggerEnter2D(Collider2D other)
    {
        InsertBall ball = other.GetComponent<InsertBall>();

        if (ball == null || ball.isPin)
        {
            return;
        }

        if (treeManager != null && treeManager.IsFull())
        {
            treeManager.ShowFull();
        }

        Destroy(ball.gameObject);
    }
}
