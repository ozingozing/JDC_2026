using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [Header("HP Bar")]
    [SerializeField] private Image hpFillImage;

    [Header("HP Drain")]
    [SerializeField] private float drainPerSecond = 5f;
    [SerializeField] private float minHP = 10f;

    private void Start()
    {
        Player.Instance.currentHP = Player.Instance.maxHP;
        UpdateHPBar();
    }

    private void Update()
    {
        if (Player.Instance.IsDead) return;

        if (Player.Instance.currentHP > minHP)
        {
            Player.Instance.currentHP = Mathf.Max(minHP, Player.Instance.currentHP - drainPerSecond * Time.deltaTime);
            UpdateHPBar();
        }
    }

    public void TakeDamage(float damage)
    {
        Player.Instance.currentHP -= damage;
        Player.Instance.currentHP = Mathf.Clamp(Player.Instance.currentHP, 0f, Player.Instance.maxHP);

        UpdateHPBar();

        if (Player.Instance.currentHP <= 0f)
        {
            Die();
        }
    }

    public void Heal(float healAmount)
    {
        Player.Instance.currentHP += healAmount;
        Player.Instance.currentHP = Mathf.Clamp(Player.Instance.currentHP, 0f, Player.Instance.maxHP);

        UpdateHPBar();
    }

    private void UpdateHPBar()
    {
        if (hpFillImage == null) return;
        hpFillImage.fillAmount = Player.Instance.currentHP / Player.Instance.maxHP;
    }

    private void Die()
    {
        Debug.Log("Player Dead!");
        GameManager.Instance.GameOver();
        Destroy(gameObject);
    }
}