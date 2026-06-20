using UnityEngine;

public class VisionController : MonoBehaviour
{
    public static VisionController Instance { get; private set; }

    [Header("Vision Radius")]
    [SerializeField] private float maxVisionRadius = 10f;
    [SerializeField] private float minVisionRadius = 2f;

    public float CurrentRadius { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Update()
    {
        float hpRatio = Player.Instance.currentHP / Player.Instance.maxHP;
        CurrentRadius = Mathf.Lerp(minVisionRadius, maxVisionRadius, hpRatio);

        // 작살 사정거리를 시야 반지름에 동기화
        Player.Instance.harpoonMaxRange = CurrentRadius;
    }
}
