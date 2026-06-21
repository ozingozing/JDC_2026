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
        // ?ν?????? ???? ???? ????? ??? ??? ??? ??Ŀ??? Renderer ???
        if (targetRenderer == null)
        {
            targetRenderer = GetComponentInChildren<Renderer>();
        }

        if (targetRenderer != null)
        {
            // ?? ??????????? ???? ??????? ?ν???? ???
            targetMaterial = targetRenderer.material;
            originColor = targetMaterial.color;
        }
        else
        {
            Debug.LogError($"{gameObject.name}: Renderer?? ??? ?? ???????.");
        }
    }

    private void Start()
    {
        if (targetMaterial != null)
        {
            StartCoroutine(FadeRoutine());
        }
    }

    private IEnumerator FadeRoutine()
    {
        if(GameManager.Instance.isBossSpawned)
        {
            yield break;
        }

        float elapsedTime = 0f;

        // ???? ?? ?????? ?????
        SetAlpha(originColor.a);

        while (elapsedTime < lifeTime)
        {
            elapsedTime += Time.deltaTime;

            float progress = Mathf.Clamp01(elapsedTime / lifeTime);

            // lifeTime ???? originColor.a ?? 0???? ????
            float alpha = Mathf.Lerp(originColor.a, 0f, progress);

            SetAlpha(alpha);

            yield return null;
        }

        // ???????? ?????? ??????? ???
        SetAlpha(0f);

        SpawnFallingObject();

        Destroy(gameObject);
    }

    private void SetAlpha(float alpha)
    {
        if (targetMaterial == null)
        {
            return;
        }

        Color newColor = originColor;
        newColor.a = alpha;
        targetMaterial.color = newColor;
    }

    private void SpawnFallingObject()
    {
        if (fallingPrefab == null || fallingPrefab.Length == 0)
        {
            Debug.LogWarning($"{gameObject.name}: fallingPrefab이 비어 있습니다.");
            return;
        }

        if (GameManager.Instance == null)
        {
            Debug.LogWarning("GameManager.Instance가 없습니다.");
            return;
        }

        Vector3 spawnPosition;

        if (fallingSpawnPoint != null)
        {
            spawnPosition = fallingSpawnPoint.position;
        }
        else if (targetRenderer != null)
        {
            spawnPosition = targetRenderer.bounds.center;
        }
        else
        {
            spawnPosition = transform.position;
        }

        int currentStage = GameManager.Instance.currentStage;

        // currentStage가 2 이상이면 마지막 몹, 즉 보스만 한 번 생성
        if (currentStage >= 2)
        {
            if (GameManager.Instance.isBossSpawned)
            {
                return;
            }

            // 중요: Instantiate보다 먼저 true로 바꿔두는 게 안전함
            GameManager.Instance.TriggerWarning();
            if(!GameManager.Instance.startWarning)
            {
                GameManager.Instance.startWarning = true;
                return;
            }

            Debug.Log("게임끝");

            int lastIndex = fallingPrefab.Length - 1;

            Instantiate(
                fallingPrefab[lastIndex],
                spawnPosition,
                Quaternion.identity
            );
            GameManager.Instance.isBossSpawned = true;

            return;
        }

        // currentStage 0 → 0번만
        // currentStage 1 → 0~1번 랜덤
        int maxIdx = Mathf.Clamp(currentStage, 0, fallingPrefab.Length - 1);
        int randomIdx = Random.Range(0, maxIdx + 1);

        Instantiate(
            fallingPrefab[randomIdx],
            spawnPosition,
            Quaternion.identity
        );
    }

    private void OnDestroy()
    {
        if (targetMaterial != null)
        {
            Destroy(targetMaterial);
        }
    }
}