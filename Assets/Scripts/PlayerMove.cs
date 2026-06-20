using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    [Header("Move Settings")]
    public float moveSpeed = 5f;

    [Header("Move Limit")]
    public float minX = -8f;
    public float maxX = 8f;

    private float fixedZ;

    private PlayerHealth playerHealth;

    private void Start()
    {
        // 시작할 때의 Z 위치를 저장해서 계속 고정
        fixedZ = transform.position.z;
        playerHealth = GetComponent<PlayerHealth>();
    }

    private void Update()
    {
        if(!playerHealth.IsDead)
            Move();
    }

    private void Move()
    {
        float xInput = Input.GetAxisRaw("Horizontal");

        Vector3 moveDir = new Vector3(xInput, 0f, 0f).normalized;

        transform.position += moveDir * moveSpeed * Time.deltaTime;

        // 이동 범위 제한
        float clampedX = Mathf.Clamp(transform.position.x, minX, maxX);

        transform.position = new Vector3(clampedX, 0, fixedZ);
    }
}