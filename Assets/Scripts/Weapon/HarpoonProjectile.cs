using UnityEngine;

public class HarpoonProjectile : MonoBehaviour
{
    private enum HarpoonState { Outbound, Inbound }
    private HarpoonState currentState = HarpoonState.Outbound;

    [SerializeField] private float fixedReturnTime = 1.0f;

    private HarpoonLauncher launcher;
    private float damage;
    private float speed;
    private float maxRange;
    private Transform ownerTransform;

    private Vector3 startPosition;
    private float returnSpeed;

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
        if(ownerTransform == null)
        {
            return;
        }
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
        // X=-90° 보정 후 날 끝이 로컬 +Z(forward)를 가리키므로 forward로 이동
        transform.Translate(Vector3.forward * speed * Time.deltaTime, Space.Self);
    }

    private void CheckRange()
    {
        if(ownerTransform == null)
        {
            Destroy(gameObject);
            Debug.Log("플레이어 죽음");
            return;
        }
        float traveledDistance = Vector3.Distance(startPosition, transform.position);

        if (traveledDistance >= maxRange)
        {
            // 복귀 시작 시점의 거리로 고정 복귀 속도 계산
            float distanceToOwner = Vector3.Distance(transform.position, ownerTransform.position);
            returnSpeed = distanceToOwner / Mathf.Max(fixedReturnTime, 0.01f);
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

        transform.position = Vector3.MoveTowards(transform.position, ownerTransform.position, returnSpeed * Time.deltaTime);

        // �÷��̾� ȸ�� ���⿡ ���� �ۻ����� �÷��̾ �ٶ󺸰� ������ (���� ����)
        Vector3 returnDir = ownerTransform.position - transform.position;
        if (returnDir != Vector3.zero)
        {
            float angle = Mathf.Atan2(returnDir.y, returnDir.x) * Mathf.Rad2Deg;
            // 발사 때와 동일한 X=-90° 보정을 유지해 날 끝이 플레이어를 향하게 함
            transform.rotation = Quaternion.AngleAxis(angle - 90f, Vector3.forward) * Quaternion.Euler(-90f, 0f, 0f);
        }

        // �÷��̾�� ������ �����ϸ� ������Ʈ �ı� �� ��ó ���� �ʱ�ȭ
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
            // 2. �ش� ������Ʈ���� Health ������Ʈ�� ������
            EnemyHealth enemyHealth = other.GetComponent<EnemyHealth>();

            // 3. ���� ������Ʈ�� �ִٸ� �������� ����
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage((int)damage);
            }
        }
    }
}