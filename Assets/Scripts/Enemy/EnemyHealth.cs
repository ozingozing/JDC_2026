using UnityEngine;
using UnityEngine.Events;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 100;
    private int currentHealth;

    public bool IsDead => currentHealth <= 0;

    [Header("Item Drop Settings")]
    [Tooltip("적 이 죽었을 때 생성할 아이템 프리팹을 연결합니다.")]
    [SerializeField] private GameObject itemPrefab;

    [Tooltip("아이템이 드롭될 확률 (0.0 ~ 1.0 -> 예: 0.5는 50%)")]
    [Range(0f, 1f)]
    [SerializeField] private float dropChance = 1f;

    [Tooltip("아이템이 스폰될 때 위쪽으로 살짝 띄워줄 오프셋 값")]
    [SerializeField] private Vector3 spawnOffset = new Vector3(0f, 0.5f, 0f);

    [Header("Events")]
    // 체력이 깎이거나 죽었을 때 연출(이펙트, 사운드, 애니메이션 등)을 연결할 수 있는 유니티 이벤트
    public UnityEvent<int> onDamageTaken;
    public UnityEvent onDeath;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damageAmount)
    {
        if (IsDead) return;

        currentHealth -= damageAmount;
        currentHealth = Mathf.Max(currentHealth, 0); // 0 이하로 떨어지지 않게 보정

        // 데미지 입었을 때 이벤트 호출 (예: UI 반영, 핏자국 이펙트 등)
        onDamageTaken?.Invoke(currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        onDeath?.Invoke();
        Debug.Log($"{gameObject.name}이(가) 사망했습니다.");

        DropItem();

        // 여기에 사망 시 처리 (예: Destroy(gameObject) 또는 오브젝트 풀 반환 등)
        Destroy(gameObject);
    }

    private void DropItem()
    {
        if (Random.value > dropChance)
        {
            return;
        }

        if (RandomSpawner.Instance == null)
        {
            Debug.LogWarning("RandomSpawner.Instance가 없습니다. 씬에 RandomSpawner 오브젝트가 있는지 확인하세요.");
            return;
        }

        GameObject randomItemPrefab = RandomSpawner.Instance.GetRandomSpawnItem();

        if (randomItemPrefab == null)
        {
            Debug.LogWarning("랜덤으로 가져올 아이템 프리팹이 없습니다.");
            return;
        }

        Vector3 spawnPosition = transform.position + spawnOffset;

        Instantiate(randomItemPrefab, spawnPosition, Quaternion.Euler(90f, 0f, 0f));
    }
}