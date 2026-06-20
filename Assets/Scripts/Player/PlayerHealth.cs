using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{

    [Header("HP Bar")]
    [SerializeField] private Image hpFillImage;

    private void Start()
    {
        Player.Instance.currentHP = Player.Instance.maxHP;
        UpdateHPBar();
    }

    public void TakeDamage(int damage)
    {
        Player.Instance.currentHP -= damage;
        Player.Instance.currentHP = Mathf.Clamp(Player.Instance.currentHP, 0, Player.Instance.maxHP);

        UpdateHPBar();

        if (Player.Instance.currentHP <= 0)
        {
            Die();
        }
    }

    public void Heal(int healAmount)
    {
        Player.Instance.currentHP += healAmount;
        Player.Instance.currentHP = Mathf.Clamp(Player.Instance.currentHP, 0, Player.Instance.maxHP);

        UpdateHPBar();
    }

    private void UpdateHPBar()
    {
        if (hpFillImage == null)
        {
            return;
        }

        hpFillImage.fillAmount = (float)Player.Instance.currentHP / Player.Instance.maxHP;
    }

    private void Die()
    {
        Debug.Log("Player Dead!");
        Destroy(gameObject);
    }
}