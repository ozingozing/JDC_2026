using UnityEngine;

public class RandomSpawner : MonoBehaviour
{
    public static RandomSpawner Instance { get; private set; }

    [Header("Warning Spawn Object")]
    public GameObject spawnPrefab;

    [Header("Spawn Items")]
    public GameObject[] spawnItems;

    [Header("Item Spawn Position")]
    public float itemSpawnY = 30f;

    [Range(0f, 1f)]
    public float itemSpawnChance = 0.2f;

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
        if (GameManager.Instance != null && GameManager.Instance.isWarning) return;

        if(GameManager.Instance.canStart)
        {
            float randomX = Random.Range(minX, maxX);
            Vector3 spawnPosition = new Vector3(randomX, enemySpawnY, fixedZ);

            GameObject selectedEnemy = spawnPrefab;

            if (selectedEnemy != null)
            {
                Instantiate(selectedEnemy, spawnPosition, Quaternion.identity);
            }

            TrySpawnItem();
        }
    }


    private void TrySpawnItem()
    {
        if (spawnItems == null || spawnItems.Length == 0)
        {
            return;
        }

        if (Random.value > itemSpawnChance)
        {
            return;
        }

        float randomX = Random.Range(minX, maxX);
        Vector3 itemSpawnPosition = new Vector3(randomX, itemSpawnY, fixedZ);

        int randomIndex = Random.Range(0, spawnItems.Length);
        GameObject selectedItem = spawnItems[randomIndex];

        if (selectedItem != null)
        {
            Instantiate(selectedItem, itemSpawnPosition, Quaternion.Euler(90f, 0f, 0f));
        }
    }

    public void TrySpawnItem(Vector3 pos)
    {
        if (spawnItems == null || spawnItems.Length == 0)
        {
            return;
        }

        if (Random.value > itemSpawnChance)
        {
            return;
        }

        int randomIndex = Random.Range(0, spawnItems.Length);
        GameObject selectedItem = spawnItems[randomIndex];

        if (selectedItem != null)
        {
            Instantiate(selectedItem, pos, Quaternion.Euler(90f, 0f, 0f));
        }
    }

    public GameObject GetRandomSpawnItem()
    {
        if (spawnItems == null || spawnItems.Length == 0)
        {
            return null;
        }

        int randomIndex = Random.Range(0, spawnItems.Length);
        return spawnItems[randomIndex];
    }
}