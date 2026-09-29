using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TreeManager : MonoBehaviour
{
    public TreeSpot[] spots;
    public GameObject fullText;
    public float lineWidth = 0.055f;
    public Color lineColor = new Color(0.2f, 0.85f, 1f, 0.55f);

    private sealed class PinMove
    {
        public InsertBall pin;
        public TreeSpot from;
        public TreeSpot to;
    }

    private void Start()
    {
        CreateTreeLines();
    }

    public TreeSpot RootSpot => spots == null || spots.Length == 0 ? null : spots[0];

    public TreeSpot GetNextSpot(TreeSpot spot, int value)
    {
        if (spot == null)
        {
            return null;
        }

        return value < spot.number ? spot.leftSpot : spot.rightSpot;
    }

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

    public void DeletePin(TreeSpot target, float moveSpeed, Action finished)
    {
        StartCoroutine(DeletePinRoutine(target, moveSpeed, finished));
    }

    private IEnumerator DeletePinRoutine(TreeSpot target, float moveSpeed, Action finished)
    {
        if (target == null || !target.isFull || target.pin == null)
        {
            finished?.Invoke();
            yield break;
        }

        InsertBall deletedPin = target.pin;
        TreeSpot left = target.leftSpot != null && target.leftSpot.isFull ? target.leftSpot : null;
        TreeSpot right = target.rightSpot != null && target.rightSpot.isFull ? target.rightSpot : null;

        target.ClearPin();
        deletedPin.currentSpot = null;
        BreakPin(deletedPin);
        yield return new WaitForSeconds(0.2f);

        if (left == null && right == null)
        {
            finished?.Invoke();
            yield break;
        }

        if (left == null || right == null)
        {
            yield return ShiftSubtree(left != null ? left : right, target, moveSpeed);
            finished?.Invoke();
            yield break;
        }

        TreeSpot successor = right;
        while (successor.leftSpot != null && successor.leftSpot.isFull)
        {
            successor = successor.leftSpot;
        }

        InsertBall successorPin = successor.pin;
        TreeSpot successorRight = successor.rightSpot != null && successor.rightSpot.isFull ? successor.rightSpot : null;
        successor.ClearPin();

        yield return MoveAlongPath(successorPin.transform, BuildUpPath(successor, target), moveSpeed);

        target.SetPin(successorPin);
        successorPin.currentSpot = target;
        successorPin.targetSpot = null;
        successorPin.transform.position = target.transform.position;

        if (successorRight != null)
        {
            yield return ShiftSubtree(successorRight, successor, moveSpeed);
        }

        finished?.Invoke();
    }

    private void BreakPin(InsertBall pin)
    {
        SpriteRenderer oldRenderer = pin.GetComponent<SpriteRenderer>();
        List<SpriteRenderer> fragments = new List<SpriteRenderer>();

        if (oldRenderer != null && oldRenderer.sprite != null)
        {
            for (int i = 0; i < 7; i++)
            {
                GameObject fragmentObject = new GameObject("Pin Fragment");
                fragmentObject.transform.position = pin.transform.position + (Vector3)UnityEngine.Random.insideUnitCircle * 0.12f;
                fragmentObject.transform.localScale = pin.transform.localScale * UnityEngine.Random.Range(0.25f, 0.42f);
                fragmentObject.transform.rotation = Quaternion.Euler(0, 0, UnityEngine.Random.Range(0f, 360f));

                SpriteRenderer fragmentRenderer = fragmentObject.AddComponent<SpriteRenderer>();
                fragmentRenderer.sprite = oldRenderer.sprite;
                fragmentRenderer.color = oldRenderer.color;
                fragmentRenderer.sortingLayerID = oldRenderer.sortingLayerID;
                fragmentRenderer.sortingOrder = oldRenderer.sortingOrder + 1;
                fragments.Add(fragmentRenderer);

                Rigidbody2D fragmentRigid = fragmentObject.AddComponent<Rigidbody2D>();
                Vector2 direction = new Vector2(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(0.4f, 1.2f)).normalized;
                fragmentRigid.gravityScale = 1.3f;
                fragmentRigid.linearVelocity = direction * UnityEngine.Random.Range(1.4f, 2.4f);
                fragmentRigid.angularVelocity = UnityEngine.Random.Range(-450f, 450f);
            }
        }

        Destroy(pin.gameObject);
        StartCoroutine(FadeFragments(fragments));
    }

    private IEnumerator FadeFragments(List<SpriteRenderer> fragments)
    {
        float time = 0;
        float lifeTime = 0.9f;

        while (time < lifeTime)
        {
            time += Time.deltaTime;
            float alpha = 1f - time / lifeTime;

            for (int i = 0; i < fragments.Count; i++)
            {
                if (fragments[i] == null)
                {
                    continue;
                }

                Color color = fragments[i].color;
                color.a = alpha;
                fragments[i].color = color;
                fragments[i].transform.localScale *= 0.98f;
            }

            yield return null;
        }

        for (int i = 0; i < fragments.Count; i++)
        {
            if (fragments[i] != null)
            {
                Destroy(fragments[i].gameObject);
            }
        }
    }

    private List<Vector3> BuildUpPath(TreeSpot from, TreeSpot to)
    {
        List<Vector3> path = new List<Vector3>();
        TreeSpot current = from.parentSpot;

        while (current != null)
        {
            path.Add(current.transform.position);
            if (current == to)
            {
                break;
            }

            current = current.parentSpot;
        }

        return path;
    }

    private IEnumerator ShiftSubtree(TreeSpot source, TreeSpot destination, float moveSpeed)
    {
        List<PinMove> moves = new List<PinMove>();
        CollectSubtreeMoves(source, destination, moves);

        for (int i = 0; i < moves.Count; i++)
        {
            moves[i].from.ClearPin();
        }

        for (int i = 0; i < moves.Count; i++)
        {
            moves[i].to.SetPin(moves[i].pin);
            moves[i].pin.currentSpot = moves[i].to;
            moves[i].pin.targetSpot = null;
        }

        bool moving = true;
        while (moving)
        {
            moving = false;

            for (int i = 0; i < moves.Count; i++)
            {
                Transform pinTransform = moves[i].pin.transform;
                Vector3 targetPosition = moves[i].to.transform.position;
                pinTransform.position = Vector3.MoveTowards(pinTransform.position, targetPosition, moveSpeed * Time.deltaTime);

                if ((pinTransform.position - targetPosition).sqrMagnitude > 0.0001f)
                {
                    moving = true;
                }
            }

            yield return null;
        }
    }

    private void CollectSubtreeMoves(TreeSpot source, TreeSpot destination, List<PinMove> moves)
    {
        if (source == null || destination == null || !source.isFull || source.pin == null)
        {
            return;
        }

        moves.Add(new PinMove { pin = source.pin, from = source, to = destination });
        CollectSubtreeMoves(source.leftSpot, destination.leftSpot, moves);
        CollectSubtreeMoves(source.rightSpot, destination.rightSpot, moves);
    }

    private IEnumerator MoveAlongPath(Transform item, List<Vector3> path, float moveSpeed)
    {
        for (int i = 0; i < path.Count; i++)
        {
            while ((item.position - path[i]).sqrMagnitude > 0.0001f)
            {
                item.position = Vector3.MoveTowards(item.position, path[i], moveSpeed * Time.deltaTime);
                yield return null;
            }

            item.position = path[i];
        }
    }

    private void CreateTreeLines()
    {
        if (spots == null)
        {
            return;
        }

        GameObject lineRoot = new GameObject("TreeLines");
        lineRoot.transform.SetParent(transform, false);
        Material lineMaterial = new Material(Shader.Find("Sprites/Default"));

        for (int i = 0; i < spots.Length; i++)
        {
            CreateLine(spots[i], spots[i].leftSpot, lineRoot.transform, lineMaterial);
            CreateLine(spots[i], spots[i].rightSpot, lineRoot.transform, lineMaterial);
        }
    }

    private void CreateLine(TreeSpot from, TreeSpot to, Transform parent, Material material)
    {
        if (from == null || to == null)
        {
            return;
        }

        GameObject lineObject = new GameObject(from.name + " - " + to.name);
        lineObject.transform.SetParent(parent, false);
        LineRenderer line = lineObject.AddComponent<LineRenderer>();
        line.useWorldSpace = true;
        line.positionCount = 2;
        line.SetPosition(0, from.transform.position);
        line.SetPosition(1, to.transform.position);
        line.startWidth = lineWidth;
        line.endWidth = lineWidth;
        line.startColor = lineColor;
        line.endColor = lineColor;
        line.material = material;
        line.sortingOrder = 1;
    }

    public void ShowFull()
    {
        ShowMessage("FULL", new Color(1f, 0.1f, 0.7f, 1f));
    }

    public void ShowAlreadyExists(int number)
    {
        ShowMessage("ALREADY EXISTS : " + number, new Color(1f, 0.65f, 0.1f, 1f));
    }

    public void ShowSearchResult(bool found, int number)
    {
        if (found)
        {
            ShowMessage("FOUND : " + number, new Color(0.2f, 1f, 0.55f, 1f));
        }
        else
        {
            ShowMessage("NOT FOUND : " + number, new Color(1f, 0.35f, 0.35f, 1f));
        }
    }

    private void ShowMessage(string message, Color color)
    {
        Debug.Log(message);

        if (fullText == null)
        {
            return;
        }

        TMP_Text text = fullText.GetComponent<TMP_Text>();
        if (text != null)
        {
            text.text = message;
            text.color = color;
        }

        fullText.SetActive(true);
        CancelInvoke("HideMessage");
        Invoke("HideMessage", 2f);
    }

    private void HideMessage()
    {
        fullText.SetActive(false);
    }
}
