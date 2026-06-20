using UnityEngine;

public class FallDashObject : MonoBehaviour
{
    [Header("Fall Settings")]
    public float fallSpeed = 5f;

    [Header("Dash Settings")]
    public float detectionRange = 4f;
    public float dashSpeed = 12f;

    [Header("Rotation Settings")]
    public float rotationOffset = -90f;

    [Header("Destroy Settings")]
    public float destroyY = -10f;

    public float dashLine;
    private Transform playerTransform;
    private bool isDashing = false;
    private Vector3 dashDirection;

    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            playerTransform = player.transform;
        }
    }

    private void Update()
    {
        if (!isDashing)
        {
            Fall();

            CheckPlayerInRange();
        }
        else
        {
            Dash();
        }

        CheckDestroy();
    }

    private void Fall()
    {
        transform.position += Vector3.down * fallSpeed * Time.deltaTime;
    }

    private void CheckPlayerInRange()
    {
        if (playerTransform == null)
        {
            return;
        }

        float distance = Vector3.Distance(transform.position, playerTransform.position);

        if (distance <= detectionRange && transform.position.y <= dashLine)
        {
            StartDash();
        }
    }

    private void StartDash()
    {
        isDashing = true;

        dashDirection = playerTransform.position - transform.position;

        // 3D 프로젝트에서 2D처럼 X/Y 평면으로만 이동
        dashDirection.z = 0f;

        dashDirection.Normalize();

        LookAtDirection(dashDirection);
    }

    private void Dash()
    {
        transform.position += dashDirection * dashSpeed * Time.deltaTime;
    }

    private void LookAtDirection(Vector3 direction)
    {
        if (direction == Vector3.zero)
        {
            return;
        }

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        transform.rotation = Quaternion.AngleAxis(angle + rotationOffset, Vector3.forward);
    }

    private void CheckDestroy()
    {
        if (transform.position.y <= destroyY)
        {
            Destroy(gameObject);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}