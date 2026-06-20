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
        // 인스펙터에 직접 넣지 않았다면 자기 자신 또는 자식에서 Renderer 찾기
        if (targetRenderer == null)
        {
            targetRenderer = GetComponentInChildren<Renderer>();
        }

        if (targetRenderer != null)
        {
            // 이 오브젝트만의 개별 머티리얼 인스턴스 사용
            targetMaterial = targetRenderer.material;
            originColor = targetMaterial.color;
        }
        else
        {
            Debug.LogError($"{gameObject.name}: Renderer를 찾을 수 없습니다.");
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
        float elapsedTime = 0f;

        // 시작 시 완전히 보이게
        SetAlpha(originColor.a);

        while (elapsedTime < lifeTime)
        {
            elapsedTime += Time.deltaTime;

            float progress = Mathf.Clamp01(elapsedTime / lifeTime);

            // lifeTime 동안 originColor.a → 0으로 감소
            float alpha = Mathf.Lerp(originColor.a, 0f, progress);

            SetAlpha(alpha);

            yield return null;
        }

        // 마지막에 완전히 투명하게 확정
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
        if (fallingPrefab == null)
        {
            Debug.LogWarning($"{gameObject.name}: fallingPrefab이 연결되지 않았습니다.");
            return;
        }

        Vector3 spawnPosition;

        if (fallingSpawnPoint != null)
        {
            spawnPosition = fallingSpawnPoint.position;
        }
        else if (targetRenderer != null)
        {
            // Renderer의 실제 화면상 중앙 위치
            spawnPosition = targetRenderer.bounds.center;
        }
        else
        {
            spawnPosition = transform.position;
        }


        int currentStage = GameManager.Instance.currentStage;
        int maxIdx = Mathf.Clamp(currentStage, 0, fallingPrefab.Length - 1);
        int randomIdx = Random.Range(0, maxIdx + 1);

        GameObject.Instantiate(fallingPrefab[randomIdx], transform.position, Quaternion.identity);
    }

    private void OnDestroy()
    {
        if (targetMaterial != null)
        {
            Destroy(targetMaterial);
        }
    }
}