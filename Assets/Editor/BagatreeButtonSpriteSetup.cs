#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class BagatreeButtonSpriteSetup
{
    private const string ScenePath = "Assets/Scenes/SampleScene.unity";
    private const string NumberNormalPath = "Assets/image/숫자.png";
    private const string NumberPressedPath = "Assets/image/숫자 눌림.png";
    private const string ActionNormalPath = "Assets/image/버튼.png";
    private const string ActionPressedPath = "Assets/image/버튼 눌림.png";
    private const string ControlPath = "Assets/image/c-눌림.png";

    [MenuItem("Tools/Bagatree/Configure Button Sprite Animation")]
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

        Dictionary<string, Sprite> numberNormal = LoadSprites(NumberNormalPath);
        Dictionary<string, Sprite> numberPressed = LoadSprites(NumberPressedPath);
        Dictionary<string, Sprite> actionNormal = LoadSprites(ActionNormalPath);
        Dictionary<string, Sprite> actionPressed = LoadSprites(ActionPressedPath);
        Dictionary<string, Sprite> controls = LoadSprites(ControlPath);

        Button[] buttons = Resources.FindObjectsOfTypeAll<Button>()
            .Where(button => button.gameObject.scene == scene)
            .ToArray();

        int configuredCount = 0;

        foreach (Button button in buttons)
        {
            if (TryGetSprites(
                    button.gameObject.name,
                    numberNormal,
                    numberPressed,
                    actionNormal,
                    actionPressed,
                    controls,
                    out Sprite normal,
                    out Sprite pressed))
            {
                ConfigureButton(button, normal, pressed);
                configuredCount++;
            }
        }

        if (configuredCount > 0)
        {
            EditorSceneManager.MarkSceneDirty(scene);
            SceneView.RepaintAll();
            Debug.Log($"Bagatree 버튼 {configuredCount}개의 눌림 스프라이트를 연결했습니다. Ctrl+S로 씬을 저장하세요.");
        }
    }

    private static bool TryGetSprites(
        string objectName,
        IReadOnlyDictionary<string, Sprite> numberNormal,
        IReadOnlyDictionary<string, Sprite> numberPressed,
        IReadOnlyDictionary<string, Sprite> actionNormal,
        IReadOnlyDictionary<string, Sprite> actionPressed,
        IReadOnlyDictionary<string, Sprite> controls,
        out Sprite normal,
        out Sprite pressed)
    {
        normal = null;
        pressed = null;

        if (int.TryParse(objectName, out int digit) && digit >= 0 && digit <= 9)
        {
            string index = digit == 0 ? "10" : (digit - 1).ToString();
            return TryPair(
                numberNormal, $"숫자_{index}",
                numberPressed, $"숫자 눌림_{index}",
                out normal, out pressed);
        }

        switch (objectName)
        {
            case "Insert":
                return TryPair(actionNormal, "버튼_0", actionPressed, "버튼 눌림_0", out normal, out pressed);

            case "Delate":
            case "Delete":
                return TryPair(actionNormal, "버튼_1", actionPressed, "버튼 눌림_1", out normal, out pressed);

            case "Search":
                return TryPair(actionNormal, "버튼_2", actionPressed, "버튼 눌림_2", out normal, out pressed);

            case "C":
                return TryPair(controls, "c-눌림_0", controls, "c-눌림_2", out normal, out pressed);

            case "del":
            case "DEL":
                return TryPair(controls, "c-눌림_1", controls, "c-눌림_3", out normal, out pressed);

            default:
                return false;
        }
    }

    private static void ConfigureButton(Button button, Sprite normal, Sprite pressed)
    {
        Image image = button.targetGraphic as Image;
        if (image == null)
        {
            image = button.GetComponent<Image>();
        }

        if (image == null)
        {
            Debug.LogWarning($"{button.name}: Button에 Image가 없어 스프라이트를 연결할 수 없습니다.", button);
            return;
        }

        Undo.RecordObjects(new UnityEngine.Object[] { button, image }, "Configure Bagatree Button Sprites");

        image.sprite = normal;
        button.targetGraphic = image;
        button.transition = Selectable.Transition.SpriteSwap;

        SpriteState state = button.spriteState;
        state.highlightedSprite = normal;
        state.pressedSprite = pressed;
        state.selectedSprite = normal;
        button.spriteState = state;

        EditorUtility.SetDirty(image);
        EditorUtility.SetDirty(button);
    }

    private static Dictionary<string, Sprite> LoadSprites(string assetPath)
    {
        return AssetDatabase.LoadAllAssetsAtPath(assetPath)
            .OfType<Sprite>()
            .ToDictionary(sprite => sprite.name, sprite => sprite, StringComparer.Ordinal);
    }

    private static bool TryPair(
        IReadOnlyDictionary<string, Sprite> normalSprites,
        string normalName,
        IReadOnlyDictionary<string, Sprite> pressedSprites,
        string pressedName,
        out Sprite normal,
        out Sprite pressed)
    {
        normal = null;
        pressed = null;

        return normalSprites.TryGetValue(normalName, out normal)
            && pressedSprites.TryGetValue(pressedName, out pressed);
    }
}
#endif
