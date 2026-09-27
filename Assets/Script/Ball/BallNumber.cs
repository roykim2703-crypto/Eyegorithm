using UnityEngine;
using TMPro;

public class BallNumber : MonoBehaviour
{
    public string ballnumber;
    public TMP_Text numberText;

    private void Awake()
    {
        if (numberText == null)
        {
            Transform numberObject = transform.Find("숫자");

            if (numberObject != null)
            {
                numberText = numberObject.GetComponent<TMP_Text>();
            }
        }

        ShowNumber();
    }

    public void AddNumber(int number)
    {
        if (number < 0 || number > 9)
        {
            Debug.Log("숫자 에러");
            return;
        }

        ballnumber += number.ToString();
        ShowNumber();
    }

    public void AddNumber0()
    {
        if (string.IsNullOrEmpty(ballnumber))
        {
            return;
        }

        AddNumber(0);
    }

    public void AddNumber1() => AddNumber(1);
    public void AddNumber2() => AddNumber(2);
    public void AddNumber3() => AddNumber(3);
    public void AddNumber4() => AddNumber(4);
    public void AddNumber5() => AddNumber(5);
    public void AddNumber6() => AddNumber(6);
    public void AddNumber7() => AddNumber(7);
    public void AddNumber8() => AddNumber(8);
    public void AddNumber9() => AddNumber(9);

    public void DeleteNumber()
    {
        if (string.IsNullOrEmpty(ballnumber))
        {
            return;
        }

        ballnumber = ballnumber.Remove(0, 1);
        ShowNumber();
    }

    public void ClearNumber()
    {
        ballnumber = null;
        ShowNumber();
    }

    private void ShowNumber()
    {
        if (numberText != null)
        {
            numberText.text = ballnumber;
        }
    }
}
