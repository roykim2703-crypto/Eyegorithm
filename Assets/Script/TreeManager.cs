using UnityEngine;

public class TreeManager : MonoBehaviour
{
    public TreeSpot[] spots;
    public GameObject fullText;

    public bool IsFull()
    {
        for (int i = 0; i < spots.Length; i++)
        {
            if (!spots[i].isFull)
            {
                return false;
            }
        }

        return true;
    }

    public void ShowFull()
    {
        Debug.Log("FULL");

        if (fullText == null)
        {
            return;
        }

        fullText.SetActive(true);
        CancelInvoke("HideFull");
        Invoke("HideFull", 2f);
    }

    private void HideFull()
    {
        fullText.SetActive(false);
    }
}
