using UnityEngine;
using UnityEngine.InputSystem;

public class HarpoonLauncher : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject harpoonPrefab;   // 발사할 작살 프리팹
    [SerializeField] private Transform firePoint;        // 작살이 발사될 위치 (작살총 끝부분)

    [Header("Harpoon Stats (Upgradable)")]
    public float damage = 10f;          // 작살 데미지
    public float speed = 15f;           // 작살 날아가는/회수되는 속도
    public float maxRange = 10f;        // 작살 사정거리

    private Camera mainCamera;
    private bool isHarpoonOut = false;  // 이미 작살이 발사되어 날아가는 중인지 체크

    void Start()
    {
        mainCamera = Camera.main;
    }

    void Update()
    {
        RotateTowardsMouse();

        // 마우스 왼쪽 클릭 시 작살 발사 (이미 발사된 작살이 없을 때만)
        if (Mouse.current.leftButton.wasPressedThisFrame && !isHarpoonOut)
        {
            FireHarpoon();
        }
    }

    private void RotateTowardsMouse()
    {
        // 1. 마우스의 스크린 좌표를 가져옴
        Vector2 mousePos = Mouse.current.position.ReadValue();

        // 2. 카메라에서 마우스 위치를 관통하는 레이(Ray)를 생성
        Ray ray = mainCamera.ScreenPointToRay(mousePos);

        // 플레이어와 같은 평면(Z=0)상의 위치를 가리키도록 레이캐스트 처리
        // Y축 전진 게임이므로, X-Y 평면 상의 충돌점을 찾습니다.
        Plane playerPlane = new Plane(Vector3.forward, transform.position);

        if (playerPlane.Raycast(ray, out float enterDistance))
        {
            Vector3 targetPoint = ray.GetPoint(enterDistance);

            // 3. 마우스 타겟을 향한 방향 벡터 계산 (Z축은 고정)
            Vector3 direction = targetPoint - transform.position;
            direction.z = 0f;

            // 4. 작살촉이 마우스를 바라보도록 회전 (Y축 전진 기준이므로 고유 각도 계산)
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            // 유니티 기본 방향(위쪽/Y축)에 맞추기 위해 -90도 보정을 해줍니다 (에셋에 따라 다를 수 있음)
            transform.rotation = Quaternion.AngleAxis(angle - 90f, Vector3.forward);
        }
    }

    private void FireHarpoon()
    {
        isHarpoonOut = true;

        // 작살 생성
        GameObject harpoonObj = Instantiate(harpoonPrefab, firePoint.position, firePoint.rotation);
        HarpoonProjectile harpoon = harpoonObj.GetComponent<HarpoonProjectile>();

        if (harpoon != null)
        {
            // 능력치 업그레이드가 반영되도록 런처의 스태츠를 작살에 주입(Injection)
            harpoon.Setup(this, damage, speed, maxRange, firePoint);
        }
    }

    // 작살이 다시 복귀 완료했을 때 호출될 함수
    public void OnHarpoonReturned()
    {
        isHarpoonOut = false;
    }
}