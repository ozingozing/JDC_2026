using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    [Header("Damage Settings")]
    public int damageAmount = 1;

    [Header("After Hit")]
    public bool destroyAfterHit = true;

    private int enemyLayer;

    private void Awake()
    {
        enemyLayer = LayerMask.NameToLayer("Enemy");

        if (enemyLayer == -1)
        {
            Debug.LogWarning("Enemy 레이어가 존재하지 않습니다. Unity에서 Enemy 레이어를 먼저 만들어주세요.");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        DamagePlayer(other);
    }

    private void OnCollisionEnter(Collision collision)
    {
        DamagePlayer(collision.collider);
    }

    private void DamagePlayer(Collider other)
    {
        // 이 오브젝트가 Enemy 레이어가 아니면 데미지 주지 않음
        if (gameObject.layer != enemyLayer)
        {
            return;
        }

        PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();

        if (playerHealth == null)
        {
            return;
        }

        playerHealth.TakeDamage(damageAmount);

        if (destroyAfterHit)
        {
            Destroy(gameObject);
        }
    }
}
