using UnityEngine;

public class Player : MonoBehaviour
{
    public static Player Instance { get; private set; }

    [Header("Harpoon Stats")]
    public float harpoonDamage = 10f;
    public float harpoonSpeed = 15f;
    public float harpoonMaxRange = 10f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void AddHarpoonDamage(float value)
    {
        harpoonDamage += value;
    }

    public void AddHarpoonSpeed(float value)
    {
        harpoonSpeed += value;
    }

    public void AddHarpoonRange(float value)
    {
        harpoonMaxRange += value;
    }
}
