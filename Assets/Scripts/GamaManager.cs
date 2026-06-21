using UnityEngine;
using TMPro;
using System.Collections;
using Unity.Burst.CompilerServices;
using UnityEngine.UI;
public enum GameState { Ready, Playing, Paused, GameOver }

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game State")]
    public GameState currentState = GameState.Ready;

    [Header("Progression (Distance)")]
    public float currentScore = 0f;                      // 현재 점수(이동 거리)
    [SerializeField] private float scorePerSecond = 10f; // 1초당 오르는 점수 (속도감 밸런싱 용도)

    [Header("Difficulty Upgrade")]
    public int currentStage = 0;
    [SerializeField] private int nextUpgradeScore = 100; // 다음 난이도로 넘어갈 목표 점수
    [SerializeField] private int upgradeScoreStep = 200; // 난이도 업그레이드 후, 다음 목표치 증가량

    [Header("References")]
    public MapManager mapManager;

    [SerializeField] private GameObject endPanel;
    [SerializeField] private TMP_Text scoreText;

    // ==========================================
    // [새로 추가됨] 경고 UI 연출을 위한 변수들
    // ==========================================
    [Header("Warning UI Setup")]
    [SerializeField] private Image warningImage;         // 캔버스에 배치한 PNG 경고 이미지 컴포넌트 연결
    [SerializeField] private float flashDuration = 0.5f; // 깜빡이는 속도 (초)
    [SerializeField] private int flashCount = 3;         // 깜빡일 횟수
    private Coroutine warningCoroutine;
    public bool startWarning = false; // 외부에서 경고를 시작할 수 있는 플래그
    // ==========================================

    public bool isBossSpawned = false;
    public bool canStart = false;
    public bool isWarning = false;

    private void Awake()
    {
        // 싱글톤 패턴 적용 (어디서든 GameManager.Instance 로 접근 가능)
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        endPanel.SetActive(false);
        warningImage.gameObject.SetActive(false);
        StartCoroutine(WaitStart());
    }

    private IEnumerator WaitStart()
    {
        yield return new WaitForSeconds(10f);
        canStart = true;
        GameStart();
    }

    // ==========================================
    // [새로 추가됨] 외부에서 호출하거나 내부에서 쓸 경고 실행 함수
    // ==========================================
    public void TriggerWarning()
    {
        if (warningImage == null) return;

        // 이미 실행 중인 경고 코루틴이 있다면 중지하고 새로 시작
        if (warningCoroutine != null) StopCoroutine(warningCoroutine);
        warningCoroutine = StartCoroutine(WarningFlashRoutine());
    }

    private IEnumerator WarningFlashRoutine()
    {
        isWarning = true;
        warningImage.gameObject.SetActive(true);
        SoundManager.Instance.PlaySFX(SFXType.BossAppear_SFX);
        for (int i = 0; i < flashCount; i++)
        {
            // 1. 점점 붉게 나타나기 (알파값 0 -> 0.6)
            float elapsed = 0f;
            while (elapsed < flashDuration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(0f, 0.6f, elapsed / flashDuration);
                warningImage.color = new Color(1f, 1f, 1f, alpha); // 빨간색 오파시티 조절
                yield return null;
            }

            // 2. 점점 투명하게 사라지기 (알파값 0.6 -> 0)
            elapsed = 0f;
            while (elapsed < flashDuration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(0.6f, 0f, elapsed / flashDuration);
                warningImage.color = new Color(1f, 1f, 1f, alpha);
                yield return null;
            }
        }

        warningImage.color = Color.clear; // 완전히 투명하게 초기화
        isWarning = false;
    }
    // ==========================================

    private void Update()
    {
        if(!canStart) return;
        if (currentState == GameState.Playing)
        {
            // 1. 시간에 따라 점수(이동 거리) 증가
            currentScore += scorePerSecond * Time.deltaTime;

            // 2. 목표 점수 도달 시 난이도 업그레이드
            if (currentScore >= nextUpgradeScore)
            {
                currentStage++;
                UpgradeDifficulty();
            }
        }
    }

    public void GameStart()
    {
        currentState = GameState.Playing;
        Time.timeScale = 1f;
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayLoopSFX(SFXType.DeepSeaBase_SFX, 0.25f);
        }
        Debug.Log("게임 시작!");
    }

    private void UpgradeDifficulty()
    {
        if (mapManager != null)
        {
            mapManager.UpgradeStage(); // 맵 테마 변경 지시
        }

        // 다음 업그레이드 목표치 갱신 (예: 100 -> 300 -> 500)
        nextUpgradeScore += upgradeScoreStep;

        Debug.Log($"난이도 상승! 다음 목표 점수: {nextUpgradeScore}");

        // TODO: 여기서 적 스폰 매니저에게 "적을 더 빨리 스폰해라" 등의 명령을 추가할 수 있습니다.
    }

    // 적을 작살로 처치했을 때 호출할 보너스 점수 함수 (선택 사항)
    public void AddBonusScore(float amount)
    {
        if (currentState == GameState.Playing)
        {
            currentScore += amount;
        }
    }

    public void GameOver()
    {
        currentState = GameState.GameOver;
        Time.timeScale = 0f; // 화면 정지

        endPanel.SetActive(true);
        scoreText.text = $"Score: {currentScore}";
    }

    public void GameOver(string txt)
    {
        currentState = GameState.GameOver;
        Time.timeScale = 0f; // 화면 정지

        endPanel.SetActive(true);
        scoreText.text = $"{txt}" +
            $"Score: {currentScore}";
    }

    public void StopGame()
    {
        currentState = GameState.Paused;
        Time.timeScale = 0f;
        Debug.Log("게임 일시정지 및 종료 시도");

        // 1. 실제 빌드된 게임 프로그램 종료
        Application.Quit();

        // 2. 유니티 에디터에서 플레이 모드(재생)를 즉시 종료하는 코드
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
