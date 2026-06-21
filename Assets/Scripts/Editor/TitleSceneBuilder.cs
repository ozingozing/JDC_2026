using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

public static class TitleSceneBuilder
{
    private const string TitleScenePath = "Assets/Scenes/Title.unity";
    private const string GameplayScenePath = "Assets/Scenes/SampleScene.unity";

    [MenuItem("Tools/JDC/Build Title Scene")]
    public static void BuildTitleScene()
    {
        Scene scene = EditorSceneManager.OpenScene(TitleScenePath, OpenSceneMode.Single);

        ClearGeneratedObjects();

        Camera camera = Camera.main;
        if (camera == null)
        {
            GameObject cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            camera = cameraObject.AddComponent<Camera>();
            cameraObject.AddComponent<AudioListener>();
        }

        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.02f, 0.12f, 0.19f);
        camera.transform.position = new Vector3(0f, 0f, -10f);

        Canvas canvas = CreateCanvas();
        CreateBackground(canvas.transform);

        Image logo = CreateImage(
            "Game Logo",
            canvas.transform,
            LoadSprite("Assets/UI/Game Logo.png"),
            new Vector2(0.5f, 0.72f),
            new Vector2(620f, 220f)
        );
        logo.preserveAspect = true;

        Button startButton = CreateImageButton(
            "Start Button",
            canvas.transform,
            LoadSprite("Assets/UI/Start.png"),
            new Vector2(0.5f, 0.42f),
            new Vector2(260f, 88f)
        );

        Button settingsButton = CreateImageButton(
            "Settings Button",
            canvas.transform,
            LoadSprite("Assets/UI/Setting.png"),
            new Vector2(0.5f, 0.3f),
            new Vector2(260f, 88f)
        );

        Button exitButton = CreateImageButton(
            "Exit Button",
            canvas.transform,
            LoadSprite("Assets/UI/Exit.png"),
            new Vector2(0.5f, 0.18f),
            new Vector2(260f, 88f)
        );

        GameObject settingsPanel = CreateSettingsPanel(canvas.transform, out Button closeSettingsButton);
        CreateEventSystem();

        SoundManager soundManager = CreateSoundManager();

        GameObject controllerObject = new GameObject("TitleMenuController");
        TitleMenuController controller = controllerObject.AddComponent<TitleMenuController>();
        SerializedObject controllerObjectData = new SerializedObject(controller);
        controllerObjectData.FindProperty("gameplaySceneName").stringValue = "SampleScene";
        controllerObjectData.FindProperty("startButton").objectReferenceValue = startButton;
        controllerObjectData.FindProperty("settingsButton").objectReferenceValue = settingsButton;
        controllerObjectData.FindProperty("closeSettingsButton").objectReferenceValue = closeSettingsButton;
        controllerObjectData.FindProperty("exitButton").objectReferenceValue = exitButton;
        controllerObjectData.FindProperty("settingsPanel").objectReferenceValue = settingsPanel;
        controllerObjectData.ApplyModifiedPropertiesWithoutUndo();

        AssignSoundClips(soundManager);
        UpdateBuildSettings();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();

        Debug.Log("Title scene was built successfully.");
    }

    private static void ClearGeneratedObjects()
    {
        string[] generatedNames =
        {
            "Title Canvas",
            "EventSystem",
            "SoundManager",
            "TitleMenuController"
        };

        foreach (string generatedName in generatedNames)
        {
            GameObject existingObject = GameObject.Find(generatedName);
            if (existingObject != null)
            {
                Object.DestroyImmediate(existingObject);
            }
        }
    }

    private static Canvas CreateCanvas()
    {
        GameObject canvasObject = new GameObject("Title Canvas");
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        canvasObject.AddComponent<GraphicRaycaster>();
        return canvas;
    }

    private static void CreateBackground(Transform parent)
    {
        GameObject backgroundObject = new GameObject("Deep Sea Background");
        backgroundObject.transform.SetParent(parent, false);

        Image background = backgroundObject.AddComponent<Image>();
        background.color = new Color(0.01f, 0.08f, 0.14f, 1f);

        RectTransform rectTransform = background.rectTransform;
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
    }

    private static GameObject CreateSettingsPanel(Transform parent, out Button closeButton)
    {
        GameObject panelObject = new GameObject("Settings Panel");
        panelObject.transform.SetParent(parent, false);

        Image panel = panelObject.AddComponent<Image>();
        panel.color = new Color(0f, 0f, 0f, 0.72f);

        RectTransform panelTransform = panel.rectTransform;
        panelTransform.anchorMin = new Vector2(0.5f, 0.5f);
        panelTransform.anchorMax = new Vector2(0.5f, 0.5f);
        panelTransform.anchoredPosition = Vector2.zero;
        panelTransform.sizeDelta = new Vector2(680f, 420f);

        Text title = CreateText("Settings Title", panelObject.transform, "SETTING", 44, new Vector2(0.5f, 0.68f));
        title.color = Color.white;

        Text body = CreateText("Settings Body", panelObject.transform, "Volume settings can be connected here later.", 26, new Vector2(0.5f, 0.5f));
        body.color = new Color(0.85f, 0.93f, 1f);

        closeButton = CreateTextButton("Close Settings Button", panelObject.transform, "BACK", new Vector2(0.5f, 0.25f), new Vector2(220f, 72f));
        panelObject.SetActive(false);

        return panelObject;
    }

    private static Image CreateImage(string name, Transform parent, Sprite sprite, Vector2 anchor, Vector2 size)
    {
        GameObject imageObject = new GameObject(name);
        imageObject.transform.SetParent(parent, false);

        Image image = imageObject.AddComponent<Image>();
        image.sprite = sprite;
        image.preserveAspect = true;

        RectTransform rectTransform = image.rectTransform;
        rectTransform.anchorMin = anchor;
        rectTransform.anchorMax = anchor;
        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.sizeDelta = size;

        return image;
    }

    private static Button CreateImageButton(string name, Transform parent, Sprite sprite, Vector2 anchor, Vector2 size)
    {
        Image image = CreateImage(name, parent, sprite, anchor, size);
        Button button = image.gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        return button;
    }

    private static Button CreateTextButton(string name, Transform parent, string label, Vector2 anchor, Vector2 size)
    {
        GameObject buttonObject = new GameObject(name);
        buttonObject.transform.SetParent(parent, false);

        Image image = buttonObject.AddComponent<Image>();
        image.color = new Color(0.08f, 0.42f, 0.58f, 1f);

        Button button = buttonObject.AddComponent<Button>();
        button.targetGraphic = image;

        RectTransform rectTransform = image.rectTransform;
        rectTransform.anchorMin = anchor;
        rectTransform.anchorMax = anchor;
        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.sizeDelta = size;

        Text text = CreateText("Label", buttonObject.transform, label, 30, new Vector2(0.5f, 0.5f));
        text.color = Color.white;
        text.rectTransform.sizeDelta = size;

        return button;
    }

    private static Text CreateText(string name, Transform parent, string value, int fontSize, Vector2 anchor)
    {
        GameObject textObject = new GameObject(name);
        textObject.transform.SetParent(parent, false);

        Text text = textObject.AddComponent<Text>();
        text.text = value;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = fontSize;
        text.alignment = TextAnchor.MiddleCenter;

        RectTransform rectTransform = text.rectTransform;
        rectTransform.anchorMin = anchor;
        rectTransform.anchorMax = anchor;
        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.sizeDelta = new Vector2(640f, 90f);

        return text;
    }

    private static void CreateEventSystem()
    {
        GameObject eventSystemObject = new GameObject("EventSystem");
        eventSystemObject.AddComponent<EventSystem>();

#if ENABLE_INPUT_SYSTEM
        eventSystemObject.AddComponent<InputSystemUIInputModule>();
#else
        eventSystemObject.AddComponent<StandaloneInputModule>();
#endif
    }

    private static SoundManager CreateSoundManager()
    {
        GameObject soundManagerObject = new GameObject("SoundManager");
        soundManagerObject.AddComponent<AudioSource>();
        soundManagerObject.AddComponent<AudioSource>();
        return soundManagerObject.AddComponent<SoundManager>();
    }

    private static void AssignSoundClips(SoundManager soundManager)
    {
        SerializedObject soundManagerData = new SerializedObject(soundManager);
        AssignBGM(soundManagerData, 0, BGMType.Title_BGM, "Assets/Sounds/BGM/BGM 1.mp3");
        AssignBGM(soundManagerData, 1, BGMType.InGame_BGM, "Assets/Sounds/BGM/BGM 2.mp3");
        soundManagerData.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void AssignBGM(SerializedObject soundManagerData, int index, BGMType type, string clipPath)
    {
        SerializedProperty bgmClips = soundManagerData.FindProperty("bgmClips");
        if (bgmClips == null || bgmClips.arraySize <= index)
        {
            return;
        }

        SerializedProperty entry = bgmClips.GetArrayElementAtIndex(index);
        entry.FindPropertyRelative("type").enumValueIndex = (int)type;
        entry.FindPropertyRelative("clip").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>(clipPath);
        entry.FindPropertyRelative("volume").floatValue = 1f;
    }

    private static void UpdateBuildSettings()
    {
        List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>
        {
            new EditorBuildSettingsScene(TitleScenePath, true),
            new EditorBuildSettingsScene(GameplayScenePath, true)
        };

        EditorBuildSettings.scenes = scenes.ToArray();
    }

    private static Sprite LoadSprite(string path)
    {
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }
}
