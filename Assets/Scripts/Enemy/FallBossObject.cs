using System.Collections;
using UnityEngine;

public class FallBossObject : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private string playerTag = "Player";
    private Transform playerTransform;

    [Header("Move Settings")]
    [SerializeField] private float moveDuration = 1.0f;
    [SerializeField] private float arcHeight = 3.0f;
    [SerializeField] private float reDashDistance = 3.0f;
    [SerializeField] private float passThroughDistance = 5.0f;

    [Header("Delay Settings")]
    [SerializeField] private float waitAfterMove = 0.3f;

    [Header("Position Settings")]
    [SerializeField] private float fixedZ = 0f;

    [Header("Rotation Settings")]
    [SerializeField] private bool lookMoveDirection = true;
    [SerializeField] private float rotationOffset = -90f;

    private bool isMoving = false;

    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag(playerTag);

        if (player != null)
        {
            playerTransform = player.transform;
        }
        else
        {
            //Debug.LogWarning("Player 태그를 가진 오브젝트를 찾지 못했습니다.");
        }
    }

    private void Update()
    {
        if (playerTransform == null)
        {
            return;
        }

        if (isMoving)
        {
            return;
        }

        float distance = Vector2.Distance(
            new Vector2(transform.position.x, transform.position.y),
            new Vector2(playerTransform.position.x, playerTransform.position.y)
        );

        if (distance >= reDashDistance)
        {
            StartCoroutine(MoveThroughPlayerByArc());
        }
    }

    private IEnumerator MoveThroughPlayerByArc()
    {
        isMoving = true;

        Vector3 startPosition = transform.position;
        startPosition.z = fixedZ;

        Vector3 playerPosition = playerTransform.position;
        playerPosition.z = fixedZ;

        // 현재 위치에서 플레이어 방향 계산
        Vector3 directionToPlayer = playerPosition - startPosition;
        directionToPlayer.z = 0f;

        if (directionToPlayer == Vector3.zero)
        {
            directionToPlayer = Vector3.down;
        }

        directionToPlayer.Normalize();

        // 핵심: 플레이어 위치보다 더 먼 지점을 최종 목적지로 설정
        Vector3 endPosition = playerPosition + directionToPlayer * passThroughDistance;
        endPosition.z = fixedZ;

        float elapsedTime = 0f;

        while (elapsedTime < moveDuration)
        {
            elapsedTime += Time.deltaTime;

            float t = Mathf.Clamp01(elapsedTime / moveDuration);

            Vector3 basePosition = Vector3.Lerp(startPosition, endPosition, t);

            // 이동 방향에 수직인 방향을 구해서 포물선처럼 휘게 만듦
            Vector3 perpendicular = new Vector3(-directionToPlayer.y, directionToPlayer.x, 0f);

            float arc = Mathf.Sin(t * Mathf.PI) * arcHeight;

            Vector3 nextPosition = basePosition + perpendicular * arc;
            nextPosition.z = fixedZ;

            if (lookMoveDirection)
            {
                Vector3 moveDirection = nextPosition - transform.position;
                LookAtDirection(moveDirection);
            }

            transform.position = nextPosition;

            yield return null;
        }

        transform.position = endPosition;

        yield return new WaitForSeconds(waitAfterMove);

        isMoving = false;
    }

    private void LookAtDirection(Vector3 direction)
    {
        direction.z = 0f;

        if (direction == Vector3.zero)
        {
            return;
        }

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle + rotationOffset, Vector3.forward);
    }
}