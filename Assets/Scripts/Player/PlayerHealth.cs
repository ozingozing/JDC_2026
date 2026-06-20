using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [Header("HP Settings")]
    public int maxHP = 3;

    [SerializeField] private int currentHP;
    public bool IsDead => currentHP <= 0;

    [Header("HP Bar")]
    [SerializeField] private Image hpFillImage;

    private void Start()
    {
        currentHP = maxHP;
        UpdateHPBar();
    }

    public void TakeDamage(int damage)
    {
        currentHP -= damage;
        currentHP = Mathf.Clamp(currentHP, 0, maxHP);

        UpdateHPBar();

        Debug.Log($"Player HP: {currentHP}/{maxHP}");

        if (currentHP <= 0)
        {
            Die();
        }
    }

    public void Heal(int healAmount)
    {
        currentHP += healAmount;
        currentHP = Mathf.Clamp(currentHP, 0, maxHP);

        UpdateHPBar();
    }

    private void UpdateHPBar()
    {
        if (hpFillImage == null)
        {
            return;
        }

        hpFillImage.fillAmount = (float)currentHP / maxHP;
    }

    private void Die()
    {
        Debug.Log("Player Dead!");
        Destroy(gameObject);
    }
}