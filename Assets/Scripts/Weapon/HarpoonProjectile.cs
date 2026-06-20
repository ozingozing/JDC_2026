using UnityEngine;

public class HarpoonProjectile : MonoBehaviour
{
    private enum HarpoonState { Outbound, Inbound }
    private HarpoonState currentState = HarpoonState.Outbound;

    private HarpoonLauncher launcher;
    private float damage;
    private float speed;
    private float maxRange;
    private Transform ownerTransform; // 돌아올 목적지 (플레이어의 발사 위치)

    private Vector3 startPosition;

    private int enemyLayer;

    private void Awake()
    {
        enemyLayer = LayerMask.NameToLayer("Enemy");
    }

    public void Setup(HarpoonLauncher launcher, float dmg, float spd, float range, Transform owner)
    {
        this.launcher = launcher;
        this.damage = dmg;
        this.speed = spd;
        this.maxRange = range;
        this.ownerTransform = owner;

        this.startPosition = transform.position;
        currentState = HarpoonState.Outbound;
    }

    void Update()
    {
        switch (currentState)
        {
            case HarpoonState.Outbound:
                MoveForward();
                CheckRange();
                break;

            case HarpoonState.Inbound:
                ReturnToOwner();
                break;
        }
    }

    private void MoveForward()
    {
        // 발사된 방향(로컬 위쪽 방향)으로 전진
        transform.Translate(Vector3.up * speed * Time.deltaTime, Space.Self);
    }

    private void CheckRange()
    {
        // 시작 지점으로부터 이동한 거리 체크
        float traveledDistance = Vector3.Distance(startPosition, transform.position);

        // 사정거리에 도달하면 돌아오는 상태로 변경
        if (traveledDistance >= maxRange)
        {
            currentState = HarpoonState.Inbound;
        }
    }

    private void ReturnToOwner()
    {
        if (ownerTransform == null)
        {
            Destroy(gameObject);
            return;
        }

        // 플레이어 위치를 향해 이동
        transform.position = Vector3.MoveTowards(transform.position, ownerTransform.position, speed * Time.deltaTime);

        // 플레이어 회전 방향에 맞춰 작살촉이 플레이어를 바라보게 뒤집기 (선택 연출)
        Vector3 returnDir = ownerTransform.position - transform.position;
        if (returnDir != Vector3.zero)
        {
            float angle = Mathf.Atan2(returnDir.y, returnDir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle - 90f, Vector3.forward);
        }

        // 플레이어에게 완전히 도착하면 오브젝트 파괴 및 런처 상태 초기화
        if (Vector3.Distance(transform.position, ownerTransform.position) < 0.1f)
        {
            launcher.OnHarpoonReturned();
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == enemyLayer)
        {
            // 2. 해당 오브젝트에서 Health 컴포넌트를 가져옴
            EnemyHealth enemyHealth = other.GetComponent<EnemyHealth>();

            // 3. 만약 컴포넌트가 있다면 데미지를 입힘
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage((int)damage);
            }
        }
    }
}