#if UNITY_EDITOR
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class BagatreeTreeSetup
{
    private const string ScenePath = "Assets/Scenes/SampleScene.unity";
    private const string BallPrefabPath = "Assets/prefeb/InsertBall.prefab";
    private const string BallMaterialPath = "Assets/prefeb/Pinball.physicsMaterial2D";

    private static readonly Vector3[] SpotPositions =
    {
        new Vector3(-2.78f, 3.0f, -0.1f),

        new Vector3(-4.65f, 1.35f, -0.1f),
        new Vector3(-0.90f, 1.35f, -0.1f),

        new Vector3(-6.20f, -0.45f, -0.1f),
        new Vector3(-3.80f, -0.45f, -0.1f),
        new Vector3(-1.75f, -0.45f, -0.1f),
        new Vector3(0.65f, -0.45f, -0.1f),

        new Vector3(-7.00f, -2.00f, -0.1f),
        new Vector3(-5.75f, -2.00f, -0.1f),
        new Vector3(-4.45f, -2.00f, -0.1f),
        new Vector3(-3.25f, -2.00f, -0.1f),
        new Vector3(-1.90f, -2.00f, -0.1f),
        new Vector3(-0.65f, -2.00f, -0.1f),
        new Vector3(0.60f, -2.00f, -0.1f),
        new Vector3(1.85f, -2.00f, -0.1f)
    };

    [MenuItem("Tools/Bagatree/Setup Insert Tree")]
    public static void Setup()
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

        InsertBall ballPrefab = CreateBallPrefab();
        GameObject treeObject = FindInScene(scene, "TreeSystem");

        if (treeObject == null)
        {
            treeObject = new GameObject("TreeSystem");
            Undo.RegisterCreatedObjectUndo(treeObject, "Create Tree System");
        }

        TreeManager treeManager = GetOrAdd<TreeManager>(treeObject);
        TreeSpot[] spots = new TreeSpot[SpotPositions.Length];

        for (int i = 0; i < SpotPositions.Length; i++)
        {
            GameObject spotObject = FindChild(treeObject.transform, "TreeSpot " + i);

            if (spotObject == null)
            {
                spotObject = new GameObject("TreeSpot " + i);
                spotObject.transform.SetParent(treeObject.transform);
            }

            spotObject.transform.position = SpotPositions[i];

            CircleCollider2D trigger = GetOrAdd<CircleCollider2D>(spotObject);
            trigger.isTrigger = true;
            trigger.radius = 0.4f;

            spots[i] = GetOrAdd<TreeSpot>(spotObject);
            spots[i].isFull = false;
            spots[i].number = 0;
        }

        for (int i = 0; i < spots.Length; i++)
        {
            spots[i].parentSpot = i == 0 ? null : spots[(i - 1) / 2];
            spots[i].leftSpot = i * 2 + 1 < spots.Length ? spots[i * 2 + 1] : null;
            spots[i].rightSpot = i * 2 + 2 < spots.Length ? spots[i * 2 + 2] : null;
            EditorUtility.SetDirty(spots[i]);
        }

        treeManager.spots = spots;

        GameObject floorObject = FindChild(treeObject.transform, "TreeFloor");
        if (floorObject == null)
        {
            floorObject = new GameObject("TreeFloor");
            floorObject.transform.SetParent(treeObject.transform);
        }

        floorObject.transform.position = new Vector3(-2.78f, -4.3f, 0);
        BoxCollider2D floorCollider = GetOrAdd<BoxCollider2D>(floorObject);
        floorCollider.isTrigger = true;
        floorCollider.size = new Vector2(17f, 0.6f);

        TreeFloor floor = GetOrAdd<TreeFloor>(floorObject);
        floor.treeManager = treeManager;

        GameObject spawnPointObject = FindChild(treeObject.transform, "SpawnPoint");
        if (spawnPointObject == null)
        {
            spawnPointObject = new GameObject("SpawnPoint");
            spawnPointObject.transform.SetParent(treeObject.transform);
        }

        spawnPointObject.transform.position = new Vector3(-2.78f, 4.25f, -0.1f);

        GameObject spawnerObject = FindChild(treeObject.transform, "BallSpawner");
        if (spawnerObject == null)
        {
            spawnerObject = new GameObject("BallSpawner");
            spawnerObject.transform.SetParent(treeObject.transform);
        }

        Ballspawn ballSpawner = GetOrAdd<Ballspawn>(spawnerObject);
        ballSpawner.spawnPoint = spawnPointObject.transform;
        ballSpawner.insertBallPrefab = ballPrefab;

        GameObject keyPad = FindInScene(scene, "KeyPad");
        if (keyPad == null)
        {
            Debug.LogError("KeyPad 오브젝트를 찾지 못했습니다.");
            return;
        }

        BallNumber ballNumber = GetOrAdd<BallNumber>(keyPad);
        BallManager ballManager = GetOrAdd<BallManager>(keyPad);
        ballManager.ballNumber = ballNumber;
        ballManager.ballSpawner = ballSpawner;

        GameObject fullObject = CreateFullText(keyPad.transform);
        treeManager.fullText = fullObject;

        Button insertButton = Resources.FindObjectsOfTypeAll<Button>()
            .FirstOrDefault(button => button.gameObject.scene == scene && button.gameObject.name == "Insert");

        if (insertButton != null && !HasListener(insertButton, ballManager, "CreateInsertBall"))
        {
            UnityEventTools.AddPersistentListener(insertButton.onClick, ballManager.CreateInsertBall);
            EditorUtility.SetDirty(insertButton);
        }

        EditorUtility.SetDirty(treeManager);
        EditorUtility.SetDirty(floor);
        EditorUtility.SetDirty(ballSpawner);
        EditorUtility.SetDirty(ballManager);
        EditorSceneManager.MarkSceneDirty(scene);
        SceneView.RepaintAll();

        Debug.Log("삽입 트리 설정 완료: 자리 15개, 바닥, 삽입 공, FULL 표시를 만들었습니다. Ctrl+S로 저장하세요.");
    }

    private static InsertBall CreateBallPrefab()
    {
        Sprite ballSprite = LoadSprite("Assets/image/구슬.png", "구슬_0");
        Sprite pinSprite = LoadSprite("Assets/image/핀.png", "핀_0");

        PhysicsMaterial2D material = AssetDatabase.LoadAssetAtPath<PhysicsMaterial2D>(BallMaterialPath);
        if (material == null)
        {
            material = new PhysicsMaterial2D("Pinball");
            material.bounciness = 0.85f;
            material.friction = 0.05f;
            AssetDatabase.CreateAsset(material, BallMaterialPath);
        }

        GameObject ballObject = new GameObject("InsertBall");
        ballObject.transform.localScale = Vector3.one * 0.23f;

        SpriteRenderer renderer = ballObject.AddComponent<SpriteRenderer>();
        renderer.sprite = ballSprite;
        renderer.sortingOrder = 10;

        Rigidbody2D rigid = ballObject.AddComponent<Rigidbody2D>();
        rigid.gravityScale = 1.8f;
        rigid.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rigid.interpolation = RigidbodyInterpolation2D.Interpolate;

        CircleCollider2D collider = ballObject.AddComponent<CircleCollider2D>();
        collider.radius = 1.45f;
        collider.sharedMaterial = material;

        InsertBall insertBall = ballObject.AddComponent<InsertBall>();
        insertBall.pinSprite = pinSprite;
        insertBall.pinScale = 0.15f;

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(ballObject, BallPrefabPath);
        Object.DestroyImmediate(ballObject);

        return prefab.GetComponent<InsertBall>();
    }

    private static GameObject CreateFullText(Transform canvas)
    {
        Transform oldText = canvas.Find("FullText");
        if (oldText != null)
        {
            return oldText.gameObject;
        }

        GameObject textObject = new GameObject("FullText", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(canvas, false);

        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = new Vector2(-300f, 0);
        rect.sizeDelta = new Vector2(500f, 160f);

        TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
        text.text = "FULL";
        text.fontSize = 80;
        text.fontStyle = FontStyles.Bold;
        text.alignment = TextAlignmentOptions.Center;
        text.color = new Color(1f, 0.1f, 0.7f, 1f);
        text.raycastTarget = false;

        textObject.SetActive(false);
        return textObject;
    }

    private static Sprite LoadSprite(string assetPath, string spriteName)
    {
        return AssetDatabase.LoadAllAssetsAtPath(assetPath)
            .OfType<Sprite>()
            .FirstOrDefault(sprite => sprite.name == spriteName);
    }

    private static GameObject FindInScene(Scene scene, string objectName)
    {
        return scene.GetRootGameObjects()
            .SelectMany(root => root.GetComponentsInChildren<Transform>(true))
            .Select(item => item.gameObject)
            .FirstOrDefault(item => item.name == objectName);
    }

    private static GameObject FindChild(Transform parent, string objectName)
    {
        Transform child = parent.Find(objectName);
        return child == null ? null : child.gameObject;
    }

    private static T GetOrAdd<T>(GameObject target) where T : Component
    {
        T component = target.GetComponent<T>();
        return component == null ? Undo.AddComponent<T>(target) : component;
    }

    private static bool HasListener(Button button, BallManager target, string methodName)
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
