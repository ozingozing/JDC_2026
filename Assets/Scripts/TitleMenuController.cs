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
        {
            settingsPanel.SetActive(false);
        }

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayBGM(BGMType.Title_BGM);
        }
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

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayBGM(BGMType.InGame_BGM, true);
        }

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

        Screen.SetResolution(1080, 1920, false);
        Screen.orientation = ScreenOrientation.Portrait;
        return gameplaySceneName;
    }
}
