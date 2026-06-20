using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target Settings")]
    [Tooltip("카메라가 따라갈 대상(플레이어)을 연결합니다.")]
    public Transform target;

    [Header("Offset Settings")]
    [Tooltip("타겟으로부터 카메라가 떨어질 거리(X, Y, Z)를 설정합니다.")]
    public Vector3 offset = new Vector3(0f, 5f, -10f);

    [Header("Movement Settings")]
    [Tooltip("값이 낮을수록 카메라가 더 부드럽고 묵직하게 따라옵니다.")]
    [Range(0.01f, 1f)]
    public float smoothSpeed = 0.125f;

    void LateUpdate()
    {
        // 1. 타겟이 지정되지 않았다면 실행하지 않음
        if (target == null) return;

        // 2. 카메라가 가야 할 목표 위치 계산
        Vector3 desiredPosition = target.position + offset;

        // 3. Mathf.Lerp 또는 Vector3.Lerp를 이용해 현재 위치에서 목표 위치로 부드럽게 보간
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

        // 4. 카메라 위치 갱신
        transform.position = smoothedPosition;

        // 5. 옵션: 카메라가 항상 타겟을 바라보게 만듦 (탑다운/쿼터뷰면 주석 처리해도 됨)
        transform.LookAt(target);
    }
}