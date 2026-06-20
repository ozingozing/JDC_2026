using UnityEngine;

public class RandomSpawner : MonoBehaviour
{
    [Header("Spawn Object")]
    public GameObject spawnPrefab;

    [Header("Spawn Range")]
    public float minX = -5f;
    public float maxX = 5f;

    [Header("Spawn Position")]
    public float spawnY = 6f;
    public float fixedZ = 0f;

    [Header("Spawn Time")]
    public float spawnInterval = 0.8f;

    private void Start()
    {
        InvokeRepeating(nameof(Spawn), 0f, spawnInterval);
    }

    private void Spawn()
    {
        float randomX = Random.Range(minX, maxX);

        Vector3 spawnPosition = new Vector3(randomX, spawnY, fixedZ);

        Instantiate(spawnPrefab, spawnPosition, Quaternion.identity);
    }

    /*private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        Vector3 start = new Vector3(minX, spawnY, fixedZ);
        Vector3 end = new Vector3(maxX, spawnY, fixedZ);

        Gizmos.DrawLine(start, end);
        Gizmos.DrawSphere(start, 0.2f);
        Gizmos.DrawSphere(end, 0.2f);
    }*/
}