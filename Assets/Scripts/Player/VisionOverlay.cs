using UnityEngine;

public class VisionOverlay : MonoBehaviour
{
    [SerializeField] private Material visionMaterial;
    [SerializeField] private float edgeSoftness = 0.02f;

    private Camera mainCamera;
    private Transform playerTransform;

    private void Start()
    {
        mainCamera = Camera.main;
        playerTransform = Player.Instance.transform;
    }

    private void Update()
    {
        if (visionMaterial == null || playerTransform == null) return;

        // 플레이어 월드 좌표 → 뷰포트 좌표 (0~1)
        Vector3 viewportPos = mainCamera.WorldToViewportPoint(playerTransform.position);
        visionMaterial.SetVector("_PlayerScreenPos", new Vector4(viewportPos.x, viewportPos.y, 0f, 0f));

        // 시야 반지름(월드) → 뷰포트 y 비율로 변환
        float worldRadius = VisionController.Instance != null ? VisionController.Instance.CurrentRadius : 0f;
        Vector3 upPoint      = playerTransform.position + mainCamera.transform.up * worldRadius;
        Vector3 upViewport   = mainCamera.WorldToViewportPoint(upPoint);
        float screenRadius   = Mathf.Abs(upViewport.y - viewportPos.y);

        visionMaterial.SetFloat("_Radius", screenRadius);
        visionMaterial.SetFloat("_EdgeSoftness", edgeSoftness);
    }
}
