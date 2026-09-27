using UnityEngine;

public class Ballspawn : MonoBehaviour
{
    [Header("Spawn Settings")]
    public Transform spawnPoint;

    [Header("Ball Prefabs")]
    public InsertBall insertBallPrefab;
    public DeleteBall deleteBallPrefab;
    public SearchBall searchBallPrefab;

    public TreeBall Spawn(BallOperation operation, int number)
    {
        TreeBall prefab = GetPrefab(operation);

        if (prefab == null)
        {
            Debug.LogError($"{operation} 공 프리팹이 연결되지 않았습니다.", this);
            return null;
        }

        Transform point = spawnPoint != null ? spawnPoint : transform;
        TreeBall ball = Instantiate(prefab, point.position, point.rotation);
        ball.Initialize(number);
        return ball;
    }

    private TreeBall GetPrefab(BallOperation operation)
    {
        switch (operation)
        {
            case BallOperation.Insert:
                return insertBallPrefab;
            case BallOperation.Delete:
                return deleteBallPrefab;
            case BallOperation.Search:
                return searchBallPrefab;
            default:
                return null;
        }
    }
}
