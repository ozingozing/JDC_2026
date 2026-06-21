using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// WarningFade 프리팹에 추가.
/// 3D 렌더러를 숨기고 시야 밖에서만 보이는 UI 경고선을 대신 표시한다.
/// </summary>
[RequireComponent(typeof(WarningFade))]
public class WarningUITracker : MonoBehaviour
{
    [SerializeField] private Material warningOutsideMaterial;
    [SerializeField] private float edgeSoftness = 0.02f;

    private static Canvas _warningCanvas;

    private Camera mainCamera;
    private WarningFade warningFade;
    private RectTransform uiRect;
    private Image uiImage;
    private Material instanceMaterial;

    private void Awake()
    {
        mainCamera  = Camera.main;
        warningFade = GetComponent<WarningFade>();
    }

    private void Start()
    {
        if (warningOutsideMaterial == null) return;

        // 경고선 전용 Canvas (Sort Order 10) — 없으면 자동 생성
        if (_warningCanvas == null)
        {
            var go = new GameObject("WarningCanvas");
            _warningCanvas = go.AddComponent<Canvas>();
            _warningCanvas.renderMode   = RenderMode.ScreenSpaceOverlay;
            _warningCanvas.sortingOrder = 10;
            go.AddComponent<CanvasScaler>();
            go.AddComponent<GraphicRaycaster>();
            DontDestroyOnLoad(go);
        }

        // UI 요소 생성
        var uiGo = new GameObject("WarningUI_" + gameObject.name);
        uiGo.transform.SetParent(_warningCanvas.transform, false);
        uiRect = uiGo.AddComponent<RectTransform>();

        uiImage               = uiGo.AddComponent<Image>();
        uiImage.raycastTarget = false;
        instanceMaterial      = new Material(warningOutsideMaterial);
        uiImage.material      = instanceMaterial;

        // 3D 렌더러 크기 → UI 크기로 근사 변환
        Renderer r = GetComponentInChildren<Renderer>();
        if (r != null)
        {
            Vector3 min = mainCamera.WorldToScreenPoint(r.bounds.min);
            Vector3 max = mainCamera.WorldToScreenPoint(r.bounds.max);
            uiRect.sizeDelta = new Vector2(Mathf.Abs(max.x - min.x), Mathf.Abs(max.y - min.y));
        }
        else
        {
            uiRect.sizeDelta = new Vector2(100f, 10f);
        }

        // 원본 3D 렌더러 숨김 (UI가 대체)
        foreach (var rd in GetComponentsInChildren<Renderer>())
            rd.enabled = false;

        Debug.Log($"[WarningUI] 생성완료 | size={uiRect.sizeDelta} | mat={instanceMaterial != null} | canvas={_warningCanvas != null}");
        StartCoroutine(FadeRoutine());
    }

    private IEnumerator FadeRoutine()
    {
        float elapsed  = 0f;
        float lifeTime = warningFade.lifeTime;

        while (elapsed < lifeTime)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / lifeTime);
            uiImage.color = new Color(1f, 1f, 1f, alpha);
            yield return null;
        }
        uiImage.color = Color.clear;
    }

    private void Update()
    {
        if (uiRect == null) return;

        // 3D 위치 → Canvas 로컬 좌표로 변환
        Vector3 screenPos = mainCamera.WorldToScreenPoint(transform.position);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _warningCanvas.GetComponent<RectTransform>(), screenPos, null, out Vector2 localPos);
        uiRect.localPosition = localPos;

        // 셰이더에 플레이어 위치 + 시야 반지름 전달
        if (instanceMaterial == null || VisionController.Instance == null)
        {
            Debug.LogWarning($"[WarningUI] Update 스킵 | mat={instanceMaterial != null} | vision={VisionController.Instance != null}");
            return;
        }

        Vector3 playerVP = mainCamera.WorldToViewportPoint(Player.Instance.transform.position);
        instanceMaterial.SetVector("_PlayerScreenPos", new Vector4(playerVP.x, playerVP.y, 0f, 0f));

        float worldRadius  = VisionController.Instance.CurrentRadius;
        Vector3 upPoint    = Player.Instance.transform.position + mainCamera.transform.up * worldRadius;
        float screenRadius = Mathf.Abs(mainCamera.WorldToViewportPoint(upPoint).y - playerVP.y);
        instanceMaterial.SetFloat("_Radius",       screenRadius);
        instanceMaterial.SetFloat("_EdgeSoftness", edgeSoftness);
    }

    private void OnDestroy()
    {
        if (uiRect           != null) Destroy(uiRect.gameObject);
        if (instanceMaterial != null) Destroy(instanceMaterial);
    }
}
