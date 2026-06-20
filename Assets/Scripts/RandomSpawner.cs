using UnityEngine;

public class RandomSpawner : MonoBehaviour
{
    public static RandomSpawner Instance { get; private set; }

    [Header("Spawn Object")]
    public GameObject spawnPrefab;

    [Header("Spawn Items")]
    public GameObject[] spawnItems;
    [Header("Itme Spawn Position")]
    public float itemSpawnY = 30f;

    [Range(0f, 1f)]
    public float itemSpawnChance = 0.2f; // 20% 확률

    [Header("Spawn Range")]
    public float minX = -5f;
    public float maxX = 5f;

    [Header("Enemy Spawn Position")]
    public float enemySpawnY = 0f;

    public float fixedZ = 0f;

    [Header("Spawn Time")]
    public float spawnInterval = 0.8f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        InvokeRepeating(nameof(Spawn), 0f, spawnInterval);
    }

    private void Spawn()
    {
        float randomX = Random.Range(minX, maxX);
        Vector3 spawnPosition = new Vector3(randomX, enemySpawnY, fixedZ);

        // 기본 오브젝트 생성
        if (spawnPrefab != null)
        {
            Instantiate(spawnPrefab, spawnPosition, Quaternion.identity);
        }

        // 특정 확률로 아이템 생성
        TrySpawnItem();
    }

    private void TrySpawnItem()
    {
        if (spawnItems == null || spawnItems.Length == 0)
        {
            return;
        }

        float randomValue = Random.value;

        if (randomValue > itemSpawnChance)
        {
            return;
        }

        float randomX = Random.Range(minX, maxX);
        Vector3 itemSpawnPosition = new Vector3(randomX, itemSpawnY, fixedZ);

        int randomIndex = Random.Range(0, spawnItems.Length);
        GameObject selectedItem = spawnItems[randomIndex];

        if (selectedItem != null)
        {
            Instantiate(selectedItem, itemSpawnPosition, Quaternion.Euler(90, 0, 0));
        }
    }

    public void TrySpawnItem(Vector3 pos)
    {
        if (spawnItems == null || spawnItems.Length == 0)
        {
            return;
        }

        float randomValue = Random.value;

        if (randomValue > itemSpawnChance)
        {
            return;
        }

        int randomIndex = Random.Range(0, spawnItems.Length);
        GameObject selectedItem = spawnItems[randomIndex];

        if (selectedItem != null)
        {
            Instantiate(selectedItem, pos, Quaternion.Euler(90, 0, 0));
        }
    }
}