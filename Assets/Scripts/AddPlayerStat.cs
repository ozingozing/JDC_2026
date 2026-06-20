using System.Collections;
using UnityEngine;

public class AddPlayerStat : MonoBehaviour
{
    // 1. 인스펙터에서 고를 수 있는 스탯 타입 Enum 정의
    public enum StatType
    {
        HarpoonDamage,
        HarpoonSpeed,
        HarpoonMaxRange
    }

    [Header("Stat Enhancement")]
    [Tooltip("강화할 스탯 종류를 선택하세요.")]
    public StatType targetStat;

    [Tooltip("추가할 스탯 수치입니다.")]
    public float statValue = 1f;

    [Header("Pickup Delay")]
    [Tooltip("아이템이 활성화된 후 획득 가능해지기까지의 시간")]
    public float pickupDelay = 0.3f;

    private bool canPickup = false;

    private void OnEnable()
    {
        canPickup = false;
        StartCoroutine(PickupDelayRoutine());
    }

    private IEnumerator PickupDelayRoutine()
    {
        yield return new WaitForSeconds(pickupDelay);
        canPickup = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(canPickup == false)
        {
            return;
        }

        // 플레이어와 충돌했을 때만 스탯을 적용
        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            ApplyStatToPlayer();
            Destroy(gameObject); // 아이템을 먹은 후 제거
        }
    }

    /// <summary>
    /// 이 함수가 호출되면 설정된 Enum 타입에 따라 플레이어의 스탯을 증가시킵니다.
    /// (아이템 획득, 레벨업, 상점 버튼 클릭 이벤트 등에 연결하여 사용하세요)
    /// </summary>
    public void ApplyStatToPlayer()
    {
        // 싱글톤 인스턴스가 존재하는지 안전하게 체크
        if (Player.Instance == null)
        {
            Debug.LogWarning("씬에 Player 인스턴스가 존재하지 않아 스탯을 추가할 수 없습니다.");
            return;
        }

        // 2. Enum 타입에 따라 분기 처리 (switch문 활용)
        switch (targetStat)
        {
            case StatType.HarpoonDamage:
                Player.Instance.AddHarpoonDamage(statValue);
                Debug.Log($"플레이어 작살 데미지 증가: +{statValue} (현재: {Player.Instance.harpoonDamage})");
                break;

            case StatType.HarpoonSpeed:
                Player.Instance.AddHarpoonSpeed(statValue);
                Debug.Log($"플레이어 작살 속도 증가: +{statValue} (현재: {Player.Instance.harpoonSpeed})");
                break;

            case StatType.HarpoonMaxRange:
                Player.Instance.AddHarpoonRange(statValue);
                Debug.Log($"플레이어 작살 사거리 증가: +{statValue} (현재: {Player.Instance.harpoonMaxRange})");
                break;
        }
    }
}