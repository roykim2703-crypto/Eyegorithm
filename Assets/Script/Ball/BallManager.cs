using UnityEngine;
using UnityEngine.UI;

public class BallManager : MonoBehaviour
{
    public BallNumber ballNumber;
    public Ballspawn ballSpawner;
    private GameObject selectUI;
    private bool oldBallFalling;
    private bool firstCheck = true;

    private void Awake()
    {
        if (Camera.main != null && Camera.main.GetComponent<FixedAspectCamera>() == null)
        {
            Camera.main.gameObject.AddComponent<FixedAspectCamera>();
        }

        AddButtonListener("Search", CreateSearchBall);
        AddButtonListener("Delate", CreateDeleteBall);
        AddButtonListener("Delete", CreateDeleteBall);
    }

    private void AddButtonListener(string buttonName, UnityEngine.Events.UnityAction action)
    {
        Button[] buttons = FindObjectsByType<Button>(FindObjectsInactive.Include);

        for (int i = 0; i < buttons.Length; i++)
        {
            if (buttons[i].gameObject.name == buttonName)
            {
                buttons[i].onClick.AddListener(action);
                return;
            }
        }
    }

    private void Update()
    {
        TreeBall[] balls = FindObjectsByType<TreeBall>();
        bool ballFalling = false;

        for (int i = 0; i < balls.Length; i++)
        {
            InsertBall insertBall = balls[i] as InsertBall;

            if (insertBall == null || !insertBall.isPin)
            {
                ballFalling = true;
                break;
            }
        }

        if (firstCheck || oldBallFalling != ballFalling)
        {
            ShowOperationUI(!ballFalling);
            oldBallFalling = ballFalling;
            firstCheck = false;
        }
    }

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

    public void ShowOperationUI(bool show)
    {
        if (selectUI == null)
        {
            GameObject[] roots = gameObject.scene.GetRootGameObjects();

            for (int i = 0; i < roots.Length; i++)
            {
                if (roots[i].name == "Select")
                {
                    selectUI = roots[i];
                    break;
                }
            }
        }

        if (selectUI != null)
        {
            selectUI.SetActive(show);
        }
    }

    public bool IsOperationUIVisible()
    {
        return selectUI != null && selectUI.activeSelf;
    }
}
