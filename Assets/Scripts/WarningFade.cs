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
        // ?¥í?????? ???? ???? ????? ??? ??? ??? ??¨¨??? Renderer ???
        if (targetRenderer == null)
        {
            targetRenderer = GetComponentInChildren<Renderer>();
        }

        if (targetRenderer != null)
        {
            // ?? ??????????? ???? ??????? ?¥í???? ???
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
        if (fallingPrefab == null)
        {
            Debug.LogWarning($"{gameObject.name}: fallingPrefab?? ??????? ???????.");
            return;
        }

        Vector3 spawnPosition;

        if (fallingSpawnPoint != null)
        {
            spawnPosition = fallingSpawnPoint.position;
        }
        else if (targetRenderer != null)
        {
            // Renderer?? ???? ???? ??? ???
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