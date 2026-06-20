using UnityEngine;
using UnityEngine.InputSystem;

public class HarpoonLauncher : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject harpoonPrefab;   // �߻��� �ۻ� ������
    [SerializeField] private Transform firePoint;        // �ۻ��� �߻�� ��ġ (�ۻ��� ���κ�)

    private Camera mainCamera;
    private bool isHarpoonOut = false;  // �̹� �ۻ��� �߻�Ǿ� ���ư��� ������ üũ

    void Start()
    {
        mainCamera = Camera.main;
    }

    void Update()
    {
        RotateTowardsMouse();

        // ���콺 ���� Ŭ�� �� �ۻ� �߻� (�̹� �߻�� �ۻ��� ���� ����)
        if (Mouse.current.leftButton.wasPressedThisFrame && !isHarpoonOut)
        {
            FireHarpoon();
        }
    }

    private void RotateTowardsMouse()
    {
        // firePoint(손 위치) 기준으로 마우스 방향 계산
        Plane playerPlane = new Plane(Vector3.forward, firePoint.position);
        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (playerPlane.Raycast(ray, out float enterDistance))
        {
            Vector3 targetPoint = ray.GetPoint(enterDistance);
            Vector3 direction = targetPoint - firePoint.position;
            direction.z = 0f;

            if (direction != Vector3.zero)
            {
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.AngleAxis(angle - 90f, Vector3.forward);
            }
        }
    }

    private void FireHarpoon()
    {
        isHarpoonOut = true;

        // 발사 위치: 애니메이션으로 변하는 실제 손 위치
        // 발사 방향: firePoint.rotation 대신 마우스 방향으로 독립 계산
        //            → 애니메이션 회전이 섞이지 않음
        Quaternion fireRotation = GetMouseAimRotation(firePoint.position);
        GameObject harpoonObj = Instantiate(harpoonPrefab, firePoint.position, fireRotation);
        HarpoonProjectile harpoon = harpoonObj.GetComponent<HarpoonProjectile>();

        if (harpoon != null)
        {
            harpoon.Setup(
                this,
                Player.Instance.harpoonDamage,
                Player.Instance.harpoonSpeed,
                Player.Instance.harpoonMaxRange,
                firePoint
            );
        }
    }

    // firePoint.position → 마우스 방향으로 회전값 계산 (메시 보정 포함)
    private Quaternion GetMouseAimRotation(Vector3 fromPosition)
    {
        Plane targetPlane = new Plane(Vector3.forward, fromPosition);
        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (targetPlane.Raycast(ray, out float enterDistance))
        {
            Vector3 targetPoint = ray.GetPoint(enterDistance);
            Vector3 direction = targetPoint - fromPosition;
            direction.z = 0f;

            if (direction != Vector3.zero)
            {
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                return Quaternion.AngleAxis(angle - 90f, Vector3.forward) * Quaternion.Euler(-90f, 0f, 0f);
            }
        }

        return Quaternion.identity;
    }

    // �ۻ��� �ٽ� ���� �Ϸ����� �� ȣ��� �Լ�
    public void OnHarpoonReturned()
    {
        isHarpoonOut = false;
    }
}