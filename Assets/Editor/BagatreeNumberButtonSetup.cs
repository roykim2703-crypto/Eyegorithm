#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class BagatreeNumberButtonSetup
{
    private const string ScenePath = "Assets/Scenes/SampleScene.unity";

    [MenuItem("Tools/Bagatree/Configure Number Button Events")]
    public static void ConfigureOpenSampleScene()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            return;
        }

        Scene scene = SceneManager.GetActiveScene();
        if (!scene.IsValid() || scene.path != ScenePath)
        {
            return;
        }

        GameObject keyPad = scene.GetRootGameObjects()
            .SelectMany(root => root.GetComponentsInChildren<Transform>(true))
            .Select(item => item.gameObject)
            .FirstOrDefault(item => item.name == "KeyPad");

        if (keyPad == null)
        {
            Debug.LogError("KeyPad 오브젝트를 찾지 못했습니다.");
            return;
        }

        BallNumber ballNumber = keyPad.GetComponent<BallNumber>();
        if (ballNumber == null)
        {
            ballNumber = Undo.AddComponent<BallNumber>(keyPad);
        }

        Button[] buttons = Resources.FindObjectsOfTypeAll<Button>()
            .Where(button => button.gameObject.scene == scene)
            .ToArray();

        int connectedCount = 0;

        foreach (Button button in buttons)
        {
            if (!int.TryParse(button.gameObject.name, out int digit) || digit < 0 || digit > 9)
            {
                continue;
            }

            UnityAction action = GetNumberAction(ballNumber, digit);
            string methodName = $"AddNumber{digit}";

            if (!HasPersistentListener(button, ballNumber, methodName))
            {
                Undo.RecordObject(button, "Connect Bagatree Number Button");
                UnityEventTools.AddPersistentListener(button.onClick, action);
                EditorUtility.SetDirty(button);
            }

            connectedCount++;
        }

        Button deleteButton = buttons.FirstOrDefault(button => button.gameObject.name == "del");
        if (deleteButton != null && !HasPersistentListener(deleteButton, ballNumber, "DeleteNumber"))
        {
            Undo.RecordObject(deleteButton, "Connect Bagatree Delete Button");
            UnityEventTools.AddPersistentListener(deleteButton.onClick, ballNumber.DeleteNumber);
            EditorUtility.SetDirty(deleteButton);
        }

        Button clearButton = buttons.FirstOrDefault(button => button.gameObject.name == "C");
        if (clearButton != null && !HasPersistentListener(clearButton, ballNumber, "ClearNumber"))
        {
            Undo.RecordObject(clearButton, "Connect Bagatree Clear Button");
            UnityEventTools.AddPersistentListener(clearButton.onClick, ballNumber.ClearNumber);
            EditorUtility.SetDirty(clearButton);
        }

        EditorUtility.SetDirty(ballNumber);
        EditorSceneManager.MarkSceneDirty(scene);
        SceneView.RepaintAll();
        Debug.Log($"숫자 버튼 {connectedCount}개와 del, C 버튼을 BallNumber에 연결했습니다. Ctrl+S로 씬을 저장하세요.");
    }

    private static UnityAction GetNumberAction(BallNumber ballNumber, int digit)
    {
        switch (digit)
        {
            case 0: return ballNumber.AddNumber0;
            case 1: return ballNumber.AddNumber1;
            case 2: return ballNumber.AddNumber2;
            case 3: return ballNumber.AddNumber3;
            case 4: return ballNumber.AddNumber4;
            case 5: return ballNumber.AddNumber5;
            case 6: return ballNumber.AddNumber6;
            case 7: return ballNumber.AddNumber7;
            case 8: return ballNumber.AddNumber8;
            case 9: return ballNumber.AddNumber9;
            default: return null;
        }
    }

    private static bool HasPersistentListener(Button button, BallNumber target, string methodName)
    {
        for (int i = 0; i < button.onClick.GetPersistentEventCount(); i++)
        {
            if (button.onClick.GetPersistentTarget(i) == target
                && button.onClick.GetPersistentMethodName(i) == methodName)
            {
                return true;
            }
        }

        return false;
    }
}
#endif
