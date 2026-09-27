using UnityEngine;

public class BallManager : MonoBehaviour
{
    public BallNumber ballNumber;
    public Ballspawn ballSpawner;

    public void CreateInsertBall()
    {
        CreateBall(BallOperation.Insert);
    }

    public void CreateDeleteBall()
    {
        CreateBall(BallOperation.Delete);
    }

    public void CreateSearchBall()
    {
        CreateBall(BallOperation.Search);
    }

    private void CreateBall(BallOperation operation)
    {
        if (ballNumber == null || ballSpawner == null)
        {
            Debug.LogError("BallNumber 또는 Ballspawn이 연결되지 않았습니다.", this);
            return;
        }

        if (!int.TryParse(ballNumber.ballnumber, out int number))
        {
            Debug.LogWarning("공에 사용할 숫자를 먼저 입력하세요.", this);
            return;
        }

        TreeBall createdBall = ballSpawner.Spawn(operation, number);

        if (createdBall != null)
        {
            ballNumber.ClearNumber();
        }
    }
}
