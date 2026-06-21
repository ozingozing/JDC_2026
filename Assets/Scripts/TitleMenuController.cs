using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TitleMenuController : MonoBehaviour
{
    private const string DefaultGameplaySceneName = "SampleScene";
    private const string PreviousTestSceneName = "SoundTest";

    [Header("Scene")]
    [SerializeField] private string gameplaySceneName = DefaultGameplaySceneName;

    [Header("Buttons")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button closeSettingsButton;
    [SerializeField] private Button exitButton;

    [Header("Panels")]
    [SerializeField] private GameObject settingsPanel;
    private void Start()
    {
        if (startButton != null) startButton.onClick.AddListener(StartGame);
        if (settingsButton != null) settingsButton.onClick.AddListener(OpenSettings);
        if (closeSettingsButton != null) closeSettingsButton.onClick.AddListener(CloseSettings);
        if (exitButton != null) exitButton.onClick.AddListener(ExitGame);

        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        // 타이틀: 가로 해상도
        ApplyResolution(1920, 1080, ScreenOrientation.LandscapeLeft);

        if (SoundManager.Instance != null)
            SoundManager.Instance.PlayBGM(BGMType.Title_BGM);
    }

    private void OnDestroy()
    {
        if (startButton != null) startButton.onClick.RemoveListener(StartGame);
        if (settingsButton != null) settingsButton.onClick.RemoveListener(OpenSettings);
        if (closeSettingsButton != null) closeSettingsButton.onClick.RemoveListener(CloseSettings);
        if (exitButton != null) exitButton.onClick.RemoveListener(ExitGame);
    }

    public void StartGame()
    {
        PlayUISound(SFXType.Click_SFX);

        // 인게임: 세로 해상도로 전환
        ApplyResolution(1080, 1920, ScreenOrientation.Portrait);

        if (SoundManager.Instance != null)
            SoundManager.Instance.PlayBGM(BGMType.InGame_BGM, true);

        SceneManager.LoadScene(GetGameplaySceneName());
    }

    public void OpenSettings()
    {
        PlayUISound(SFXType.OpenMenu_SFX);

        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
        }
    }

    public void CloseSettings()
    {
        PlayUISound(SFXType.UICancel_SFX);

        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
    }

    public void ExitGame()
    {
        PlayUISound(SFXType.UIExit_SFX);

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private static void ApplyResolution(int width, int height, ScreenOrientation orientation)
    {
#if UNITY_ANDROID || UNITY_IOS
        Screen.orientation = orientation;
#else
        // 모니터 안에 딱 맞는 최소 비율로 세로 창 생성
        float scale = Mathf.Min(
            (float)Screen.currentResolution.width  / width,
            (float)Screen.currentResolution.height / height
        );
        Screen.SetResolution(
            Mathf.RoundToInt(width  * scale),
            Mathf.RoundToInt(height * scale),
            false
        );
#endif
    }

    private static void PlayUISound(SFXType type)
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySFX(type);
        }
    }

    private string GetGameplaySceneName()
    {
        if (string.IsNullOrWhiteSpace(gameplaySceneName) || gameplaySceneName == PreviousTestSceneName)
        {
            return DefaultGameplaySceneName;
        }

        return gameplaySceneName;
    }
}
