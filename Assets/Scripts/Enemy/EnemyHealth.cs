using UnityEngine;
using UnityEngine.Events;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 100;
    private int currentHealth;

    public bool IsDead => currentHealth <= 0;

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

        // 여기에 사망 시 처리 (예: Destroy(gameObject) 또는 오브젝트 풀 반환 등)
        Destroy(gameObject);
    }
}