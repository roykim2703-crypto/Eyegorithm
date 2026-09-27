using UnityEngine;

public enum BallOperation
{
    Insert,
    Delete,
    Search
}

public abstract class TreeBall : MonoBehaviour
{
    [SerializeField] private int number;

    public int Number => number;
    public abstract BallOperation Operation { get; }

    public void Initialize(int value)
    {
        number = value;
        gameObject.name = $"{Operation} Ball ({number})";
    }
}
