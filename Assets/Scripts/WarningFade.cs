using System.Collections;
using UnityEngine;

public class WarningFade : MonoBehaviour
{
    [Header("Fade Settings")]
    public float lifeTime = 1.5f;

    [Header("References")]
    [SerializeField] private Renderer targetRenderer;

    [Header("Spawn Point")]
    [SerializeField] private Transform fallingSpawnPoint;

    [Header("Next Object")]
    public GameObject[] fallingPrefab;

    private Material targetMaterial;
    private Color originColor;

    private void Awake()
    {
        if (targetRenderer == null)
            targetRenderer = GetComponentInChildren<Renderer>();

        if (targetRenderer != null)
        {
            targetMaterial = targetRenderer.material;
            originColor    = targetMaterial.color;
        }
        else
        {
            Debug.LogError($"{gameObject.name}: Renderer를 찾을 수 없습니다.");
        }
    }

    private void Start()
    {
        if (targetMaterial != null)
            StartCoroutine(FadeRoutine());
    }

    private IEnumerator FadeRoutine()
    {
        if (GameManager.Instance != null && GameManager.Instance.isBossSpawned)
            yield break;

        float elapsed = 0f;
        SetAlpha(originColor.a);

        while (elapsed < lifeTime)
        {
            elapsed += Time.deltaTime;
            SetAlpha(Mathf.Lerp(originColor.a, 0f, Mathf.Clamp01(elapsed / lifeTime)));
            yield return null;
        }

        SetAlpha(0f);
        SpawnFallingObject();
        Destroy(gameObject);
    }

    private void SetAlpha(float alpha)
    {
        if (targetMaterial == null) return;
        Color c = originColor;
        c.a = alpha;
        targetMaterial.color = c;
    }

    // ─── 책임: 스폰 여부 판단 + 경고 1회 트리거 ───────────────────────
    private void SpawnFallingObject()
    {
        if (fallingPrefab == null || fallingPrefab.Length == 0) return;
        if (GameManager.Instance == null) return;

        int stage = GameManager.Instance.currentStage;

        if (stage >= 2 && !GameManager.Instance.isBossSpawned)
        {
            if (!GameManager.Instance.startWarning)
            {
                // 보스 등장 전 경고 UI는 여기서 딱 1번만 트리거
                GameManager.Instance.TriggerWarning();
                GameManager.Instance.startWarning = true;
                return; // 이번엔 스폰 안 함, 다음 WarningFade에서 스폰
            }
        }

        // 경고 UI가 재생 중이면 끝날 때까지 대기 후 스폰
        if (GameManager.Instance.isWarning)
        {
            StartCoroutine(WaitThenSpawn());
            return;
        }

        DoSpawn();
    }

    // ─── 책임: 실제 프리팹 인스턴스화만 수행 ─────────────────────────
    private void DoSpawn()
    {
        if (GameManager.Instance == null) return;
        if (GameManager.Instance.isBossSpawned) return;

        Vector3 pos = fallingSpawnPoint != null ? fallingSpawnPoint.position
                    : targetRenderer != null    ? targetRenderer.bounds.center
                    : transform.position;

        int stage = GameManager.Instance.currentStage;

        if (stage >= 2)
        {
            int lastIdx = fallingPrefab.Length - 1;
            Instantiate(fallingPrefab[lastIdx], pos, Quaternion.identity);
            GameManager.Instance.isBossSpawned = true;
            Debug.Log("보스 스폰");
        }
        else
        {
            int maxIdx   = Mathf.Clamp(stage, 0, fallingPrefab.Length - 1);
            int randomIdx = Random.Range(0, maxIdx + 1);
            Instantiate(fallingPrefab[randomIdx], pos, Quaternion.identity);
        }
    }

    // ─── 경고 UI 종료 대기 후 DoSpawn 직행 (재귀 없음) ──────────────
    private IEnumerator WaitThenSpawn()
    {
        yield return new WaitWhile(() => GameManager.Instance != null && GameManager.Instance.isWarning);
        DoSpawn();
    }

    private void OnDestroy()
    {
        if (targetMaterial != null)
            Destroy(targetMaterial);
    }
}
