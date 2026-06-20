using UnityEngine;

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
    [SerializeField] private int nextUpgradeScore = 100; // 다음 난이도로 넘어갈 목표 점수
    [SerializeField] private int upgradeScoreStep = 200; // 난이도 업그레이드 후, 다음 목표치 증가량

    [Header("References")]
    public MapManager mapManager;

    private void Awake()
    {
        // 싱글톤 패턴 적용 (어디서든 GameManager.Instance 로 접근 가능)
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        GameStart();
    }

    private void Update()
    {
        if (currentState == GameState.Playing)
        {
            // 1. 시간에 따라 점수(이동 거리) 증가
            currentScore += scorePerSecond * Time.deltaTime;

            // 2. 목표 점수 도달 시 난이도 업그레이드
            if (currentScore >= nextUpgradeScore)
            {
                UpgradeDifficulty();
            }
        }
    }

    public void GameStart()
    {
        currentState = GameState.Playing;
        Time.timeScale = 1f;
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

        // UI가 소수점까지 나오면 지저분하므로 내림(FloorToInt) 처리하여 출력
        Debug.Log($"게임 오버! 최종 이동 거리: {Mathf.FloorToInt(currentScore)}");
    }
}